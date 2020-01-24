using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEditor; 

[Serializable]
public class DialogCondition : DialogNode
{
    #region Fields and Properties
    [SerializeField] private string m_condition = "";
    [SerializeField] private int m_linkedToken = -1; 
    public int LinkedToken { get { return m_linkedToken; } set { m_linkedToken = value; } }

#if UNITY_EDITOR
    private Action<DialogCondition> m_onRemoveDialogCondition = null;
    private bool m_displayLUACode = false; 

    public Rect OutPointRect 
    { 
        get
        {
            if (m_nodeRect == Rect.zero) return m_nodeRect; 
            return new Rect(m_nodeRect.position.x + m_nodeRect.width - 8.0f , m_nodeRect.position.y + 6.0f, 25, 25); 
        }
    }
#endif
    #endregion

    #region Constructor 
#if UNITY_EDITOR
    public DialogCondition(Vector2 _nodePosition, Action<DialogCondition> _onRemoveCondition, GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _conditionIcon, GUIContent _pointIcon)
    {
        m_NodeToken = UnityEngine.Random.Range(0, int.MaxValue);
        m_nodeRect = new Rect(_nodePosition.x, _nodePosition.y, Dialog.INITIAL_RECT_WIDTH, 50);
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
    public void Draw(string _conditionDescriptor, List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, Action<DialogNode> _onInDialogNodeSelected, Action<DialogCondition> _onOutDialogNodeSelected)
    {
        // --- Draw the connections between the parts --- //
        if (GUI.Button(InPointRect, m_pointIcon, m_connectionPointStyle))
        {
            _onInDialogNodeSelected.Invoke(this);
        }

        GUI.Box(m_nodeRect, "", m_nodeStyle);
        Rect _r = new Rect(m_nodeRect.position.x + m_nodeRect.width - 35, m_nodeRect.position.y + Dialog.MARGIN_HEIGHT, 25, 25);
        if (GUI.Button(_r, m_currentIcon, m_nodeStyle))
        {
            ProcessContextMenu();
        }
        _r = new Rect(m_nodeRect.x + 10, m_nodeRect.y + Dialog.MARGIN_HEIGHT, Dialog.CONTENT_WIDTH, Dialog.TITLE_HEIGHT);
        GUI.Label(_r, "Condition : " + m_NodeToken.ToString());

        // --- Draw the conditions --- //


        // ------ //
        if (m_linkedToken != -1)
        {
            Rect _linkedRect = Rect.zero; 
            if (_otherSets.Any(s => s.NodeToken == m_linkedToken))
            {
                _linkedRect = _otherSets.Where(p => p.NodeToken == m_linkedToken).First().InPointRect;
                
            }
            else if(_otherConditions.Any(c => c.NodeToken == m_linkedToken))
            {
                _linkedRect = _otherConditions.Where(p => p.NodeToken == m_linkedToken).First().InPointRect;
            }
            else
            {
                m_linkedToken = -1; 
            }
            if(_linkedRect != Rect.zero)
            {
                Handles.DrawBezier(OutPointRect.center, _linkedRect.center, OutPointRect.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.black, null, 2.0f);
                Handles.color = Color.black;
                if (Handles.Button((OutPointRect.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                {
                    m_linkedToken = -1;
                }
            }
        }
        if (GUI.Button(OutPointRect, m_pointIcon, m_connectionPointStyle))
        {
            _onOutDialogNodeSelected.Invoke(this);
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
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _connectionPointStyle, GUIContent _conditionIcon, GUIContent _pointIcon, Action<DialogCondition> _onRemoveCondition)
    {
        m_nodeStyle = _nodeStyle;
        m_connectionPointStyle = _connectionPointStyle;
        m_currentIcon = _conditionIcon; 
        m_pointIcon = _pointIcon;
        m_onRemoveDialogCondition = _onRemoveCondition;
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
        GenericMenu _genericMenu = new GenericMenu();
        _genericMenu.AddItem(new GUIContent(m_displayLUACode ? " Switch to non-code" : "Switch to LUA code"), false, () => m_displayLUACode = !m_displayLUACode); 
        _genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        
        _genericMenu.ShowAsContext(); 
    }
#endif

    #endregion
}
