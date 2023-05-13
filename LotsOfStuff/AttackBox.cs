using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public List<SphereCollider> colliders;
    public bool canDamage = true;
    public bool isCavalry = true;

    [SerializeField] private SoldierModel parentModel;
    private void Start()
    {
        SphereCollider[] array = GetComponents<SphereCollider>();
        colliders.AddRange(array);

        ToggleAttackBox(isCavalry);
    }

    public void ApproximateCharge()
    {
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 10;
        Collider[] hitCols = new Collider[maxColliders];
        float radius = 0.5f;
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, radius, hitCols, layerMask, QueryTriggerInteraction.Ignore);
        if (numColliders > 0)
        { 
            for (int i = 0; i < numColliders; i++) //go for hurtboxes
            {
                if (hitCols[i].gameObject.tag == "Hurtbox") //if is hurtbox
                {
                    SoldierModel model = hitCols[i].GetComponentInParent<SoldierModel>();
                    if (model != null)
                    {
                        if (model.alive && model.team != parentModel.team) //alive and enemy
                        {
                            Impact(hitCols[i]);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void Rearm()
    {
        canDamage = true; 
        if (isCavalry || parentModel.formPos.charging) //if we are cavalry or they are charging we should turn the collider on
        {
            ToggleAttackBox(true);
        }
    }
    public void Disarm()
    {
        canDamage = false;
        ToggleAttackBox(false);
    }
    public void ToggleAttackBox(bool val)
    {
        foreach (SphereCollider col in colliders)
        {
            col.enabled = val;
        }
    }
    private void Impact(Collider other)
    {
        //Debug.Log("collision");
        float unitSpeed = parentModel.normalizedSpeed;
        float speedThreshold = 0.5f;
        if (unitSpeed < speedThreshold)
        {
            return;
        }
        if (isCavalry)
        {
            if (other.gameObject.tag == "Hurtbox") //
            {
                 
                if (canDamage && parentModel.currentModelState == SoldierModel.ModelState.Moving)
                {
                    float toleranceForKnockDown = 2;
                    SoldierModel hitModel = other.GetComponentInParent<SoldierModel>(); 
                    if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && !hitModel.knockedDown && hitModel.getUpTime <= toleranceForKnockDown)
                    {
                        //Debug.Log("Launching");
                        Disarm();
                        bool canWeLaunchThem = false;
                        if (hitModel.formPos.formationType != FormationPosition.FormationType.Cavalry)
                        {
                            canWeLaunchThem = true;
                        }
                        parentModel.DealDamageToModel(hitModel, canWeLaunchThem, true, false);
                        parentModel.currentAttackTime = 0;
                    }
                }
                else if (!canDamage && parentModel.currentModelState == SoldierModel.ModelState.Moving && unitSpeed > speedThreshold) //trample
                {
                    SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                    if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && !hitModel.knockedDown)
                    {
                        //Debug.Log("Trampling");
                        Disarm();
                        parentModel.DealDamageToModel(hitModel, false, true, true);
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
                        float toleranceForKnockDown = 2;
                        SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                        if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && hitModel.normalizedSpeed > speedThreshold && !hitModel.airborne && hitModel.getUpTime <= toleranceForKnockDown)
                        {
                            Disarm();
                            parentModel.DealDamageToModel(hitModel, false, false);
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
                        if (hitModel != null && hitModel.alive && hitModel.team != parentModel.team && !hitModel.airborne && !hitModel.knockedDown && hitModel.getUpTime <= toleranceForKnockDown)
                        {
                            Disarm();
                            bool canWeLaunchThem = false;
                            if (hitModel.formPos != null && hitModel.formPos.formationType != FormationPosition.FormationType.Cavalry)
                            {
                                canWeLaunchThem = true;
                            }
                            else if (hitModel.formPos != null) //stop charging if we hit cavalry
                            {
                                parentModel.formPos.StopCharging();
                            }
                            parentModel.DealDamageToModel(hitModel, canWeLaunchThem, false);
                            parentModel.currentAttackTime = 0;
                        }
                    }
                }
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Impact(other); 
    }
}
