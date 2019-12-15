using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor; 

[Serializable]
public class DialogPart
{
    #region Fields and Properties
    [SerializeField] private Rect m_nodeRect = new Rect();
    [SerializeField] private List<DialogContent> m_contents = new List<DialogContent>(); 
    [SerializeField] private int m_partToken = 0; 
    public int PartToken { get { return m_partToken; } }
    private bool m_isDragged = false;
    public bool IsSelected { get; set; }

    private Action<DialogPart> m_onRemoveDialogPart = null;
    private GUIStyle m_currentNodeStyle = null; 
    private GUIStyle m_normalNodeStyle = null;
    private GUIStyle m_selectedNodeStyle = null;
    #endregion


    #region Constructor
    public DialogPart(Vector2 _nodePosition, Action<DialogPart> _onRemovePart, GUIStyle _normalStyle, GUIStyle _selectedStyle)
    {
        m_partToken = UnityEngine.Random.Range(0, 10000); 
        m_nodeRect = new Rect(_nodePosition.x, _nodePosition.y, Dialog.INITIAL_RECT_WIDTH, 0);
        m_onRemoveDialogPart = _onRemovePart;
        m_normalNodeStyle = _normalStyle;
        m_selectedNodeStyle = _selectedStyle;
        m_currentNodeStyle = m_normalNodeStyle;
        m_contents = new List<DialogContent>();
        AddNewContent();
    }
    #endregion 


    #region Methods
#if UNITY_EDITOR
    private void AddNewContent()
    {
        if (m_contents == null) m_contents = new List<DialogContent>();
        m_contents.Add(new DialogContent());
        m_nodeRect = new Rect(m_nodeRect.position.x,
            m_nodeRect.position.y,
            Dialog.INITIAL_RECT_WIDTH, 
            (Dialog.MARGIN_HEIGHT * 2) + Dialog.TITLE_HEIGHT + (Dialog.POPUP_HEIGHT * m_contents.Count) + (Dialog.BASIC_CONTENT_HEIGHT * m_contents.Count) + (Dialog.SPACE_HEIGHT * ((m_contents.Count * 2)+1)) + Dialog.BUTTON_HEIGHT); 
    }

    /// <summary>
    /// Draw the Dialog part editor
    /// </summary>
    public void Draw(List<string> _linesID, List<string> _linesContent, List<DialogPart> _otherParts)
    {
        GUI.Box(m_nodeRect, "", m_currentNodeStyle);
        Rect _r = new Rect(m_nodeRect.x + 10, m_nodeRect.y + Dialog.MARGIN_HEIGHT, Dialog.CONTENT_WIDTH , Dialog.TITLE_HEIGHT);
        GUI.Label(_r, "Dialog Part " + m_partToken.ToString() );
        _r.y += Dialog.TITLE_HEIGHT;
        DialogContent _c; 
        for (int i = 0; i < m_contents.Count; i++)
        {
            _c = m_contents[i]; 
            _r.y = _c.Draw(_r.position, _linesID, _linesContent, RemoveContent);
            if (_c.LinkedToken == -1) continue; 
        }
        _r = new Rect(_r.position.x, _r.position.y + Dialog.SPACE_HEIGHT, _r.width, Dialog.BUTTON_HEIGHT); 
        if(GUI.Button(_r, "Add Content"))
        {
            AddNewContent(); 
        }
    }

    /// <summary>
    /// Move the position of the Node of the delta
    /// </summary>
    /// <param name="_delta">Where to move the node position</param>
    public void Drag(Vector2 _delta)
    {
        Rect _r = new Rect(m_nodeRect.position + _delta, m_nodeRect.size);
        m_nodeRect = _r;
    }

    /// <summary>
    ///Process the events relatives to the Dialog parts
    /// </summary>
    /// <param name="_e"></param>
    /// <returns></returns>
    public bool ProcessEvent(Event _e)
    {
        switch (_e.type)
        {
            case EventType.MouseDown:
                if (_e.button == 0)
                {
                    if (m_nodeRect.Contains(_e.mousePosition))
                    {
                        m_isDragged = true;
                        GUI.changed = true;
                        IsSelected = true;
                        m_currentNodeStyle = m_selectedNodeStyle; 
                    }
                    else
                    {
                        GUI.changed = true;
                        IsSelected = false;
                        m_currentNodeStyle = m_normalNodeStyle; 
                    }
                }
                else if (_e.button == 1 && IsSelected && m_nodeRect.Contains(_e.mousePosition))
                {
                    ProcessContextMenu();
                    _e.Use();
                }
                else
                    m_isDragged = false;
                break;
            case EventType.MouseUp:
                m_isDragged = false;
                break;
            case EventType.MouseDrag:
                if (_e.button == 0 && m_isDragged)
                {
                    Drag(_e.delta);
                    _e.Use();
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    /// <summary>
    /// Call the event "m_onRemoveDialogPart" with argument as itself
    /// -> Remove this part from the Dialog 
    /// </summary>
    private void OnClickRemoveNode()
    {
        m_onRemoveDialogPart?.Invoke(this);
    }

    /// <summary>
    /// Display the Context menu on the selected DialogPart
    /// </summary>
    private void ProcessContextMenu()
    {
        UnityEditor.GenericMenu _genericMenu = new UnityEditor.GenericMenu();
        _genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        _genericMenu.ShowAsContext();
    }
    
    /// <summary>
    /// Remove the selected Content and rescale the Rect
    /// </summary>
    /// <param name="_content"></param>
    private void RemoveContent(DialogContent _content)
    {
        m_contents.Remove(_content);
        m_nodeRect = new Rect(m_nodeRect.position.x, m_nodeRect.position.y, Dialog.INITIAL_RECT_WIDTH, (Dialog.MARGIN_HEIGHT * 2) + Dialog.TITLE_HEIGHT + (Dialog.POPUP_HEIGHT * m_contents.Count) + (Dialog.BASIC_CONTENT_HEIGHT * m_contents.Count) + (Dialog.SPACE_HEIGHT * ((m_contents.Count * 2) + 1)) + Dialog.BUTTON_HEIGHT);
    }

    /// <summary>
    /// Initialize the Editor Settings for the part
    /// </summary>
    /// <param name="_nodeStyle">The Node Style when the Part isn't selected</param>
    /// <param name="_selectedNodeStyle">The Node Style when the Part is selected</param>
    /// <param name="_onRemovePart">Action Called to remove the Part from the Dialog</param>
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _selectedNodeStyle, Action<DialogPart> _onRemovePart)
    {
        m_normalNodeStyle = _nodeStyle;
        m_selectedNodeStyle = _selectedNodeStyle;
        m_currentNodeStyle = m_normalNodeStyle; 
        m_onRemoveDialogPart = _onRemovePart; 
    }
#endif

    #endregion
}