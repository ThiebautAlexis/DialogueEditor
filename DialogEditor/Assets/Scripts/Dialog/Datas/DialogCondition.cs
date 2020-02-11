using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEditor; 

[Serializable]
public class DialogCondition : DialogNode
{
    #region Fields and Properties
    // CONDITION STRUCTURE //
    // (condition_1 == true)
    // and (condition_2 == false)
    // or (condition_3 == true)
    [SerializeField] private string m_condition = "";
    [SerializeField] private int m_linkedTokenTrue = -1;
    [SerializeField] private int m_linkedTokenFalse = -1; 
    public string Condition { get { return m_condition; } }
    public int LinkedTokenTrue { get { return m_linkedTokenTrue; } set { m_linkedTokenTrue = value; } }
    public int LinkedTokenFalse { get { return m_linkedTokenFalse; } set { m_linkedTokenFalse = value; } }

#if UNITY_EDITOR
    private Action<DialogCondition> m_onRemoveDialogCondition = null;
    private bool m_displayLUACode = false;
    private List<Condition> m_conditionsConverted = new List<Condition>();
    private DialogsSettings m_dialogSettings; 
    private string[] m_conditionsDescriptor = null; 
    public static string[] Equalities = new string[2] { "==", "~=" };
    public static string[] BoolValues = new string[2] { "true", "false" };
    public static string[] LogicalOperator = new string[2] { "and", "or" }; 
    public Rect OutPointRectTrue 
    { 
        get
        {
            if (m_nodeRect == Rect.zero) return m_nodeRect; 
            return new Rect(m_nodeRect.position.x + m_nodeRect.width - 22.5f, m_nodeRect.position.y + 12.5f, 80, 25); 
        }
    }
    public Rect OutPointRectFalse
    {
        get
        {
            if (m_nodeRect == Rect.zero) return m_nodeRect;
            return new Rect(m_nodeRect.position.x + m_nodeRect.width - 22.5f, m_nodeRect.position.y + 37.5f, 80, 25);
        }
    }
#endif
    #endregion

    #region Constructor 
#if UNITY_EDITOR
    public DialogCondition(Vector2 _nodePosition, Action<DialogCondition> _onRemoveCondition, GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _conditionIcon, GUIContent _pointIcon)
    {
        m_NodeToken = UnityEngine.Random.Range(0, int.MaxValue);
        m_nodeRect = new Rect(_nodePosition.x, _nodePosition.y, INITIAL_NODE_WIDTH, INITIAL_NODE_HEIGHT + SPACE_HEIGHT + SPACE_HEIGHT + BUTTON_HEIGHT * 2);
        m_onRemoveDialogCondition = _onRemoveCondition;
        m_nodeStyle = _nodeStyle;
        m_currentIcon = _conditionIcon;
        m_pointIcon = _pointIcon;
        m_connectionPointStyle = _connectionPointStyle; 
    }
#endif
    #endregion

    #region Methods

#if UNITY_EDITOR
    /// <summary>
    /// Add a new condition and rescale the Node Rect
    /// </summary>
    private void AddCondition()
    {
        string _conditionString = $"{(m_conditionsConverted.Count > 0 ? "and" : string.Empty)}({m_conditionsDescriptor[0]} == false)";
        m_conditionsConverted.Add(new Condition(_conditionString, m_conditionsDescriptor)); 
        m_nodeRect.height += POPUP_HEIGHT; 
    }

    /// <summary>
    /// Draw the condition node with its links
    /// </summary>
    /// <param name="_otherSets">The others sets to be linked with</param>
    /// <param name="_otherConditions">The others conditions to be linked with</param>
    /// <param name="_onInDialogNodeSelected">Action called when the In point is selected</param>
    /// <param name="_onOutDialogNodeSelected">Action called when one of the out point is selected</param>
    public void Draw(List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, Action<DialogNode> _onInDialogNodeSelected, Action<DialogCondition, bool> _onOutDialogNodeSelected)
    {
        GUI.Box(m_nodeRect, "", m_nodeStyle);
        Rect _r = new Rect(m_nodeRect.position.x + m_nodeRect.width - 35, m_nodeRect.position.y + MARGIN_HEIGHT, 25, 25);
        if (GUI.Button(_r, m_currentIcon, m_nodeStyle))
        {
            ProcessContextMenu();
        }
        _r = new Rect(m_nodeRect.x + 10, _r.y, CONTENT_WIDTH, TITLE_HEIGHT);
        GUI.Label(_r, "Condition : " + m_NodeToken.ToString());
        _r.y = m_nodeRect.y + INITIAL_NODE_HEIGHT + SPACE_HEIGHT;
        // --- Draw the conditions --- //
        if (m_displayLUACode)
        {
            _r = new Rect(_r.position.x, _r.y , CONTENT_WIDTH, BASIC_CONTENT_HEIGHT);
            m_condition = GUI.TextArea(_r, m_condition); 
        }
        else
        {
            for (int i = 0; i < m_conditionsConverted.Count; i++)
            {
                _r.y = DrawCondition(_r.position, m_conditionsConverted[i], i);
            }
            //_r.y += SPACE_HEIGHT;
            _r = new Rect(_r.x, _r.y, CONTENT_WIDTH, BUTTON_HEIGHT); 
            if(GUI.Button(_r, "Add Condition"))
            {
                AddCondition(); 
            }
            _r = new Rect(_r.x, _r.y + BUTTON_HEIGHT, CONTENT_WIDTH / 2, BUTTON_HEIGHT);
            if(GUI.Button(_r, "Apply Conditions"))
            {
                m_condition = string.Empty;
                for (int i = 0; i < m_conditionsConverted.Count; i++)
                {
                    if (i > 0) m_condition += '\n';
                    m_condition += m_conditionsConverted[i].ToString(); 
                }
            }
            _r.x += CONTENT_WIDTH / 2; 
            if(GUI.Button(_r, "Reset Conditions"))
            {
                InitConditionsFromString(); 
            }
        }

        // ------ //
        if (m_linkedTokenTrue != -1)
        {
            Rect _linkedRect = Rect.zero; 
            if (_otherSets.Any(s => s.NodeToken == m_linkedTokenTrue))
            {
                _linkedRect = _otherSets.Where(p => p.NodeToken == m_linkedTokenTrue).First().InPointRect;
                
            }
            else if(_otherConditions.Any(c => c.NodeToken == m_linkedTokenTrue))
            {
                _linkedRect = _otherConditions.Where(p => p.NodeToken == m_linkedTokenTrue).First().InPointRect;
            }
            else
            {
                m_linkedTokenTrue = -1; 
            }
            if(_linkedRect != Rect.zero)
            {
                Handles.DrawBezier(OutPointRectTrue.center, _linkedRect.center, OutPointRectTrue.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.black, null, 2.0f);
                Handles.color = Color.black;
                if (Handles.Button((OutPointRectTrue.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                {
                    m_linkedTokenTrue = -1;
                }
            }
        }
        // ------ //
        if (m_linkedTokenFalse != -1)
        {
            Rect _linkedRect = Rect.zero;
            if (_otherSets.Any(s => s.NodeToken == m_linkedTokenFalse))
            {
                _linkedRect = _otherSets.Where(p => p.NodeToken == m_linkedTokenFalse).First().InPointRect;

            }
            else if (_otherConditions.Any(c => c.NodeToken == m_linkedTokenFalse))
            {
                _linkedRect = _otherConditions.Where(p => p.NodeToken == m_linkedTokenFalse).First().InPointRect;
            }
            else
            {
                m_linkedTokenFalse = -1;
            }
            if (_linkedRect != Rect.zero)
            {
                Handles.DrawBezier(OutPointRectFalse.center, _linkedRect.center, OutPointRectFalse.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.black, null, 2.0f);
                Handles.color = Color.black;
                if (Handles.Button((OutPointRectFalse.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                {
                    m_linkedTokenTrue = -1;
                }
            }
        }
        // --- Draw the Buttons to connect the parts --- //
        if (GUI.Button(InPointRect, m_pointIcon, m_connectionPointStyle))
        {
            _onInDialogNodeSelected.Invoke(this);
        }
        if (GUI.Button(OutPointRectTrue, new GUIContent("True",  m_pointIcon.image), m_connectionPointStyle))
        {
            _onOutDialogNodeSelected.Invoke(this, true);
        }
        if (GUI.Button(OutPointRectFalse, new GUIContent("False", m_pointIcon.image), m_connectionPointStyle))
        {
            _onOutDialogNodeSelected.Invoke(this, false);
        }
    }

    /// <summary>
    /// Draw one particulaty condition
    /// </summary>
    /// <param name="_startPos">The starting position of the rect</param>
    /// <param name="_conditionConverted">Condition to draw</param>
    /// <param name="_index">Index of the condition</param>
    /// <returns></returns>
    private float DrawCondition(Vector2 _startPos, Condition _conditionConverted, int _index)
    {
        Rect _r = new Rect(_startPos, new Vector2(35.5f, POPUP_HEIGHT));
        if(_index > 0)
        {
            _conditionConverted.StartStatement = LogicalOperator[EditorGUI.Popup(_r, _conditionConverted.LogicalOperatorIndex, LogicalOperator)]; 
        }
        else
        {
            if(_conditionConverted.StartStatement != string.Empty)_conditionConverted.StartStatement = string.Empty; 
            GUI.Label(_r, "if"); 
        }
        // Condition
        _r = new Rect(_r.x + 35.5f , _r.y, 75, _r.height);
        _conditionConverted.NextIndex = EditorGUI.Popup(_r, _conditionConverted.NextIndex,m_conditionsDescriptor);
        if(_conditionConverted.NextIndex != _conditionConverted.CurrentIndex)
        {
            _conditionConverted.ConditionName = m_conditionsDescriptor[_conditionConverted.NextIndex];
            _conditionConverted.CurrentIndex = _conditionConverted.NextIndex; 
        }

        // Equals
        _r = new Rect(_r.x + 75, _r.y, 32.5f, _r.height);
        _conditionConverted.Equality = Equalities[EditorGUI.Popup(_r, _conditionConverted.EqualityIndex, Equalities)];

        // True false 
        _r = new Rect(_r.x + 32.5f, _r.y, 72, _r.height);
        _conditionConverted.Value = BoolValues[EditorGUI.Popup(_r, _conditionConverted.ValueIndex, BoolValues)];

        //Remove
        _r = new Rect(_r.x + 72, _r.y, 15, _r.height);
        if(GUI.Button(_r, "-"))
        {
            m_conditionsConverted.RemoveAt(_index);
            m_nodeRect.height -= POPUP_HEIGHT; 
        }

        return _r.y + _r.height;
    }

    /// <summary>
    /// Use m_condition to set a list of conditions
    /// </summary>
    private void InitConditionsFromString()
    {
        m_nodeRect.height = INITIAL_NODE_HEIGHT + SPACE_HEIGHT + SPACE_HEIGHT + BUTTON_HEIGHT * 2;
        m_conditionsConverted = new List<Condition>();
        m_condition = m_condition.Trim();
        if (m_condition == string.Empty) return; 
        string[] _stringConditions = m_condition.Split('\n');
        for (int i = 0; i < _stringConditions.Length; i++)
        {
            if (_stringConditions[i] == string.Empty) continue; 
            m_conditionsConverted.Add(new Condition(_stringConditions[i].Trim(), m_conditionsDescriptor));
            m_nodeRect.height += POPUP_HEIGHT;
        }
    }

    /// <summary>
    /// Init the Editor Settings of the DialogCondition Node
    /// </summary>
    /// <param name="_nodeStyle">Style of the node</param>
    /// <param name="_connectionPointStyle">Style of the conection points</param>
    /// <param name="_conditionIcon">Icon of the Condition Node</param>
    /// <param name="_pointIcon">Icon of the connection points</param>
    /// <param name="_onRemoveCondition">Event called when the Icon is removed</param>
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _conditionIcon, GUIContent _pointIcon, Action<DialogCondition> _onRemoveCondition, DialogsSettings _dialogSettings)
    {
        m_nodeStyle = _nodeStyle;
        m_connectionPointStyle = _connectionPointStyle;
        m_connectionPointStyle.alignment = TextAnchor.UpperCenter; 
        m_currentIcon = _conditionIcon; 
        m_pointIcon = _pointIcon;
        m_onRemoveDialogCondition = _onRemoveCondition;
        m_dialogSettings = _dialogSettings;
        m_conditionsDescriptor = m_dialogSettings.LuaConditions.Split('\n').Select(c => c.Split('=')[0].Trim()).ToArray(); 
        InitConditionsFromString();
    }

    /// <summary>
    /// Call the event "m_onRemoveDialogCondition" with argument as itself
    /// -> Remove this condition from the Dialog 
    /// </summary>
    private void OnClickRemoveNode()
    {
        m_onRemoveDialogCondition?.Invoke(this); 
    }

    /// <summary>
    /// Display the context Menu of the condition Node
    /// </summary>
    protected override void ProcessContextMenu()
    {
        IsSelected = true; 
        GenericMenu _genericMenu = new GenericMenu();
        _genericMenu.AddItem(new GUIContent(m_displayLUACode ? " Switch to non-code" : "Switch to LUA code"), false, SwitchLUACode ); 
        _genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        
        _genericMenu.ShowAsContext(); 
    }

    /// <summary>
    /// Switch to LUA code and rescale the Node Rect
    /// </summary>
    private void SwitchLUACode()
    {
        m_displayLUACode = !m_displayLUACode;
        if (m_displayLUACode)
            m_nodeRect.height = INITIAL_NODE_HEIGHT + SPACE_HEIGHT + BASIC_CONTENT_HEIGHT + MARGIN_HEIGHT;
        else
        {
            m_nodeRect.height = INITIAL_NODE_HEIGHT + SPACE_HEIGHT + SPACE_HEIGHT + (BUTTON_HEIGHT * 2);
            if (m_condition != string.Empty) m_nodeRect.height += (m_condition.Split('\n').Length * POPUP_HEIGHT);
        }
    }
#endif

    #endregion
}

#if UNITY_EDITOR
public class Condition
{
    public string StartStatement { get; set; }
    public string ConditionName { get; set; }
    public string Equality { get; set; }
    public string Value { get; set; }

    public int EqualityIndex { get { return Equality == "==" ? 0 : 1; } }
    public int ValueIndex { get { return Value == "true" ? 0 : 1; } }
    public int LogicalOperatorIndex { get { return StartStatement == "and" ? 0 : 1;  } }

    public int CurrentIndex { get; set; }
    public int NextIndex { get; set; }

    public Condition(string _stringCondition, string[] _conditionDescriptor)
    {
        string[] _condition = _stringCondition.Split('(');
        StartStatement = _condition[0].Trim();
        _condition = _condition[1].Split(' ');
        ConditionName = _condition[0];
        CurrentIndex = _conditionDescriptor.ToList().IndexOf(ConditionName);
        NextIndex = CurrentIndex; 
        Equality = _condition[1];
        Value = _condition[2].Remove(_condition[2].Length - 1);
    }

    public override string ToString()
    {
        return $"{StartStatement}({ConditionName} {Equality} {Value})"; 
    }
}
#endif
