using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using MoonSharp.Interpreter; 

[Serializable]
public class DialogLine
{
    #region Fields and Properties
    [SerializeField] private int m_index = -1;
    private int m_nextIndex = -1;
    [SerializeField] private string m_key = "";
    private string m_content = "";
    [SerializeField] private int m_linkedToken = -1;
    [SerializeField] private float m_initalWaitingTime = 1.0f;
    [SerializeField] private WaitingType m_waitingType = WaitingType.WaitForTime;
    [SerializeField] private float m_extraWaitingTime = 0.0f;
    private Rect m_pointRect;

    private string[] m_ids = null; 
    public Rect PointRect { get { return m_pointRect;  } }
    
    public string Key { get { return m_key; } }
    public string CharacterIdentifier { get { return m_key.Substring(0, 2); } }
    public int LinkedToken { get { return m_linkedToken; } set { m_linkedToken = value; } }
    public float InitialWaitingTime { get { return m_initalWaitingTime; } }
    public WaitingType WaitingType { get { return m_waitingType; } }
    public float ExtraWaitingTime { get { return m_extraWaitingTime; } }
    #endregion

    #region Methods

#if UNITY_EDITOR
    /// <summary>
    /// Get all the Ids of the dialog lines from the Line Descriptor
    /// </summary>
    /// <param name="_lineDescriptor"></param>
    public void InitEditor(string _lineDescriptor)
    {
        Script _luaScript = new Script();
        _luaScript.DoString(_lineDescriptor);
        List<DynValue> _dynValues = _luaScript.Globals.Values.Where(v => (v.Type == DataType.Table) && (v.Table.Get("ID").IsNotNil())).ToList();
        m_ids = _dynValues.Select(v => v.Table.Get("ID").String).ToArray();
    }

    /// <summary>
    /// Draw the Dialog Line
    /// </summary>
    /// <param name="_startPos">Starting position of the rect within the line will be drawn</param>
    /// <param name="_lineDescriptor">Line Descriptor</param>
    /// <param name="_removeAction">Action called when the Dialog Line is removed</param>
    /// <param name="_isLastPoint">Does this Dialog Line has to draw an out point</param>
    /// <param name="_pointIcon">Icon of the out point</param>
    /// <param name="_pointStyle"> Style of the point
    /// <param name="_onOutLineSelected">Action called when the out point of this line is selected</param>
    /// <param name="_otherSets">The other sets in the current Dialog</param>
    /// <returns>Height used to draw the dialog Line</returns>
    public float Draw(Vector2 _startPos, string _lineDescriptor, Action<DialogLine> _removeAction, DialogSetType _dialogSetType, bool _isLastPoint, GUIContent _pointIcon, GUIStyle _pointStyle, Action<DialogLine> _onOutLineSelected, List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, List<CharacterColorSettings> _colorSettings)
    {
        if (m_ids == null)
            InitEditor(_lineDescriptor); 
        if(m_content == string.Empty && m_key != string.Empty)
        {
            Script _luaScript = new Script();
            _luaScript.DoString(_lineDescriptor);
            DynValue _content = _luaScript.Globals.Get(m_key).Table.Get("Text_En_en");

            m_content = _content.String;
        }
        Rect _r = new Rect(_startPos.x, _startPos.y, DialogNode.POPUP_HEIGHT, DialogNode.POPUP_HEIGHT);
        if (GUI.Button(_r, "-"))
        {
            _removeAction.Invoke(this);
            return _r.y;
        }
        _r = new Rect(_r.position.x + DialogNode.POPUP_HEIGHT, _r.position.y, DialogNode.CONTENT_WIDTH - DialogNode.POPUP_HEIGHT, DialogNode.POPUP_HEIGHT);

        Color _originalColor = GUI.backgroundColor;
        if(_colorSettings != null && m_key != string.Empty && _colorSettings.Any(s => s.CharacterIdentifier == m_key.Substring(0,2)))
        {
            GUI.backgroundColor = _colorSettings.Where(s => s.CharacterIdentifier == m_key.Substring(0, 2)).First().CharacterColor; 
        }
        m_nextIndex = EditorGUI.Popup(_r, "Line ID", m_index, m_ids) ;
        GUI.backgroundColor = _originalColor; 
        if (m_nextIndex != m_index)
        {
            m_index = m_nextIndex;
            m_key = m_ids[m_index];

            Script _luaScript = new Script();
            _luaScript.DoString(_lineDescriptor);
            DynValue _content = _luaScript.Globals.Get(m_key).Table.Get("Text_En_en"); 
            
            m_content = _content.String;
        }
        _r.y += DialogNode.POPUP_HEIGHT;

        // -- Draw the dialog line content -- //
        _r = new Rect(_startPos.x, _r.position.y, DialogNode.CONTENT_WIDTH, DialogNode.BASIC_CONTENT_HEIGHT);
        GUI.TextArea(_r, m_content);
        _r.y += DialogNode.BASIC_CONTENT_HEIGHT;

        _r = new Rect(_startPos.x, _r.position.y, DialogNode.CONTENT_WIDTH, DialogNode.POPUP_HEIGHT);
        m_initalWaitingTime = EditorGUI.Slider(_r, new GUIContent("Inital Waiting Time (s): ", "Used if there is no audioclip linked to this line"), m_initalWaitingTime, 0, 10);
        _r.y += DialogNode.POPUP_HEIGHT;

        EditorGUI.BeginDisabledGroup(_dialogSetType == DialogSetType.PlayerAnswer);
        // -- Draw the Dialog Line Waiting Type -- // 
        _r = new Rect(_startPos.x, _r.position.y, DialogNode.CONTENT_WIDTH, DialogNode.POPUP_HEIGHT);
        m_waitingType = (WaitingType)EditorGUI.EnumPopup(_r, "Extra Waiting Type: ", m_waitingType);
        _r.y += DialogNode.POPUP_HEIGHT;

        EditorGUI.BeginDisabledGroup(m_waitingType == WaitingType.WaitForClick); 
        // -- Draw the Dialog Waiting Time Value -- //
        _r = new Rect(_startPos.x, _r.position.y, DialogNode.CONTENT_WIDTH, DialogNode.POPUP_HEIGHT);
        m_extraWaitingTime = EditorGUI.Slider(_r, new GUIContent("Extra Waiting Time (s): ", "Duration to wait after the end of the inital waiting time") ,m_extraWaitingTime, 0, 10); 
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
        _r.y += DialogNode.POPUP_HEIGHT;

        _r.y += DialogNode.SPACE_HEIGHT / 2;
        Color _c = GUI.color;
        GUI.color = Color.black;
        _r = new Rect(_startPos.x, _r.position.y, DialogNode.CONTENT_WIDTH, .5f);
        GUI.Box(_r, "");
        GUI.color = _c;        

        _r.y += DialogNode.SPACE_HEIGHT/2;


        m_pointRect = new Rect(_startPos.x + DialogNode.CONTENT_WIDTH - 2, (_startPos.y + _r.y) / 2, 38, 38);
        if (_isLastPoint || _dialogSetType == DialogSetType.PlayerAnswer)
        {
            if(m_linkedToken != -1)
            {
                Rect _linkedRect = Rect.zero;
                if (_otherSets.Any(s => s.NodeToken == m_linkedToken))
                    _linkedRect = _otherSets.Where(p => p.NodeToken == m_linkedToken).First().InPointRect;
                else if (_otherConditions.Any(c => c.NodeToken == m_linkedToken))
                    _linkedRect = _otherConditions.Where(p => p.NodeToken == m_linkedToken).First().InPointRect;
                else
                    m_linkedToken = -1;
                if (_linkedRect != Rect.zero)
                {
                    Handles.DrawBezier(m_pointRect.center, _linkedRect.center, m_pointRect.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.white, null, 2.0f);
                    Handles.color = Color.white;
                    if (Handles.Button((m_pointRect.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                    {
                        m_linkedToken = -1;
                    }
                }
            }
            if (GUI.Button(m_pointRect, _pointIcon, _pointStyle))
            {
                _onOutLineSelected?.Invoke(this);
            }
        }
        return _r.y; 
    }
#endif
    #endregion
}