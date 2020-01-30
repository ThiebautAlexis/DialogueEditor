using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogSettingsEditorWindow : EditorWindow
{ 
    private List<ConditionPair> m_conditionsPair = null;
    private string m_addedCondition = "";
    private string m_addedCharacterName = ""; 
    private GUIStyle m_titleStyle;

    private DialogsSettings m_dialogsSettings = null; 
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
        for (int i = 0; i < m_conditionsPair.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_conditionsPair[i].Key, GUILayout.MinWidth(150), GUILayout.MaxWidth(200));
            m_conditionsPair[i].Value = GUILayout.Toggle(m_conditionsPair[i].Value, "Initial Value");
            if(GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(15)))
            {
                m_conditionsPair.RemoveAt(i);
                GUILayout.EndHorizontal(); 
                continue; 
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        m_addedCondition = GUILayout.TextField(m_addedCondition, GUILayout.MinWidth(200));
        if (GUILayout.Button("Add Condition to database") && m_addedCondition.Trim() != string.Empty)
        {
            m_addedCondition = m_addedCondition.Trim().Replace(' ', '_'); 
            m_conditionsPair.Add(new ConditionPair(m_addedCondition, false));
            m_addedCondition = "";
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Apply Conditions"))
        {
            string _savedDatas = string.Empty;
            for (int i = 0; i < m_conditionsPair.Count; i++)
            {
                _savedDatas += m_conditionsPair[i].Key + " = " + m_conditionsPair[i].Value.ToString().ToLower() + ";\n";
            }
            m_dialogsSettings.LuaConditions = _savedDatas; 
        }
    }

    private void DrawColors()
    {
        GUILayout.Label("COLORS", m_titleStyle);
        m_dialogsSettings.OverrideCharacterColor = EditorGUILayout.Toggle("Override Character Color?", m_dialogsSettings.OverrideCharacterColor); 

        CharacterColorSettings _settings = null;
        for (int i = 0; i < m_dialogsSettings.CharactersColor.Count; i++)
        {
            _settings = m_dialogsSettings.CharactersColor[i]; 
            GUILayout.BeginHorizontal(); 
            GUILayout.Label(_settings.CharacterName, GUILayout.Width(position.width / 2 - 10));
            _settings.CharacterColor = EditorGUILayout.ColorField(_settings.CharacterColor, GUILayout.Width(position.width / 2 - 20));
            if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(15)))
            {
                m_dialogsSettings.CharactersColor.RemoveAt(i);
                GUILayout.EndHorizontal();
                continue; 
            }
            GUILayout.EndHorizontal(); 
        }
        GUILayout.BeginHorizontal();
        m_addedCharacterName = GUILayout.TextField(m_addedCharacterName, GUILayout.Width(position.width/2)); 
        if (GUILayout.Button("Add new Character", GUILayout.Width(position.width / 2)) && m_addedCharacterName.Length >= 3)
        {
            m_addedCharacterName = m_addedCharacterName.Replace(' ', '_'); 
            m_dialogsSettings.CharactersColor.Add(new CharacterColorSettings(m_addedCharacterName));
            m_addedCharacterName = string.Empty;
        }
        GUILayout.EndHorizontal(); 
    }

    private void LoadSettings()
    {
        if (!File.Exists(DialogsSettings.SettingsFilePath)) return; 
        string _jsonSettings = File.ReadAllText(DialogsSettings.SettingsFilePath); 
        m_dialogsSettings = JsonUtility.FromJson<DialogsSettings>(_jsonSettings); 
    }

    private void SaveSettings(bool _displayFeedback = true)
    {
        string _jsonSettings = JsonUtility.ToJson(m_dialogsSettings, true);
        if (!Directory.Exists(DialogsSettings.SettingsPath))
        {
            Directory.CreateDirectory(DialogsSettings.SettingsPath);
        }
        File.WriteAllText(DialogsSettings.SettingsFilePath, _jsonSettings);
        if(_displayFeedback)
        {
            EditorUtility.DisplayDialog("File saved", $"The Dialogs settings has been successfully saved", "Ok!");
        }
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {

        m_titleStyle = new GUIStyle();
        m_titleStyle.fontStyle = FontStyle.Bold;
        m_titleStyle.fontSize = 20;
        m_titleStyle.alignment = TextAnchor.MiddleCenter;

        if (!Directory.Exists(DialogsSettings.SettingsPath))
            Directory.CreateDirectory(DialogsSettings.SettingsPath);
        if (!File.Exists(DialogsSettings.SettingsFilePath))
        {
            m_dialogsSettings = new DialogsSettings();
            SaveSettings(false); 
        }
        else
        {
            LoadSettings(); 
        }
        if (m_dialogsSettings == null) return;
        m_conditionsPair = new List<ConditionPair>();
        if (m_dialogsSettings.LuaConditions == string.Empty) return; 
        string[] _conditions = m_dialogsSettings.LuaConditions.Split('\n'); 
        for (int i = 0; i < _conditions.Length; i++)
        {
            string[] _pair = _conditions[i].Split('=');
            if (_pair[0].Trim() == string.Empty || _pair[1].Trim() == string.Empty) return; 
            m_conditionsPair.Add(new ConditionPair(_pair[0].Trim(), _pair[1].Trim())); 
        }
    }

    private void OnGUI()
    {
        DrawConditions();
        DrawColors(); 

        if (GUILayout.Button("Save Settings"))
        {
            SaveSettings();
        }
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