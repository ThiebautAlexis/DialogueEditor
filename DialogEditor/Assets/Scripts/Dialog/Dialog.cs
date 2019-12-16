using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO; 

[Serializable]
public class Dialog
{
    #region Fields and Properties
#if UNITY_EDITOR
    private GUIStyle m_nodeStyle = null;
    private GUIStyle m_connectionPointStyle = null; 
    private GUIContent m_icon = null;
    private GUIContent m_answerIcon = null;
    private GUIContent m_pointIcon = null; 
    public const float INITIAL_RECT_WIDTH = 300;
    public const float CONTENT_WIDTH = 280;
    public const float MARGIN_HEIGHT = 12;
    public const float TITLE_HEIGHT = 20;
    public const float SPACE_HEIGHT = 10;
    public const float POPUP_HEIGHT = 15;
    public const float BUTTON_HEIGHT = 30;
    public const float BASIC_CONTENT_HEIGHT = 100;
#endif
    [SerializeField]private string m_dialogName = "";
    [SerializeField]private string m_spreadSheetID = "";
    public string SpreadSheetID { get { return m_spreadSheetID; } }

    [SerializeField]private List<DialogPart> m_dialogParts = new List<DialogPart>();
    private List<string> m_linesID = new List<string>();
    private List<string> m_linesContent = new List<string>(); 
    public bool AnyPartIsSelected {  get { return m_dialogParts.Any(p => p.IsSelected);  } }
#endregion

    #region Constructor
    public Dialog(string _name, string _id)
    {
        m_dialogName = _name;
        m_spreadSheetID = _id;
        m_dialogParts = new List<DialogPart>(); 
    }
#endregion

    #region Methods
    /// <summary>
    /// Add a new part to the dialog
    /// </summary>
    /// <param name="_pos"></param>
    public void AddPart(Vector2 _pos)
    {
        m_dialogParts.Add(new DialogPart(_pos, RemovePart, m_nodeStyle, m_connectionPointStyle, m_icon, m_answerIcon, m_pointIcon)); 
    }

    /// <summary>
    /// Drag all the dialog parts
    /// </summary>
    /// <param name="_delta"></param>
    public void DragAll(Vector2 _delta)
    {
        if (m_dialogParts == null) return; 
        for(int i = 0; i < m_dialogParts.Count; i++)
        {
            m_dialogParts[i].Drag(_delta); 
        }
    }

    /// <summary>
    /// Draw the dialog
    /// </summary>
    public void Draw(Action<DialogContent> _onOutContentSelected, Action<DialogPart> _onInPartSelected)
    {
        if (m_dialogParts == null) m_dialogParts = new List<DialogPart>();
        bool _change = false;
        for(int i = 0; i < m_dialogParts.Count; i++)
        {
            m_dialogParts[i].Draw(m_linesID, m_linesContent, m_dialogParts, _onOutContentSelected, _onInPartSelected);
            if(m_dialogParts[i].ProcessEvent(Event.current))
            {
                _change = true;
            }
        }
        _change = ProcessEvent(Event.current); 
        GUI.changed = _change; 
    }

    /// <summary>
    /// Init the Editor settings for the dialog and all of them parts
    /// </summary>
    /// <param name="_nodeStyle">Style of the node</param>
    /// <param name="_basicIcon">Icon of the basic Node Type</param>
    /// <param name="_answerIcon">Icon of the Answer Node Type</param>
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _basicIcon, GUIContent _answerIcon, GUIContent _pointIcon)
    {
        m_nodeStyle = _nodeStyle;
        m_icon = _basicIcon;
        m_answerIcon = _answerIcon;
        m_pointIcon = _pointIcon; 
        if (m_dialogParts == null) m_dialogParts = new List<DialogPart>(); 
        for(int i = 0; i < m_dialogParts.Count; i++)
        {
            m_dialogParts[i].InitEditorSettings(_nodeStyle, _connectionPointStyle, _basicIcon, _answerIcon, m_pointIcon, RemovePart); 
        }
        if(File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor", $"{m_spreadSheetID}.csv")))
        {
            string[] _text = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor", $"{m_spreadSheetID}.csv"));
            List<string> _tempIDs = new List<string>();
            List<string> _tempQuotes = new List<string>();
            string[] _part; 
            for (int i = 1; i < _text.Length; i++)
            {
               _part = _text[i].Split(',');
                if (_part.Length < 2)
                {
                    Debug.Log("Can't Split"); 
                    continue;
                }
               _tempIDs.Add(_part[0]);
               _tempQuotes.Add(_part[1]); 
            }
            m_linesID = _tempIDs;
            m_linesContent = _tempQuotes; 
        }
    }

    /// <summary>
    /// Process the events of the Dialog
    /// </summary>
    /// <param name="_e">Current Event</param>
    /// <returns></returns>
    public bool ProcessEvent(Event _e)
    {
        if(m_dialogParts.Any(p => p.IsSelected) && _e.keyCode == KeyCode.Delete)
        {
            DialogPart _selectedPart = m_dialogParts.Where(p => p.IsSelected).FirstOrDefault();
            m_dialogParts.Remove(_selectedPart); 
            return true; 
        }
        if (_e.type == EventType.KeyDown && _e.control && _e.keyCode == KeyCode.S)
        {
            SaveDialog();
        }
        return false; 
    }

    /// <summary>
    /// Remove the part
    /// </summary>
    /// <param name="_part">Part to remove</param>
    private void RemovePart(DialogPart _part)
    {
        if(m_dialogParts.Contains(_part))
            m_dialogParts.Remove(_part); 
    }

    /// <summary>
    /// Save the dialog as a Json File
    /// </summary>
    private void SaveDialog()
    {
        string _jsonDialog = JsonUtility.ToJson(this);
        string _name = m_dialogName.Replace(" ", string.Empty);
        Debug.Log(m_dialogName + " has been saved in " + Path.Combine(Application.persistentDataPath, "Dialogs", _name)); 
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "Dialogs", _name), _jsonDialog);
        UnityEditor.EditorUtility.DisplayDialog("File saved", $"The {m_dialogName} dialog has been successfully saved", "Ok!"); 
    }

    #endregion
}
