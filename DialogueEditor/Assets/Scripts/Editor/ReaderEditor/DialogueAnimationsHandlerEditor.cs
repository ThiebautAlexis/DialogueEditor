using System.IO;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter;

namespace DialogueEditor
{
    [CustomEditor(typeof(DialogueAnimationsHandler))]
    public class DialogueAnimationsHandlerEditor : Editor
    {

        #region Fields and Properties 
        [SerializeField] private SerializedProperty m_dialogueReader = null;
        [SerializeField] private SerializedProperty m_dialogueAnimations = null;


        private string[] m_keys = new string[] { };
        private List<int> m_currentAnimationIndexes = new List<int>();
        private List<bool> m_displayAnimationsAtIndex = new List<bool>();
        #endregion


        #region Methods

        #region Original Methods
        private void DrawEditor()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_dialogueReader, new GUIContent("Dialog Reader"));
            if (EditorGUI.EndChangeCheck())
            {
                InitDialogueReaderKeys();
            }
            if (m_displayAnimationsAtIndex.Count != m_dialogueAnimations.arraySize || m_currentAnimationIndexes.Count != m_dialogueAnimations.arraySize)
            {
                InitDialogueReaderKeys(); 
            }
            if (m_dialogueReader.objectReferenceValue != null)
            {
                Color _originalBackgroundColor = GUI.backgroundColor;

                GUILayout.Space(11);
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 3), Color.blue); 
                GUILayout.Space(11);

                if(m_keys != null)
                {
                    SerializedProperty _prop;
                    SerializedProperty _animationProperty; 
                    for (int i = 0; i < m_dialogueAnimations.arraySize; i++)
                    {
                        _prop = m_dialogueAnimations.GetArrayElementAtIndex(i);
                        m_displayAnimationsAtIndex[i] = EditorGUILayout.Foldout(m_displayAnimationsAtIndex[i], "Dialogue Animations called at " + _prop.FindPropertyRelative("m_activationKey").stringValue); 
                        if(m_displayAnimationsAtIndex[i])
                        {
                            EditorGUI.BeginChangeCheck();
                            m_currentAnimationIndexes[i] = EditorGUILayout.Popup("Activation Key", m_currentAnimationIndexes[i], m_keys);
                            if (EditorGUI.EndChangeCheck())
                            {
                                _prop.FindPropertyRelative("m_activationKey").stringValue = m_keys[m_currentAnimationIndexes[i]];
                            }
                            GUILayout.Space(10);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Triggered Animations");
                            _animationProperty = _prop.FindPropertyRelative("m_animationsTrigger");
                            if(GUILayout.Button(new GUIContent("+"), GUILayout.Width(Screen.width/8)))
                            {
                                _animationProperty.InsertArrayElementAtIndex(_animationProperty.arraySize);
                            }
                            GUILayout.EndHorizontal(); 
                            for (int j = 0; j < _animationProperty.arraySize; j++)
                            {
                                SerializedProperty _subProp = _animationProperty.GetArrayElementAtIndex(j);
                                GUILayout.BeginHorizontal(); 
                                if (GUILayout.Button(new GUIContent("-"), GUILayout.Width(15), GUILayout.Height(12)))
                                {
                                    _animationProperty.DeleteArrayElementAtIndex(j);
                                    continue;
                                }
                                EditorGUILayout.PropertyField(_subProp.FindPropertyRelative("m_triggeredAnimator"), new GUIContent("Animator"));
                                GUILayout.EndHorizontal(); 
                                _subProp.FindPropertyRelative("m_triggerName").stringValue = EditorGUILayout.TextField(new GUIContent("Trigger Name", "Name of the Trigger used to start the animation"),_subProp.FindPropertyRelative("m_triggerName").stringValue);

                                GUILayout.Space(5);
                            }

                            GUI.backgroundColor = Color.red;
                            if(GUILayout.Button("Remove Animations"))
                            {
                                m_dialogueAnimations.DeleteArrayElementAtIndex(i);
                                m_currentAnimationIndexes.RemoveAt(i);
                                m_displayAnimationsAtIndex.RemoveAt(i);
                            }
                            GUI.backgroundColor = _originalBackgroundColor;
                            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 3), Color.red);
                        }
                    }
                    GUILayout.Space(5);
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Add Animations"))
                    {
                        m_dialogueAnimations.InsertArrayElementAtIndex(m_dialogueAnimations.arraySize);
                        m_currentAnimationIndexes.Add(-1);
                        m_displayAnimationsAtIndex.Add(false); 
                    }
                    GUI.backgroundColor = _originalBackgroundColor;
                }
            }
            serializedObject.ApplyModifiedProperties(); 
        }


        private void InitDialogueReaderKeys()
        {
            if (m_dialogueReader == null) return; 
            m_keys = new string[] { };
            m_currentAnimationIndexes = new List<int>();
            m_displayAnimationsAtIndex = new List<bool>(); 
            string _spreadsheetID = JsonUtility.FromJson<Dialogue>(File.ReadAllText(Path.Combine(Dialogue.DialogAssetPath, (m_dialogueReader.objectReferenceValue as DialogueReader).DialogName + Dialogue.DialogAssetExtension))).SpreadSheetID;
            string _lineDescriptor = File.ReadAllText(Path.Combine(Dialogue.LineDescriptorPath, _spreadsheetID.GetHashCode().ToString() + Dialogue.LineDescriptorPostfixWithExtension));
            LoadKeys(_lineDescriptor);
            m_currentAnimationIndexes = new List<int>();
            if (m_dialogueAnimations.arraySize != 0)
            {
                for (int i = 0; i < m_dialogueAnimations.arraySize; i++)
                {
                    if (m_dialogueAnimations.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue != string.Empty)
                        m_currentAnimationIndexes.Add(m_keys.ToList().IndexOf(m_dialogueAnimations.GetArrayElementAtIndex(i).FindPropertyRelative("m_activationKey").stringValue));
                    else m_currentAnimationIndexes.Add(-1);
                    m_displayAnimationsAtIndex.Add(false); 
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

        #region Unity Methods
        private void OnEnable()
        {
            m_dialogueReader = serializedObject.FindProperty("m_dialogueReader");
            m_dialogueAnimations = serializedObject.FindProperty("m_dialogueAnimations");
            m_displayAnimationsAtIndex = new List<bool>(m_dialogueAnimations.arraySize) { false };
            if(m_dialogueReader.objectReferenceValue != null)
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

