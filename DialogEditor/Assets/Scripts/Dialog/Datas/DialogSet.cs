using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq; 
using UnityEditor; 

[Serializable]
public class DialogSet : DialogNode
{
    #region Fields and Properties
    [SerializeField] private List<DialogLine> m_dialogLines = new List<DialogLine>(); 
    [SerializeField] private DialogSetType m_type = DialogSetType.BasicType;
    [SerializeField] private bool m_isStartingSet = false; 

    public List<DialogLine> DialogLines { get { return m_dialogLines; }}
    public DialogSetType Type { get { return m_type; } }
    public bool IsStartingSet
    {
        get
        {
            return m_isStartingSet;
        }
        set
        {
            m_isStartingSet = value; 
        }
    }

    private Action<DialogSet> m_onRemoveDialogPart = null;
    private Action<DialogSet> m_onSetStartingSet = null; 
    private GUIContent m_basicSetIcon = null;
    private GUIContent m_answerIcon = null;
    private GUIContent m_startingSetIcon = null; 
    #endregion


    #region Constructor
#if UNITY_EDITOR
    public DialogSet(Vector2 _nodePosition, Action<DialogSet> _onRemovePart, Action<DialogSet> _onSetStartingSet, GUIStyle _normalStyle, GUIStyle _connectionPointStyle, GUIContent _dialogPartIcon, GUIContent _answerIcon, GUIContent _startingSetIcon, GUIContent _pointIcon)
    {
        m_NodeToken = UnityEngine.Random.Range(0, int.MaxValue); 
        m_nodeRect = new Rect(_nodePosition.x, _nodePosition.y, INITIAL_NODE_WIDTH, 0);
        m_onRemoveDialogPart = _onRemovePart;
        m_onSetStartingSet = _onSetStartingSet;
        m_nodeStyle = _normalStyle;
        m_connectionPointStyle = _connectionPointStyle;
        m_dialogLines = new List<DialogLine>();
        m_basicSetIcon = _dialogPartIcon;
        m_answerIcon = _answerIcon;
        m_startingSetIcon = _startingSetIcon;
        m_currentIcon = m_type == DialogSetType.BasicType ? m_basicSetIcon : m_answerIcon;
        m_pointIcon = _pointIcon;
        AddNewContent();
    }
#endif
    #endregion


    #region Methods
#if UNITY_EDITOR
    /// <summary>
    /// Add new content to this part
    /// </summary>
    private void AddNewContent()
    {
        if (m_dialogLines == null) m_dialogLines = new List<DialogLine>();
        if (m_type == DialogSetType.BasicType && m_dialogLines.Count > 0)
        {
            m_dialogLines.Last().LinkedToken = -1;
        }
        m_dialogLines.Add(new DialogLine());
        m_nodeRect = new Rect(m_nodeRect.position.x,
            m_nodeRect.position.y,
            INITIAL_NODE_WIDTH, 
            INITIAL_NODE_HEIGHT + SPACE_HEIGHT + (DIALOGLINE_SETTINGS_HEIGHT * m_dialogLines.Count) + (SPACE_HEIGHT * (m_dialogLines.Count+1)) + BUTTON_HEIGHT); 
    }

    /// <summary>
    /// Change the type of the dialog part and modify the GUI according to the new type
    /// </summary>
    /// <param name="_type"></param>
    private void ChangeType(DialogSetType _type)
    {
        m_type = _type;
        switch (m_type)
        {
            case DialogSetType.BasicType:
                m_currentIcon = m_basicSetIcon;
                for (int i = 0; i < m_dialogLines.Count - 1; i++)
                {
                    m_dialogLines[i].LinkedToken = -1; 
                }
                break;
            case DialogSetType.PlayerAnswer:
                m_currentIcon = m_answerIcon; 
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Draw the Dialog Set Editor
    /// </summary>
    /// <param name="_lineDescriptor">Line Descriptor</param>
    /// <param name="_otherSets">The other dialog sets</param>
    /// <param name="_onOutDialogLineSelected">Action called when an out point is selected</param>
    /// <param name="_onInDialogNodeSelected">Action called when the In point of the Dialog set is selected</param>
    public void Draw(string _lineDescriptor, List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, Action<DialogLine> _onOutDialogLineSelected, Action<DialogNode> _onInDialogNodeSelected, List<CharacterColorSettings> _colorSettings)
    {
        
        // --- Draw the connections between the parts --- //
        if(GUI.Button(InPointRect, m_pointIcon, m_connectionPointStyle))
        {
            _onInDialogNodeSelected.Invoke(this); 
        }
        
        // --- Draw the Set and its Lines --- //
        GUI.Box(m_nodeRect, "", m_nodeStyle);
        Rect _r = new Rect(m_nodeRect.position.x + m_nodeRect.width - 35, m_nodeRect.position.y + MARGIN_HEIGHT, 25, 25);
        if(GUI.Button(_r, m_currentIcon, m_nodeStyle))
        {
            ProcessContextMenu();  
        }
        _r = new Rect(m_nodeRect.x + 10, _r.y, CONTENT_WIDTH , TITLE_HEIGHT);
        GUI.Label(_r, m_type.ToString() + " " + m_NodeToken.ToString() );
        _r.y = m_nodeRect.y + INITIAL_NODE_HEIGHT + SPACE_HEIGHT; 
        DialogLine _c; 
        for (int i = 0; i < m_dialogLines.Count; i++)
        {
            _c = m_dialogLines[i]; 
            _r.y = _c.Draw(_r.position, _lineDescriptor, RemoveContent, m_type , (m_type == DialogSetType.BasicType && i == m_dialogLines.Count - 1), m_pointIcon, m_connectionPointStyle,_onOutDialogLineSelected, _otherSets, _otherConditions, _colorSettings);
        }
        _r = new Rect(_r.position.x, _r.y, _r.width, BUTTON_HEIGHT); 
        if(GUI.Button(_r,"Add new Dialog Line"))
        {
            AddNewContent(); 
        }
        // --- Draw the starting Icon if this set is the Starting Set --- //
        if (m_isStartingSet)
        {
            _r = new Rect(m_nodeRect.position.x + m_nodeRect.width - 35 - 25 , m_nodeRect.position.y + MARGIN_HEIGHT, 25, 25);
            GUI.Box(_r, m_startingSetIcon, m_nodeStyle);
        }
    }

    /// <summary>
    /// Call the event "m_onRemoveDialogPart" with argument as itself
    /// -> Remove this set from the Dialog 
    /// </summary>
    private void OnClickRemoveNode()
    {
        m_onRemoveDialogPart?.Invoke(this);
    }

    /// <summary>
    /// Display the Context menu on the selected DialogPart
    /// </summary>
    protected override void ProcessContextMenu()
    {
        IsSelected = true;
        GenericMenu _genericMenu = new GenericMenu();
        switch (m_type)
        {
            case DialogSetType.BasicType:
                _genericMenu.AddDisabledItem(new GUIContent("Set as Basic Dialog Part"));
                _genericMenu.AddItem(new GUIContent("Set as Answer Dialog Part"), false, () => ChangeType(DialogSetType.PlayerAnswer));
                break;
            case DialogSetType.PlayerAnswer:
                _genericMenu.AddItem(new GUIContent("Set as Basic Dialog Part"), false, () => ChangeType(DialogSetType.BasicType));
                _genericMenu.AddDisabledItem(new GUIContent("Set as Answer Dialog Part"));
                break;
            default:
                break;
        }
        if(m_isStartingSet)
            _genericMenu.AddDisabledItem(new GUIContent("Set as Starting Dialog Set"));
        else 
            _genericMenu.AddItem(new GUIContent("Set as Starting Dialog Set"), false, SetStartingSet);
        _genericMenu.AddSeparator("");
        _genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        _genericMenu.ShowAsContext();
    }
    
    /// <summary>
    /// Set this set as the starting one in the Dialog
    /// </summary>
    private void SetStartingSet()
    {
        m_onSetStartingSet?.Invoke(this); 
    }

    /// <summary>
    /// Remove the selected Content and rescale the Rect
    /// </summary>
    /// <param name="_content"></param>
    private void RemoveContent(DialogLine _content)
    {
        m_dialogLines.Remove(_content);
        m_nodeRect = new Rect(m_nodeRect.position.x, m_nodeRect.position.y, INITIAL_NODE_WIDTH, INITIAL_NODE_HEIGHT + SPACE_HEIGHT + (DIALOGLINE_SETTINGS_HEIGHT * m_dialogLines.Count) + (SPACE_HEIGHT * (m_dialogLines.Count + 1)) + BUTTON_HEIGHT);
    }

    /// <summary>
    /// Initialize the Editor Settings for the part
    /// </summary>
    /// <param name="_nodeStyle">The Node Style of the Set</param>
    /// <param name="_connectionPointStyle">Style of the connection point</param>
    /// <param name="_dialogSetIcon">Icon of the Basic Dailog set</param>
    /// <param name="_answerIcon">Icon of the Answer Dialog Set</param>
    /// <param name="_startingSetIcon">Icon of the Starting Set</param>
    /// <param name="_pointIcon">Icon of the in/out points</param>
    /// <param name="_onRemoveSet">Action Called to remove the Set from the Dialog</param>
    /// <param name="_setStartingSet">Action called when the set is switch as the starting set</param>
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _dialogSetIcon, GUIContent _answerIcon, GUIContent _startingSetIcon, GUIContent _pointIcon ,Action<DialogSet> _onRemoveSet, Action<DialogSet> _setStartingSet)
    {
        m_nodeStyle = _nodeStyle;
        m_onRemoveDialogPart = _onRemoveSet;
        m_onSetStartingSet = _setStartingSet; 
        m_basicSetIcon = _dialogSetIcon;
        m_answerIcon = _answerIcon;
        m_startingSetIcon = _startingSetIcon;
        m_currentIcon = m_type == DialogSetType.BasicType ? m_basicSetIcon : m_answerIcon;
        m_connectionPointStyle = _connectionPointStyle;
        m_pointIcon = _pointIcon; 
    }
#endif

    #endregion
}

public enum DialogSetType
{
    BasicType, 
    PlayerAnswer
}