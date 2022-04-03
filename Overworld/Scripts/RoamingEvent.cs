using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;
using Pathfinding;

public class RoamingEvent : MonoBehaviour
{
    [SerializeField] private bool moving = false;
    [SerializeField] private VisualEffect dustVFX;
    private Tween shakeTween;
    [SerializeField] private GameObject icon;
    [SerializeField] private int numberOfMovementAttempts = 0;
    [SerializeField] private float remainingDistanceNew = 0;
    [SerializeField] private float remainingDistanceOld = 0;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private ABPath path;
    [SerializeField] private BlockManager blockManager;
    BlockManager.TraversalProvider traversalProvider;
    [SerializeField] private Transform target;
    [SerializeField] private GameObject aiTarget;
    [SerializeField] private int nodeNumber = 0;
    [SerializeField] private List<Transform> patrolNodes;
    private Vector3 rotationAngles;
    private void Awake()
    {
        rotationAngles = icon.transform.rotation.eulerAngles;

        //setup the little shake while moving
        shakeTween = icon.transform.DORotate(new Vector3(rotationAngles.x, rotationAngles.y+3, rotationAngles.z), .25f).OnComplete(ShakeCallBack);
        shakeTween.Pause();
    }
    private void ShakeCallBack()
    {
        shakeTween = icon.transform.DORotate(new Vector3(rotationAngles.x, rotationAngles.y - 3, rotationAngles.z), .25f).OnComplete(ShakeCallBack2);
    }
    private void ShakeCallBack2()
    {
        shakeTween = icon.transform.DORotate(new Vector3(rotationAngles.x, rotationAngles.y + 3, rotationAngles.z), .25f).OnComplete(ShakeCallBack);
    }
    public void StartMoving()
    {
        moving = true;
        dustVFX.Play();
        shakeTween.Play();

        nodeNumber++;
        if (nodeNumber >= patrolNodes.Count)
        {
            nodeNumber = 0;
        }
        target.position = patrolNodes[nodeNumber].transform.position;

        numberOfMovementAttempts = 0;
        MoveOneNode(); //start the movement
        remainingDistanceNew = aiPath.remainingDistance;
        remainingDistanceOld = aiPath.remainingDistance;
        StartCoroutine(WaitUntilMovementOver());
        StartCoroutine(NoticeIfBlocked());
    }
    private ABPath MakePathToTarget()
    {
        // Create a new Path object
        path = ABPath.Construct(transform.position, target.position, null);

        // Make the path use a specific traversal provider
        path.traversalProvider = traversalProvider;

        // Calculate the path synchronously
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (path.error)
        {
            Debug.Log("No path was found");
        }
        else
        {
            Debug.Log("A path was found with " + path.vectorPath.Count + " nodes");

            // Draw the path in the scene view
            for (int i = 0; i < path.vectorPath.Count - 1; i++)
            {
                Debug.DrawLine(path.vectorPath[i], path.vectorPath[i + 1], Color.red, 1); //add number at end to increase time on screen
            }
        }
        return path;
    }
    private void MoveOneNode()
    {
        MakePathToTarget(); //pathfind

        if (path.error == false)
        {
            if (path.vectorPath.Count >= 2)
            {
                //Debug.LogError("Starting moving");
                //aiTarget.transform.position = path.vectorPath[path.vectorPath.Count-1]; //set ai destination to 1 tile in path

                int tempSpeedMax = 1;
                if (tempSpeedMax > path.vectorPath.Count - 1)
                {
                    tempSpeedMax = path.vectorPath.Count - 1;
                }
                aiTarget.transform.position = path.vectorPath[tempSpeedMax];
            }
            else
            {
                aiTarget.transform.position = path.vectorPath[0]; //set ai destination to 1 tile in path 
            }
            aiPath.canMove = true; //let ai move 
        }
    }
    private IEnumerator WaitUntilMovementOver()
    {
        yield return new WaitForSeconds(0.1f);

        if (path.error) //if we can't get a path, try to
        {
            MakePathToTarget();
        }
        if ((aiPath.reachedDestination && MatchPositions(transform.position, target.position))) //finished moving
        {
            Debug.LogError("Reached destination");
            StopAllCoroutines();
            aiPath.canMove = false;

            dustVFX.Stop();
            shakeTween.Pause();
            Tween fixTween = icon.transform.DORotate(new Vector3(rotationAngles.x, rotationAngles.y, rotationAngles.z), .5f);

            moving = false;

            yield break;
        }
        StartCoroutine(WaitUntilMovementOver());
    }

    private bool MatchPositions(Vector3 pos1, Vector3 pos2)
    {
        Vector3 diff = pos1 - pos2;
        diff.x = Mathf.Abs(diff.x);
        diff.z = Mathf.Abs(diff.z);

        if (diff.x < 0.5f && diff.z < 0.5f) //safe check to see if we clicked on army
        {
            return true;
        }
        return false;
    }

    private IEnumerator NoticeIfBlocked()
    {
        yield return new WaitForSeconds(0.1f);
        remainingDistanceNew = aiPath.remainingDistance;

        float diff = remainingDistanceNew - remainingDistanceOld;

        diff = Mathf.Abs(diff);

        if (diff < .02f) //if diff is very low
        {
            numberOfMovementAttempts++;
        }
        if (numberOfMovementAttempts >= 10)
        {
            float fixedCoordsx = RoundToZeroOrHalf(transform.position.x);
            float fixedCoordsz = RoundToZeroOrHalf(transform.position.z);

            aiTarget.transform.position = new Vector3(fixedCoordsx, 0, fixedCoordsz);
        }
        remainingDistanceOld = remainingDistanceNew;
        StartCoroutine(NoticeIfBlocked());
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
