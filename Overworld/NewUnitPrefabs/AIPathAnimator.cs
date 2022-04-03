using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AIPathAnimator : AIPath
{
    private SoldierModel soldierModel;
    public override void OnTargetReached()
    {
        if (soldierModel == null)
        {
            soldierModel = GetComponent<SoldierModel>();
        }
        soldierModel.FinishedMovement();
    }
}
