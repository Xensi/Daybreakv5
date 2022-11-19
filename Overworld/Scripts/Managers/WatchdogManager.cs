using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchdogManager : MonoBehaviour
{
    public Vector3 lastSpottedPosition; 
    public void EnemyArmyLostSight()
    {
        Debug.LogError("Lost sight of you");
        Transform lastPos = OverworldManager.Instance.soleArmy.transform;

        float fixedCoordsx = RoundToZeroOrHalf(lastPos.position.x);
        float fixedCoordsz = RoundToZeroOrHalf(lastPos.position.z);
        lastSpottedPosition = new Vector3(fixedCoordsx, 0, fixedCoordsz);
        foreach (Army enemyArmy in OverworldManager.Instance.enemyArmies)
        {
            enemyArmy.detectedNotSpottedArmy = null;
            enemyArmy.focusedOnArmy = null;
            enemyArmy.enemyLastSeenPos = lastSpottedPosition;
            enemyArmy.lastPosKnown = true;
        }
    }
    public void EnemyArmySpotted()
    {
        Debug.LogError("Spotted you");
        foreach (Army enemyArmy in OverworldManager.Instance.enemyArmies)
        {
            enemyArmy.focusedOnArmy = enemyArmy.detectedNotSpottedArmy; //allow army to chase army they've "detected"
            enemyArmy.detectedNotSpottedArmy = null;
            enemyArmy.lastPosKnown = false;
        }
    }
    private float RoundToZeroOrHalf(float a) //1.52 will be 1.5, 1.1232 will be 1
    {
        int b = Mathf.RoundToInt(a);
        if (a > b)
        {
            return b + .5f;
        }
        else
        {
            return b - .5f;
        }
    }
}
