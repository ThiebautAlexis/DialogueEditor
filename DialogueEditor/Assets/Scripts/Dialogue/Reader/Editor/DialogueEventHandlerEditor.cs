using System.Collections.Generic;
using System.Linq;
using System.IO; 
using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter;

namespace DialogueEditor
{
    [CustomEditor(typeof(DialogueEventHandler))]
    public class DialogueEventHandlerEditor : Editor
    {
        #region Fields and Properties
        [SerializeField] private SerializedProperty m_dialogueReader = null;
        [SerializeField] private SerializedProperty m_dialogueEvents = null;

        private string[] m_keys = new string[] { };
        private string[] m_conditions = new string[] { };
        private List<int> m_currentEventIndexes = new List<int>();
        private List<bool> m_displayEventAtIndex = new List<bool>();
        #endregion

        #region Methods

        #region Original Methods
        private void DrawEditor()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_dialogueReader, new GUIContent("Dialogue Reader"));
            if (EditorGUI.EndChangeCheck())
            {
                InitDialogueReaderKeys();
            }
            if(m_displayEventAtIndex.Count != m_dialogueEvents.arraySize ||m_currentEventIndexes.Count != m_dialogueEvents.arraySize)
            {
                InitDialogueReaderKeys(); 
            }
            if (m_dialogueReader.objectReferenceValue != null)
            {
                Color _originalBackgroundColor = GUI.backgroundColor;

                GUILayout.Space(11);
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 3), Color.blue);
                GUILayout.Space(11);

                if (m_keys != null)
                {
                    SerializedProperty _prop;
                    SerializedProperty _conditionEventProp;
                    for (int i = 0; i < m_dialogueEvents.arraySize; i++)
                    {
                        _prop = m_dialogueEvents.GetArrayElementAtIndex(i);
                        m_displayEventAtIndex[i] = EditorGUILayout.Foldout(m_displayEventAtIndex[i], "Dialogue Event called at " + _prop.FindPropertyRelative("m_activationKey").stringValue);
                        if (m_displayEventAtIndex[i])
                        {
                            EditorGUI.BeginChangeCheck();
                            m_currentEventIndexes[i] = EditorGUILayout.Popup("Activation Key", m_currentEventIndexes[i], m_keys);
                            if (EditorGUI.EndChangeCheck())
                            {
                                _prop.FindPropertyRelative("m_activationKey").stringValue = m_keys[m_currentEventIndexes[i]];
                            }
                            
                            GUILayout.Space(10);
                            GUILayout.Label("Unity Events");
                            EditorGUILayout.PropertyField(_prop.FindPropertyRelative("m_dialogueEvent"), new GUIContent("Dialogue Unity Event"));
                            _conditionEventProp = _prop.FindPropertyRelative("m_changedConditions");

                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Changing Condition Events");
                            if (GUILayout.Button("+", GUILayout.Width(Screen.width / 8)))
                            {
                                _conditionEventProp.InsertArrayElementAtIndex(0);
                            }
                            GUILayout.EndHorizontal();
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

                            GUI.backgroundColor = Color.red;
                            if (GUILayout.Button("Remove DialogueEvent"))
                            {
                                m_dialogueEvents.DeleteArrayElementAtIndex(i);
                                m_currentEventIndexes.RemoveAt(i);
                                m_displayEventAtIndex.RemoveAt(i);
                            }
                            GUI.backgroundColor = _originalBackgroundColor;
                            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 3), Color.red);
                        }
                    }
                }
                GUILayout.Space(5); 

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add Dialogue Event"))
                {
                    m_dialogueEvents.InsertArrayElementAtIndex(m_dialogueEvents.arraySize);
                    m_currentEventIndexes.Add(-1);
                    m_displayEventAtIndex.Add(false);
                }
                GUI.backgroundColor = _originalBackgroundColor;

            }
            serializedObject.ApplyModifiedProperties();
        }

        private void InitDialogueReaderKeys()
        {
            m_keys = new string[] { };
            m_currentEventIndexes = new List<int>();
            m_displayEventAtIndex = new List<bool>();
            string _spreadsheetID = JsonUtility.FromJson<Dialogue>(File.ReadAllText(Path.Combine(Dialogue.DialogAssetPath, (m_dialogueReader.objectReferenceValue as DialogueReader).DialogName + Dialogue.DialogAssetExtension))).SpreadSheetID;
            string _lineDescriptor = File.ReadAllText(Path.Combine(Dialogue.LineDescriptorPath, _spreadsheetID.GetHashCode().ToString() + Dialogue.LineDescriptorPostfixWithExtension));
            LoadKeys(_lineDescriptor);
            m_currentEventIndexes = new List<int>();
            if (m_dialogueEvents.arraySize != 0)
            {
                for (int i = 0; i < m_dialogueEvents.arraySize; i++)
                {
                    if (m_dialogueEvents.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue != string.Empty)
                        m_currentEventIndexes.Add(m_keys.ToList().IndexOf(m_dialogueEvents.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue));
                    else m_currentEventIndexes.Add(-1);
                    m_displayEventAtIndex.Add(false); 
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
            m_dialogueEvents = serializedObject.FindProperty("m_dialogueEvents");
            m_dialogueReader = serializedObject.FindProperty("m_dialogueReader");
            m_displayEventAtIndex = new List<bool>(m_dialogueEvents.arraySize) { false };
            string[] _conditionsLua = DialoguesSettingsManager.DialogsSettings.LuaConditions.Split('\n');
            m_conditions = new string[_conditionsLua.Length];
            for (int i = 0; i < _conditionsLua.Length; i++)
            {
                m_conditions[i] = _conditionsLua[i].Split('=')[0].Trim();
            }
            if (m_dialogueReader.objectReferenceValue != null)
            {
                InitDialogueReaderKeys();
            }
        }
        public override void OnInspectorGUI()
        {
            DrawEditor();
        }
        #endregion

        #endregion
    }
}