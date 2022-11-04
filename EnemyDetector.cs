using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] private SphereCollider coll; 
    [SerializeField] private Rigidbody rigid;
    public FormationPosition formPosParent;
    public SoldierModel soldierParent;

    private void Start()
    {
        soldierParent = GetComponentInParent<SoldierModel>();
        formPosParent = soldierParent.formPos;
        coll.radius = soldierParent.attackRange;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hurtbox")
        {
            SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
            if (hitModel != null && hitModel.alive && hitModel.team != soldierParent.team)  
            {
                if (!soldierParent.nearbyEnemyModels.Contains(hitModel))
                { 
                    soldierParent.nearbyEnemyModels.Add(hitModel);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hurtbox")
        {
            SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
            if (hitModel != null && hitModel.alive && hitModel.team != soldierParent.team)
            {
                if (soldierParent.nearbyEnemyModels.Contains(hitModel))
                {
                    soldierParent.nearbyEnemyModels.Remove(hitModel);
                }
            }
        }
    }
}
