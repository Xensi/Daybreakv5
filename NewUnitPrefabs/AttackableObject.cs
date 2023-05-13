using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableObject : DamageableEntity
{
    //visual effect to display when destroyed
    public List<GameObject> enableOnDeathFromBeingFlamed;
    public List<GameObject> enableOnDeathFromBeingAttacked;
    public AudioClip soundOnDeath;
    public ModelSpawner spawner;
    public int inc = 0;
    public override void OnDamageEffect(bool flamed = false)
    {
        if (flamed)
        {

        }
        else
        {
            if (enableOnDeathFromBeingAttacked.Count > 0)
            { 
                if (inc < enableOnDeathFromBeingAttacked.Count && spawner != null && spawner.storedModels > 0)
                {
                    enableOnDeathFromBeingAttacked[inc].SetActive(true);
                    inc++;
                }
            }
        }
    }
    public override void OnDeathEffect(bool flamed = false)
    {
        if (flamed)
        {

            foreach (GameObject item in enableOnDeathFromBeingFlamed)
            {
                item.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject item in enableOnDeathFromBeingAttacked)
            {
                item.SetActive(true);
            }
        }
        /*if (spawner != null)
        {
            spawner.condition = FormationSpawner.SpawnCondition.Disabled;
        }*/
    }

}
