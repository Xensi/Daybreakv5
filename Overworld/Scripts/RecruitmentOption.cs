using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitmentOption : MonoBehaviour
{
    public ArmyCardScriptableObj information;
    public Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text optionName;
    [SerializeField] private TMP_Text spoilsText;

    private void Awake()
    {
        icon.sprite = information.cardIcon;
        optionName.text = information.cardName;
        spoilsText.text = "Cost:" + information.spoilsCost;
    }
}
