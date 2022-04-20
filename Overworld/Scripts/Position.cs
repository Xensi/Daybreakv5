using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
public class Position : MonoBehaviour
{
    public SoldierModel assignedSoldierModel;
    public int row = 1;
     
    [SerializeField] private List<SoldierModel> candidates;

    [SerializeField] private int numTimesSought = 0;

    public string team = "Altgard";
    public FormationPosition formPos;

    private void OnDrawGizmosSelected()
    {

         //Gizmos.DrawWireSphere(transform.position, 5+numTimesSought);
    }
    public void SeekReplacement(float range = 5f)
    {
        if (assignedSoldierModel != null)
        {
            return;
        } 

        candidates.Clear();

        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 80;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, range+numTimesSought, colliders, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++)
        { 
            SoldierModel model = colliders[i].GetComponent<SoldierModel>();

            if (model != null)
            {
                if (model.formPos == formPos && model.alive && model.position.row > row) //must be same team, be alive, and be in a row lower than ours (greater in number)
                { 
                    candidates.Add(model);
                }
            }
        }
        numTimesSought++;
        //Debug.LogError(candidates.Count + team + row);
        if (candidates.Count <= 0)
        { 
        }
        else
        {
            GetClosest();
        }
    }

    private void GetClosest()
    {
        if (assignedSoldierModel != null)
        {
            return;
        }
        if (candidates.Count <= 0) //if no candidates
        {
            assignedSoldierModel = null;
            return;
        }
        if (candidates.Count == 1)
        {
            assignedSoldierModel = candidates[0];
        }
        assignedSoldierModel = candidates[0];
        float initDist = GetDistance(transform, candidates[0].transform);
        float compareDist = initDist;
        foreach (SoldierModel model in candidates) //doesn't work yet
        {
            float dist = GetDistance(transform, model.gameObject.transform);
            //Debug.LogError(dist);
            if (dist < compareDist)
            {
                assignedSoldierModel = model;
                compareDist = dist;
            }
        }
        UpdateModelPosition();
    }

    private void UpdateModelPosition()
    {
        if (assignedSoldierModel == null)
        {
            return;
        }
        AIDestinationSetter aiDesSet = assignedSoldierModel.GetComponent<AIDestinationSetter>();
        aiDesSet.target = transform; //it will go here
        assignedSoldierModel.position.assignedSoldierModel = null; //remove from original position
        assignedSoldierModel.position = this; //update its assigned position
    }

    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    }

}
