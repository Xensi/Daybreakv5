using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamingEvent : MonoBehaviour
{

    public void StartMoving()
    {
        /*moving = true;
        checkedForcedMarch = false;
        dustVFX.Play();
        shakeTween.Play();

        if (aiControlled)
        {
            //check if we're reached last known pos
            bool reachedLastPos = false;
            //check if we've reached lastPosKnown
            Vector3 diff = transform.position - enemyLastSeenPos;
            diff.x = Mathf.Abs(diff.x);
            diff.z = Mathf.Abs(diff.z);

            if (diff.x < 0.5f && diff.z < 0.5f) //safe check to see if we're here
            {
                reachedLastPos = true;
                lastPosKnown = false;
            }

            if (!withinWatchdogBounds) //out of watchdog bounds go home
            {
                target.position = patrolNodes[nodeNumber].transform.position;
            }
            else if (focusedOnArmy != null) //if we see army, go to them
            {
                target.position = focusedOnArmy.transform.position;
            }
            else if (!reachedLastPos && lastPosKnown) //if haven't reached last pos, go there
            {
                target.position = enemyLastSeenPos;

            }
            else if (withinWatchdogBounds) //if ai, patrol
            {
                nodeNumber++;
                if (nodeNumber >= patrolNodes.Count)
                {
                    nodeNumber = 0;
                }
                target.position = patrolNodes[nodeNumber].transform.position;
            }
        }


        UpdateVisionRange();
        //aiPath.maxSpeed = maxSpeed;
        speedCurrent = 0;
        numberOfMovementAttempts = 0;
        CheckSizeAndChangeSpeed();
        MoveOneNode(); //start the movement
        remainingDistanceNew = aiPath.remainingDistance;
        remainingDistanceOld = aiPath.remainingDistance;
        UpkeepTrigger();
        if (predictedMovementSpaces >= 4)
        {
            ConsumeUpkeep();
        }
        StartCoroutine(WaitUntilMovementOver());
        StartCoroutine(NoticeIfBlocked());*/
    }
}
