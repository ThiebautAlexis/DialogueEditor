using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueEditor
{
    public class DialogueAnswerHandler : MonoBehaviour
    {
        #region Fields and Properties
        [SerializeField] private GameObject m_buttonPrefab = null;
        #endregion

        #region Methods

        #region Original Methods
        /// <summary>
        /// Instanciate x buttons and set their texts to the dialog Line Content
        /// Add on each button the events to display the selected dialog line on the <paramref name="_owner"/> and then destroy this object
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_lines"></param>
        public void InitHandler(DialogueReader _owner, List<DialogueLine> _lines)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                GameObject _buttonObj = Instantiate(m_buttonPrefab, transform);
                DialogueLine _line = _lines[i];
                _buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = _owner.GetDialogueLineContent(_lines[i].Key, DialoguesSettingsManager.DialogsSettings.CurrentLocalisationKey);
                _buttonObj.GetComponent<Button>().onClick.AddListener(() => _owner.StartCoroutine(_owner.DisplayDialogueLine(_line)));
                _buttonObj.GetComponent<Button>().onClick.AddListener(() => Destroy(gameObject));
            }
        }
        #endregion

        #endregion
    }
}