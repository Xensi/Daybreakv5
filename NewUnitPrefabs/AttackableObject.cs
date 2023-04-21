using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableObject : DamageableEntity
{
    //visual effect to display when destroyed
    public List<GameObject> enableOnDeath;
    public AudioClip soundOnDeath;

    public override void OnDeathEffect()
    { 
        foreach (GameObject item in enableOnDeath)
        {
            item.SetActive(true);
        }
    }

}
