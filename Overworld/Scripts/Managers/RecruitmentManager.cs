using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecruitmentManager : MonoBehaviour
{
    [SerializeField] private OverworldManager overworldManager;
    [SerializeField] private GameObject recruitmentParent;

    [SerializeField] private List<RecruitmentSlot> recruitmentSlots;
    [SerializeField] private List<RecruitmentOption> recruitmentOptions;

    [SerializeField] private RecruitmentSlot selectedRecruitmentSlot;
    [SerializeField] private int availableSpoils;

    [SerializeField] private List<ArmyCardScriptableObj> recruitingChoicesMade;
    [SerializeField] private Sprite empty;
    [SerializeField] private TMP_Text spoilsNum;
    [SerializeField] private Army targetedArmy;

    public SupplyGiver recruitingFromSupplyGiver;

    private void Awake()
    {
        selectedRecruitmentSlot = null;
        DisableRecruitmentOptions();
        UpdateSpoilsText();
    }

    public void CheckIfPathClear(SupplyGiver recruitingFrom)
    {

    }

    public void ShowRecruitmentScreen()
    {
        selectedRecruitmentSlot = null;
        recruitmentParent.SetActive(true);
        DisableRecruitmentOptions();
        targetedArmy = overworldManager.selectedArmy;
        availableSpoils = targetedArmy.spoils;
        UpdateSpoilsText();
    }
    public void HideRecruitmentScreen()
    {
        selectedRecruitmentSlot = null;
        recruitmentParent.SetActive(false);
        DisableRecruitmentOptions();
        //availableSpoils = targetedArmy.spoils;
        UpdateSpoilsText();
    }

    private void SetInteractiveBasedOnSpoils()
    {
        foreach (RecruitmentOption option in recruitmentOptions)
        {
            if (option.information.spoilsCost <= availableSpoils)
            {
                option.button.interactable = true;
            }
            else
            {
                option.button.interactable = false;
            }
        }
    }

    public void ConfirmRecruitmentChoices()
    {
        recruitmentParent.SetActive(false);
        DisableRecruitmentOptions();

        //process recruitment choices by storing them somewhere
        foreach (RecruitmentSlot slot in recruitmentSlots)
        {
            recruitingChoicesMade.Add(slot.information);
        }
    }

    public void ClickRecruitmentSlot(RecruitmentSlot slot)
    {

        selectedRecruitmentSlot = slot;
        
        if (selectedRecruitmentSlot.information != null)
        {
            availableSpoils += selectedRecruitmentSlot.information.spoilsCost;
            UpdateSpoilsText();
            ClearInformation(selectedRecruitmentSlot);

        }

        //EnableRecruitmentOptions();
        SetInteractiveBasedOnSpoils();
    }

    private void ClearInformation(RecruitmentSlot slot)
    {
        slot.slotName.text = "";
        slot.icon.sprite = empty;
        slot.information = null;
    }

    public void ClickRecruitmentOption(RecruitmentOption option)
    {
        //type is the type of unit selected
        //selectedRecruitmentSlot is the modified slot

        ModifyRecruitmentSlot(option);
        availableSpoils -= option.information.spoilsCost;
        UpdateSpoilsText();
        DisableRecruitmentOptions();

    }

    private void UpdateSpoilsText()
    {
        spoilsNum.text = "Spoils: " + availableSpoils;

    }


    private void ModifyRecruitmentSlot(RecruitmentOption option)
    {
        selectedRecruitmentSlot.slotName.text = option.information.cardName;
        selectedRecruitmentSlot.icon.sprite = option.information.cardIcon;
        selectedRecruitmentSlot.information = option.information;
    }

    private void EnableRecruitmentOptions()
    {
        foreach (RecruitmentOption option in recruitmentOptions)
        {
            option.button.interactable = true;
        }
    }
    private void DisableRecruitmentOptions()
    {
        foreach (RecruitmentOption option in recruitmentOptions)
        {
            option.button.interactable = false;
        }
    }
}
