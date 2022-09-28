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
        //first get a row behind us
        int behindUs = row.rowPositionInList + 1 + numTimesSought;
        if (behindUs == 8)
        {
            //out of bounds 
            return;
        }
        //get candidates
        candidates.Clear();
        //instead of this, just select a suitable replacement from a row that is behind us if possible 
        Row desiredRow = row.soldierBlock.rows[behindUs];
        foreach (Position item in desiredRow.positionsInRow)
        {
            if (item.assignedSoldierModel != null)
            {
                candidates.Add(item.assignedSoldierModel);
            }
        }    
        if (candidates.Count > 0)
        {
            GetClosest();
        }
        else
        { 
            numTimesSought++;
            //SeekReplacement(); //keep going until out of bounds?
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
}
