using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damage = 1;
    [SerializeField]
    private float armorPiercingDamage = 0;
    [SerializeField] private float interval = 1;
    [SerializeField] private float radius = 10; 
    [SerializeField] private float timer = 0;
    void Start()
    { 
    }
    private void Update()
    {
        if (timer < interval)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            DealDamage();
        }
    }
    private void DealDamage()
    { 
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 80;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++)
        {
            if (colliders[i].gameObject.tag == "Hurtbox")
            { 
                SoldierModel model = colliders[i].GetComponentInParent<SoldierModel>(); 
                if (model != null)
                {
                    if (model.alive)
                    { 
                        model.pendingDamage = damage;
                        model.pendingArmorPiercingDamage = armorPiercingDamage;
                    }
                }
            }
        } 

    }

    private void OnDrawGizmos()
    { 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }


}
