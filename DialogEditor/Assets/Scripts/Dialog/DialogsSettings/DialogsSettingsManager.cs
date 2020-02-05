using System.Collections.Generic;
using System.IO; 
using UnityEngine;

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

                // ELSE CREATE A PROFILE FROM THE TEMPLATE (in the Adressables Assets) AND SAVE IT ON THE COMPUTER
                // THEN THE SAVED PROFILE IS THE DIALOGS SETTINGS
#endif
            }
            return m_dialogsSettings; 
        }
    }
}
