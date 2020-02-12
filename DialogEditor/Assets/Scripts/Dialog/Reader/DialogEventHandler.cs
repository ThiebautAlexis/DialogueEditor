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
        if(m_dialogEvents.Any(e => e.ActivationKey == _key))
        {
            m_dialogEvents.Where(e => e.ActivationKey == _key).ToList().ForEach(e => e.DialogEventCallBack?.Invoke());
        }
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

    public UnityEvent DialogEventCallBack
    {
        get { return m_dialogEvent; }
    }
    public string ActivationKey
    {
        get { return m_activationKey; }
    }
}