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
        if (!isCavalry)
        { 
            ToggleAttackBox(false);
        }
        SphereCollider[] array = GetComponents<SphereCollider>();
        colliders.AddRange(array);
    }
    public void ToggleAttackBox(bool val)
    {
        foreach (SphereCollider col in colliders)
        {
            col.enabled = val;
        }
    }
    public void Rearm()
    {
        canDamage = true;
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

    private void Impact(Collider other)
    {
        if (isCavalry)
        {
            float unitSpeed = parentModel.normalizedSpeed;
            float speedThreshold = 0.5f;
            if (other.gameObject.tag == "Hurtbox") //
            {
                if (canDamage && parentModel.moving && unitSpeed > speedThreshold)
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
                else if (!canDamage && parentModel.moving && unitSpeed > speedThreshold) //trample
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
                            parentModel.DealDamage(hitModel, false, false);
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
    void OnTriggerEnter(Collider other)
    {
        Impact(other); 
    }
}
