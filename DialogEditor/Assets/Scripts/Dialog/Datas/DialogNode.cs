using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class DialogNode
{
    [SerializeField] protected Rect m_nodeRect = new Rect();
    [SerializeField] protected int m_NodeToken = 0;
    protected bool m_isDragged = false;

    protected GUIStyle m_nodeStyle = null;
    protected GUIStyle m_connectionPointStyle = null;
    protected GUIContent m_pointIcon = null;
    protected GUIContent m_currentIcon = null;

    public bool IsSelected { get; set; }
    public int NodeToken { get { return m_NodeToken; } }
    public Rect InPointRect { get { return new Rect(m_nodeRect.position.x - 15.5f, m_nodeRect.position.y + 6.0f, 25, 25); } }



    #region Methods
#if UNITY_EDITOR
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
                    }
                    else
                    {
                        GUI.changed = true;
                        IsSelected = false;
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
    /// Process the contect menu of the Node 
    /// </summary>
    protected virtual void ProcessContextMenu()
    {
        
    }
#endif
    #endregion
}
