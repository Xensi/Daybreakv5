using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
public class Position : MonoBehaviour
{
    public SoldierModel assignedSoldierModel;
    public Row row;
     
    [SerializeField] private List<SoldierModel> candidates;

    [SerializeField] private int numTimesSought = 0;

    public string team = "Altgard";
    public FormationPosition formPos;

    public bool activeController = false;
    private void Start()
    {
        
    }
    private void OnDrawGizmosSelected()
    {

         //Gizmos.DrawWireSphere(transform.position, 5+numTimesSought);
    }
    public void SeekReplacement(float range = 5f)
    {
        if (assignedSoldierModel != null)
        { 
            if (assignedSoldierModel.alive)
            {
                return;
            }
        }
        //safety check

        //get candidates
        candidates.Clear(); 
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 80;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, range+numTimesSought, colliders, layerMask, QueryTriggerInteraction.Ignore); //use hurtboxes now
        for (int i = 0; i < numColliders; i++)
        { 
            SoldierModel model = colliders[i].GetComponentInParent<SoldierModel>();

            if (model != null)
            {
                if (model.modelPosition != null)
                { 
                    if (model.formPos != null)
                    {
                        if (model.modelPosition.row != null)
                        { 
                            if (model.formPos == formPos && model.alive && model.modelPosition.row.rowPositionInList > row.rowPositionInList) //must be same team, be alive, and be in a row in higher position (farther back)
                            {
                                candidates.Add(model);
                            }
                        }
                    }
                }
            }
        }
        //
        numTimesSought++; 
        if (candidates.Count > 0)
        {
            GetClosest();
        }
    }

    private void GetClosest()
    {
        if (assignedSoldierModel != null)
        {
            if (assignedSoldierModel.alive)
            {
                return;
            }
        }
        //safety
        if (candidates.Count <= 0) //if no candidates
        { 
            return;
        }
        SoldierModel model = candidates[0];
        if (model != null)
        {
            if (candidates.Count == 1) //only 1 candidate, so let's do it
            {
                assignedSoldierModel = model;
                UpdateModelPosition();
            }
            else //multiple candidates
            {
                assignedSoldierModel = model; //set for now
                if (candidates.Contains(assignedSoldierModel))
                { 
                    candidates.Remove(assignedSoldierModel);
                    //Check if others are closer
                    float compareDist = Vector3.Distance(transform.position, assignedSoldierModel.transform.position);
                    foreach (SoldierModel modelCandidate in candidates) 
                    {
                        float dist = Vector3.Distance(assignedSoldierModel.transform.position, modelCandidate.transform.position); 
                        if (dist < compareDist)
                        { 
                            assignedSoldierModel = modelCandidate;
                            compareDist = dist;
                        }
                    }
                    UpdateModelPosition();
                } 
            }
        }
        
    }

    private void UpdateModelPosition()
    { 
        if (assignedSoldierModel != null)
        { 
            AIDestinationSetter aiDesSet = assignedSoldierModel.GetComponent<AIDestinationSetter>();
            aiDesSet.target = transform; //it will go here
            if (assignedSoldierModel.modelPosition != null)
            { 
                assignedSoldierModel.modelPosition.assignedSoldierModel = null; //remove from original position
            }
            assignedSoldierModel.modelPosition = this; //update its assigned position
            row.UpdateModelsInRow();


            numTimesSought = 0;
        } 
    }


    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    }

}
