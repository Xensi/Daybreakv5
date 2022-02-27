using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using System.Collections;
using UnityEngine.UI;
using FoW;

[HelpURL("http://arongranberg.com/astar/docs/class_blocker_path_test.php")]
public class Army : MonoBehaviour
{
    public GameObject parent;
    private OverworldManager overworldManager;

    public List<SingleNodeBlocker> obstacles;
    public Transform target;
    public AIPath aiPath;
    public GameObject aiTarget;
    public int speedMax = 4;
    public int speedCurrent = 0;

    public BlockManager blockManager;
    BlockManager.TraversalProvider traversalProvider;

    public ABPath path;
    public int numberOfUnitsInArmy = 10;
    public int availableUnitsInArmy = 10;

    public float remainingDistanceNew = 0;
    public float remainingDistanceOld = 0;

    public int numberOfMovementAttempts = 0;

    public Collider armyCollider;
    public Army awaitingCollisionWith;

    public List<int> strengthToSplitOff;
    public List<Vector3> destinationForSplitOff;
    public List<bool> actuallySplitOffOrNot;
    public List<GameObject> indicators;

    public int timePassed = 0;

    public int sightRadius = 4;
    public int provisions = 8;

    public List<Button> listOfSplitOffs;
    public FogOfWarUnit fowUnit;

    public void Awake() //Setup when spawned
    {
        aiPath.canMove = false;
        if (overworldManager == null)
        {
            var oManager = GameObject.FindWithTag("OverworldManager");
            overworldManager = oManager.GetComponent<OverworldManager>();
        }

        if (blockManager == null)
        {
            var manager = GameObject.FindWithTag("BlockManager");
            blockManager = manager.GetComponent<BlockManager>();
        }

        // Create a traversal provider which says that a path should be blocked by all the SingleNodeBlockers in the obstacles array
        traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.OnlySelector, obstacles);

        CheckSizeAndChangeSpeed();
    }

    public void Start()
    {
        availableUnitsInArmy = numberOfUnitsInArmy;
    }
    public void SpawnSplitArmy()
    {
        for (int i = 0; i < strengthToSplitOff.Count; i++)
        {
            if (actuallySplitOffOrNot[i])
            {
                Army army = overworldManager.SpawnArmy(transform.position); //create the army
                army.numberOfUnitsInArmy = strengthToSplitOff[i]; //give it correct strength
                numberOfUnitsInArmy -= strengthToSplitOff[i]; //subtract strength

                if (army.target != null)
                {
                    army.target.position = destinationForSplitOff[i]; //give it a path to the clicked position
                    ABPath path = army.DrawPath();
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogError("collision?");
        if (awaitingCollisionWith != null)
        {
            Army collidedArmy = other.gameObject.GetComponent<Army>();
            if (collidedArmy == awaitingCollisionWith)
            {
                //Debug.LogError("First army collided with second army");'
                collidedArmy.numberOfUnitsInArmy += numberOfUnitsInArmy;
                awaitingCollisionWith = null;
                overworldManager.armies.Remove(this);
                Destroy(parent);
            }
        }
    }

    public void Update()
    {
    }

    public void StartMoving()
    {
        timePassed = 0;
        speedCurrent = 0;
        numberOfMovementAttempts = 0;
        CheckSizeAndChangeSpeed();
        MoveOneNode();
        remainingDistanceNew = aiPath.remainingDistance;
        remainingDistanceOld = aiPath.remainingDistance;
        StartCoroutine(WaitUntilMovementOver());
        StartCoroutine(NoticeIfBlocked());
    }
    public void CheckSizeAndChangeSpeed()
    {
        if (numberOfUnitsInArmy >= 24) //large
        {
            speedMax = 2;
        }
        else if (numberOfUnitsInArmy >= 12) //med
        {
            speedMax = 4;
        }
        else if (numberOfUnitsInArmy >= 6) //small
        {
            speedMax = 5;
        }
        else //scout
        {
            speedMax = 6;
            sightRadius = 8;
            fowUnit.circleRadius = 8;
        }
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
                Debug.LogError("Starting moving");
                //aiTarget.transform.position = path.vectorPath[path.vectorPath.Count-1]; //set ai destination to 1 tile in path

                int tempSpeedMax = speedMax;
                if (tempSpeedMax > path.vectorPath.Count - 1)
                {
                    tempSpeedMax = path.vectorPath.Count - 1;
                }
                aiTarget.transform.position = path.vectorPath[tempSpeedMax];
            }
            else
            {
                aiTarget.transform.position = path.vectorPath[0]; //set ai destination to 1 tile in path
                speedCurrent = speedMax; //stop movement
            }
            aiPath.canMove = true; //let ai move
            speedCurrent++; //increment speed = moving 1 space
        }
    }
    private IEnumerator WaitUntilMovementOver()
    {
        yield return new WaitForSeconds(0.01f);
        if (path.error) //if we can't get a path, try to
        {
            MakePathToTarget();
        }

        if (aiPath.reachedDestination)
        {
            //StopCoroutine(NoticeIfBlocked());
            StopAllCoroutines();
            aiPath.canMove = false;
            yield break;
        }
        /*if (aiPath.reachedDestination && speedCurrent >= speedMax)
        {
            aiPath.canMove = false; //stop ai movement
            yield break;
        }*/
        /*else if (aiPath.reachedDestination && path.vectorPath.Count >= 2 && speedCurrent < speedMax) //if reached destination and still more tiles to move to and hasn't exceeded max movement
        {
            MoveOneNode(); //move another node

        }*/
        StartCoroutine(WaitUntilMovementOver());
    }
    private  IEnumerator NoticeIfBlocked()
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
            //StopCoroutine(WaitUntilMovementOver());
            //aiPath.canMove = false;

            float fixedCoordsx = RoundToZeroOrHalf(transform.position.x);
            float fixedCoordsz = RoundToZeroOrHalf(transform.position.z);

            aiTarget.transform.position = new Vector3(fixedCoordsx, 0, fixedCoordsz);

            //yield break;
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