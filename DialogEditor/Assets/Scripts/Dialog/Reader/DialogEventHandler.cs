using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.Events;
public class DialogEventHandler : MonoBehaviour
{
    #region Fields and properties
    [SerializeField] private DialogReader m_dialogReader = null;
    [SerializeField] private DialogEvent[] m_dialogEvents = new DialogEvent[] { }; 
    #endregion

    #region Methods

    #region Original Methods
    private void CallEvent(string _key)
    {
        m_dialogEvents.ToList().ForEach(e => e.CallEvents(_key));
    }
    #endregion

    #region UnityMethods
    private void Start()
    {
        if(m_dialogReader != null)
        {
            m_dialogReader.OnDialogLineRead.AddListener(CallEvent);
        }
    }
    #endregion

    #endregion
}

[System.Serializable]
public class DialogEvent
{
    [SerializeField] private UnityEvent m_dialogEvent = null;
    [SerializeField] private string m_activationKey = string.Empty;
    [SerializeField] private ConditionEvent[] m_changedConditions = new ConditionEvent[] { };   

    public UnityEvent DialogEventCallBack
    {
        get { return m_dialogEvent; }
    }
    public string ActivationKey
    {
        get { return m_activationKey; }
    }

    public void CallEvents(string _key)
    {
        if(m_activationKey == _key)
        {
            m_dialogEvent?.Invoke();
            m_changedConditions.ToList().ForEach(e => DialogsSettingsManager.SetConditionBoolValue(e.ConditionName, e.ConditionValue));
        }
    }
}

[System.Serializable]
public class ConditionEvent
{
    [SerializeField] private string m_conditionName = string.Empty;
    [SerializeField] private bool m_conditionValue = false; 

    public string ConditionName { get { return m_conditionName;  } }
    public bool ConditionValue { get { return m_conditionValue; } }
}