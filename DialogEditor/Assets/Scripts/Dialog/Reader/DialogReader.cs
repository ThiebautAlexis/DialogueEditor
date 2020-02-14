using System.Collections;
using UnityEngine;
using TMPro;
using MoonSharp.Interpreter;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Events;

public class DialogReader : MonoBehaviour
{
    #region Fields and Property 
    [SerializeField] private string m_dialogName = "";
    [SerializeField] private TMP_Text m_textDisplayer = null;
    [SerializeField] private TMP_FontAsset m_font = null;
    [SerializeField] private float m_fontSize = 12;
    [SerializeField] private Color m_fontColor = Color.black;
    [SerializeField] private string m_dialogAnswerHandlerName = "MultipleChoicesHandler"; 

    private AsyncOperationHandle<TextAsset> m_lineDescriptorAsyncHandler;
    private AsyncOperationHandle<TextAsset> m_dialogAssetAsyncHandler;
    private GameObject m_dialogAnswerHandler = null; 

    private Dialog m_dialog = null;
    private Script m_lineDescriptor = null;

    private System.Action m_onMouseClicked = null; 


    public string DialogName
    {
        get
        {
            return m_dialogName;
        }
    }
    public UnityEventString OnDialogLineRead { get; private set; } = new UnityEventString();
    #endregion

    #region Methods

    #region Original Methods

    #region Load Dialog
    /// <summary>
    /// Initialize the settings of the text displayer
    /// </summary>
    private void InitReader()
    {
        if (m_textDisplayer)
        {
            if (m_font) m_textDisplayer.font = m_font;
            m_textDisplayer.fontSize = m_fontSize;
            m_textDisplayer.color = m_fontColor;
        }
    }

    /// <summary>
    /// Called when the DialogAsset is loaded
    /// Get the Dialog and Start loading the LineDescriptor
    /// </summary>
    /// <param name="_loadedAsset">The loaded asset Handler</param>
    private void OnDialogAssetLoaded(AsyncOperationHandle<TextAsset> _loadedAsset)
    {
        if (_loadedAsset.Result == null)
        {
            Debug.LogError("IS NULL");
            return;
        }
        m_dialog = JsonUtility.FromJson<Dialog>(_loadedAsset.Result.ToString());
        Debug.Log("Dialog is ready");
        m_lineDescriptorAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(m_dialog.SpreadSheetID.GetHashCode().ToString() + Dialog.LineDescriptorPostfix);
        m_lineDescriptorAsyncHandler.Completed += OnLineDescriptorLoaded;
    }

    /// <summary>
    /// Called when the linedescriptor is loaded
    /// DoString on the line descriptor content 
    /// </summary>
    /// <param name="_loadedAsset">The loaded asset Handler</param>
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

    /// <summary>
    /// Called when the <paramref name="_answerHandler"/> is loaded with success. 
    /// Set the <see cref="m_dialogAnswerHandler"/> as the <paramref name="_answerHandler"/> Result
    /// </summary>
    /// <param name="_answerHandler">Loaded asset</param>
    private void OnAnswerHandlerAssetLoaded(AsyncOperationHandle<GameObject> _answerHandler)
    {
        if (_answerHandler.Result == null)
        {
            Debug.LogError("IS NULL");
            return;
        }
        m_dialogAnswerHandler = _answerHandler.Result;
    }
    #endregion

    #region Display Dialogues lines
    /// <summary>
    /// Display the whole dialog
    /// </summary>
    /// <returns></returns>
    public void StartDisplayingDialog()
    {
        if (m_dialog == null)
        {
            Debug.Log("Dialog is null");
            return; 
        }
        // Get the Starting Dialog Set //
        DialogSet _set = m_dialog.GetFirstSet();
        DisplayDialogSet(_set);
    }

    /// <summary>
    /// Display the dialog set according to its type
    /// </summary>
    /// <param name="_set"></param>
    /// <param name="_index"></param>
    private void DisplayDialogSet(DialogSet _set, int _index = 0)
    {
        if (_set == null)
        {
            m_textDisplayer.text = string.Empty;
            return;
        }
        switch (_set.Type)
        {
            case DialogSetType.BasicType:
                DisplayDialogLineAtIndex(_set, _index);
                break;
            case DialogSetType.PlayerAnswer:
                DisplayAnswerDialogSet(_set);
                break;
        }
    }

    /// <summary>
    /// Instanciate the loaded asset <see cref="m_dialogAnswerHandler"/> and Initialise it using the dialog <paramref name="_set"/>
    /// </summary>
    /// <param name="_set">Displayed Set</param>
    private void DisplayAnswerDialogSet(DialogSet _set)
    {
        m_onMouseClicked = null; 
        Transform _canvas = FindObjectOfType<Canvas>().transform; 
        Instantiate(m_dialogAnswerHandler, _canvas).GetComponent<DialogAnswerHandler>().InitHandler(this, _set.DialogLines); 
    }

    /// <summary>
    /// USED FROM A DIALOG ANSWER HANDLER
    /// Display the selected Dialog Line and procede to the next dialog set
    /// </summary>
    /// <param name="_line">The line to display</param>
    public void DisplayDialogLine(DialogLine _line)
    {
        OnDialogLineRead?.Invoke(_line.Key);
        if (!DialogsSettingsManager.DialogsSettings.OverrideCharacterColor)
        {
            if (DialogsSettingsManager.DialogsSettings.CharactersColor.Any(c => c.CharacterIdentifier == _line.CharacterIdentifier))
                m_textDisplayer.color = DialogsSettingsManager.DialogsSettings.CharactersColor.Where(c => c.CharacterIdentifier == _line.CharacterIdentifier).Select(c => c.CharacterColor).FirstOrDefault();
            else m_textDisplayer.color = m_fontColor;
        }
        else
            m_textDisplayer.color = m_fontColor;
        m_textDisplayer.text = GetDialogLineContent(_line.Key, DialogsSettingsManager.DialogsSettings.CurrentLocalisationKey);
        DialogSet _nextSet = m_dialog.GetNextSet(_line.LinkedToken);
        if(_line.WaitingType == WaitingType.WaitForClick)
            m_onMouseClicked += () => DisplayDialogSet(_nextSet);
        else
            StartCoroutine(WaitBeforeDisplayDialogSet(_nextSet, 0, _line.WaitingTime));

    }

    /// <summary>
    /// Display the dialog line of the dialog set at the selected index
    /// </summary>
    /// <param name="_set">Displayed Dialog Set</param>
    /// <returns></returns>
    private void DisplayDialogLineAtIndex(DialogSet _set, int _index = 0)
    {
        m_onMouseClicked = null; 
        DialogLine _line = _set.DialogLines[_index]; 
        OnDialogLineRead?.Invoke(_line.Key); 
        if (!DialogsSettingsManager.DialogsSettings.OverrideCharacterColor)
        {
            if (DialogsSettingsManager.DialogsSettings.CharactersColor.Any(c => c.CharacterIdentifier == _line.CharacterIdentifier))
                m_textDisplayer.color = DialogsSettingsManager.DialogsSettings.CharactersColor.Where(c => c.CharacterIdentifier == _line.CharacterIdentifier).Select(c => c.CharacterColor).FirstOrDefault();
            else m_textDisplayer.color = m_fontColor;
        }
        else
            m_textDisplayer.color = m_fontColor; 
        m_textDisplayer.text = GetDialogLineContent(_line.Key, DialogsSettingsManager.DialogsSettings.CurrentLocalisationKey);
        _index++; 
        if(_set.DialogLines.Count == _index)
        {
            _set = m_dialog.GetNextSet(_line.LinkedToken);
            _index = 0; 
        }
        if (_line.WaitingType == WaitingType.WaitForClick)
            m_onMouseClicked += () => DisplayDialogSet(_set, _index);
        else
            StartCoroutine(WaitBeforeDisplayDialogSet(_set, _index, _line.WaitingTime)); 
    }

    /// <summary>
    /// Get the Content of the Dialog Line according to the localisationKey Selected
    /// </summary>
    /// <param name="_dialogLineID">ID of the Dialog Line</param>
    /// <param name="_localisationKey">Localisation Key to use</param>
    /// <returns></returns>
    public string GetDialogLineContent(string _dialogLineID, string _localisationKey)
    {
        DynValue _content = m_lineDescriptor.Globals.Get(_dialogLineID).Table.Get(_localisationKey);
        return _content.String;
    }

    /// <summary>
    /// Wait <paramref name="_waitingTime"/> seconds.
    /// Then Display the dialog line at the <paramref name="_index"/> Index of the <paramref name="_set"/>
    /// </summary>
    /// <param name="_set">Next set to display</param>
    /// <param name="_index">Index of the dialog line to display</param>
    /// <param name="_waitingTime">Time to wait before displaying</param>
    /// <returns></returns>
    private IEnumerator WaitBeforeDisplayDialogSet(DialogSet _set, int _index, float _waitingTime)
    {
        yield return new WaitForSeconds(_waitingTime);
        DisplayDialogSet(_set, _index);
    }
    #endregion

    #region Set Localisation Key
    /// <summary>
    /// Call the method <see cref="DialogsSettingsManager.SetLocalisationKeyIndex(int)"/> to update the localisation key of the current profile
    /// </summary>
    /// <param name="_index">Index of the new Localisation Key</param>
    public void SetLocalisationKey(int _index) => DialogsSettingsManager.SetLocalisationKeyIndex(_index);

    #endregion

    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (m_dialogName != string.Empty)
        {
            m_dialogAssetAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(m_dialogName);
            m_dialogAssetAsyncHandler.Completed += OnDialogAssetLoaded;
        }
        if(m_dialogAnswerHandlerName != string.Empty)
        {
            Addressables.LoadAssetAsync<GameObject>(m_dialogAnswerHandlerName).Completed += OnAnswerHandlerAssetLoaded; 
        }
        InitReader(); 
    }
    private void Start()
    {
        m_onMouseClicked += StartDisplayingDialog;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            m_onMouseClicked?.Invoke(); 
    }
    #endregion

    #endregion
}

[System.Serializable]
public class UnityEventString : UnityEvent<string>{}