using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyPoint : LocaleInvestigatable
{ 
    public Army armyOnThisSupplyPoint;
    public BattleGroup battleGroupAtThisSupplyPoint;
    public OverworldManager overworldManager;

    private int turnCounter = 0;
    public int storedSpoils = 10;
    public int reservedSpoils = 7;
    public int extortionReservedSpoils = 5;
    public float storedSupplies = 10;
    public float maxProvisions = 100;
    public float amountOfProvisionsToReserve = 7;
    public int extortionReservedProvisions = 5;
    //public int maxProvisions = 20;

    public float mood = 0;

    public string supplyName = "Reevewood";
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;
    public string relations = "Unfriendly";

    public bool isFort = false;

    public int population = 500;

    public int posNeutralityBuffer = 1;
    public int negNeutralityBuffer = -1;

    public List<ArmyCardScriptableObj> cardsAvailable;
    public bool talkDescriptionRead = false;
    public DialogueScriptableObject talkToDialogue;
    public DialogueScriptableObject afterReadTalkToDialogue;
    public DialogueScriptableObject eventDialogue;
    public bool eventTriggered = false;

    public List<bool> npcTalkedTo;

    public bool extortable = true;
    public bool pillageable = true;

    public bool routeClear = false;

    public List<SupplyPoint> placesBlockingRoute;

    //public List<Army> armiesToSupport;
    //public GameObject caravanPrefab;
    //public int provisionsGainedEveryInterval = 1;
    //public int turnsUntilNextRegain = 5;
    //public int regainTracker = 5;
    //public int provisionsToSendEveryInterval = 2;
    //public int supportedArmyRotater = 0;
    //public DialogueScriptableObject dialogueUponRequestingSupplies;
    //public bool suppliesRequestedForFirstTime = false;
    public float provisionGainRate = 1;
    private float provisionGainModifier = .1f;

    public void Awake()
    {
        //regainTracker = turnsUntilNextRegain;

        if (overworldManager == null)
        {
            var oManager = GameObject.FindWithTag("OverworldManager");
            overworldManager = oManager.GetComponent<OverworldManager>();
        }
    }
    private void Start()
    {
        InvokeRepeating("GainProvisions", 1, 1);
    }
    private void GainProvisions()
    {
        storedSupplies += provisionGainRate * BattleGroupManager.Instance.timeScale * provisionGainModifier;
        Mathf.Clamp(storedSupplies, 0, maxProvisions);
    }

    public void UpdateRelations()
    {
        if (population <= 0)
        {
            relations = "Abandoned";
            return;
        }

        if (isFort && team == overworldManager.currentTeam)
        {
            relations = "Loyal";
        }
        else if (isFort && team != overworldManager.currentTeam)
        {
            relations = "Hostile";
        }
        else if (!isFort && team != overworldManager.currentTeam)
        {
            relations = "Unfriendly";
        }
        else if (!isFort && team == overworldManager.currentTeam)
        {
            if (mood > posNeutralityBuffer)
            {
                relations = "Welcoming";
            }
            else if (mood >= 0 && mood <= posNeutralityBuffer)
            {
                relations = "Cooperative";
            }
            else if (mood <= 0 && mood >= negNeutralityBuffer)
            {
                relations = "Wary";
            }
            else if (mood < negNeutralityBuffer)
            {
                relations = "Unfriendly";
            }
        }
    }

    /*public void SpawnCaravans()
    {
        turnCounter++;
        if (turnCounter % 2 == 0) //every other turn we'll send a caravan
        {
            turnCounter = 0;


            if (storedProvisions > 1) // if we have provisions, create a caravan
            {

                GameObject caravan = Instantiate(caravanPrefab, Vector3.zero, Quaternion.identity);

                Caravan caravanComp = caravan.GetComponentInChildren<Caravan>(); //get army

                Transform cTransform = caravan.transform.GetChild(0); //get transform of figurine
                cTransform.position = transform.position; //move figurine to click pos

                caravanComp.targetArmy = armiesToSupport[supportedArmyRotater];
                caravanComp.provisionsCarried = 0;
                while (caravanComp.provisionsCarried < provisionsToSendEveryInterval && storedProvisions > 0 && storedProvisions > reservedProvisions) //try to give caravan provisions
                {
                    caravanComp.provisionsCarried++;
                    storedProvisions--;
                }

                caravanComp.homeSupplySource = this;

                overworldManager.caravans.Add(caravanComp);

                supportedArmyRotater++;
                if (supportedArmyRotater >= armiesToSupport.Count)
                {
                    supportedArmyRotater = 0;
                }

            }
        }


        if (regainTracker > 0)
        {
            regainTracker--;
        }
        else
        {
            regainTracker = turnsUntilNextRegain;


            storedProvisions += provisionsGainedEveryInterval;
            if (storedProvisions > maxProvisions)
            {
                storedProvisions = maxProvisions;
            }
        }

    }*/
}
