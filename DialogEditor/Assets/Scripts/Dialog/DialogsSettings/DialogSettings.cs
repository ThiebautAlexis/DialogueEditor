using System.Collections.Generic;
using System.IO; 
using UnityEngine;

[System.Serializable]
public class DialogSettings
{
    #region Static 
    public static string ConditionsPath { get { return Path.Combine(Application.dataPath, "DialogsDatas", "Conditions"); } }
    public static string ConditionsFileName { get { return "ConditionsTemplate"; } }
    public static string ConditionsFileExtension { get { return ".txt"; } }
    public static string ConditionsFilePath { get { return Path.Combine(ConditionsPath, ConditionsFileName + ConditionsFileExtension); } }
    #endregion

    #region Fields and Properties
    [SerializeField] private List<CharacterColorSettings> m_charactersColor = new List<CharacterColorSettings>();
    [SerializeField] private bool m_overrideCharacterColor = false;
    
    public List<CharacterColorSettings> CharactersColor { get { return m_charactersColor; } }
    public bool OverrideCharacterColor { get { return m_overrideCharacterColor; } }
    #endregion 
}

[System.Serializable]
public class CharacterColorSettings
{
    #region Fields and Properties
    [SerializeField] private string m_characterName = string.Empty;
    [SerializeField] private Color m_characterColor = Color.black; 

    public string CharacterIdentifier { get { return m_characterName.Substring(0,3).ToUpper(); } }
    public Color CharacterColor { get { return m_characterColor; } }
    #endregion
}
