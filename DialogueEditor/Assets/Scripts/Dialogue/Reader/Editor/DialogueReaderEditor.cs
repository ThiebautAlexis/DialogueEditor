using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace DialogueEditor
{
    [CustomEditor(typeof(DialogueReader))]
    public class DialogueReaderEditor : Editor
    {
        private SerializedProperty m_dialogName;
        private SerializedProperty m_textDisplayer;
        private SerializedProperty m_font;
        private SerializedProperty m_fontSize;
        private SerializedProperty m_fontColor;
        private SerializedProperty m_audioSource;

        private string[] m_dialogsName;
        private int m_currentIndex = -1;
        private GUIStyle m_style;

        private void OnEnable()
        {
            m_style = new GUIStyle();
            m_style.fontStyle = FontStyle.Bold;
            m_dialogName = serializedObject.FindProperty("m_dialogName");
            m_textDisplayer = serializedObject.FindProperty("m_textDisplayer");
            m_font = serializedObject.FindProperty("m_font");
            m_fontSize = serializedObject.FindProperty("m_fontSize");
            m_fontColor = serializedObject.FindProperty("m_fontColor");
            m_audioSource = serializedObject.FindProperty("m_audioSource");
            m_dialogsName = Directory.GetFiles(Dialogue.DialogAssetPath, "*" + Dialogue.DialogAssetExtension).Select(Path.GetFileNameWithoutExtension).ToArray();

            if (m_dialogName.stringValue != string.Empty)
                m_currentIndex = m_dialogsName.ToList().IndexOf(m_dialogName.stringValue);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(new GUIContent("Dialogue"), m_style);
            EditorGUI.BeginChangeCheck();
            m_currentIndex = EditorGUILayout.Popup("Displayed Dialogue", m_currentIndex, m_dialogsName);
            if (EditorGUI.EndChangeCheck())
            {
                m_dialogName.stringValue = m_dialogsName[m_currentIndex];
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Text Mesh Settings"), m_style);
            EditorGUILayout.PropertyField(m_textDisplayer, new GUIContent("Text Displayer"));
            EditorGUILayout.PropertyField(m_font, new GUIContent("Font"));
            EditorGUILayout.PropertyField(m_fontSize, new GUIContent("Font size"));
            EditorGUILayout.PropertyField(m_fontColor, new GUIContent("Font Color"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Audio Settings"), m_style);
            EditorGUILayout.PropertyField(m_audioSource, new GUIContent("Audio Source"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}