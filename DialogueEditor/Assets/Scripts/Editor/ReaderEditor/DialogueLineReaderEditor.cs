using System.Collections.Generic;
using System.IO;
using System.Linq; 
using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter; 

namespace DialogueEditor
{
    [CustomEditor(typeof(DialogueLineReader))]
    public class DialogueLineReaderEditor : Editor
    {
        #region Fields and Properties
        private SerializedProperty m_lineDescriptorName = null;
        private SerializedProperty m_dialogueLineKey = null;
        private SerializedProperty m_intialWaitingTime = null;

        private SerializedProperty m_textDisplayer = null;
        private SerializedProperty m_fontColor = null;
        private SerializedProperty m_audioSource = null;

        private SerializedProperty m_event = null;
        private SerializedProperty m_conditionEvents = null; 

        private string m_content = string.Empty; 
        private string[] m_lineDescriptors = null;
        private string[] m_linesKey = null;
        private string[] m_conditions = new string[] { };
        private int m_lineDescriptorIndex = -1;
        private int m_dialogueLineKeyIndex = -1; 
        private GUIStyle m_style = null; 
        #endregion

        #region Methods

        #region Original Methods
        /// <summary>
        /// Draw the Inspector
        /// </summary>
        private void DrawEditor()
        {
            EditorGUILayout.LabelField(new GUIContent("Dialogue Line"), m_style);
            if (m_lineDescriptors == null) return; 
            EditorGUI.BeginChangeCheck();
            m_lineDescriptorIndex = EditorGUILayout.Popup(new GUIContent("Line Descriptor"), m_lineDescriptorIndex, m_lineDescriptors); 
            if(EditorGUI.EndChangeCheck())
            {
                m_lineDescriptorName.stringValue = m_lineDescriptors[m_lineDescriptorIndex];
                InitLineKeys(); 
            }
            if (m_linesKey != null)
            {
                EditorGUI.BeginChangeCheck();
                m_dialogueLineKeyIndex = EditorGUILayout.Popup(new GUIContent("Line Key"), m_dialogueLineKeyIndex, m_linesKey);
                if (EditorGUI.EndChangeCheck())
                {
                    m_dialogueLineKey.stringValue = m_linesKey[m_dialogueLineKeyIndex];
                    m_content = GetContent();
                }
                if(m_dialogueLineKey.stringValue.Length > 0)
                {
                    Color _originalColor = GUI.color;
                    CharacterColorSettings _s;
                    for (int i = 0; i < DialoguesSettingsManager.DialogsSettings.CharactersColor.Count; i++)
                    {
                        _s = DialoguesSettingsManager.DialogsSettings.CharactersColor[i];
                        if (_s.CharacterIdentifier == m_dialogueLineKey.stringValue.Substring(0, 2))
                        {
                            GUI.color = _s.CharacterColor;
                            break;
                        }
                    }
                    EditorGUILayout.TextArea(m_content);
                    GUI.color = _originalColor;
                }
            }


            EditorGUILayout.LabelField(new GUIContent("Settings"), m_style);
            m_intialWaitingTime.floatValue = EditorGUILayout.Slider(new GUIContent("Initial Waiting Time", "Used only if the dialogue line has no Audio Asset linked to it."), m_intialWaitingTime.floatValue, .1f, 5.0f);
            EditorGUILayout.PropertyField(m_textDisplayer);
            EditorGUILayout.PropertyField(m_audioSource);
            EditorGUILayout.PropertyField(m_fontColor);

            EditorGUILayout.LabelField(new GUIContent("Events"), m_style);
            EditorGUILayout.PropertyField(m_event);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Changing Condition Events");
            if (GUILayout.Button("+", GUILayout.Width(Screen.width / 8)))
            {
                m_conditionEvents.InsertArrayElementAtIndex(0);
            }
            GUILayout.EndHorizontal();
            for (int j = 0; j < m_conditionEvents.arraySize; j++)
            {
                SerializedProperty _subProperty = m_conditionEvents.GetArrayElementAtIndex(j);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(12)))
                {
                    m_conditionEvents.DeleteArrayElementAtIndex(j);
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
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Get all line keys in the LineDescriptor
        /// </summary>
        private void InitLineKeys()
        {
            Script _luaScript = new Script();
            string _lineDescriptor = File.ReadAllText(Path.Combine(Dialogue.LineDescriptorPath, m_lineDescriptorName.stringValue + ".txt"));
            _luaScript.DoString(_lineDescriptor);
            List<DynValue> _dynValues = _luaScript.Globals.Values.Where(v => (v.Type == DataType.Table) && (v.Table.Get("ID").IsNotNil())).ToList();
            m_linesKey = _dynValues.Select(v => v.Table.Get("ID").String).ToArray();
            if (m_dialogueLineKey.stringValue != string.Empty && m_linesKey.Contains(m_dialogueLineKey.stringValue))
            {
                m_dialogueLineKeyIndex = m_linesKey.ToList().IndexOf(m_dialogueLineKey.stringValue);
                m_content = GetContent();
            }
            else m_dialogueLineKeyIndex = -1; 
        }

        /// <summary>
        /// Get the content of the current KeyIndex in the LineDescriptor
        /// </summary>
        /// <returns></returns>
        private string GetContent()
        {
            Script _luaScript = new Script();
            string _lineDescriptor = File.ReadAllText(Path.Combine(Dialogue.LineDescriptorPath, m_lineDescriptorName.stringValue + ".txt"));
            _luaScript.DoString(_lineDescriptor);
            List<DynValue> _dynValues = _luaScript.Globals.Values.Where(v => (v.Type == DataType.Table) && (v.Table.Get("ID").IsNotNil())).ToList();
            return _dynValues.Where(v => v.Table.Get("ID").String == m_dialogueLineKey.stringValue).Select(v => v.Table.Get(DialoguesSettingsManager.DialogsSettings.CurrentLocalisationKey).String).First();
        }
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            m_style = new GUIStyle();
            m_style.fontStyle = FontStyle.Bold;

            m_lineDescriptorName = serializedObject.FindProperty("m_lineDescriptorName");
            m_dialogueLineKey = serializedObject.FindProperty("m_dialogueLineKey");
            m_intialWaitingTime = serializedObject.FindProperty("m_intialWaitingTime");

            m_textDisplayer = serializedObject.FindProperty("m_textDisplayer");
            m_fontColor = serializedObject.FindProperty("m_fontColor");
            m_audioSource = serializedObject.FindProperty("m_audioSource");

            m_lineDescriptors = Directory.GetFiles(Dialogue.LineDescriptorPath, "*" + ".txt").Select(Path.GetFileNameWithoutExtension).ToArray();
            if(m_lineDescriptorName.stringValue != string.Empty)
            {
                m_lineDescriptorIndex = m_lineDescriptors.ToList().IndexOf(m_lineDescriptorName.stringValue);
                InitLineKeys();
            }

            m_event = serializedObject.FindProperty("m_event"); 
            m_conditionEvents = serializedObject.FindProperty("m_conditionEvents");
            string[] _conditionsLua = DialoguesSettingsManager.DialogsSettings.LuaConditions.Split('\n');
            m_conditions = new string[_conditionsLua.Length];
            for (int i = 0; i < _conditionsLua.Length; i++)
            {
                m_conditions[i] = _conditionsLua[i].Split('=')[0].Trim();
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

