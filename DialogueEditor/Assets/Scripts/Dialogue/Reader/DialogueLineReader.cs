using System.Collections;
using System.Linq; 
using UnityEngine;
using TMPro;
using MoonSharp.Interpreter;

namespace DialogueEditor
{
    public class DialogueLineReader : MonoBehaviour
    {
        #region Fields and Properties
        [SerializeField] private string m_lineDescriptorName = string.Empty;
        [SerializeField] private string m_dialogueLineKey = string.Empty;
        [SerializeField] private float m_intialWaitingTime = 0;

        [SerializeField] private TMP_Text m_textDisplayer = null;
        [SerializeField] private Color m_fontColor = Color.black;
        [SerializeField] private AudioSource m_audioSource = null;

        [SerializeField] private UnityEngine.Events.UnityEvent m_event = new UnityEngine.Events.UnityEvent();
        [SerializeField] private ConditionEvent[] m_conditionEvents = new ConditionEvent[] { }; 

        private Script m_lineDescriptor = null; 
        #endregion

        #region Methods

        #region Original Methods
        /// <summary>
        /// Once the line descritpros are loaded, get its linedescriptor
        /// </summary>
        private void OnLineDescriptorLoaded()
        {
            if (DialogueAssetsManager.LineDescriptorsTextAsset.Any(a => a.name == m_lineDescriptorName))
            {
                m_lineDescriptor = new Script();
                m_lineDescriptor.DoString(DialogueAssetsManager.LineDescriptorsTextAsset.Where(a => a.name == m_lineDescriptorName).First().ToString());
            }
        }

        /// <summary>
        /// Play the line once with the associated color and Audio Clip
        /// Then clear the text
        /// </summary>
        /// <returns></returns>
        public IEnumerator PlayLine()
        {
            if (!m_textDisplayer) yield break;
            // Change the color of the font if needed
            if (!DialoguesSettingsManager.DialogsSettings.OverrideCharacterColor)
            {
                if (DialoguesSettingsManager.DialogsSettings.CharactersColor.Any(c => c.CharacterIdentifier == m_dialogueLineKey.Substring(0, 2)))
                    m_textDisplayer.color = DialoguesSettingsManager.DialogsSettings.CharactersColor.Where(c => c.CharacterIdentifier == m_dialogueLineKey.Substring(0, 2)).Select(c => c.CharacterColor).FirstOrDefault();
                else m_textDisplayer.color = m_fontColor;
            }
            else
                m_textDisplayer.color = m_fontColor;
            // Change the text of the text displayer
            m_textDisplayer.text = GetDialogueLineContent(m_dialogueLineKey, DialoguesSettingsManager.DialogsSettings.CurrentLocalisationKey);
            // If there is an audiosource and the AudioClip Exists in the DialogsAssetsManager, play the audioclip in OneShot
            if (m_audioSource != null && DialogueAssetsManager.DialogLinesAudioClips.ContainsKey(m_dialogueLineKey + "_" + DialoguesSettingsManager.DialogsSettings.CurrentAudioLocalisationKey))
            {
                AudioClip _c = DialogueAssetsManager.DialogLinesAudioClips[m_dialogueLineKey + "_" + DialoguesSettingsManager.DialogsSettings.CurrentAudioLocalisationKey];

                m_audioSource.PlayOneShot(_c);
                yield return new WaitForSeconds(_c.length + .05f);
            }
            else
            {
                yield return new WaitForSeconds(m_intialWaitingTime);
            }
            m_event?.Invoke();
            m_conditionEvents.ToList().ForEach(e => DialoguesSettingsManager.SetConditionBoolValue(e.ConditionName, e.ConditionValue));
            m_textDisplayer.text = string.Empty;
        }

        /// <summary>
        /// Get the Content of the Dialog Line according to the localisationKey Selected
        /// </summary>
        /// <param name="_dialogLineID">ID of the Dialog Line</param>
        /// <param name="_localisationKey">Localisation Key to use</param>
        /// <returns></returns>
        public string GetDialogueLineContent(string _dialogLineID, string _localisationKey)
        {
            if (_dialogLineID == string.Empty) return string.Empty;
            DynValue _content = m_lineDescriptor.Globals.Get(_dialogLineID).Table.Get(_localisationKey);
            return _content.String;
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            DialogueAssetsManager.LineDescriptorsLoadedCallBack += OnLineDescriptorLoaded;
        }
        #endregion 

        #endregion
    }
}

