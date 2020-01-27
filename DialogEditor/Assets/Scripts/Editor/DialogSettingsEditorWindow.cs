using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogSettingsEditorWindow : EditorWindow
{ 
    private List<ConditionPair> m_conditions = null;
    private string m_addedCondition = "";
    private GUIStyle m_titleStyle;

    #region Methods 

    #region Static Method
    [MenuItem("Window/Dialog Editor/Edit Settings")]
    public static void OpenWindow()
    {
        DialogSettingsEditorWindow _window = GetWindow<DialogSettingsEditorWindow>();
        _window.titleContent = new GUIContent("Dialog Editor");
    }
    #endregion

    #region Original Methods
    private void DrawConditions()
    {
        GUILayout.Label("CONDITIONS", m_titleStyle);
        for (int i = 0; i < m_conditions.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_conditions[i].Key, GUILayout.MinWidth(150), GUILayout.MaxWidth(200));
            m_conditions[i].Value = GUILayout.Toggle(m_conditions[i].Value, "Initial Value");
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        m_addedCondition = GUILayout.TextField(m_addedCondition, GUILayout.MinWidth(200));
        if (GUILayout.Button("Add Condition to database") && m_addedCondition.Trim() != string.Empty)
        {
            m_addedCondition = m_addedCondition.Trim().Replace(' ', '_'); 
            m_conditions.Add(new ConditionPair(m_addedCondition, false));
            m_addedCondition = "";
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Save Conditions"))
        {
            string _savedDatas = string.Empty;
            for (int i = 0; i < m_conditions.Count; i++)
            {
                _savedDatas += m_conditions[i].Key + " = " + m_conditions[i].Value.ToString().ToLower() + ";\n";
            }
            File.WriteAllText(DialogSettings.ConditionsFilePath, _savedDatas);
        }
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        if (!Directory.Exists(DialogSettings.ConditionsPath))
            Directory.CreateDirectory(DialogSettings.ConditionsPath);
        if (!File.Exists(DialogSettings.ConditionsFilePath))
            File.WriteAllText(DialogSettings.ConditionsFilePath, "");
        string[] _conditions = File.ReadAllLines(DialogSettings.ConditionsFilePath);
        m_conditions = new List<ConditionPair>();
        for (int i = 0; i < _conditions.Length; i++)
        {
            string[] _pair = _conditions[i].Split('=');
            m_conditions.Add(new ConditionPair(_pair[0].Trim(), _pair[1].Trim())); 
        }

        m_titleStyle = new GUIStyle();
        m_titleStyle.fontStyle = FontStyle.Bold;
        m_titleStyle.fontSize = 20;
        m_titleStyle.alignment = TextAnchor.MiddleCenter; 
    }

    private void OnGUI()
    {
        DrawConditions(); 
    }
    #endregion

    #endregion
}

public class ConditionPair
{
    public string Key { get; private set; }
    public bool Value { get; set; }

    public ConditionPair(string _key, string _value)
    {
        Key = _key;
        Value = (_value == "true;"); 
    }

    public ConditionPair(string _key, bool _value)
    {
        Key = _key;
        Value = _value;
    }
}