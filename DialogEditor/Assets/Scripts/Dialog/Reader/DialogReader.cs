using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using MoonSharp.Interpreter;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations; 

public class DialogReader : MonoBehaviour
{
    #region Fields and Property 
    [SerializeField] private string m_dialogName = "";
    [SerializeField] private TMP_Text m_textDisplayer = null;
    [SerializeField] private TMP_FontAsset m_font = null;
    [SerializeField] private float m_fontSize = 12;
    [SerializeField] private Color m_fontColor = Color.black;

    private AsyncOperationHandle<TextAsset> m_lineDescriptorAsyncHandler;
    private AsyncOperationHandle<TextAsset> m_dialogAssetAsyncHandler;

    private Dialog m_dialog = null;
    private Script m_lineDescriptor = null;
    #endregion

    #region Methods

    #region Original Methods
    private void InitReader()
    {
        if(m_textDisplayer)
        {
            if (m_font) m_textDisplayer.font = m_font;
            m_textDisplayer.fontSize = m_fontSize;
            m_textDisplayer.color = m_fontColor; 
        }      
    }

    private IEnumerator DisplayDialog()
    {
        while (!m_dialogAssetAsyncHandler.IsDone ||!m_lineDescriptorAsyncHandler.IsDone)
        {
            yield return new WaitForSeconds(.5f); 
        }
        if (m_dialog == null)
        {
            Debug.Log("Dialog is null"); 
            yield break;
        }
        // Get the Starting Dialog Set //
        DialogSet _set = m_dialog.GetNextSet();
        yield return StartCoroutine(DisplayDialogSet(_set)); 
        while (true)
        {
            _set = m_dialog.GetNextSet(_set.DialogLines.Last().LinkedToken);
            if (_set == null) break;
            yield return StartCoroutine(DisplayDialogSet(_set));
        }
        yield return null;
        m_textDisplayer.text = string.Empty; 
    }

    private IEnumerator DisplayDialogSet(DialogSet _set)
    {
        switch (_set.Type)
        {
            case DialogPartType.BasicType:
                for (int i = 0; i < _set.DialogLines.Count; i++)
                {
                    m_textDisplayer.text = GetDialogLineContent(_set.DialogLines[i].Key, "Text_En_en");
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            case DialogPartType.PlayerAnswer:
                break;
            default:
                break;
        }
        
    }

    private string GetDialogLineContent(string _dialogLineID, string _localisationKey)
    {
        DynValue _content = m_lineDescriptor.Globals.Get(_dialogLineID).Table.Get(_localisationKey);
        return _content.String;
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (m_dialogName != string.Empty)
        {
            m_lineDescriptorAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(m_dialogName);
            m_lineDescriptorAsyncHandler.Completed += OnDialogAssetLoaded;
            m_dialogAssetAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(m_dialogName + Dialog.LineDescriptorPostfix);
            m_dialogAssetAsyncHandler.Completed += OnLineDescriptorLoaded;
        }
        
        InitReader(); 

    }

    private void OnDialogAssetLoaded(AsyncOperationHandle<TextAsset> _loadedAsset)
    {
        if(_loadedAsset.Result == null)
        {
            Debug.LogError("IS NULL");
            return; 
        }
        m_dialog = JsonUtility.FromJson<Dialog>(_loadedAsset.Result.ToString());
        Debug.Log("Dialog is ready"); 
    }

    private void OnLineDescriptorLoaded(AsyncOperationHandle<TextAsset> _loadedAsset)
    {
        if (_loadedAsset.Result == null)
        {
            Debug.LogError("IS NULL");
            return;
        }
        m_lineDescriptor = new Script();
        m_lineDescriptor.DoString(_loadedAsset.Result.ToString());
        Debug.Log("Line Descriptor is ready"); 
    }

    private void Start()
    {
        StartCoroutine(DisplayDialog()); 
    }

    #endregion

    #endregion
}
