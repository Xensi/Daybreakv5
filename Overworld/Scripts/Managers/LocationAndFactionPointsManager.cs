using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationAndFactionPointsManager : MonoBehaviour
{
    public List<SupplyPoint> importantFactionPoints; 

    [SerializeField] private List<SupplyPoint> otherLocations; 

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
}
