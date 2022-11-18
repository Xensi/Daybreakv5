using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SutlerManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private OverworldManager overworldManager;
    [SerializeField] private GameObject sutlerParent;
    [SerializeField] private TMP_Text spoilsText;
    [SerializeField] private TMP_Text suppliesText;
    [SerializeField] private TMP_Text moraleText;
    [SerializeField] private TMP_Text horsesText;

    private int startingSpoils;
    private int startingSupplies;
    private int startingMorale;
    private int startingHorses;

    private int tradingSpoils;
    private int tradingSupplies;
    private int tradingMorale;
    private int tradingHorses;

    [SerializeField] private Button trade1;
    [SerializeField] private Button trade2;
    [SerializeField] private Button trade3;
    [SerializeField] private Button trade4;
    [SerializeField] private DialogueScriptableObject tradeDialogue;

    public bool tradingInDialogue = false;

    private void Start()
    {
        trade1.onClick.AddListener(() => Transaction(3, "supplies", 1, "spoils"));
        trade2.onClick.AddListener(() => Transaction(1, "spoils", 3, "supplies"));
        trade3.onClick.AddListener(() => Transaction(1, "supplies", 1, "morale"));
        trade4.onClick.AddListener(() => Transaction(1, "supplies", 50, "horses"));
    }
    public void ShowSutlerScreen()
    {
        sutlerParent.SetActive(true);
        SetStartingResources();
        UpdateResources();
        //overworldManager.DeselectArmy();
    }
    public void CloseSutlerScreen()
    {
        sutlerParent.SetActive(false);
        if (tradingInDialogue)
        {
            tradingInDialogue = false;
            dialogueManager.loadedDialogue = tradeDialogue;
            dialogueManager.StartDialogue();
        }
    }

    private void SetStartingResources()
    {
        Army army = overworldManager.soleArmy;
        startingSpoils = army.spoils;
        startingSupplies = army.provisions;
        startingMorale = army.overallMorale;
        startingHorses = army.horses;

        tradingSpoils = army.spoils;
        tradingSupplies = army.provisions;
        tradingMorale = army.overallMorale;
        tradingHorses = army.horses;
    }

    private void UpdateResources()
    {
        Army army = overworldManager.soleArmy;
        spoilsText.text = "Spoils: " + tradingSpoils + "/" + army.maxSpoils;
        suppliesText.text = "Supplies: " + tradingSupplies + "/" + army.maxProvisions;
        moraleText.text = "Morale: " + tradingMorale + "/" + army.maxMorale;
        horsesText.text = "Horses: " + tradingHorses;
    }

    public void Transaction(int give, string giveType, int get, string getType) //give, get
    {
        int tempSpoils = tradingSpoils;
        int tempSupplies = tradingSupplies;
        int tempMorale = tradingMorale;
        int tempHorses = tradingHorses;
        //Debug.Log("Temp" + " " + tempSpoils + " " + tempSupplies + " " + tempMorale + " " + tempHorses);
        Army army = overworldManager.soleArmy;
        bool tradeAllowed = false;
        bool tradeAllowed2 = false;
        switch (giveType)
        {
            case "spoils":
                if (tradingSpoils >= give)
                {
                    tradingSpoils -= give;
                    tradeAllowed = true;
                }
                break;
            case "supplies":
                if (tradingSupplies >= give)
                { 
                    tradingSupplies -= give;
                    tradeAllowed = true;
                }
                break;
            case "morale":
                if (tradingMorale >= give)
                {
                    tradingMorale -= give;
                    tradeAllowed = true;
                }
                    
                break;
            case "horses":
                if (tradingHorses >= give)
                {
                    tradingHorses -= give;
                    tradeAllowed = true;
                }
                break;
        }
        switch (getType)
        {
            case "spoils":
                if (tradingSpoils + get <= army.maxSpoils)
                {
                    tradingSpoils += get;
                    tradeAllowed2 = true;
                }
                break;
            case "supplies":
                if (tradingSupplies + get <= army.maxProvisions)
                {
                    tradingSupplies += get;
                    tradeAllowed2 = true;
                }
                break;
            case "morale":
                if (tradingMorale + get <= army.maxMorale)
                {
                    tradingMorale += get;
                    tradeAllowed2 = true;
                }
                break;
            case "horses":
                tradingHorses += get;
                tradeAllowed2 = true;
                break;
        }
        if (tradeAllowed && tradeAllowed2)
        { 
            UpdateResources();
        }
        else
        {
            tradingSpoils = tempSpoils;
            tradingSupplies = tempSupplies;
            tradingMorale = tempMorale;
            tradingHorses = tempHorses;
            UpdateResources();
        }
    }

    public void ResetTrades()
    {
        tradingSpoils = startingSpoils;
        tradingSupplies = startingSupplies;
        tradingMorale = startingMorale;
        tradingHorses = startingHorses;
        UpdateResources();
    }

    public void FinishTrades()
    {
        Army army = overworldManager.soleArmy;
        army.spoils = tradingSpoils;
        army.provisions = tradingSupplies;
        army.overallMorale = tradingMorale;
        army.horses = tradingHorses;
        CloseSutlerScreen();
    }


}
