using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    public SoldierBlock soldierBlock;
    public int rowNum = 1;
    public int rowPositionInList = 0;
    public List<Position> positionsInRow = new List<Position>();
    public List<SoldierModel> modelsInRow = new List<SoldierModel>();
     
    public void UpdateModelsInRow()
    {
        modelsInRow.Clear();
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            { 
                if (model.modelPosition != null)
                { 
                    if (model.modelPosition.row == this)
                    {
                        modelsInRow.Add(model);
                    }
                }
            }
        }
        /*foreach (Position pos in positionsInRow)
        {
            if (pos.assignedSoldierModel != null)
            {
                if (!pos.assignedSoldierModel.alive && modelsInRow.Contains(pos.assignedSoldierModel)) //if dead and in list 
                {
                    modelsInRow.Remove(pos.assignedSoldierModel); //get em outta here
                }
                if (pos.assignedSoldierModel.alive && !modelsInRow.Contains(pos.assignedSoldierModel)) //if alive and not in list
                {
                    modelsInRow.Add(pos.assignedSoldierModel); //get em in here
                }

            }
            else
            {
                
            }
        }*/
    }

    public void RemoveModelFromRow(SoldierModel model)
    {
        if (modelsInRow.Contains(model)) //if dead and in list 
        {
            modelsInRow.Remove(model); //get em outta here
        }
    }
}
