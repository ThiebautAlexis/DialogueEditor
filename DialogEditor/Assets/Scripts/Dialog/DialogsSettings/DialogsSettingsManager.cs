using System.IO; 
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class DialogsSettingsManager 
{
    private static DialogsSettings m_dialogsSettings = null; 
    public static DialogsSettings DialogsSettings
    {
        get 
        { 
            if(m_dialogsSettings == null)
            {
#if UNITY_EDITOR
                // IF UNITY EDITOR --> LOAD THE DEVELOPER TEMPLATE
                if (File.Exists(DialogsSettings.SettingsFilePath))
                {
                    m_dialogsSettings = JsonUtility.FromJson<DialogsSettings>(File.ReadAllText(DialogsSettings.SettingsFilePath));
                }
#else
                // ELSE IF THERE IS AN EXISTING PROFILE --> LOAD THE EXISTING PROFILE
                m_dialogsSettings = LoadProfile();
                // ELSE CREATE A PROFILE FROM THE TEMPLATE (in the Adressables Assets) AND SAVE IT ON THE COMPUTER
                // THEN THE SAVED PROFILE IS THE DIALOGS SETTINGS
#endif
            }
            return m_dialogsSettings; 
        }
    }


    #region Methods

    #region Profile Methods
    /// <summary>
    /// Create a new profile based on the Settings Template profile
    /// </summary>
    public static void CreateProfile()
    {
        AsyncOperationHandle<TextAsset> _settingsAssetAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(DialogsSettings.SettingsFileName);
        _settingsAssetAsyncHandler.Completed += OnSettingsAssetLoaded;
    }

    /// <summary>
    /// When the Template Settings is loaded, create a copy and save it.
    /// Set the variable <see cref="m_dialogsSettings"/> to the profile newly created
    /// </summary>
    /// <param name="_loadedAsset">Template Settings Asset</param>
    private static void OnSettingsAssetLoaded(AsyncOperationHandle<TextAsset> _loadedAsset)
    {
        if (_loadedAsset.Result == null)
        {
            Debug.LogError("IS NULL");
            return;
        }
        m_dialogsSettings = JsonUtility.FromJson<DialogsSettings>(_loadedAsset.Result.ToString());
        SaveProfile();
    }

    /// <summary>
    /// Load the profile named <see cref="_profileName"/> in the persistant data path.
    /// Set the <see cref="m_dialogsSettings"/> as the loaded profile
    /// </summary>
    /// <param name="_profileName"></param>
    /// <returns></returns>
    public static DialogsSettings LoadProfile(string _profileName = "defaultProfile")
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, _profileName + ".json")))
        {
            CreateProfile();
            return null;
        }
        DialogsSettings _settings = JsonUtility.FromJson<DialogsSettings>(File.ReadAllText(Path.Combine(Application.persistentDataPath, _profileName + ".json")));
        return _settings;
    }

    /// <summary>
    /// Save the current profile in the persistant datapath
    /// </summary>
    /// <param name="_profileName">Name of the current profile</param>
    public static void SaveProfile(string _profileName = "defaultProfile")
    {
        if (!Directory.Exists(Application.persistentDataPath))
            Directory.CreateDirectory(Application.persistentDataPath);
        string _jsonSettings = JsonUtility.ToJson(m_dialogsSettings);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, _profileName + ".json"), _jsonSettings);
    }
    #endregion

    #region Other Methods
    /// <summary>
    /// Set the condition named <paramref name="_conditionName"/> to the bool <paramref name="_value"/> in the current profile
    /// Then save it.
    /// </summary>
    /// <param name="_conditionName">Name of the condition to change</param>
    /// <param name="_value">New value of the condition</param>
    public static void SetConditionBoolValue(string _conditionName, bool _value)
    {
        string[] _conditions = m_dialogsSettings.LuaConditions.Split('\n');
        string[] _variable;
        for (int i = 0; i < _conditions.Length; i++)
        {
            _variable = _conditions[i].Trim().Split('=');
            if (_variable[0].Trim() == _conditionName.Trim())
            {
                _variable[1] = _value.ToString().ToLower();
                _conditions[i] = _variable[0] + "=" + _variable[1];
                break;
            }
        }
        string _temp = string.Empty; 
        for (int i = 0; i < _conditions.Length; i++)
        {
            _temp += _conditions[i] + "\n";
        }
        m_dialogsSettings.LuaConditions = _temp;
        SaveProfile(); 
    }

    /// <summary>
    /// Set the Localisation Key to the selected Localisation Key with the index in <paramref name="_newIndex"/> in the current Profile
    /// </summary>
    /// <param name="_newIndex">Localisation Key Index</param>
    public static void SetLocalisationKeyIndex(int _newIndex)
    {
        m_dialogsSettings.CurrentLocalisationKeyIndex = _newIndex;
        SaveProfile(); 
    }
    #endregion 

    #endregion
   
}
