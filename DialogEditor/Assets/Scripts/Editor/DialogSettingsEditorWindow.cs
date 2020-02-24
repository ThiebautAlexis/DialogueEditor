using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq; 

public class DialogSettingsEditorWindow : EditorWindow
{ 
    private List<ConditionPair> m_conditionsPair = null;
    private string m_addedCondition = "";
    private string m_addedCharacterName = "";
    private string m_addedKey = "";
    private string m_addedAudioKey = ""; 
    private string[] m_localisationKeys = new string[] { };
    private string[] m_audioLocalisationKeys = new string[] { }; 
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
    /// <summary>
    /// Draw the conditions of the Settings Template Profile
    /// Allow the user to remove conditions, to add new conditions and set their default values 
    /// </summary>
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

    /// <summary>
    /// Draw the Character colors of the Settings Template Profile
    /// Allow the user to remove, add or modify them 
    /// </summary>
    private void DrawColors()
    {
        GUILayout.Label("COLORS", m_titleStyle);
        m_dialogsSettings.OverrideCharacterColor = EditorGUILayout.Toggle("Override Characters Color", m_dialogsSettings.OverrideCharacterColor); 

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
        m_addedCharacterName = GUILayout.TextField(m_addedCharacterName, GUILayout.Width(position.width / 2));
        if (GUILayout.Button("Add new Character", GUILayout.Width(position.width / 2)) && m_addedCharacterName.Length >= 3)
        {
            m_addedCharacterName = m_addedCharacterName.Replace(' ', '_');
            m_dialogsSettings.CharactersColor.Add(new CharacterColorSettings(m_addedCharacterName));
            m_addedCharacterName = string.Empty;
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw the localisation keys of the Settings Template Profile
    /// Allow the user to add, remove or edit them
    /// </summary>
    private void DrawLocalisationsKeys()
    {
        GUILayout.Label("LOCALISATION KEYS - Text", m_titleStyle);
        for (int i = 0; i < m_localisationKeys.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_localisationKeys[i]); 
            if(GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(15)))
            {
                List<string> _temp = m_localisationKeys.ToList();
                _temp.RemoveAt(i);
                m_localisationKeys = _temp.ToArray();
                m_dialogsSettings.LocalisationKeys = m_localisationKeys; 
            }
            GUILayout.EndHorizontal(); 
        }
        GUILayout.BeginHorizontal();
        m_addedKey = GUILayout.TextField(m_addedKey, GUILayout.MinWidth(200));
        if (GUILayout.Button("Add Key to database") && m_addedKey.Trim() != string.Empty)
        {
            m_addedKey = m_addedKey.Trim().Replace(' ', '_');
            List<string> _temp = m_localisationKeys.ToList(); 
            _temp.Add(m_addedKey);
            m_localisationKeys = _temp.ToArray(); 
            m_dialogsSettings.LocalisationKeys = m_localisationKeys;
            m_addedKey = "";
        }
        GUILayout.EndHorizontal();
        m_dialogsSettings.CurrentLocalisationKeyIndex = EditorGUILayout.Popup("Current Localisation Key", m_dialogsSettings.CurrentLocalisationKeyIndex, m_localisationKeys);

        GUILayout.Label("LOCALISATION KEYS - Audio", m_titleStyle);
        for (int i = 0; i < m_audioLocalisationKeys.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_audioLocalisationKeys[i]);
            if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(15)))
            {
                List<string> _temp = m_audioLocalisationKeys.ToList();
                _temp.RemoveAt(i);
                m_audioLocalisationKeys = _temp.ToArray();
                m_dialogsSettings.AudioLocalisationKeys = m_audioLocalisationKeys;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        m_addedAudioKey = GUILayout.TextField(m_addedAudioKey, GUILayout.MinWidth(200));
        if (GUILayout.Button("Add Key to database") && m_addedAudioKey.Trim() != string.Empty)
        {
            m_addedAudioKey = m_addedAudioKey.Trim().Replace(' ', '_');
            List<string> _temp = m_audioLocalisationKeys.ToList();
            _temp.Add(m_addedAudioKey);
            m_audioLocalisationKeys = _temp.ToArray();
            m_dialogsSettings.AudioLocalisationKeys = m_audioLocalisationKeys;
            m_addedAudioKey = "";
        }
        GUILayout.EndHorizontal();
        m_dialogsSettings.CurrentAudioLocalisationKeyIndex = EditorGUILayout.Popup("Current Localisation Key", m_dialogsSettings.CurrentAudioLocalisationKeyIndex, m_audioLocalisationKeys);
    }

    /// <summary>
    /// Draw the Condiditions, Colors and Localisation Settings of <see cref="DialogsSettingsManager.DialogsSettings"/> during play mode
    /// </summary>
    private void DrawPlayingSettings()
    {
        GUILayout.Label("CONDITIONS", m_titleStyle);
        for (int i = 0; i < m_conditionsPair.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_conditionsPair[i].Key, GUILayout.MinWidth(150), GUILayout.MaxWidth(200));
            EditorGUI.BeginChangeCheck();
            m_conditionsPair[i].Value = GUILayout.Toggle(m_conditionsPair[i].Value, "Current Value");
            if (EditorGUI.EndChangeCheck())
            {
                string _savedDatas = string.Empty;
                DialogsSettingsManager.SetConditionBoolValue(m_conditionsPair[i].Key, m_conditionsPair[i].Value);
                m_dialogsSettings.LuaConditions = DialogsSettingsManager.DialogsSettings.LuaConditions;
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.HelpBox("Use this window to change the value of any condition", MessageType.Info);

        GUILayout.Space(15);
        GUILayout.Label("LOCALISATION KEYS - Text", m_titleStyle);
        EditorGUI.BeginChangeCheck();
        m_dialogsSettings.CurrentLocalisationKeyIndex = EditorGUILayout.Popup("Current Localisation Key", m_dialogsSettings.CurrentLocalisationKeyIndex, m_dialogsSettings.LocalisationKeys);
        if (EditorGUI.EndChangeCheck())
        {
            DialogsSettingsManager.SetTextLocalisationKeyIndex(m_dialogsSettings.CurrentLocalisationKeyIndex);
        }
        GUILayout.Space(15);
        GUILayout.Label("LOCALISATION KEYS - Audio", m_titleStyle);
        EditorGUI.BeginChangeCheck();
        m_dialogsSettings.CurrentAudioLocalisationKeyIndex = EditorGUILayout.Popup("Current Localisation Key", m_dialogsSettings.CurrentAudioLocalisationKeyIndex, m_dialogsSettings.AudioLocalisationKeys);
        if (EditorGUI.EndChangeCheck())
        {
            DialogsSettingsManager.SetAudioLocalisationKeyIndex(m_dialogsSettings.CurrentAudioLocalisationKeyIndex);
        }
    }

    /// <summary>
    /// Load the Settings Template stored in <see cref="DialogsSettings.SettingsFilePath"/>  
    /// </summary>
    private void LoadSettingsFiles()
    {
        if (!File.Exists(DialogsSettings.SettingsFilePath)) return; 
        string _jsonSettings = File.ReadAllText(DialogsSettings.SettingsFilePath); 
        m_dialogsSettings = JsonUtility.FromJson<DialogsSettings>(_jsonSettings); 
    }

    /// <summary>
    /// Load the DialogsSettings Profile according to the PlayMode
    /// In <see cref="PlayModeStateChange.EnteredEditMode"/> load the stored Profile in <see cref="DialogsSettings.SettingsFilePath"/>
    /// In <see cref="PlayModeStateChange.EnteredPlayMode"/> get the <see cref="DialogsSettingsManager.DialogsSettings"/> profile
    /// </summary>
    /// <param name="_playmode">PlayModeStateChange</param>
    private void LoadSettingsForPlayMode(PlayModeStateChange _playmode)
    {
        switch (_playmode)
        {
            case PlayModeStateChange.EnteredEditMode:
                if (!Directory.Exists(DialogsSettings.SettingsPath))
                    Directory.CreateDirectory(DialogsSettings.SettingsPath);
                if (!File.Exists(DialogsSettings.SettingsFilePath))
                {
                    m_dialogsSettings = new DialogsSettings();
                    SaveSettings(false);
                }
                else
                {
                    LoadSettingsFiles();
                }
                m_localisationKeys = m_dialogsSettings.LocalisationKeys;
                m_audioLocalisationKeys = m_dialogsSettings.AudioLocalisationKeys; 
                break;
            case PlayModeStateChange.EnteredPlayMode:
                m_dialogsSettings = DialogsSettingsManager.DialogsSettings; 
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
            default:
                break;
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

    /// <summary>
    /// Save the edited profile as <see cref="DialogsSettings.SettingsFilePath"/>
    /// </summary>
    /// <param name="_displayFeedback">Does a feedback has been to be displayed?</param>
    private void SaveSettings(bool _displayFeedback = true)
    {
        string _jsonSettings = JsonUtility.ToJson(m_dialogsSettings, true);
        if (!Directory.Exists(DialogsSettings.SettingsPath))
        {
            Directory.CreateDirectory(DialogsSettings.SettingsPath);
        }
        File.WriteAllText(DialogsSettings.SettingsFilePath, _jsonSettings);
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
        if(_displayFeedback)
        {
            EditorUtility.DisplayDialog("File saved", $"The Dialogs settings has been successfully saved", "Ok!");
        }
    }

    private void UpdateConditions()
    {
        m_conditionsPair = new List<ConditionPair>();
        string[] _conditions = DialogsSettingsManager.DialogsSettings.LuaConditions.Split('\n');
        for (int i = 0; i < _conditions.Length; i++)
        {
            string[] _pair = _conditions[i].Split('=');
            if (_pair[0].Trim() == string.Empty || _pair[1].Trim() == string.Empty) return;
            m_conditionsPair.Add(new ConditionPair(_pair[0].Trim(), _pair[1].Trim()));
        }
        Repaint(); 
    }
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        m_titleStyle = new GUIStyle();
        m_titleStyle.fontStyle = FontStyle.Bold;
        m_titleStyle.fontSize = 20;
        m_titleStyle.alignment = TextAnchor.MiddleCenter;

        LoadSettingsForPlayMode(Application.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode); 

        EditorApplication.playModeStateChanged += LoadSettingsForPlayMode;
        DialogsSettingsManager.OnSettingsModified += UpdateConditions;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= LoadSettingsForPlayMode;
        DialogsSettingsManager.OnSettingsModified -= UpdateConditions;
    }

    private void OnGUI()
    {
        if(Application.isPlaying)
        {
            DrawPlayingSettings();
            return; 
        }
        DrawConditions();
        DrawColors();
        DrawLocalisationsKeys(); 

        if (GUILayout.Button("Save Settings"))
        {
            SaveSettings();
        }
    }
    #endregion

    #endregion
}
