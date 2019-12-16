using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq; 

[Serializable]
public class DialogContent
{
    #region Fields and Properties
    [SerializeField] private int m_index = -1;
    [SerializeField] private int m_nextIndex = -1;
    [SerializeField] private string m_key = "";
    [SerializeField] private string m_content = "";
    [SerializeField] private int m_linkedToken = -1;
    private Rect m_pointRect; 
    public Rect PointRect { get { return m_pointRect;  } }

    public string Content { get { return m_content; } set { m_content = value;  } }
    public int LinkedToken { get { return m_linkedToken; } set { m_linkedToken = value; } }
    #endregion

    #region Constructor 

    #endregion

    #region Methods
    public float Draw(Vector2 _startPos, List<string> _ids, List<string> _quotes, Action<DialogContent> _removeAction, bool _drawPoint, GUIContent _pointIcon, Action<DialogContent> _onOutContentSelected, List<DialogPart> _otherParts)
    {
        Rect _r = new Rect(_startPos.x, _startPos.y + Dialog.SPACE_HEIGHT, Dialog.POPUP_HEIGHT, Dialog.POPUP_HEIGHT);
        if (GUI.Button(_r, "-"))
        {
            _removeAction.Invoke(this);
            return _r.y;
        }
        _r = new Rect(_r.position.x + Dialog.POPUP_HEIGHT, _r.position.y, Dialog.CONTENT_WIDTH - Dialog.POPUP_HEIGHT, Dialog.POPUP_HEIGHT);
        m_nextIndex = EditorGUI.Popup(_r, "Line ID", m_index, _ids.ToArray());
        if (m_nextIndex != m_index)
        {
            m_index = m_nextIndex;
            m_key = _ids[m_index];
            m_content = _quotes[m_index];

        }
        _r.y += Dialog.POPUP_HEIGHT;
        _r = new Rect(_startPos.x, _r.position.y + Dialog.SPACE_HEIGHT, Dialog.CONTENT_WIDTH, Dialog.BASIC_CONTENT_HEIGHT);
        GUI.TextArea(_r, m_content);
        _r.y += Dialog.BASIC_CONTENT_HEIGHT;
        m_pointRect = new Rect(_startPos.x + Dialog.CONTENT_WIDTH, (_startPos.y + _r.y) / 2, 25, 25);
        if (_drawPoint)
        {
            if(m_linkedToken != -1)
            {
                if (_otherParts.Any(p => p.PartToken == m_linkedToken))
                {
                    Rect _linkedRect = _otherParts.Where(p => p.PartToken == m_linkedToken).First().InPointRect; 
                    Handles.DrawBezier(m_pointRect.center, _linkedRect.center, m_pointRect.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.black, null, 2.0f);
                    Handles.color = Color.black; 
                    if (Handles.Button((m_pointRect.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                    {
                        m_linkedToken = -1; 
                    }
                }
                else m_linkedToken = -1; 
            }
            if (GUI.Button(m_pointRect, _pointIcon))
            {
                _onOutContentSelected?.Invoke(this);
            }
        }
        return _r.y; 
    }
    #endregion
}
