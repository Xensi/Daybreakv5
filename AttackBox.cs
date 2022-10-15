using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public List<Collider> colliders;
    public bool canDamage = true;
    public bool isCavalry = true;

    [SerializeField] private SoldierModel parentModel;
    private void Start()
    {
        if (!isCavalry)
        { 
            ToggleAttackBox(false);
        }
        Collider[] array = GetComponents<Collider>();
        colliders.AddRange(array);
    }
    public void ToggleAttackBox(bool val)
    {
        foreach (Collider col in colliders)
        {
            col.enabled = val;
        }
    }
    public void Rearm()
    {
        canDamage = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if (isCavalry)
        {
            float speedThreshold = 0.5f;
            if (other.gameObject.tag == "Hurtbox") //
            {
                if (canDamage && parentModel.moving && parentModel.normalizedSpeed > speedThreshold)
                {
                    float toleranceForKnockDown = 2;
                    SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                    if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && hitModel.getUpTime <= toleranceForKnockDown)
                    {
                        canDamage = false;
                        parentModel.DealDamage(hitModel, true, true);
                        parentModel.currentAttackTime = 0;
                    }
                }
                else if (!canDamage && parentModel.moving && parentModel.normalizedSpeed > speedThreshold) //trample
                {
                    SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                    if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && !hitModel.knockedDown)
                    {
                        canDamage = false;
                        parentModel.DealDamage(hitModel, false, true, true);
                        parentModel.currentAttackTime = 0;
                    }
                }
            }
        }
        else
        {
            if (other.gameObject.tag == "Hurtbox") //
            {
                if (parentModel.braced)
                { 
                    if (canDamage)
                    {
                        float speedThreshold = 0.5f;
                        float toleranceForKnockDown = 2;
                        SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                        if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && hitModel.normalizedSpeed > speedThreshold && !hitModel.airborne && hitModel.getUpTime <= toleranceForKnockDown)
                        {
                            canDamage = false;
                            parentModel.DealDamage(hitModel, true, false);
                            parentModel.currentAttackTime = 0;
                        }
                    }
                }
                else if (parentModel.formPos.charging)
                {
                    if (canDamage)
                    { 
                        float toleranceForKnockDown = 2;
                        SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                        if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && hitModel.getUpTime <= toleranceForKnockDown)
                        {
                            canDamage = false;
                            parentModel.DealDamage(hitModel, true, false);
                            parentModel.currentAttackTime = 0;
                        }
                    }
                }
            }
        }
        
    }
}
