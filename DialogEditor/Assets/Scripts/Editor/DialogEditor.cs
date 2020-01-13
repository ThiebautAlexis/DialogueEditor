using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System;
using System.IO;
using System.Linq; 

public class DialogEditor : EditorWindow
{
    #region Fields and Properties
    #region GUIStyles
    private GUIStyle m_defaultNodeStyle = null;
    private GUIStyle m_pointStyle = null;
    private GUIContent m_dialogPartIcon = null;
    private GUIContent m_answerPartIcon = null;
    private GUIContent m_startingSetIcon = null; 
    private GUIContent m_pointIcon = null; 
    #endregion

    #region Editor Fields
    private Vector2 m_drag;
    private Vector2 m_offset;
    private bool m_isCreationPopupOpen = false;
    private bool m_isSelectingPopupOpen = false;
    private int m_DialogIndex = -1; 

    private string m_dialogName = ""; 
    private string m_spreadsheetId = "";

    private Dialog m_currentDialog = null; 
    public Dialog CurrentDialog
    {
        get { return m_currentDialog;  }
        set
        {
            m_currentDialog = value;
            if (m_defaultNodeStyle == null) LoadStyles(); 
            m_currentDialog.InitEditorSettings(m_defaultNodeStyle, m_pointStyle, m_dialogPartIcon, m_answerPartIcon, m_startingSetIcon, m_pointIcon); 
        }
    }

    private DialogSet m_inSelectedPart = null;
    private DialogLine m_outSelectedContent = null; 
    #endregion

    #endregion

    #region Methods

    #region Static Methods
    [MenuItem("Window/Dialog Editor")]
    public static void OpenWindow()
    {
        DialogEditor _window = GetWindow<DialogEditor>();
        _window.titleContent = new GUIContent("Dialog Editor");
    }
    #endregion

    #region Original Methods

    #if UNITY_EDITOR
    /// <summary>
    /// Create a Dialog and save it as a Json file. Also download the spreadsheet
    /// </summary>
    /// <param name="_dialogName">Name of the Dialog</param>
    /// <param name="_spreadsheetId">Id of the spreadsheet</param>
    private void CreateDialog(string _dialogName, string _spreadsheetId)
    {
        _dialogName = _dialogName.Replace(" " , string.Empty);
        if(!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor")))
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor")); 
        }
        DownloadSpreadSheet(_spreadsheetId); 
        SetNewDialog(new Dialog(_dialogName, _spreadsheetId));
        string _jsonDialog = JsonUtility.ToJson(CurrentDialog);
        if (!Directory.Exists(Dialog.DialogAssetPath))
        {
            Directory.CreateDirectory(Dialog.DialogAssetPath);
        }
        File.WriteAllText(Path.Combine(Dialog.DialogAssetPath, _dialogName + Dialog.DialogAssetExtension), _jsonDialog);
        titleContent = new GUIContent(_dialogName); 
    }

    /// <summary>
    /// Download the spreadsheet with the selected ID 
    /// </summary>
    /// <param name="_spreadsheetID">ID of the spreadsheet</param>
    private void DownloadSpreadSheet(string _spreadsheetID)
    {
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor")))
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor")); 
        WebClient _get = new WebClient();
        _get.DownloadFile(new Uri($"https://docs.google.com/spreadsheets/d/{_spreadsheetID}/export?format=tsv"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor", $"{_spreadsheetID}.tsv"));
        // Create the Line Descriptor

        string[] _text = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor", $"{_spreadsheetID}.tsv"));
        string[] _variablesName = _text[0].Split(Convert.ToChar(9));
        string _lineDescriptor = "";
        string[] _variables;
        string _luaData; 
        for (int i = 1; i < _text.Length; i++)
        {
            _variables = _text[i].Split(Convert.ToChar(9));
            _luaData = $"{_variables[0]} = {{\n";
            for(int j = 0; j < _variablesName.Length; j++)
            {
                _luaData += $"{_variablesName[j]} = \"{_variables[j]}\";\n";
            }
            _luaData += "};\n"; 
            _lineDescriptor += _luaData;
        }
        if (!Directory.Exists(Dialog.LineDescriptorPath))
            Directory.CreateDirectory(Dialog.LineDescriptorPath);
        File.WriteAllText(Path.Combine(Dialog.LineDescriptorPath, m_currentDialog.DialogName + ".lua"), _lineDescriptor); 
    }   

    /// <summary>
    /// Draw a grid into the Editor Window
    /// </summary>
    /// <param name="_gridSpacing">Spacing</param>
    /// <param name="_gridOpacity">Opacity</param>
    /// <param name="_gridColor">Color</param>
    private void DrawGrid(float _gridSpacing, float _gridOpacity, Color _gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / _gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / _gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);

        m_offset += m_drag * 0.5f;
        Vector3 newOffset = new Vector3(m_offset.x % _gridSpacing, m_offset.y % _gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(_gridSpacing * i, -_gridSpacing, 0) + newOffset, new Vector3(_gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-_gridSpacing, _gridSpacing * j, 0) + newOffset, new Vector3(position.width, _gridSpacing * j, 0f) + newOffset);
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    /// <summary>
    /// Draw the popup window to create a new Dialog
    /// </summary>
    /// <param name="_unusedWindowID"></param>
    private void DrawCreatingPopup(int _unusedWindowID)
    {
        m_isSelectingPopupOpen = false;
        GUILayout.Label("Create new Dialog");
        m_dialogName = EditorGUILayout.TextField("Dialog Name: ", m_dialogName);
        m_spreadsheetId = EditorGUILayout.TextField("Spreadsheet ID:", m_spreadsheetId); 
        if(GUILayout.Button("Create Dialog and Load SpreadSheet"))
        {
            CreateDialog(m_dialogName, m_spreadsheetId); 
            m_isCreationPopupOpen = false;
        }
        if (GUILayout.Button("Cancel"))
        {
            m_isCreationPopupOpen = false;
        }
    }

    /// <summary>
    /// Draw the popup to open an existing Dialog
    /// </summary>
    /// <param name="_unusedWindowID"></param>
    private void DrawSelectingPopup(int _unusedWindowID)
    {
        m_isCreationPopupOpen = false;
        GUILayout.Label("Open Dialog");
        if(Directory.Exists(Path.Combine(Application.persistentDataPath, "Dialogs")))
        {
            string[] _names = Directory.GetFiles(Dialog.DialogAssetPath).Select(Path.GetFileNameWithoutExtension).ToArray();
            m_DialogIndex = EditorGUILayout.Popup(m_DialogIndex, _names); 
            if(GUILayout.Button("Open Dialog") && m_DialogIndex > -1)
            {
                string _jsonFile = File.ReadAllText(Path.Combine(Dialog.DialogAssetPath, _names[m_DialogIndex] + Dialog.DialogAssetExtension));
                SetNewDialog(JsonUtility.FromJson<Dialog>(_jsonFile));
                m_isSelectingPopupOpen = false;
                titleContent = new GUIContent(_names[m_DialogIndex]);
                m_DialogIndex = -1;
            }
        }
        if (GUILayout.Button("Close"))
        {
            m_isSelectingPopupOpen = false;
        }
    }

    /// <summary>
    /// Drag the editor
    /// </summary>
    /// <param name="_delta"></param>
    private void OnDrag(Vector2 _delta)
    {
        m_drag = _delta;
        if (CurrentDialog != null) CurrentDialog.DragAll(_delta); 
        GUI.changed = true;
    }

    /// <summary>
    /// Load of styles and Icons of the Dialogs Editor
    /// </summary>
    private void LoadStyles()
    {
        m_defaultNodeStyle = new GUIStyle();
        m_defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        m_defaultNodeStyle.border = new RectOffset(12, 12, 12, 12);

        m_pointStyle = new GUIStyle();
        m_pointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        m_pointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        m_pointStyle.border = new RectOffset(4, 4, 12, 12);

        m_dialogPartIcon = EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo");
        m_answerPartIcon = EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo");
        m_startingSetIcon = EditorGUIUtility.IconContent("Favorite Icon");
        m_pointIcon = EditorGUIUtility.IconContent("PlayButton"); 
        // StepButton ou PlayButton
    }

    /// <summary>
    /// Process the EditorEvents according to the mouse button pressed
    /// </summary>
    /// <param name="_e">Current Event</param>
    private void ProcessEditorEvents(Event _e)
    {
        m_drag = Vector2.zero;
        switch (_e.type)
        {
            case EventType.MouseDown:
                if (_e.button == 1)
                    ShowContextMenu(_e.mousePosition);
                break;
            case EventType.MouseDrag:
                if (_e.button == 0 && (CurrentDialog != null && !CurrentDialog.AnyPartIsSelected))
                {
                    OnDrag(_e.delta);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Display the Context Menu to add node
    /// </summary>
    /// <param name="_mousePosition"></param>
    protected virtual void ShowContextMenu(Vector2 _mousePosition)
    {
        GenericMenu _genericMenu = new GenericMenu();
        _genericMenu.AddItem(new GUIContent("Create new Dialog"), false, () => m_isCreationPopupOpen = true) ;
        _genericMenu.AddItem(new GUIContent("Open Dialog"), false, () => m_isSelectingPopupOpen = true);  ;
        if(CurrentDialog == null)
        {
            _genericMenu.AddDisabledItem(new GUIContent("Update SpreadSheet"));
            _genericMenu.AddSeparator(""); 
            _genericMenu.AddDisabledItem(new GUIContent("Add Dialog Node")); 
        }
        else
        {
            _genericMenu.AddItem(new GUIContent("Update SpreadSheet"), false, () => DownloadSpreadSheet(CurrentDialog.SpreadSheetID));
            _genericMenu.AddSeparator("");
            _genericMenu.AddItem(new GUIContent("Add Dialog Node"), false, () => CurrentDialog.AddPart(_mousePosition));
        }
        _genericMenu.ShowAsContext();
    }

    /// <summary>
    /// Set the current Dialog to the new one
    /// Add Initialize the editor settings
    /// </summary>
    /// <param name="_newDialog"></param>
    private void SetNewDialog(Dialog _newDialog)
    {
        CurrentDialog = _newDialog;
    }

    private void SelectInPart(DialogSet _part)
    {
        m_inSelectedPart = _part; 
        if(m_inSelectedPart != null && m_outSelectedContent != null)
        {
            LinkDialogSet(); 
        }
    }

    private void SelectOutContent(DialogLine _content)
    {
        m_outSelectedContent = _content;
        if (m_inSelectedPart != null && m_outSelectedContent != null)
        {
            LinkDialogSet(); 
        }
    }

    private void LinkDialogSet()
    {
        m_outSelectedContent.LinkedToken = m_inSelectedPart.PartToken; 
        m_inSelectedPart = null;
        m_outSelectedContent = null;
    }
    #endif
    #endregion

    #region Unity Methods
    protected virtual void OnEnable()
    {
        LoadStyles(); 
        if (CurrentDialog != null) CurrentDialog.InitEditorSettings(m_defaultNodeStyle, m_pointStyle, m_dialogPartIcon, m_answerPartIcon, m_startingSetIcon, m_pointIcon);
    } 
    protected virtual void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.black);
        DrawGrid(100, 0.4f, Color.black);

        ProcessEditorEvents(Event.current);
        if(CurrentDialog != null) CurrentDialog.Draw(SelectOutContent, SelectInPart);
        if(m_outSelectedContent != null && m_inSelectedPart == null)
        {
            Handles.DrawBezier(m_outSelectedContent.PointRect.center, Event.current.mousePosition, m_outSelectedContent.PointRect.center + Vector2.right * 100.0f, Event.current.mousePosition + Vector2.left * 100.0f, Color.black, null, 2.0f);
            GUI.changed = true; 
        }

        if (m_isCreationPopupOpen)
        {
            Rect windowRect = new Rect(0, 0, 500, 100);
            BeginWindows();
            windowRect = GUILayout.Window(1, windowRect, DrawCreatingPopup, "Create new Dialog");
            EndWindows();
        }
        if(m_isSelectingPopupOpen)
        {
            Rect windowRectBis = new Rect(0, 0, 500, 100);
            BeginWindows();
            windowRectBis = GUILayout.Window(1, windowRectBis, DrawSelectingPopup, "Open Dialog");
            EndWindows();
        }
        if (GUI.changed) Repaint(); 
    }
    #endregion

    #endregion
}