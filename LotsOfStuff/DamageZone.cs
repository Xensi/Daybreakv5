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
    [SerializeField] private float allowedDamageTicks = 1;
    [SerializeField] private float currentDamageTicks = 0;
    [SerializeField] private bool eternal = true;
    [SerializeField] private bool working = true;
    [SerializeField] private bool knocksBack = false;
    [SerializeField] private bool damageFromThisPreventsCastingMagic = false;
    [SerializeField] private float denyMagicForTime = 60;
    void Start()
    { 
    }
    private void Update()
    {
        if (working)
        { 
            if (timer < interval)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
                DealDamage();
                if (!eternal)
                {
                    currentDamageTicks++;
                    if (currentDamageTicks >= allowedDamageTicks)
                    {
                        working = false;
                    }
                }
            }
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
                        model.pendingLaunched = knocksBack;
                        model.pendingDamageSource = transform;
                        if (damageFromThisPreventsCastingMagic)
                        { 
                            model.formPos.allowedToCastMagic = false;
                            model.formPos.timeUntilAllowedToCastMagicAgain = denyMagicForTime;

                            FightManager obj = FindObjectOfType<FightManager>();
                            obj.UpdateGUI();
                        }
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
