using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyGiver : MonoBehaviour
{

    public List<Army> armiesToSupport;
    public GameObject caravanPrefab;
    public OverworldManager overworldManager;

    public int turnCounter = 0;

    public int storedProvisions = 10;
    public int maxProvisions = 20;
    public int provisionsGainedEveryInterval = 1;
    public int turnsUntilNextRegain = 5;

    public int regainTracker = 5;
    public int provisionsToSendEveryInterval = 2; 

    public int supportedArmyRotater = 0;

    public int reservedProvisions = 10;
    public int extortionReservedProvisions = 5;

    public int anger = 0;

    public Army armyOnThisSupplyPoint;

    public void Awake()
    {
        regainTracker = turnsUntilNextRegain;

        if (overworldManager == null)
        {
            var oManager = GameObject.FindWithTag("OverworldManager");
            overworldManager = oManager.GetComponent<OverworldManager>();
        }
    }

    public void SpawnCaravans()
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

    }
}
