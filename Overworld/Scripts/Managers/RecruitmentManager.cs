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
    public int startingAvailableSpoils;

    [SerializeField] private List<ArmyCardScriptableObj> recruitingChoicesMade;
    [SerializeField] private Sprite empty;
    [SerializeField] private TMP_Text spoilsNum;
    [SerializeField] private Army targetedArmy;

    public SupplyPoint recruitingFromSupplyGiver;

    private void Awake()
    {
        selectedRecruitmentSlot = null;
        DisableRecruitmentOptions();
        UpdateSpoilsText();
    }

    public void CheckIfPathClear(SupplyPoint recruitingFrom)
    {

    }

    public void ShowRecruitmentScreen()
    {
        selectedRecruitmentSlot = null;
        recruitmentParent.SetActive(true);
        DisableRecruitmentOptions(); 
        availableSpoils = OverworldManager.Instance.playerBattleGroup.spoils;
        startingAvailableSpoils = availableSpoils;
        UpdateSpoilsText();
        ClearSlots();
    }
    public void HideRecruitmentScreen()
    {
        selectedRecruitmentSlot = null;
        recruitmentParent.SetActive(false);
        DisableRecruitmentOptions();
        UpdateSpoilsText();
        ClearSlots();
    }
    private void ClearSlots()
    {
        foreach (RecruitmentSlot slot in recruitmentSlots)
        {
            ClearInformation(slot);
        }
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
         
        recruitingChoicesMade.Clear();
        //process recruitment choices by storing them somewhere
        foreach (RecruitmentSlot slot in recruitmentSlots)
        {
            recruitingChoicesMade.Add(slot.card);
        }
        foreach (ArmyCardScriptableObj item in recruitingChoicesMade)
        {
            if (item != null)
            { 
                OverworldManager.Instance.playerBattleGroup.AddUnitToArmy(item);
            }
        } 
        recruitingChoicesMade.Clear();
        ClearSlots();
        OverworldManager.Instance.playerBattleGroup.spoils -= startingAvailableSpoils - availableSpoils; //10-5 means spent 5. 10-0 means spent 10. 10-10 = 0
        OverworldManager.Instance.ShowArmyInfoAndUpdateArmyBars();
    }

    public void ClickRecruitmentSlot(RecruitmentSlot slot)
    { 
        selectedRecruitmentSlot = slot; 
        if (selectedRecruitmentSlot.card != null)
        {
            availableSpoils += selectedRecruitmentSlot.card.spoilsCost;
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
        slot.card = null;
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
        selectedRecruitmentSlot.card = option.information;
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
