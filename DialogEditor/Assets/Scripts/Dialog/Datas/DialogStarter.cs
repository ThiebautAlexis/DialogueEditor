using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEditor; 

[Serializable]
public class DialogStarter : DialogNode
{
    #region Fields and Properties
    [SerializeField] private List<StarterPair> m_startersEnum = new List<StarterPair>();
    public List<StarterPair> StarterEnums { get { return m_startersEnum;  } }

#if UNITY_EDITOR
#endif
    #endregion

    #region Constructor
#if UNITY_EDITOR
    public DialogStarter(Vector2 _nodePosition, GUIStyle _normalStyle, GUIStyle _selectedStyle, GUIStyle _connectionPointStyle, GUIContent _startingSetIcon, GUIContent _pointIcon)
    {
        m_NodeToken = UnityEngine.Random.Range(0, int.MaxValue);
        m_nodeStyle = _normalStyle;
        m_selectedNodeStyle = _selectedStyle;
        m_connectionPointStyle = _connectionPointStyle;
        m_startersEnum = new List<StarterPair>();
        foreach (int s in Enum.GetValues(typeof(DialogStarterEnum)))
        {
            m_startersEnum.Add(new StarterPair((DialogStarterEnum)s));
        }
        m_nodeRect = new Rect(_nodePosition.x, _nodePosition.y, INITIAL_NODE_WIDTH, INITIAL_NODE_HEIGHT + SPACE_HEIGHT + (m_startersEnum.Count * (25 + DialogNode.SPACE_HEIGHT)));
        m_currentIcon = _startingSetIcon;
        m_pointIcon = _pointIcon;
    }
#endif
    #endregion

    #region Methods
#if UNITY_EDITOR
    public void Draw(List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, Action<StarterPair> _onOutPointSelected)
    {
        GUI.Box(m_nodeRect, "", IsSelected ? m_selectedNodeStyle : m_nodeStyle);
        Rect _r = new Rect(m_nodeRect.position.x + m_nodeRect.width - 35, m_nodeRect.position.y + MARGIN_HEIGHT, 25, 25);
        GUI.Box(_r, m_currentIcon, m_nodeStyle);
        _r = new Rect(m_nodeRect.x + 10, _r.y, CONTENT_WIDTH, TITLE_HEIGHT);
        GUI.Label(_r, "Starting Node");
        _r.y += TITLE_HEIGHT + SPACE_HEIGHT - 1;

        for (int i = 0; i < m_startersEnum.Count; i++)
        {
            _r.y = m_startersEnum[i].Draw(_r.position, _otherSets, _otherConditions, _onOutPointSelected, m_connectionPointStyle, m_pointIcon); 
        }
        _r.y += SPACE_HEIGHT / 2;
        _r.height = 1.0f;
        Color _c = GUI.color;
        GUI.color = Color.black;
        GUI.Box(_r, "");
        GUI.color = _c;
    }

    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _selectedNodeStyle, GUIStyle _connectionPointStyle, GUIContent _startingSetIcon, GUIContent _pointIcon)
    {
        m_nodeStyle = _nodeStyle;
        m_selectedNodeStyle = _selectedNodeStyle;
        m_connectionPointStyle = _connectionPointStyle;
        m_currentIcon = _startingSetIcon;
        m_pointIcon = _pointIcon; 
        if(m_startersEnum == null || m_startersEnum.Count == 0)
        {
            m_startersEnum = new List<StarterPair>();
            foreach (int s in Enum.GetValues(typeof(DialogStarterEnum)))
            {
                m_startersEnum.Add(new StarterPair((DialogStarterEnum)s)); 
            }

        }
        else if(m_startersEnum.Count > Enum.GetNames(typeof(DialogStarterEnum)).Length)
        {
            for (int i = 0; i < m_startersEnum.Count; i++)
            {
                if (Enum.IsDefined(typeof(DialogStarterEnum), (int)m_startersEnum[i].Starter)) continue;
                m_startersEnum.RemoveAt(i);

                i--; 
            }
        }
        else if(m_startersEnum.Count < Enum.GetNames(typeof(DialogStarterEnum)).Length)
        {
            foreach (int s in Enum.GetValues(typeof(DialogStarterEnum)))
            {
                if (m_startersEnum.Any(p => (int)p.Starter == s)) continue; 
                m_startersEnum.Add(new StarterPair((DialogStarterEnum)s));
            }
        }
        m_nodeRect = new Rect(m_nodeRect.x, m_nodeRect.y, INITIAL_NODE_WIDTH, INITIAL_NODE_HEIGHT + SPACE_HEIGHT + 20 + (m_startersEnum.Count * (25 + SPACE_HEIGHT)));


    }
#endif
    #endregion
}

[System.Serializable]
public class StarterPair
{
    [SerializeField] private DialogStarterEnum m_starter = DialogStarterEnum.Default;
    [SerializeField] private int m_linkedToken = -1;
    public int LinkedToken { get { return m_linkedToken; } set { m_linkedToken = value; } }
    public DialogStarterEnum Starter { get { return m_starter; } }
    public Rect OutPointRect { get; private set; }

    public StarterPair(DialogStarterEnum _starter)
    {
        m_starter = _starter;
    }
#if UNITY_EDITOR
    public float Draw(Vector2 _statringPoint, List<DialogSet> _otherSets, List<DialogCondition> _otherConditions, Action<StarterPair> _onOutPointSelected, GUIStyle _outPointStyle, GUIContent _outPointIcon)
    {
        Rect _r = new Rect(_statringPoint.x, _statringPoint.y, DialogNode.CONTENT_WIDTH, 1.0f);
        _r.y += DialogNode.SPACE_HEIGHT / 2;
        Color _c = GUI.color;
        GUI.color = Color.black;
        GUI.Box(_r, "");
        GUI.color = _c;
        _r.y += DialogNode.SPACE_HEIGHT / 2;
        _r.height = 20;
        GUIStyle _style = new GUIStyle();
        _style.alignment = TextAnchor.MiddleLeft; 
        GUI.Box(_r, m_starter.ToString(), _style);
        OutPointRect = new Rect(_statringPoint.x + DialogNode.CONTENT_WIDTH - 30, (_statringPoint.y + _r.y) / 2, 38, 38);

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
                Handles.DrawBezier(OutPointRect.center, _linkedRect.center, OutPointRect.center + Vector2.right * 100.0f, _linkedRect.center + Vector2.left * 100.0f, Color.white, null, 2.0f);
                Handles.color = Color.white;
                if (Handles.Button((OutPointRect.center + _linkedRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
                {
                    m_linkedToken = -1;
                }
            }
        }
        if (GUI.Button(OutPointRect, _outPointIcon, _outPointStyle))
        {
            _onOutPointSelected?.Invoke(this);
        }
        _r.y += 20;
        return _r.y;
    }
#endif
}