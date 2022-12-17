using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Pathfinding;
using UnityEngine.UI;
using FoW;
using DG.Tweening;
using UnityEngine.VFX;

[HelpURL("http://arongranberg.com/astar/docs/class_blocker_path_test.php")]
public class Army : MonoBehaviour
{
    [Tooltip("Make this army controlled by AI, not the player.")]
    [SerializeField] private bool aiControlled = false;

    [Tooltip("Only fill out if AI controlled.")]
    [SerializeField] private List<Transform> patrolNodes; //AI ONLY
    public string faction = "Altgard";
    public Army detectedNotSpottedArmy;
    public Army focusedOnArmy;
    public Vector3 enemyLastSeenPos; //ai only
    public bool lastPosKnown;
    [SerializeField] private DetectPlayerArmies detector;

    [SerializeField] private int nodeNumber = 0;

    public GameObject parent;
    private OverworldManager overworldManager;
    public List<SingleNodeBlocker> obstacles;
    public Transform target;
    public GameObject targetVisual;
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
    public int turnCounter = 0;
    public int sightRadius = 4;
    public int provisions = 8; //also known as supplies
    public int maxProvisions = 12;
    public int supplyUpkeep = 1;
    public int overallMorale = 8; //army morale
    public int maxMorale = 8;
    public int spoils = 0;
    public int maxSpoils = 20;
    public int starvation = 0;
    public bool garrisoned = false;
    public List<Button> listOfSplitOffs;
    public FogOfWarUnit fowUnit;
    public bool onSupplyPoint = false;
    public SupplyPoint currentSupplyPoint;
    public LocaleInvestigatable currentLocale;
    [SerializeField] private List<ArmyCardScriptableObj> startingArmy;
    [SerializeField] private VisualEffect dustVFX;
    [SerializeField] private GameObject icon;
    private Tween shakeTween;
    [SerializeField] private int maxSpeed = 1;
    public string befestigungName = "Hammer";
    public string oberkommandantName = "Friedrich Weiss";
    [SerializeField] private ArmyCard armyCardPrefab;
    private int startingMaxProv = 8;
    public string size = "Medium";
    public int horses = 0;
    public int predictedSupplyConsumption = 0;

    public List<ArmyCard> cards;
    [SerializeField] private bool checkedForcedMarch = false;

    public int predictedMovementSpaces = 0;
    private int forcedMarchDistance = 4;

    public bool moving = false;

    public bool arrestedDernoth = false;
    public bool arrestedButcher = false;


    public Collider watchdogBounds;
    public bool withinWatchdogBounds = true;
    private bool suddenStop = false;

    public List<UnitInfoClass> unitsInArmyList;

    public void Awake() //Setup when spawned
    {
        //setup the little shake while moving
        shakeTween = icon.transform.DORotate(new Vector3(0, 93, 0), .25f).OnComplete(ShakeCallBack);
        shakeTween.Pause();
        //set still movement
        aiPath.canMove = false;
        //setup managers 
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
        //generate army 
        if (!aiControlled)
        {
            GenerateStartingArmy();
        }
        //update size
        CheckSizeAndChangeSpeed();
        UpdateVisionRange();

        //change team if ai controlled
        if (aiControlled)
        {
            fowUnit.team = 1;
            target.transform.position = patrolNodes[nodeNumber].transform.position; //set destination;
            detector.parentArmy = this;
        }
    }

    public void TriggerEventsAtLocales()
    {
        //Debug.LogError("EVENT");
        if (currentSupplyPoint != null)
        {
            if (currentSupplyPoint.eventDialogue != null && currentSupplyPoint.eventTriggered == false)
            {
                overworldManager.dialogueEvent = true;
                overworldManager.localeArmy = this;
                DialogueManager.Instance.loadedDialogue = currentSupplyPoint.eventDialogue;
                DialogueManager.Instance.StartDialogue();
                currentSupplyPoint.eventTriggered = true;
            }
        }
    }
    private void UpdateVisionRange()
    {
        fowUnit.circleRadius = sightRadius + .5f;

    }
    private void ShakeCallBack()
    {
        shakeTween = icon.transform.DORotate(new Vector3(0, 87, 0), .25f).OnComplete(ShakeCallBack2);
    }
    private void ShakeCallBack2()
    {
        shakeTween = icon.transform.DORotate(new Vector3(0, 93, 0), .25f).OnComplete(ShakeCallBack);
    }
    private void GenerateStartingArmy()
    {
        numberOfUnitsInArmy = 0;
        var x = 0;
        var y = 0;
        foreach (ArmyCardScriptableObj card in startingArmy)
        {
            ArmyCard newCard = Instantiate(armyCardPrefab, overworldManager.leftAnchor.position, Quaternion.identity, overworldManager.armyCompBoxParent);
            newCard.cardName = card.cardName; //take information
            newCard.cardColor = card.cardColor;
            newCard.cardIcon = card.cardIcon;
            newCard.cardTroops = card.cardTroops;
            newCard.cardMaxTroops = card.cardMaxTroops;
            CardVisual visuals = newCard.GetComponent<CardVisual>(); //apply to visuals
            visuals.colorBG.color = newCard.cardColor;
            visuals.cardName.text = newCard.cardName;
            visuals.troopNum.text = newCard.cardTroops + "/" + newCard.cardMaxTroops;
            visuals.icon.sprite = newCard.cardIcon;
            numberOfUnitsInArmy++;
            cards.Add(newCard);
            newCard.transform.position += new Vector3(212 * x, -215 * y, 0);
            x++;
            if (x >= 5)
            {
                y++;
                x = 0;
            }
        }
        availableUnitsInArmy = numberOfUnitsInArmy;
        maxProvisions = startingMaxProv + numberOfUnitsInArmy;
        provisions = maxProvisions;
    }
    public void AddArmyCard(ArmyCardScriptableObj card)
    {
        ArmyCard newCard = Instantiate(armyCardPrefab, overworldManager.leftAnchor.position, Quaternion.identity, overworldManager.armyCompBoxParent);
        newCard.cardName = card.cardName; //take information
        newCard.cardColor = card.cardColor;
        newCard.cardIcon = card.cardIcon;
        newCard.cardTroops = card.cardTroops;
        newCard.cardMaxTroops = card.cardMaxTroops;

        CardVisual visuals = newCard.GetComponent<CardVisual>(); //apply to visuals
        visuals.colorBG.color = newCard.cardColor;
        visuals.cardName.text = newCard.cardName;
        visuals.troopNum.text = newCard.cardTroops + "/" + newCard.cardMaxTroops;
        visuals.icon.sprite = newCard.cardIcon;
        numberOfUnitsInArmy++;
        cards.Add(newCard);
        AlignArmyCards();
    }
    private void AlignArmyCards()
    {
        var x = 0;
        var y = 0;
        foreach (ArmyCard card in cards)
        {
            card.transform.position += new Vector3(212 * x, -215 * y, 0);
            x++;
            if (x >= 5)
            {
                y++;
                x = 0;
            }
        } 
    }
    public void CheckSizeAndChangeSpeed()
    {
        if (aiControlled)
        {
            if (numberOfUnitsInArmy <= 6) //small
            {
                speedMax = 3;
                supplyUpkeep = 1;
                size = "Small";
            }
            else if (numberOfUnitsInArmy <= 12) //med
            {
                speedMax = 3;
                supplyUpkeep = 2;
                size = "Medium";
            }
            else if (numberOfUnitsInArmy <= 18) //large
            {
                speedMax = 2;
                supplyUpkeep = 3;
                size = "Large";

            }
            else if (numberOfUnitsInArmy > 18) //full army
            {
                speedMax = 1;
                supplyUpkeep = 4;
                size = "Full";
            }
            return;
        }

        if (numberOfUnitsInArmy <= 6) //small
        {
            speedMax = 6;
            supplyUpkeep = 1;
            size = "Small";
        }
        else if (numberOfUnitsInArmy <= 12) //med
        {
            speedMax = 4;
            supplyUpkeep = 2;
            size = "Medium";
        }
        else if (numberOfUnitsInArmy <= 18) //large
        {
            speedMax = 2;
            supplyUpkeep = 3;
            size = "Large";

        }
        else if (numberOfUnitsInArmy > 18) //full army
        {
            speedMax = 1;
            supplyUpkeep = 4;
            size = "Full";
        }
        /*else //scout. so far no way to trigger this
        {
            speedMax = 6;
            supplyUpkeep = 1;
            sightRadius = 8;
            fowUnit.circleRadius = 8;
            size = "Scout";
        }*/
    } 
    private void OnTriggerEnter(Collider other)
    {
        if (!aiControlled)
        {
            SurpriseEvent surprise = other.gameObject.GetComponent<SurpriseEvent>();
            if (surprise != null)
            {
                if (surprise.eventDialogue != null && surprise.eventTriggered == false)
                {
                    suddenStop = true;
                    numberOfMovementAttempts = 100; //stop the movement of player
                    overworldManager.dialogueEvent = true;
                    overworldManager.localeArmy = this;
                    DialogueManager.Instance.loadedDialogue = surprise.eventDialogue;
                    DialogueManager.Instance.StartDialogue();
                    surprise.eventTriggered = true;
                }
            }
        } 
        Army collidedArmy = other.gameObject.GetComponent<Army>();
        //Debug.LogError("collision?");
        if (awaitingCollisionWith != null)
        {
            if (collidedArmy == awaitingCollisionWith)
            {
                //Debug.LogError("First army collided with second army");'
                collidedArmy.numberOfUnitsInArmy += numberOfUnitsInArmy;
                awaitingCollisionWith = null;
                overworldManager.armies.Remove(this);
                Destroy(parent);
            }
        }
        SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>();
        if (collidedSupplyPoint != null)
        {
            onSupplyPoint = true;
            currentSupplyPoint = collidedSupplyPoint;
            collidedSupplyPoint.armyOnThisSupplyPoint = this;
            if (overworldManager.selectedArmy == this)
            {
                overworldManager.PlayerBattleGroupEnteredSupplyPoint();
            }
        }
        if (!aiControlled && collidedArmy != null && collidedArmy.faction != faction)
        {
            Debug.Log("WAR");
            //OverworldToFieldBattleManager.Instance.StartFieldBattleWithEnemyBattleGroup(collidedArmy);
            //numberOfMovementAttempts = 100;
            //collidedArmy.numberOfMovementAttempts = 100;
        } 
        LocaleInvestigatable collidedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
        if (collidedLocale != null)
        {
            currentLocale = collidedLocale;
        } 
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = true;
        } 
    }
    private void OnTriggerExit(Collider other)
    {
        LocaleInvestigatable exitedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
        if (exitedLocale != null)
        {
            suddenStop = false;
            currentLocale = null;
        }
        SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>();
        if (collidedSupplyPoint != null)
        {
            onSupplyPoint = false;
            currentSupplyPoint = null;
            collidedSupplyPoint.armyOnThisSupplyPoint = null;

            if (overworldManager.selectedArmy == this)
            {
                overworldManager.PlayerBattleGroupExitedSupplyPoint();
            }
        }
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = false;
        } 
    }
    public void StartMoving()
    {
        moving = true;
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
        StartCoroutine(NoticeIfBlocked());
    }
    private void UpkeepTrigger()
    {
        turnCounter++;
        if (turnCounter % 2 == 0)
        {
            turnCounter = 0;
            ConsumeUpkeep();
        }

    }
    private void ConsumeUpkeep()
    {
        int requiredProvisions = 0;
        while (requiredProvisions < supplyUpkeep)
        {
            requiredProvisions++;
            provisions--;
            if (provisions < 0)
            {
                provisions = 0;
                starvation++;
            }
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
            //Debug.Log("No path was found");
        }
        else
        {
            //Debug.Log("A path was found with " + path.vectorPath.Count + " nodes");

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
            //Debug.Log("No path was found");
        }
        else
        {
            //Debug.Log("A path was found with " + path.vectorPath.Count + " nodes");

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
                //aiTarget.transform.position = path.vectorPath[path.vectorPath.Count-1]; //set ai destination to 1 tile in path

                int tempSpeedMax = speedMax;
                if (tempSpeedMax > path.vectorPath.Count - 1)
                {
                    tempSpeedMax = path.vectorPath.Count - 1;
                }
                //Debug.LogError(tempSpeedMax);
                aiTarget.transform.position = path.vectorPath[tempSpeedMax];
            }
            else
            {
                //Debug.LogError("");
                aiTarget.transform.position = path.vectorPath[0]; //set ai destination to 1 tile in path
                speedCurrent = speedMax; //stop movement
            }
            aiPath.canMove = true; //let ai move
            speedCurrent++; //increment speed = moving 1 space
        }
    }
    private IEnumerator WaitUntilMovementOver()
    {
        yield return new WaitForSeconds(0.1f);

        if (path.error) //if we can't get a path, try to
        {
            MakePathToTarget();
        }
        if (aiControlled)
        {
            if (aiPath.reachedDestination)
            {
                suddenStop = false;
                StopAllCoroutines();
                aiPath.canMove = false;
                dustVFX.Stop();
                shakeTween.Pause();
                Tween fixTween = icon.transform.DORotate(new Vector3(0, 90, 0), .5f);
                predictedMovementSpaces = 0;
                moving = false;

                yield break;
            }
        }
        else
        {
            if ((aiPath.reachedDestination && MatchPositions(transform.position, target.position)) || suddenStop) //finished moving
            {
                suddenStop = false;
                StopAllCoroutines();
                aiPath.canMove = false;
                dustVFX.Stop();
                shakeTween.Pause();
                Tween fixTween = icon.transform.DORotate(new Vector3(0, 90, 0), .5f);
                predictedMovementSpaces = 0;
                moving = false;
                OverworldManager.Instance.HideNavIndicator();

                yield break;
            }
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
            if (checkedForcedMarch == false)
            {
                float traversed = predictedMovementSpaces - aiPath.remainingDistance;
                int traversedInt = Mathf.CeilToInt(traversed);

                if (traversedInt >= forcedMarchDistance)
                {
                    ConsumeUpkeep();
                }
                checkedForcedMarch = true;
            }
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