using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class DialoguesSettingsManager 
{
    #region Const 
    private const string PROFILE_KEY = "LAST_KEY_USED";
    #endregion 
    
    #region Events and Actions
    public static event Action OnSettingsModified = null;
    public static event Action OnAudioLocalisationKeyChanged = null;
    #endregion

    #region Fields and Properties
    public static string CurrentProfileName = ""; 
    private static DialoguesSettings m_dialogsSettings = null; 
    public static DialoguesSettings DialogsSettings
    {
        get 
        { 
            if(m_dialogsSettings == null)
            {
#if UNITY_EDITOR
                // IF UNITY EDITOR --> LOAD THE DEVELOPER TEMPLATE
                if (File.Exists(DialoguesSettings.SettingsFilePath))
                {
                    m_dialogsSettings = JsonUtility.FromJson<DialoguesSettings>(File.ReadAllText(DialoguesSettings.SettingsFilePath));
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
    #endregion 

    #region Methods

    #region Profile Methods
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    /// <summary>
    /// Create a new profile based on the Settings Template profile
    /// </summary>
    public static void CreateOrLoadProfile()
    {
#if UNITY_EDITOR
        AsyncOperationHandle<TextAsset> _settingsAssetAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(DialoguesSettings.SettingsFileName);
        _settingsAssetAsyncHandler.Completed += OnSettingsAssetLoaded;
#else
        if (PlayerPrefs.HasKey(PROFILE_KEY))
        {
            CurrentProfileName = PlayerPrefs.GetString(PROFILE_KEY);
            m_dialogsSettings = LoadProfile(CurrentProfileName);
            if (m_dialogsSettings != null) return; 
        }
        AsyncOperationHandle<TextAsset> _settingsAssetAsyncHandler = Addressables.LoadAssetAsync<TextAsset>(DialogsSettings.SettingsFileName);
        _settingsAssetAsyncHandler.Completed += OnSettingsAssetLoaded;
#endif


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
        m_dialogsSettings = JsonUtility.FromJson<DialoguesSettings>(_loadedAsset.Result.ToString());
        SaveProfile();
    }

    /// <summary>
    /// Load the profile named <see cref="_profileName"/> in the persistant data path.
    /// Set the <see cref="m_dialogsSettings"/> as the loaded profile
    /// </summary>
    /// <param name="_profileName"></param>
    /// <returns></returns>
    public static DialoguesSettings LoadProfile(string _profileName = "defaultProfile")
    {
        string _path = Path.Combine(Application.persistentDataPath, _profileName + ".sav"); 
        if (!File.Exists(_path))
        {
            CreateOrLoadProfile();
            return null;
        }
        BinaryFormatter _formatter = new BinaryFormatter();
        FileStream _stream = new FileStream(_path, FileMode.Open);
        string _jsonSettings = _formatter.Deserialize(_stream) as string; 

        DialoguesSettings _settings = JsonUtility.FromJson<DialoguesSettings>(_jsonSettings);
        _stream.Close(); 
        return _settings;
    }

    /// <summary>
    /// Save the current profile in the persistant datapath
    /// </summary>
    /// <param name="_profileName">Name of the current profile</param>
    public static void SaveProfile(string _profileName = "defaultProfile")
    {
#if UNITY_STANDALONE
        if (!Directory.Exists(Application.persistentDataPath))
            Directory.CreateDirectory(Application.persistentDataPath);
        string _jsonSettings = JsonUtility.ToJson(m_dialogsSettings);

        BinaryFormatter _formatter = new BinaryFormatter();
        string _path = Path.Combine(Application.persistentDataPath, _profileName + ".sav");
        FileStream _stream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        _formatter.Serialize(_stream, _jsonSettings);
        _stream.Close();

        PlayerPrefs.SetString(PROFILE_KEY, _profileName); 
        //System.Diagnostics.Process.Start(Application.persistentDataPath); 
#endif
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
                _conditions[i] = _variable[0] + "=" + _variable[1]+";";
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
        OnSettingsModified?.Invoke(); 
    }

    /// <summary>
    /// Set the Localisation Key to the selected Localisation Key with the index in <paramref name="_newIndex"/> in the current Profile
    /// </summary>
    /// <param name="_newIndex">Localisation Key Index</param>
    public static void SetTextLocalisationKeyIndex(int _newIndex)
    {
        m_dialogsSettings.CurrentLocalisationKeyIndex = _newIndex;
        SaveProfile(); 
    }

    /// <summary>
    /// Set the Localisation Key to the selected Localisation Key with the index in <paramref name="_newIndex"/> in the current Profile
    /// </summary>
    /// <param name="_newIndex">Localisation Key Index</param>
    public static void SetAudioLocalisationKeyIndex(int _newIndex)
    {
        m_dialogsSettings.CurrentAudioLocalisationKeyIndex = _newIndex;
        OnAudioLocalisationKeyChanged?.Invoke();
        SaveProfile();
    }
#endregion

#endregion

}
