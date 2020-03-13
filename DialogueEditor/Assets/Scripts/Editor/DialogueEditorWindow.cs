using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System;
using System.IO;
using System.Linq;

namespace DialogueEditor
{
    public class DialogueEditorWindow : EditorWindow
    {
        #region Fields and Properties

        #region GUIStyles
        private GUIStyle m_defaultNodeStyle = null;
        private GUIStyle m_defaultNodeStyleHighLighted = null;

        private GUIStyle m_conditionNodeStyle = null;
        private GUIStyle m_conditionNodeStyleHighLighted = null;

        private GUIStyle m_startingNodeStyle = null;
        private GUIStyle m_startingNodeStyleHighLighted = null;

        private GUIStyle m_defaultPointStyle = null;
        private GUIStyle m_conditionPointStyle = null;
        private GUIStyle m_startingPointStyle = null;

        private GUIContent m_dialogPartIcon = null;
        private GUIContent m_answerPartIcon = null;
        private GUIContent m_startingSetIcon = null;
        private GUIContent m_pointIcon = null;
        private GUIContent m_conditionIcon = null;
        #endregion

        #region Editor Fields
        private Vector2 m_drag;
        private Vector2 m_offset;
        private bool m_isCreationPopupOpen = false;
        private bool m_isSelectingPopupOpen = false;
        private int m_DialogIndex = -1;
        private float m_zoomScale = 1.0f;
        private Vector2 m_zoomCoordOrigine = Vector2.zero;

        private string m_dialogName = "";
        private string m_spreadsheetId = "";

        private Dialogue m_currentDialog = null;
        public Dialogue CurrentDialog
        {
            get { return m_currentDialog; }
            set
            {
                m_currentDialog = value;
                if (m_defaultNodeStyle == null) LoadStyles();
                CurrentDialog.InitEditorSettings(m_defaultNodeStyle, m_defaultNodeStyleHighLighted, m_conditionNodeStyle, m_conditionNodeStyleHighLighted, m_startingNodeStyle, m_startingNodeStyleHighLighted, m_defaultPointStyle, m_conditionPointStyle, m_startingPointStyle, m_dialogPartIcon, m_answerPartIcon, m_startingSetIcon, m_pointIcon, m_conditionIcon);
            }
        }

        private DialogueNode m_inSelectedNode = null;
        private DialogueLine m_outSelectedLine = null;
        private StarterPair m_outSelectedStart = null;
        private DialogueCondition m_outSelectedCondition = null;
        private bool m_outConditionValue = true;
        #endregion

        #endregion

        #region Methods

        #region Static Methods
        [MenuItem("Window/Dialogue Editor/Open Editor")]
        public static void OpenWindow()
        {
            DialogueEditorWindow _window = GetWindow<DialogueEditorWindow>();
            _window.titleContent = new GUIContent("Dialogue Editor");
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
            _dialogName = _dialogName.Replace(" ", string.Empty);
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DialogEditor"));
            }
            DownloadSpreadSheet(_spreadsheetId);
            SetNewDialog(new Dialogue(_dialogName, _spreadsheetId));
            string _jsonDialog = JsonUtility.ToJson(CurrentDialog, true);
            if (!Directory.Exists(Dialogue.DialogAssetPath))
            {
                Directory.CreateDirectory(Dialogue.DialogAssetPath);
            }
            File.WriteAllText(Path.Combine(Dialogue.DialogAssetPath, _dialogName + Dialogue.DialogAssetExtension), _jsonDialog);
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
                for (int j = 0; j < _variablesName.Length; j++)
                {
                    _luaData += $"{_variablesName[j]} = \"{_variables[j]}\";\n";
                }
                _luaData += "};\n";
                _lineDescriptor += _luaData;
            }
            if (!Directory.Exists(Dialogue.LineDescriptorPath))
                Directory.CreateDirectory(Dialogue.LineDescriptorPath);
            File.WriteAllText(Path.Combine(Dialogue.LineDescriptorPath, _spreadsheetID.GetHashCode().ToString() + Dialogue.LineDescriptorPostfixWithExtension), _lineDescriptor);
        }

        /// <summary>
        /// Draw a grid into the Editor Window
        /// </summary>
        /// <param name="_gridSpacing">Spacing</param>
        /// <param name="_gridOpacity">Opacity</param>
        /// <param name="_gridColor">Color</param>
        private void DrawGrid(float _gridSpacing, float _gridOpacity, Color _gridColor)
        {
            _gridSpacing *= m_zoomScale;
            if (_gridSpacing < 10)
            {
                _gridSpacing *= 25;
                _gridOpacity += .4f;
            }
            int widthDivs = Mathf.CeilToInt(position.width / _gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / _gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);

            m_offset += m_drag * 0.5f * m_zoomScale;
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
            if (GUILayout.Button("Create Dialog and Load SpreadSheet"))
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
            if (Directory.Exists(Path.Combine(Application.persistentDataPath, "Dialogs")))
            {
                string[] _names = Directory.GetFiles(Dialogue.DialogAssetPath, "*" + Dialogue.DialogAssetExtension).Select(Path.GetFileNameWithoutExtension).ToArray();
                m_DialogIndex = EditorGUILayout.Popup(m_DialogIndex, _names);
                if (GUILayout.Button("Open Dialog") && m_DialogIndex > -1)
                {
                    string _jsonFile = File.ReadAllText(Path.Combine(Dialogue.DialogAssetPath, _names[m_DialogIndex] + Dialogue.DialogAssetExtension));
                    SetNewDialog(JsonUtility.FromJson<Dialogue>(_jsonFile));
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
            m_drag = _delta / m_zoomScale;
            if (CurrentDialog != null) CurrentDialog.DragAll(_delta / m_zoomScale);
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

            m_defaultNodeStyleHighLighted = new GUIStyle();
            m_defaultNodeStyleHighLighted.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            m_defaultNodeStyleHighLighted.border = new RectOffset(12, 12, 12, 12);


            m_conditionNodeStyle = new GUIStyle();
            m_conditionNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
            m_conditionNodeStyle.border = new RectOffset(12, 12, 12, 12);

            m_conditionNodeStyleHighLighted = new GUIStyle();
            m_conditionNodeStyleHighLighted.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
            m_conditionNodeStyleHighLighted.border = new RectOffset(12, 12, 12, 12);

            m_startingNodeStyle = new GUIStyle();
            m_startingNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
            m_startingNodeStyle.border = new RectOffset(12, 12, 12, 12);

            m_startingNodeStyleHighLighted = new GUIStyle();
            m_startingNodeStyleHighLighted.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
            m_startingNodeStyleHighLighted.border = new RectOffset(12, 12, 12, 12);

            m_defaultPointStyle = new GUIStyle();
            m_defaultPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            m_defaultPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            m_defaultPointStyle.border = new RectOffset(0, 0, 0, 0);
            m_defaultPointStyle.alignment = TextAnchor.MiddleCenter;

            m_conditionPointStyle = new GUIStyle();
            m_conditionPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
            m_conditionPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
            m_conditionPointStyle.border = new RectOffset(0, 0, 0, 0);
            m_conditionPointStyle.alignment = TextAnchor.MiddleCenter;

            m_startingPointStyle = new GUIStyle();
            m_startingPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
            m_startingPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
            m_startingPointStyle.border = new RectOffset(0, 0, 0, 0);
            m_startingPointStyle.alignment = TextAnchor.MiddleCenter;

            m_dialogPartIcon = EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo");
            m_answerPartIcon = EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo");
            m_conditionIcon = EditorGUIUtility.IconContent("sv_icon_dot12_pix16_gizmo");
            m_startingSetIcon = EditorGUIUtility.IconContent("Favorite Icon");
            m_pointIcon = EditorGUIUtility.IconContent("PlayButton");
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
                    {
                        if (m_inSelectedNode == null && (m_outSelectedLine != null || m_outSelectedCondition != null))
                        {
                            m_outSelectedLine = null;
                            m_outSelectedCondition = null;
                            break;
                        }
                        ShowContextMenu(_e.mousePosition);
                    }
                    break;
                case EventType.MouseDrag:
                    if (_e.button == 0 && (CurrentDialog != null && !CurrentDialog.AnyPartIsSelected))
                    {
                        OnDrag(_e.delta);
                    }
                    break;
                case EventType.ScrollWheel:
                    if (m_zoomScale == .25f && _e.delta.y > 0) break;

                    Vector2 screenCoordsMousePos = Event.current.mousePosition;
                    Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
                    float _oldZoomScale = m_zoomScale;
                    m_zoomScale = Mathf.Clamp(m_zoomScale - (_e.delta.y * .05f), .25f, 1.0f);
                    m_zoomCoordOrigine += (zoomCoordsMousePos - m_zoomCoordOrigine) - (_oldZoomScale / m_zoomScale) * (zoomCoordsMousePos - m_zoomCoordOrigine);
                    Repaint();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Convert the screen coordinates into zommed Coordinates according to the zoom scale
        /// </summary>
        /// <param name="screenCoords">Screen coordinates to convert</param>
        /// <returns></returns>
        private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
        {
            return (screenCoords - position.GetTopLeft()) / m_zoomScale + m_zoomCoordOrigine;
        }

        /// <summary>
        /// Display the Context Menu to add node
        /// </summary>
        /// <param name="_mousePosition"></param>
        protected virtual void ShowContextMenu(Vector2 _mousePosition)
        {
            GenericMenu _genericMenu = new GenericMenu();
            _genericMenu.AddItem(new GUIContent("Create new Dialog"), false, () => m_isCreationPopupOpen = true);
            _genericMenu.AddItem(new GUIContent("Open Dialog"), false, () => m_isSelectingPopupOpen = true); ;
            if (CurrentDialog == null)
            {
                _genericMenu.AddDisabledItem(new GUIContent("Update SpreadSheet"));
                _genericMenu.AddSeparator("");
                _genericMenu.AddDisabledItem(new GUIContent("Add Dialog Node"));
                _genericMenu.AddDisabledItem(new GUIContent("Add Condition Node"));
            }
            else
            {
                _genericMenu.AddItem(new GUIContent("Update SpreadSheet"), false, () => DownloadSpreadSheet(CurrentDialog.SpreadSheetID));
                _genericMenu.AddSeparator("");
                _genericMenu.AddItem(new GUIContent("Add Dialog Node"), false, () => CurrentDialog.AddSet(_mousePosition));
                _genericMenu.AddItem(new GUIContent("Add Condition Node"), false, () => CurrentDialog.AddCondition(_mousePosition));
            }
            _genericMenu.ShowAsContext();
        }

        /// <summary>
        /// Set the current Dialog to the new one
        /// Add Initialize the editor settings
        /// </summary>
        /// <param name="_newDialog"></param>
        private void SetNewDialog(Dialogue _newDialog)
        {
            CurrentDialog = _newDialog;
        }

        /// <summary>
        /// Select the In point of the DialogSet 
        /// </summary>
        /// <param name="_node">Set to select</param>
        private void SelectInPart(DialogueNode _node)
        {
            m_inSelectedNode = _node;
            if (m_inSelectedNode != null && (m_outSelectedLine != null || m_outSelectedStart != null || m_outSelectedCondition != null))
            {
                LinkDialogSet();
            }
        }

        /// <summary>
        /// Select the out point of a dialog set
        /// </summary>
        /// <param name="_line">Content linked to the out point selected</param>
        private void SelectOutLine(DialogueLine _line)
        {
            if (m_outSelectedCondition != null) m_outSelectedCondition = null;
            if (m_outSelectedStart != null) m_outSelectedStart = null;
            m_outSelectedLine = _line;
            if (m_inSelectedNode != null && m_outSelectedLine != null)
            {
                LinkDialogSet();
            }
        }

        /// <summary>
        /// Select the out point of a condition
        /// </summary>
        /// <param name="_condition">Content linked to the out point selected</param>
        private void SelectOutCondition(DialogueCondition _condition, bool _valueCondition)
        {
            if (m_outSelectedLine != null) m_outSelectedLine = null;
            if (m_outSelectedStart != null) m_outSelectedStart = null;
            m_outSelectedCondition = _condition;
            m_outConditionValue = _valueCondition;
            if (m_inSelectedNode != null && m_outSelectedCondition != null)
            {
                LinkDialogSet();
            }
        }

        private void SelectOutStarter(StarterPair _startingPair)
        {
            if (m_outSelectedCondition != null) m_outSelectedCondition = null;
            if (m_outSelectedLine != null) m_outSelectedLine = null;

            m_outSelectedStart = _startingPair;

            if (m_inSelectedNode != null && m_outSelectedStart != null)
            {
                LinkDialogSet();
            }
        }

        /// <summary>
        /// Link two dialog sets
        /// </summary>
        private void LinkDialogSet()
        {
            if (m_outSelectedCondition != null)
            {
                if (m_outConditionValue)
                    m_outSelectedCondition.LinkedTokenTrue = m_inSelectedNode.NodeToken;
                else
                    m_outSelectedCondition.LinkedTokenFalse = m_inSelectedNode.NodeToken;
            }
            else if (m_outSelectedLine != null)
                m_outSelectedLine.LinkedToken = m_inSelectedNode.NodeToken;
            else if (m_outSelectedStart != null)
                m_outSelectedStart.LinkedToken = m_inSelectedNode.NodeToken;

            m_inSelectedNode = null;
            m_outSelectedLine = null;
            m_outSelectedCondition = null;
            m_outSelectedStart = null;
        }
#endif
        #endregion

        #region Unity Methods
        protected virtual void OnEnable()
        {
            LoadStyles();
            if (CurrentDialog != null) CurrentDialog.InitEditorSettings(m_defaultNodeStyle, m_defaultNodeStyleHighLighted, m_conditionNodeStyle, m_conditionNodeStyleHighLighted, m_startingNodeStyle, m_startingNodeStyleHighLighted, m_defaultPointStyle, m_conditionPointStyle, m_startingPointStyle, m_dialogPartIcon, m_answerPartIcon, m_startingSetIcon, m_pointIcon, m_conditionIcon);
        }
        protected virtual void OnGUI()
        {

            Color _originalColor = GUI.color;
            GUI.color = new Color(.12f, .12f, .12f);
            GUI.Box(new Rect(0, 0, maxSize.x, maxSize.y), "");
            GUI.color = _originalColor;

            DrawGrid(20, 0.2f, Color.white);
            DrawGrid(100, 0.4f, Color.white);

            ProcessEditorEvents(Event.current);

            ZoomAreaEditor.Begin(m_zoomScale, new Rect(0, 0, maxSize.x, maxSize.y));

            if (CurrentDialog != null) CurrentDialog.Draw(SelectOutLine, SelectInPart, SelectOutCondition, SelectOutStarter);
            if (m_inSelectedNode == null)
            {
                if (m_outSelectedLine != null && m_outSelectedLine.PointRect != Rect.zero)
                {
                    Handles.DrawBezier(m_outSelectedLine.PointRect.center, Event.current.mousePosition, m_outSelectedLine.PointRect.center + Vector2.right * 100.0f, Event.current.mousePosition + Vector2.left * 100.0f, Color.white, null, 2.0f);
                    GUI.changed = true;
                }
                else if (m_outSelectedCondition != null && m_outSelectedCondition.OutPointRectTrue != Rect.zero && m_outConditionValue)
                {
                    Handles.DrawBezier(m_outSelectedCondition.OutPointRectTrue.center, Event.current.mousePosition, m_outSelectedCondition.OutPointRectTrue.center + Vector2.right * 100.0f, Event.current.mousePosition + Vector2.left * 100.0f, Color.white, null, 2.0f);
                    GUI.changed = true;
                }
                else if (m_outSelectedCondition != null && m_outSelectedCondition.OutPointRectFalse != Rect.zero && !m_outConditionValue)
                {
                    Handles.DrawBezier(m_outSelectedCondition.OutPointRectFalse.center, Event.current.mousePosition, m_outSelectedCondition.OutPointRectFalse.center + Vector2.right * 100.0f, Event.current.mousePosition + Vector2.left * 100.0f, Color.white, null, 2.0f);
                    GUI.changed = true;
                }
                else if (m_outSelectedStart != null && m_outSelectedStart.OutPointRect != Rect.zero)
                {
                    Handles.DrawBezier(m_outSelectedStart.OutPointRect.center, Event.current.mousePosition, m_outSelectedStart.OutPointRect.center + Vector2.right * 100.0f, Event.current.mousePosition + Vector2.left * 100.0f, Color.white, null, 2.0f);
                    GUI.changed = true;
                }
            }

            ZoomAreaEditor.End();

            if (m_isCreationPopupOpen)
            {
                Rect windowRect = new Rect(0, 0, 500, 100);
                BeginWindows();
                windowRect = GUILayout.Window(1, windowRect, DrawCreatingPopup, "Create new Dialog");
                EndWindows();
            }
            if (m_isSelectingPopupOpen)
            {
                Rect windowRectBis = new Rect(0, 0, 500, 100);
                BeginWindows();
                windowRectBis = GUILayout.Window(1, windowRectBis, DrawSelectingPopup, "Open Dialog");
                EndWindows();
            }
            GUI.Box(new Rect(10, Screen.height, 100, 20), " Zoom = " + (m_zoomScale * 100).ToString("0") + "%");
            if (GUI.changed) Repaint();
        }
        #endregion

        #endregion
    }
}