using System.Linq; 
using UnityEngine;

namespace DialogueEditor
{
    public class DialogueAnimationsHandler : MonoBehaviour
    {
        #region Fields and Properties
        [SerializeField] private DialogueReader m_dialogueReader = null;
        [SerializeField] private DialogueAnimations[] m_dialogueAnimations = new DialogueAnimations[] { }; 
        #endregion

        #region Methods

        #region Original Methods
        /// <summary>
        /// Call all animations according to the read Dialogue Line 
        /// </summary>
        /// <param name="_key">Key of the read DialogueLine</param>
        private void CallAnimations(string _key)
        {
            m_dialogueAnimations.ToList().ForEach(a => a.CallAnimations(_key)); 
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Add Listener to the Dialogue Reader
        /// </summary>
        private void Start()
        {
            if(m_dialogueReader != null)
            {
                m_dialogueReader.OnDialogLineRead.AddListener(CallAnimations); 
            }
        }
        #endregion

        #endregion
    }

    [System.Serializable]
    public class DialogueAnimations
    {
        [SerializeField] private string m_activationKey = string.Empty;
        [SerializeField] private AnimatorTriggerPair[] m_animationsTrigger = new AnimatorTriggerPair[] { };

        /// <summary>
        /// Check if the activation key is equal to the Read Dialogue Line Key 
        /// </summary>
        /// <param name="_key">Read Dialogue Line Key</param>
        public void CallAnimations(string _key)
        {
            if (m_activationKey == _key)
                m_animationsTrigger.ToList().ForEach(a => a.TriggerAnimation()); 
        }
    }

    [System.Serializable]
    public class AnimatorTriggerPair
    {
        [SerializeField] private Animator m_triggeredAnimator = null;
        [SerializeField] private string m_triggerName = string.Empty; 

        /// <summary>
        /// Trigger the animator with the triggerName Trigger 
        /// </summary>
        public void TriggerAnimation()
        {
            if (m_triggeredAnimator == null || m_triggerName == string.Empty)
                return;
            m_triggeredAnimator.SetTrigger(m_triggerName); 
        }
    }
}

