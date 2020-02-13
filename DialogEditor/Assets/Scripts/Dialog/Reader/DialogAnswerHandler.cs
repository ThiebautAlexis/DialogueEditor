using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class DialogAnswerHandler : MonoBehaviour
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
    public void InitHandler(DialogReader _owner, List<DialogLine> _lines)
    {
        for (int i = 0; i < _lines.Count; i++)
        {
            GameObject _buttonObj = Instantiate(m_buttonPrefab, transform);
            DialogLine _line = _lines[i]; 
            _buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = _owner.GetDialogLineContent(_lines[i].Key, DialogsSettingsManager.DialogsSettings.CurrentLocalisationKey);
            _buttonObj.GetComponent<Button>().onClick.AddListener(() => _owner.DisplayDialogLine(_line));
            _buttonObj.GetComponent<Button>().onClick.AddListener(() => Destroy(gameObject)); 
        }
    }
    #endregion 

    #endregion



}
