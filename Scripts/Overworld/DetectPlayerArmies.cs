using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayerArmies : MonoBehaviour
{
    public Army parentArmy;
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogError("collision?");
        Army collidedArmy = other.gameObject.GetComponent<Army>();
        if (collidedArmy != null)
        {
            if (collidedArmy.faction != parentArmy.faction) //if we touch another army that is another team
            {
                parentArmy.focusedOnArmy = collidedArmy;
            }
        }
    }
}
