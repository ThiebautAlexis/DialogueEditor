using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using MoonSharp.Interpreter; 

[Serializable]
public class Dialog
{
    #region Fields and Properties

    #region CONSTANTS
    public static string LineDescriptorPath { get { return Path.Combine(Application.dataPath, "DialogsDatas", "LineDescriptors"); } }
    public static string LineDescriptorPostfixWithExtension { get { return "_luaDescriptor.txt";  } }
    public static string LineDescriptorPostfix { get { return "_luaDescriptor"; } }
    public static string DialogAssetPath { get { return Path.Combine(Application.dataPath, "DialogsDatas", "Dialogs"); } }
    public static string DialogAssetExtension { get { return ".json"; } }
    #endregion

    #region Editor
#if UNITY_EDITOR
    private GUIStyle m_defaultNodeStyle = null;
    private GUIStyle m_defaultNodeStyleSelected = null; 
    private GUIStyle m_conditionNodeStyle = null;
    private GUIStyle m_conditionNodeStyleSelected = null;
    private GUIStyle m_starterNodeStyle = null;
    private GUIStyle m_starterNodeStyleSelected = null;
    private GUIStyle m_defaultConnectionPointStyle = null;
    private GUIStyle m_conditionConnectionPointStyle = null;
    private GUIStyle m_startConnectionPointStyle = null; 
    private GUIContent m_icon = null;
    private GUIContent m_answerIcon = null;
    private GUIContent m_pointIcon = null;
    private GUIContent m_startingSetIcon = null;
    private GUIContent m_conditionIcon = null;
    public bool AnyPartIsSelected { get { return m_dialogSets.Any(p => p.IsSelected) || m_dialogConditions.Any(c => c.IsSelected) || m_dialogStarter.IsSelected; } }
#endif
    #endregion 

    [SerializeField]private string m_dialogName = "";
    [SerializeField]private string m_spreadSheetID = "";
    public string SpreadSheetID { get { return m_spreadSheetID; } }

    [SerializeField] private DialogStarter m_dialogStarter; 
    [SerializeField] private List<DialogSet> m_dialogSets = new List<DialogSet>();
    [SerializeField] private List<DialogCondition> m_dialogConditions = new List<DialogCondition>(); 
    private string m_lineDescriptor = "";
    private DialogsSettings m_dialogSettings = null;
    private Script m_script;  
    public string DialogName { get { return m_dialogName; } }
    #endregion

    #region Constructor
    public Dialog(string _name, string _id)
    {
        m_dialogName = _name;
        m_spreadSheetID = _id;
        m_dialogSets = new List<DialogSet>();
        m_dialogConditions = new List<DialogCondition>();
    }
    #endregion

    #region Methods

#if UNITY_EDITOR
    public void AddCondition(Vector2 _pos)
    {
        m_dialogConditions.Add(new DialogCondition(_pos, RemoveCondition, m_conditionNodeStyle, m_conditionNodeStyleSelected, m_conditionConnectionPointStyle, m_conditionIcon, m_pointIcon)) ; 
    }

    /// <summary>
    /// Add a new DialogSet to the dialog
    /// </summary>
    /// <param name="_pos"></param>
    public void AddSet(Vector2 _pos)
    {
        m_dialogSets.Add(new DialogSet(_pos, RemoveSet, m_defaultNodeStyle, m_defaultNodeStyleSelected, m_defaultConnectionPointStyle, m_icon, m_answerIcon, m_pointIcon)); 
    }

    /// <summary>
    /// Drag all the dialog parts
    /// </summary>
    /// <param name="_delta"></param>
    public void DragAll(Vector2 _delta)
    {
        if (m_dialogSets == null && m_dialogConditions == null) return;
        m_dialogStarter.Drag(_delta); 
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].Drag(_delta); 
        }
        for (int i = 0; i < m_dialogConditions.Count; i++)
        {
            m_dialogConditions[i].Drag(_delta);
        }
    }

    /// <summary>
    /// Draw the dialog
    /// </summary>
    public void Draw(Action<DialogLine> _onOutLineSelected, Action<DialogNode> _onInNodeSelected, Action<DialogCondition, bool> _onOutConditionSelected, Action<SituationPair> _onOutSituationPairSelected)
    {
        if (m_dialogSets == null) m_dialogSets = new List<DialogSet>();
        bool _change = false;
        m_dialogStarter.Draw(m_dialogSets, m_dialogConditions, _onOutSituationPairSelected);
        if(m_dialogStarter.ProcessEvent(Event.current))
        {
            _change = true; 
        }
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].Draw(m_lineDescriptor, m_dialogSets, m_dialogConditions, _onOutLineSelected, _onInNodeSelected, m_dialogSettings.CharactersColor);
            if(m_dialogSets[i].ProcessEvent(Event.current))
            {
                _change = true;
            }
        }
        for (int i = 0; i < m_dialogConditions.Count; i++)
        {
            m_dialogConditions[i].Draw(m_dialogSets, m_dialogConditions, _onInNodeSelected, _onOutConditionSelected);
            if (m_dialogConditions[i].ProcessEvent(Event.current))
            {
                _change = true;
            }
        }
        _change = ProcessEvent(Event.current); 
        GUI.changed = _change; 
    }

    /// <summary>
    /// Init the Editor settings for the dialog and all of them parts
    /// </summary>
    /// <param name="_nodeStyle">Style of the node</param>
    /// <param name="_connectionPointStyle">Style of the Connection Point</param>
    /// <param name="_basicIcon">Icon of the basic Node Type</param>
    /// <param name="_answerIcon">Icon of the Answer Node Type</param>
    /// <param name="_startingSetIcon">Icon of the starting set</param>
    /// <param name="_pointIcon">Connection Point Icon</param>
    public void InitEditorSettings(GUIStyle _nodeStyle, GUIStyle _selectedNodeStyle, GUIStyle _conditionStyle, GUIStyle _selectedConditionStyle, GUIStyle _starterNodeStyle, GUIStyle _starterNodeStyleSelected, GUIStyle _connectionPointStyle, GUIStyle _conditionConnectionPointStyle, GUIStyle _starterConnectionPointStyle, GUIContent _basicIcon, GUIContent _answerIcon, GUIContent _startingSetIcon, GUIContent _pointIcon, GUIContent _conditionIcon)
    {
        m_defaultNodeStyle = _nodeStyle;
        m_defaultNodeStyleSelected = _selectedNodeStyle; 
        m_conditionNodeStyle = _conditionStyle;
        m_conditionNodeStyleSelected = _selectedConditionStyle;
        m_defaultConnectionPointStyle = _connectionPointStyle;
        m_conditionConnectionPointStyle = _conditionConnectionPointStyle;
        m_starterNodeStyle = _starterNodeStyle;
        m_starterNodeStyleSelected = _starterNodeStyleSelected;
        m_startConnectionPointStyle = _starterConnectionPointStyle;
        m_icon = _basicIcon;
        m_answerIcon = _answerIcon;
        m_startingSetIcon = _startingSetIcon; 
        m_pointIcon = _pointIcon;
        m_conditionIcon = _conditionIcon;
        if (File.Exists(Path.Combine(LineDescriptorPath, m_spreadSheetID.GetHashCode().ToString() + LineDescriptorPostfixWithExtension)))
        {
            m_lineDescriptor = File.ReadAllText(Path.Combine(LineDescriptorPath, m_spreadSheetID.GetHashCode().ToString() + LineDescriptorPostfixWithExtension));
        }
        if (File.Exists(DialogsSettings.SettingsFilePath))
        {
            m_dialogSettings = JsonUtility.FromJson<DialogsSettings>(File.ReadAllText(DialogsSettings.SettingsFilePath)); 
        }
        if (m_dialogStarter == null) m_dialogStarter = new DialogStarter(Vector2.zero, m_starterNodeStyle, m_starterNodeStyleSelected, m_startConnectionPointStyle, m_startingSetIcon, m_pointIcon); 
        else m_dialogStarter.InitEditorSettings(m_starterNodeStyle, m_starterNodeStyleSelected, m_startConnectionPointStyle, m_startingSetIcon, m_pointIcon); 
        if (m_dialogSets == null) m_dialogSets = new List<DialogSet>();
        if (m_dialogConditions == null) m_dialogConditions = new List<DialogCondition>(); 
        for(int i = 0; i < m_dialogSets.Count; i++)
        {
            m_dialogSets[i].InitEditorSettings(_nodeStyle, _selectedNodeStyle, _connectionPointStyle, _basicIcon, _answerIcon, m_pointIcon, RemoveSet); 
        }
        for (int i = 0; i < m_dialogConditions.Count; i++)
        {
            m_dialogConditions[i].InitEditorSettings(_conditionStyle, _selectedConditionStyle ,_conditionConnectionPointStyle, _conditionIcon, _pointIcon, RemoveCondition, m_dialogSettings) ; 
        }
    }

    /// <summary>
    /// Process the events of the Dialog
    /// </summary>
    /// <param name="_e">Current Event</param>
    /// <returns></returns>
    public bool ProcessEvent(Event _e)
    {
        if(m_dialogSets.Any(p => p.IsSelected) && _e.keyCode == KeyCode.Delete)
        {
            DialogSet _selectedPart = m_dialogSets.Where(p => p.IsSelected).FirstOrDefault();
            m_dialogSets.Remove(_selectedPart); 
            return true; 
        }
        if (m_dialogConditions.Any(c => c.IsSelected) && _e.keyCode == KeyCode.Delete)
        {
            DialogCondition _selectedCondition = m_dialogConditions.Where(c => c.IsSelected).FirstOrDefault();
            m_dialogConditions.Remove(_selectedCondition);
            return true;
        }
        if (_e.type == EventType.KeyDown && _e.control && _e.keyCode == KeyCode.S)
        {
            SaveDialog();
        }
        return false; 
    }

    /// <summary>
    /// Remove the condition
    /// </summary>
    /// <param name="_condition"></param>
    private void RemoveCondition(DialogCondition _condition)
    {
        if (m_dialogConditions.Contains(_condition))
            m_dialogConditions.Remove(_condition);
    }

    /// <summary>
    /// Remove the set
    /// </summary>
    /// <param name="_set">Set to remove</param>
    private void RemoveSet(DialogSet _set)
    {
        if(m_dialogSets.Contains(_set))
            m_dialogSets.Remove(_set); 
    }

    /// <summary>
    /// Save the dialog as a Json File
    /// </summary>
    private void SaveDialog()
    {
        string _jsonDialog = JsonUtility.ToJson(this, true);
        string _name = m_dialogName.Replace(" ", string.Empty);
        Debug.Log("The Dialog Asset " + m_dialogName + " has been saved in " + DialogAssetPath); 
        File.WriteAllText(Path.Combine(DialogAssetPath, _name + DialogAssetExtension), _jsonDialog);
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
        UnityEditor.EditorUtility.DisplayDialog("File saved", $"The {m_dialogName} dialog has been successfully saved", "Ok!"); 
    }
#endif

    /// <summary>
    /// Return the string value of the function which check if the selected conditon is true
    /// </summary>
    /// <param name="_condition">Checked Condition</param>
    /// <returns></returns>
    public static string GetStringConditionMethod(string _condition)
    {
        return $@"
function check_condition()
            return {_condition}
end;
";
    }

    /// <summary>
    /// Check if the condition of the node is true or false and return this value
    /// </summary>
    /// <param name="_condition">Condition (string) of the condition node</param>
    /// <returns></returns>
    private bool CheckCondition(string _condition)
    {
        string _conditionFuncString = "";
        Debug.Log(DialogsSettingsManager.DialogsSettings.LuaConditions);
        _conditionFuncString = DialogsSettingsManager.DialogsSettings.LuaConditions; 
        _conditionFuncString += GetStringConditionMethod(_condition);
        m_script = new Script();
        m_script.DoString(_conditionFuncString);

        DynValue _operation = m_script.Globals.Get("check_condition");
        return m_script.Call(_operation).Boolean; 
    }

    /// <summary>
    /// Get the Next set of the dialog according to the next token
    /// </summary>
    /// <param name="_nextToken">Token of the next Dialog Set</param>
    /// <returns></returns>
    public DialogSet GetNextSet(int _nextToken)
    {
        if(m_dialogConditions.Any(c => c.NodeToken == _nextToken))
        {
            DialogCondition _condition = m_dialogConditions.Where(c => c.NodeToken == _nextToken).FirstOrDefault();
            if (_condition == null) return null;

            int _conditionToken = CheckCondition(_condition.Condition) ? _condition.LinkedTokenTrue : _condition.LinkedTokenFalse; 
            return GetNextSet(_conditionToken); 
        }
        return m_dialogSets.Where(s => s.NodeToken == _nextToken).FirstOrDefault();
    }

    public DialogSet GetFirstSet(SituationsEnum _situation)
    {
        int _nodeToken = -1; 

        if (m_dialogStarter.SituationPairs.Any(s => s.Situation == _situation))
        {
            _nodeToken = m_dialogStarter.SituationPairs.Where(s => s.Situation == _situation).First().LinkedToken; 
        }
        if(_nodeToken == -1)
            return null;
        return GetNextSet(_nodeToken); 
    }

    #endregion
}
