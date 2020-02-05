using System.Collections.Generic;
using System.IO; 
using UnityEngine;

[System.Serializable]
public class DialogsSettings
{
    #region Static 
    public static string SettingsPath { get { return Path.Combine(Application.dataPath, "DialogsDatas", "Settings"); } }
    public static string SettingsFileName { get { return "SettingsTemplate"; } }
    public static string SettingsExtension { get { return ".json";  } }
    public static string SettingsFilePath { get { return Path.Combine(SettingsPath, SettingsFileName) + SettingsExtension; } }
    #endregion

    #region Fields and Properties
    [SerializeField] private string m_luaConditions = string.Empty; 
    [SerializeField] private List<CharacterColorSettings> m_charactersColor = new List<CharacterColorSettings>();
    [SerializeField] private bool m_overrideCharacterColor = false;
    [SerializeField] private string[] m_localisationKeys = new string[] { };
    [SerializeField] private int m_currentLocalisationKeyIndex = 0; 

    public string LuaConditions { get { return m_luaConditions; } set { m_luaConditions = value; } }
    public List<CharacterColorSettings> CharactersColor { get { return m_charactersColor; } }
    public bool OverrideCharacterColor { get { return m_overrideCharacterColor; } set { m_overrideCharacterColor = value; } }
    public string[] LocalisationKeys { get { return m_localisationKeys; } set { m_localisationKeys = value; } }
    public int CurrentLocalisationKeyIndex { get { return m_currentLocalisationKeyIndex; } set { m_currentLocalisationKeyIndex = value; } }
    public string CurrentLocalisationKey { get { return m_localisationKeys[m_currentLocalisationKeyIndex]; } }
    #endregion 
}

[System.Serializable]
public class CharacterColorSettings
{
    #region Fields and Properties
    [SerializeField] private string m_characterName = string.Empty;
    [SerializeField] private Color m_characterColor = Color.black; 

    public string CharacterName { get { return m_characterName;  } }
    public string CharacterIdentifier { get { return m_characterName.Substring(0,2).ToUpper(); } }
    public Color CharacterColor { get { return m_characterColor; } set { m_characterColor = value; } }
    #endregion

    #region Constructor
    public CharacterColorSettings(string _name)
    {
        m_characterName = _name;
        m_characterColor = Color.black; 
    }
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
