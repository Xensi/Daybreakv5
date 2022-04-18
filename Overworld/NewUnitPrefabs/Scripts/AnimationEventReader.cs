using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReader : MonoBehaviour
{
    [SerializeField] private SoldierModel model;

    private void DealDamage()
    {
        //Debug.LogError("Dealing damage");
        model.DealDamage();
    }
    private void TookDamage()
    {
        model.TookDamage();
    }
}
