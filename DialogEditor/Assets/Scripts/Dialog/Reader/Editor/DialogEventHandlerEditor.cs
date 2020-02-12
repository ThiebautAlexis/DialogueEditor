using System.Collections.Generic;
using System.Linq;
using System.IO; 
using UnityEngine;
using UnityEditor;
using MoonSharp.Interpreter; 

[CustomEditor(typeof(DialogEventHandler))]
public class DialogEventHandlerEditor : Editor
{
    #region Fields and Properties
    [SerializeField] private SerializedProperty m_dialogReader = null;
    [SerializeField] private SerializedProperty m_dialogEvents = null;

    private string[] m_keys = new string[] { };
    private List<int> m_currentIndexes = new List<int>();
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

            GUILayout.Space(11);
            GUI.color = Color.blue;
            GUILayout.Box("", GUILayout.Height(3), GUILayout.Width(Screen.width - 20));
            GUI.color = _originalColor;
            GUILayout.Space(11);

            if (m_keys != null)
            {
                SerializedProperty _prop;
                for (int i = 0; i < m_dialogEvents.arraySize; i++)
                {
                    _prop = m_dialogEvents.GetArrayElementAtIndex(i); 
                    EditorGUI.BeginChangeCheck();
                    m_currentIndexes[i] = EditorGUILayout.Popup("Activation Key", m_currentIndexes[i], m_keys);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _prop.FindPropertyRelative("m_activationKey").stringValue = m_keys[m_currentIndexes[i]];
                    }

                    EditorGUILayout.PropertyField(_prop.FindPropertyRelative("m_dialogEvent"), new GUIContent("Dialog Event"));
                    if (GUILayout.Button("Remove DialogEvent"))
                    {
                        m_dialogEvents.DeleteArrayElementAtIndex(i);
                        m_currentIndexes.RemoveAt(i); 
                    }
                    GUI.color = Color.red; 
                    GUILayout.Box("", GUILayout.Height(3), GUILayout.Width(Screen.width - 20));
                    GUI.color = _originalColor; 
                }
            }
            if(GUILayout.Button("Add DialogEvent"))
            {
                m_dialogEvents.InsertArrayElementAtIndex(m_dialogEvents.arraySize);
                m_currentIndexes.Add(-1); 
            }
            
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
        string _spreadsheetID = JsonUtility.FromJson<Dialog>(File.ReadAllText(Path.Combine(Dialog.DialogAssetPath, (m_dialogReader.objectReferenceValue as DialogReader).DialogName + Dialog.DialogAssetExtension))).SpreadSheetID;
        string _lineDescriptor = File.ReadAllText(Path.Combine(Dialog.LineDescriptorPath, _spreadsheetID.GetHashCode().ToString() + Dialog.LineDescriptorPostfixWithExtension));
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
