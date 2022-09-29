using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{

    public bool canDamage = true;
    public bool isCavalry = true;

    [SerializeField] private SoldierModel parentModel;
    // Start is called before the first frame update
    void Start()
    {
        
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
                if (canDamage && parentModel.braced)
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
        }
        
    }
}
