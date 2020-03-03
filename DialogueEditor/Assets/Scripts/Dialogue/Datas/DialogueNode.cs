using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueEditor
{
    [System.Serializable]
    public abstract class DialogueNode
    {
        [SerializeField] protected Rect m_nodeRect = new Rect();
        [SerializeField] protected int m_NodeToken = 0;
        protected bool m_isDragged = false;

#if UNITY_EDITOR
        protected GUIStyle m_nodeStyle = null;
        protected GUIStyle m_selectedNodeStyle = null;
        protected GUIStyle m_connectionPointStyle = null;
        protected GUIContent m_pointIcon = null;
        protected GUIContent m_currentIcon = null;
        public const float INITIAL_NODE_WIDTH = 250;
        public const float INITIAL_NODE_HEIGHT = MARGIN_HEIGHT + TITLE_HEIGHT;
        public const float CONTENT_WIDTH = 230;
        public const float MARGIN_HEIGHT = 12;
        public const float TITLE_HEIGHT = 20;
        public const float SPACE_HEIGHT = 15;
        public const float POPUP_HEIGHT = 15;
        public const float BUTTON_HEIGHT = 30;
        public const float DIALOGLINE_SETTINGS_HEIGHT = BASIC_CONTENT_HEIGHT + (POPUP_HEIGHT * 3) + SPACE_HEIGHT;
        public const float BASIC_CONTENT_HEIGHT = 40;
#endif
        public bool IsSelected { get; set; }
        public int NodeToken { get { return m_NodeToken; } }
        public Rect InPointRect { get { return new Rect(m_nodeRect.position.x - 38 / 4 * 3 + 1, m_nodeRect.position.y + 2, 38, 38); } }


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
}