using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic; 

public class DialogSettingsEditor : EditorWindow
{
    public static string ConditionsPath{ get { return Path.Combine(Application.dataPath, "DialogsDatas", "Conditions"); } }
    public static string ConditionsFileName { get { return "ConditionsTemplate"; } }
    public static string ConditionsFileExtension { get { return ".txt";  } }
    public static string ConditionsFilePath { get { return Path.Combine(ConditionsPath, ConditionsFileName + ConditionsFileExtension); } }

    private List<string> m_conditions = null;
    private string m_addedCondition = ""; 

    [MenuItem("Window/Dialog Editor/Edit Settings")]
    public static void OpenWindow()
    {
        DialogSettingsEditor _window = GetWindow<DialogSettingsEditor>();
        _window.titleContent = new GUIContent("Dialog Editor");
    }

    private void OnEnable()
    {
        if (!Directory.Exists(ConditionsPath))
            Directory.CreateDirectory(ConditionsPath);
        if (!File.Exists(ConditionsFilePath))
            File.WriteAllText(ConditionsFilePath, "");

        string[] _conditions = File.ReadAllLines(ConditionsFilePath);
        m_conditions = new List<string>(); 
        for (int i = 0; i < _conditions.Length; i++)
        {
            m_conditions.Add(_conditions[i].Split('=')[0].Trim());
        }
    }

    private void OnGUI()
    {
        for (int i = 0; i < m_conditions.Count; i++)
        {
            GUILayout.Label(m_conditions[i]); 
        }
        GUILayout.BeginHorizontal(); 
        m_addedCondition = GUILayout.TextField(m_addedCondition);
        if (GUILayout.Button("Add Condition to database") && m_addedCondition.Trim() != string.Empty)
        {
            m_conditions.Add(m_addedCondition);
            m_addedCondition = ""; 
        }
        GUILayout.EndHorizontal(); 
    }
}
