using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using MoonSharp.Interpreter;
using System.Linq; 

public class DialogReader : MonoBehaviour
{
    #region Fields and Property 
    [SerializeField] private string m_dialogName = "";
    [SerializeField] private TMP_Text m_textDisplayer = null;
    [SerializeField] private TMP_FontAsset m_font = null;
    [SerializeField] private float m_fontSize = 12;
    [SerializeField] private Color m_fontColor = Color.black;

    private Dialog m_dialog = null;
    private Script m_lineDescriptor = null;
    #endregion

    #region Methods

    #region Original Methods
    private void InitReader()
    {
        if (m_dialogName == string.Empty) return;
        string _lineDescriptor = File.ReadAllText(Path.Combine(Dialog.LineDescriptorPath, m_dialogName + ".lua"));
        m_lineDescriptor = new Script();
        m_lineDescriptor.DoString(_lineDescriptor);

        string _jsonFile = File.ReadAllText(Path.Combine(Dialog.DialogAssetPath, m_dialogName));
        m_dialog = JsonUtility.FromJson<Dialog>(_jsonFile); 

        if(m_textDisplayer)
        {
            if (m_font) m_textDisplayer.font = m_font;
            m_textDisplayer.fontSize = m_fontSize;
            m_textDisplayer.color = m_fontColor; 
        }      
    }

    private IEnumerator DisplayDialog()
    {
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
        InitReader(); 

    }

    private void Start()
    {
        StartCoroutine(DisplayDialog()); 
    }

    #endregion

    #endregion
}
