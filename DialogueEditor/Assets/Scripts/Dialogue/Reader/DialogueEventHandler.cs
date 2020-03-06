using System.Linq; 
using UnityEngine;
using UnityEngine.Events;

namespace DialogueEditor
{
    public class DialogueEventHandler : MonoBehaviour
    {
        #region Fields and properties
        [SerializeField] private DialogueReader m_dialogueReader = null;
        [SerializeField] private DialogueEvent[] m_dialogueEvents = new DialogueEvent[] { };
        #endregion

        #region Methods

        #region Original Methods
        private void CallEvent(string _key)
        {
            m_dialogueEvents.ToList().ForEach(e => e.CallEvents(_key));
        }
        #endregion

        #region UnityMethods
        private void Start()
        {
            if (m_dialogueReader != null)
            {
                m_dialogueReader.OnDialogLineRead.AddListener(CallEvent);
            }
        }
        #endregion

        #endregion
    }

    [System.Serializable]
    public class DialogueEvent
    {
        [SerializeField] private UnityEvent m_dialogueEvent = null;
        [SerializeField] private string m_activationKey = string.Empty;
        [SerializeField] private ConditionEvent[] m_changedConditions = new ConditionEvent[] { };

        public UnityEvent DialogueEventCallBack
        {
            get { return m_dialogueEvent; }
        }
        public string ActivationKey
        {
            get { return m_activationKey; }
        }

        public void CallEvents(string _key)
        {
            if (m_activationKey == _key)
            {
                m_dialogueEvent?.Invoke();
                m_changedConditions.ToList().ForEach(e => DialoguesSettingsManager.SetConditionBoolValue(e.ConditionName, e.ConditionValue));
            }
        }
    }

    [System.Serializable]
    public class ConditionEvent
    {
        [SerializeField] private string m_conditionName = string.Empty;
        [SerializeField] private bool m_conditionValue = false;

        public string ConditionName { get { return m_conditionName; } }
        public bool ConditionValue { get { return m_conditionValue; } }
    }
}
