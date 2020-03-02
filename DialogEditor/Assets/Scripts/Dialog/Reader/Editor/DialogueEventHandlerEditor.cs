using System.Collections.Generic;
using System.Linq;
using System.IO; 
using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter; 

[CustomEditor(typeof(DialogueEventHandler))]
public class DialogueEventHandlerEditor : Editor
{
    #region Fields and Properties
    [SerializeField] private SerializedProperty m_dialogReader = null;
    [SerializeField] private SerializedProperty m_dialogEvents = null;

    private string[] m_keys = new string[] { };
    private string[] m_conditions = new string[] { };
    private List<int> m_currentIndexes = new List<int>();
    private List<bool> m_displayEventAtIndex = new List<bool>(); 
    #endregion

    #region Methods

    #region Original Methods
    private void DrawEditor()
    {
        EditorGUI.BeginChangeCheck(); 
        EditorGUILayout.PropertyField(m_dialogReader, new GUIContent("Dialog Reader"));
        if(EditorGUI.EndChangeCheck())
        {
            InitDialogReaderKeys(); 
        }
        if (m_dialogReader.objectReferenceValue != null)
        {
            Color _originalColor = GUI.color;
            Color _originalBackgroundColor = GUI.backgroundColor; 

            GUILayout.Space(11);
            GUI.color = Color.blue;
            GUILayout.Box("", GUILayout.Height(3), GUILayout.Width(Screen.width - 20));
            GUI.color = _originalColor;
            GUILayout.Space(11);

            if (m_keys != null)
            {
                SerializedProperty _prop;
                SerializedProperty _conditionEventProp; 
                for (int i = 0; i < m_dialogEvents.arraySize; i++)
                {
                    _prop = m_dialogEvents.GetArrayElementAtIndex(i);
                    m_displayEventAtIndex[i] = EditorGUILayout.Foldout(m_displayEventAtIndex[i], "Dialog Event called at " +_prop.FindPropertyRelative("m_activationKey").stringValue);
                    if(m_displayEventAtIndex[i])
                    {
                        EditorGUI.BeginChangeCheck();
                        m_currentIndexes[i] = EditorGUILayout.Popup("Activation Key", m_currentIndexes[i], m_keys);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _prop.FindPropertyRelative("m_activationKey").stringValue = m_keys[m_currentIndexes[i]];
                        }

                        GUILayout.Space(10);
                        GUILayout.Label("Unity Events");
                        EditorGUILayout.PropertyField(_prop.FindPropertyRelative("m_dialogEvent"), new GUIContent("Dialog Unity Event"));

                        GUILayout.Space(10);
                        GUILayout.Label("Changing Condtiion Events");
                        _conditionEventProp = _prop.FindPropertyRelative("m_changedConditions");
                        for (int j = 0; j < _conditionEventProp.arraySize; j++)
                        {
                            SerializedProperty _subProperty = _conditionEventProp.GetArrayElementAtIndex(j);
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(12)))
                            {
                                _conditionEventProp.DeleteArrayElementAtIndex(j);
                                continue;
                            }
                            int _index = m_conditions.ToList().Contains(_subProperty.FindPropertyRelative("m_conditionName").stringValue) ? m_conditions.ToList().IndexOf(_subProperty.FindPropertyRelative("m_conditionName").stringValue) : -1;
                            EditorGUI.BeginChangeCheck();
                            _index = EditorGUILayout.Popup("Set Condition", _index, m_conditions);
                            if (EditorGUI.EndChangeCheck())
                            {
                                _subProperty.FindPropertyRelative("m_conditionName").stringValue = m_conditions[_index];
                            }
                            GUILayout.EndHorizontal();
                            _subProperty.FindPropertyRelative("m_conditionValue").boolValue = EditorGUILayout.Toggle("To the value", _subProperty.FindPropertyRelative("m_conditionValue").boolValue);
                            GUILayout.Space(5);
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Add Changing condition Event");
                        if (GUILayout.Button("+", GUILayout.Width(Screen.width / 8)))
                        {
                            _conditionEventProp.InsertArrayElementAtIndex(0);
                        }
                        GUILayout.EndHorizontal();

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Remove DialogEvent"))
                        {
                            m_dialogEvents.DeleteArrayElementAtIndex(i);
                            m_currentIndexes.RemoveAt(i);
                            m_displayEventAtIndex.RemoveAt(i);
                        }
                        GUI.backgroundColor = _originalBackgroundColor;
                        GUI.color = Color.red;
                        GUILayout.Box("", GUILayout.Height(3), GUILayout.Width(Screen.width - 20));
                        GUI.color = _originalColor;
                    }
                    
                }
            }
            GUI.backgroundColor = Color.green; 
            if(GUILayout.Button("Add DialogEvent"))
            {
                m_dialogEvents.InsertArrayElementAtIndex(m_dialogEvents.arraySize);
                m_currentIndexes.Add(-1);
                m_displayEventAtIndex.Add(false);
            }
            GUI.backgroundColor = _originalBackgroundColor;
            
        }
        serializedObject.ApplyModifiedProperties(); 
    }


    private void InitDialogReaderKeys()
    {
        if(m_dialogReader.objectReferenceValue == null)
        {
            m_keys = new string[] { };
            m_currentIndexes = new List<int>(); 
            return; 
        }
        string _spreadsheetID = JsonUtility.FromJson<Dialogue>(File.ReadAllText(Path.Combine(Dialogue.DialogAssetPath, (m_dialogReader.objectReferenceValue as DialogueReader).DialogName + Dialogue.DialogAssetExtension))).SpreadSheetID;
        string _lineDescriptor = File.ReadAllText(Path.Combine(Dialogue.LineDescriptorPath, _spreadsheetID.GetHashCode().ToString() + Dialogue.LineDescriptorPostfixWithExtension));
        LoadKeys(_lineDescriptor);
        m_currentIndexes = new List<int>(); 
        if (m_dialogEvents.arraySize != 0)
        {
            for (int i = 0; i < m_dialogEvents.arraySize; i++)
            {
                if(m_dialogEvents.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue != string.Empty)
                    m_currentIndexes.Add(m_keys.ToList().IndexOf(m_dialogEvents.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue));
                else m_currentIndexes.Add(-1);
            }
        }
    }

    /// <summary>
    /// Get all the Ids of the dialog lines from the Line Descriptor
    /// </summary>
    /// <param name="_lineDescriptor"></param>
    private void LoadKeys(string _lineDescriptor)
    {
        Script _luaScript = new Script();
        _luaScript.DoString(_lineDescriptor);
        List<DynValue> _dynValues = _luaScript.Globals.Values.Where(v => (v.Type == DataType.Table) && (v.Table.Get("ID").IsNotNil())).ToList();
        m_keys = _dynValues.Select(v => v.Table.Get("ID").String).ToArray();
    }
    #endregion

    #region UnityMethods
    private void OnEnable()
    {
        m_dialogEvents = serializedObject.FindProperty("m_dialogEvents");
        m_dialogReader = serializedObject.FindProperty("m_dialogReader");
        m_displayEventAtIndex = new List<bool>(m_dialogEvents.arraySize) { false }; 
        string[] _conditionsLua = DialoguesSettingsManager.DialogsSettings.LuaConditions.Split('\n');
        m_conditions = new string[_conditionsLua.Length];
        for (int i = 0; i < _conditionsLua.Length; i++)
        {
            m_conditions[i] = _conditionsLua[i].Split('=')[0].Trim(); 
        }
        if(m_dialogReader.objectReferenceValue != null)
        {
            InitDialogReaderKeys(); 
        }
    }
    public override void OnInspectorGUI()
    {
        DrawEditor(); 
    }
    #endregion 

    #endregion
}
