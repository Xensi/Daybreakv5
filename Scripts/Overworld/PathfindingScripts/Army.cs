using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using DG.Tweening;
using System.Collections;

[HelpURL("http://arongranberg.com/astar/docs/class_blocker_path_test.php")]
public class Army : MonoBehaviour
{
    public BlockManager blockManager;
    public List<SingleNodeBlocker> obstacles;
    public Transform target;
    private IObjectTweener tweener;
    public AIPath aiPath;
    public GameObject aiTarget;
    public int speedMax = 4;
    public int speedCurrent = 0;
    BlockManager.TraversalProvider traversalProvider;

    public ABPath path;

    public void Start()
    {
        aiPath.canMove = false;
        tweener = GetComponent<IObjectTweener>();
        // Create a traversal provider which says that a path should be blocked by all the SingleNodeBlockers in the obstacles array
        traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.OnlySelector, obstacles);
    }

    public void Update()
    {
    }

    public void StartMoving()
    {
        speedCurrent = 0;
        MoveOneNode();
        StartCoroutine(WaitUntilMovementOver());
    }
    public ABPath DrawPath()
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
                Debug.DrawLine(path.vectorPath[i], path.vectorPath[i + 1], Color.green, 1); //add number at end to increase time on screen
            }
        }
        return path;
    }
    private void MakePathToTarget()
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
    }

    private void MoveOneNode()
    {
        MakePathToTarget(); //pathfind
        if (path.vectorPath.Count >= 2)
        {
            aiTarget.transform.position = path.vectorPath[1]; //set ai destination to 1 tile in path
        }
        else
        {
            aiTarget.transform.position = path.vectorPath[0]; //set ai destination to 1 tile in path
            speedCurrent = speedMax; //stop movement
        }
        aiPath.canMove = true; //let ai move
        speedCurrent++; //increment speed = moving 1 space
    }
    private IEnumerator WaitUntilMovementOver()
    {
        yield return new WaitForSeconds(0.01f);

        if (aiPath.reachedDestination && speedCurrent >= speedMax)
        {
            aiPath.canMove = false; //stop ai movement
            yield break;
        }
        else if (aiPath.reachedDestination && path.vectorPath.Count >= 2 && speedCurrent < speedMax) //if reached destination and still more tiles to move to and hasn't exceeded max movement
        {
            MoveOneNode(); //move another node

        }
        StartCoroutine(WaitUntilMovementOver());
    }
}