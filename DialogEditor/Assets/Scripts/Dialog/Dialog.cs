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

    [SerializeField]private List<DialogSet> m_dialogSets = new List<DialogSet>();
    private string m_lineDescriptor = ""; 
    public bool AnyPartIsSelected {  get { return m_dialogSets.Any(p => p.IsSelected);  } }

    public string DialogName { get { return m_dialogName; } }
    #endregion

    #region Constructor
    public Dialog(string _name, string _id)
    {
        m_dialogName = _name;
        m_spreadSheetID = _id;
        m_dialogSets = new List<DialogSet>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Add a new part to the dialog
    /// </summary>
    /// <param name="_pos"></param>
    public void AddPart(Vector2 _pos)
    {
        m_dialogSets.Add(new DialogSet(_pos, RemovePart, m_nodeStyle, m_connectionPointStyle, m_icon, m_answerIcon, m_pointIcon)); 
    }

    /// <summary>
    /// Drag all the dialog parts
    /// </summary>
    /// <param name="_delta"></param>
    public void DragAll(Vector2 _delta)
    {
        if (m_dialogSets == null) return; 
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].Drag(_delta); 
        }
    }

    /// <summary>
    /// Draw the dialog
    /// </summary>
    public void Draw(Action<DialogLine> _onOutContentSelected, Action<DialogSet> _onInPartSelected)
    {
        if (m_dialogSets == null) m_dialogSets = new List<DialogSet>();
        bool _change = false;
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].Draw(m_lineDescriptor, m_dialogSets, _onOutContentSelected, _onInPartSelected);
            if(m_dialogSets[i].ProcessEvent(Event.current))
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
        if (m_dialogSets == null) m_dialogSets = new List<DialogSet>(); 
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].InitEditorSettings(_nodeStyle, _connectionPointStyle, _basicIcon, _answerIcon, m_pointIcon, RemovePart); 
        }
        if (File.Exists(Path.Combine(Application.persistentDataPath, "LineDescriptors", m_dialogName + ".lua")))
        {
            m_lineDescriptor = File.ReadAllText(Path.Combine(Application.persistentDataPath, "LineDescriptors", m_dialogName + ".lua"));
        }
    }

    /// <summary>
    /// Process the events of the Dialog
    /// </summary>
    /// <param name="_e">Current Event</param>
    /// <returns></returns>
    public bool ProcessEvent(Event _e)
    {
        if(m_dialogSets.Any(p => p.IsSelected) && _e.keyCode == KeyCode.Delete)
        {
            DialogSet _selectedPart = m_dialogSets.Where(p => p.IsSelected).FirstOrDefault();
            m_dialogSets.Remove(_selectedPart); 
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
    private void RemovePart(DialogSet _part)
    {
        if(m_dialogSets.Contains(_part))
            m_dialogSets.Remove(_part); 
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
