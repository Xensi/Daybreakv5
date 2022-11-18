using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationAndFactionPointsManager : MonoBehaviour
{
    public List<SupplyPoint> importantFactionPoints;

    public List<ImportantNPC> importantNPCs;

    [SerializeField] private List<SupplyPoint> otherLocations;

    public void SetHelped(string npcName)
    {
        foreach (ImportantNPC npc in importantNPCs)
        {
            if (npc.NPCName == npcName)
            {
                Debug.LogError(npcName);
                npc.helpedNPC = true;
            }
        }
    }

    public bool CheckIfVisited(string location)
    {
        //Debug.LogError(location);
        foreach (SupplyPoint important in importantFactionPoints)
        {
            //Debug.LogError(important.supplyName);
            if (important.supplyName == location)
            {
                return important.eventTriggered;
            }
        }

        return false;
    }

    public bool CheckIfHelped(string npcName)
    {
        foreach(ImportantNPC npc in importantNPCs)
        {
            if (npc.NPCName == npcName)
            {
                return npc.helpedNPC;
            }
        }
        return false;
    }


}
