using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Threading.Tasks;
using System.Threading;
public class FormationPosition : MonoBehaviour
{

    private CancellationTokenSource cancelToken;
    [HideInInspector] public FormationPosition formationToFollow;


    #region Generic 
    [HideInInspector] public enum FormationType
    {
        Infantry,
        RangedInfantry,
        Cavalry,
        RangedCavalry,
        SpearInfantry
    }
    public GlobalDefines.SoldierTypes soldierType = GlobalDefines.SoldierTypes.conscript;
    #endregion

    #region MustBeSet 

    [HideInInspector] public Rigidbody rigid;
    [SerializeField] private NavmeshCut navCutter;
    [SerializeField] private bool simultaneousPositionCheck = false;
    public SoldierBlock soldierBlock;
    [Tooltip("Checks nearby formations. Nearby formations can be moved towards automatically.")]
    public float engageEnemyRadius = 10;
    public float rangedRadius = 100;
    public SpriteRenderer farAwayIcon;
    public Collider cameraCollider;
    public SpriteRenderer routingIcon;
    public SpriteRenderer shatteredIcon;
    public GameObject farAwayIconMask;
    public GameObject formationIconsParent;
    //public bool usesSpears = true;
    public FormationType formationType = FormationType.Infantry; //just by default
    #endregion

    #region AutoSet
    public ShakeSource shaker;
    public FightManager fightManager;

    #endregion

    #region AISetters
    [SerializeField] private float AIIntelligence = 50;
    [SerializeField] private float aiBraceRadius = 75;
    #endregion

    #region Status 
    [HideInInspector] public bool routing = false;
    public bool chargeRecharged = true;
    [HideInInspector] public bool alive = true;
    [HideInInspector] public bool abilityCharged = true;
    [HideInInspector] public int shotsHit = 0;
    public bool showSoldierModels = true;
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;
    public List<FormationPosition> listOfNearbyEnemies;
    [HideInInspector] public float startingPursueRadius = 0; //do not modify manually
    public FormationPosition enemyFormationToTarget;
    [Tooltip("Should we try to attack nearby enemies?")]
    [HideInInspector] public bool holdFire = false;
    [HideInInspector] public bool chaseDetectedEnemies = true;
    [HideInInspector] public bool engagedInMelee = false;
    [HideInInspector] public bool focusFire = false; //should we pick targets automatically or fire on a specific place/unit
    [HideInInspector] public bool deployedPikes = false;
    [HideInInspector] public bool modelAttacked = false;
    [HideInInspector] public bool modelTookDamage = false;
    [HideInInspector] public bool inCombat = false;
    [HideInInspector] public bool modelAttacking = false;
    [HideInInspector] public bool playingIdleChatter = false;
    [HideInInspector] public bool playingAttackChatter = false;
    [HideInInspector] public bool playingDeathReactionChatter = false;
    [HideInInspector] public bool playingMarchChatter = false;
    [HideInInspector] public Vector3 focusFirePos = new Vector3(0, 0, 0);
    [HideInInspector] public FormationPosition formationToFocusFire;
    public bool movementManuallyStopped = false;
    [HideInInspector] public List<Vector3> destinationsList = new List<Vector3>();
    [HideInInspector] public bool finishedChangedFacing = true;
    [HideInInspector] public float averagePositionBasedOnSoldierModels = 0;
    [HideInInspector] public Vector3 formationPositionBasedOnSoldierModels;
    public bool charging = false;
    [HideInInspector] public bool selectable = true;
    public bool braced = false;
    [HideInInspector] public bool enableAnimations = false;
    [HideInInspector] public float walkingSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    [HideInInspector] public bool selected = false;
    #endregion

    #region AAA
    [SerializeField] private float abilityRechargeTime = 60f;
    [SerializeField] private float currentAbilityRechargeTime = 0;
    public bool allowedToCastMagic = true;
    public float timeUntilAllowedToCastMagicAgain = 0;
    public bool canBrace = false;
    [HideInInspector] public RichAI aiPath; //must be rich ai for collisions
    private float threshold = .5f;
    public Transform aiTarget;
    public Transform rotTarget;
    #endregion

    #region EEE
    [Tooltip("When to stop moving when auto-engaging.")]
    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;
    private bool oldEnableAnimations = false;
    public float movingSpeed = 0;
    [SerializeField] private float currentSpeed = 0;
    public LineRenderer lineRenderer;
    public LineRenderer lineRenderer2;
    [SerializeField] private bool weaponsDeployed = false;
    private bool oldWeaponsDeployed = false;
    [SerializeField] private float waitThreshold = 1;
    [SerializeField] private float offsetThreshold = 4; //when remaining distance above this threshold
    [SerializeField] private Transform posParentTransform;
    [SerializeField] private Vector3 posParentStartingPos;
    [SerializeField] private Vector3 offsetAmount;
    [SerializeField] private Vector3 deployedOffsetAmount;
    [SerializeField] private float requiredVelocity = 2;
    [SerializeField] private float deployedRequiredVelocity = 4;
    [SerializeField] private float finishedPathRotSpeed = 1;
    public bool pathSet = false;
    public List<Position> frontlinePositions;
    public bool obeyingMovementOrder;
    public int numberOfAliveSoldiers = 80;
    public int maxSoldiers = 80;
    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;
    [SerializeField] private float normRotate = 30;
    [SerializeField] private float secondRowOffsetAmount = 0f;
    public BoxCollider formationCollider; 
    [SerializeField] private bool freezeFormPos = false;
    [SerializeField] private float freezeTimer = 0;
    [SerializeField] private float cohesionTimer = 0;
    public bool shouldRotateToward = false;
    [SerializeField] private int soldierModelToCheck = 0;
    [SerializeField] private bool checkForNearbyEnemies = true;
    [SerializeField] private bool swapRowsAfterFiring = false;
    [SerializeField] private int requiredModelsThatFiredInRow = 5; //all of them
    [SerializeField] private int matched = 0;
    [SerializeField] private int lowerRow = 0;
    [SerializeField] private int upperRow = 9;
    [SerializeField] private List<SoldierModel> modelsInFrontRowThatFired;
    public GameObject missileTarget;
    [SerializeField] private bool alwaysRotateTowardMovementPos = false;
    public SpriteRenderer selectedSprite;
    [SerializeField] private float chargeSpeed = 7;
    [SerializeField] private float maxChargeTime = 20;
    [SerializeField] private float currentChargeTime = 0;
    [SerializeField] private float chargeRechargeTime = 60;
    private float currentChargeRechargeTime = 0;
    #endregion


    [SerializeField] private float maxToleratedDeaths = 30;
    [SerializeField] private int deathsThisTimeFrame = 0;
    private float timeFrame = 30;
    [SerializeField] private int shatterThreshold = 15;
    [SerializeField] private int hardRoutOnSoftRoutThreshold = 40;

    public int numKills = 0;

    public float stamina = 2000;
    public float maxStamina = 2000;
    private float originalAttackTime;
    private void Start()
    { 
        Color color = farAwayIcon.color;
        color.a = 0;
        farAwayIcon.color = color;
        frontIcon.color = color;
        if (fightManager == null)
        {
            fightManager = FightManager.Instance;
            //fightManager = FindObjectOfType<FightManager>();
        }
        if (shaker == null)
        {
            shaker = GetComponentInChildren<ShakeSource>();
        }
        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody>();
        }
        if (aiPath == null)
        {
            aiPath = GetComponent<RichAI>();
        }
        rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigid.drag = 50;


        ShowSelected(false);
        PlaceThisOnGround();
        int FormIcon = LayerMask.NameToLayer("FormIcon");
        lineRenderer.gameObject.layer = FormIcon;
        lineRenderer2.gameObject.layer = FormIcon;


        float starting = 15;
        transform.position = new Vector3(transform.position.x, starting, transform.position.z);
        aiTarget.position = transform.position;
        PlaceAITargetOnTerrain();


        threshold = 1;

        SetStandardEngagementRanges();

        requiredModelsThatFiredInRow = 5;

        lineRenderer2.SetPosition(0, new Vector3(transform.position.x, -100, transform.position.z));
        lineRenderer2.SetPosition(1, new Vector3(transform.position.x, -100, transform.position.z));
        currentSpeed = walkingSpeed;
        chargeSpeed = walkingSpeed * 2; // * 3;
        aiPath.maxSpeed = currentSpeed;
        originalToleratedDeaths = maxToleratedDeaths;
        //PlaceAITargetOnTerrain(); 
        if (formationType == FormationType.Cavalry)
        {
            //formationCollider.enabled = false;
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null)
                { 
                    soldierBlock.modelsArray[i].ToggleAttackBox(true);
                }
            }

        }
        originalToleratedDeaths = maxToleratedDeaths;
    } 
    private void OnEnable()
    {
        cancelToken = new CancellationTokenSource();
    }
    public bool updatesBegun = false;
    public void BeginUpdates()
    {
        if (updatesBegun)
        {
            return;
        }
        Debug.Log("Beginning Updates: " + team);
        //return;
        //PathfindingUpdate(cancelToken.Token); //in parallel //major fps improvement when removed, due to lack of pathfinding required since dest not set

        FastUpdate(100, cancelToken.Token); //minor fps improvement when removed

        ReinforceUpdate(25, cancelToken.Token); //cycles through positions //very minor fps improvement when removed
        //CheckEnemyUpdate(10, cancelToken.Token); //cycles through soldiers 1 by one

        SlowUpdate(500, cancelToken.Token); //no real fps improvement
        VerySlowUpdate(1000, cancelToken.Token); //no real improvement
        InvokeRepeating("TimeFrameAdvance", 0, timeFrame);


        //InvokeRepeating("LockSoldiers", 0, lockTime);
        //InvokeRepeating("LockSoldiersToTerrain", 0, terrainLockTime);
        //InvokeRepeating("UpdateFarAwayIconPos", 0, .1f);
        updatesBegun = true;
    }

    #region FastUpdate 
    private async void FastUpdate(int time, CancellationToken cancelToken)
    {
        FixFormationRotation();
        UpdateFormationMovementStatus();

        CalculateMovingSpeed();
        float magic = 15;
        transform.position = new Vector3(transform.position.x, magic, transform.position.z);
        FastSoldierUpdate();
        FollowFormation();
        UpdateStaminaFormation(time);

        //LockSoldiers();

        UpdateFarAwayIconPos();

        await Task.Delay(time, cancelToken);
        FastUpdate(time, cancelToken);
    }

    private void CalculateMovingSpeed()
    {
        if (aiPath != null)
        { 
            movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector 
        }
        float min = .01f;
        if (movingSpeed < min)
        {
            movingSpeed = 0;
        }
        if (movingSpeed == 0 && charging)
        {
            StopCharging();
        }
    }
    private async void FixFormationRotation()
    {
        if (!aiPath.canMove && !obeyingMovementOrder && !tangledUp && shouldRotateToward)
        {
            Vector3 targetDirection = rotTarget.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            newDirection.y = 0; //so that our rotation is not vertical
            //Debug.DrawRay(transform.position, newDirection, Color.red);

            transform.rotation = Quaternion.LookRotation(newDirection);

        }
        float angle = 5;
        if (Vector3.Angle(transform.forward, rotTarget.position - transform.position) < angle && !aiPath.canMove)
        {
            finishedChangedFacing = true;
        }
        else
        {
            finishedChangedFacing = false;
        }
        await Task.Yield();
    }

    private async void UpdateFormationMovementStatus()
    {
        float remainingDistance = Vector3.Distance(formationPositionBasedOnSoldierModels, aiTarget.transform.position);

        if (movementManuallyStopped)
        {
            SetMoving(false);
            obeyingMovementOrder = false;
        }
        else
        {
            if (chaseDetectedEnemies && enemyFormationToTarget != null && soldierBlock.melee)
            {
                SetMoving(true);
            }
            else
            {
                if (modelAttacking && !soldierBlock.melee) //if ranged and attacking, freeze formation
                {
                    SetMoving(false);
                    obeyingMovementOrder = false;
                }
                else
                {
                    if (remainingDistance > threshold) // if there's still path to traverse
                    {
                        SetMoving(true);
                    }
                    else if (aiPath.reachedEndOfPath && remainingDistance <= threshold) //aiPath.reachedDestination && 
                    {
                        if (destinationsList.Count <= 1)
                        {
                            SetMoving(false);
                            obeyingMovementOrder = false;
                        }
                        else if (destinationsList.Count > 1)
                        {
                            destinationsList.RemoveAt(0);
                            if (destinationsList.Count > 0)
                            {
                                //Debug.Log("setting target to destination 1");
                                aiTarget.transform.position = destinationsList[0];
                                CheckIfRotateOrNot();
                            }
                        }
                    }
                }
            }
        }
        await Task.Yield();
    }
    private async void FastSoldierUpdate() //doesn't like being parallel- crashes
    {

        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateModelState(cancelToken.Token);
                    model.CheckForPendingDamage();
                    ////model.UpdateVisibility(); 
                }
            }
        }
        await Task.Yield();
    }
    #endregion
    private void TimeFrameAdvance()
    {
        deathsThisTimeFrame = 0;
    }
    private void OnDisable()
    {
        cancelToken.Cancel();
    }
    public void CancelTasks()
    {
        cancelToken.Cancel();
    }
    #region Updates 
    #region Async
    /*private async void CheckEnemyUpdate(int time, CancellationToken cancelToken)
    {
        CheckModelsIndividually();
        await Task.Delay(time, cancelToken);
        CheckEnemyUpdate(time, cancelToken);
        await Task.Yield();
    }*/
    private async void CheckModelsIndividually()
    {
        if (numberOfAliveSoldiers <= 0)
        {
            return;
        }
        SoldierModel checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
        if (checkingModel == null)
        {
            while (checkingModel == null || !checkingModel.alive) //let us skip over those that have died
            {
                if (numberOfAliveSoldiers <= 0)
                {
                    break;
                }
                soldierModelToCheck++;
                if (soldierModelToCheck >= maxSoldiers) //reset on 80
                {
                    soldierModelToCheck = 0;
                }
                checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
            }
        }

        if (checkingModel != null && checkingModel.alive)
        {
            if (checkingModel.attackType == SoldierModel.AttackType.Melee) //melee should not need an enemyformationtotarget to detect nearby models
            {
                if (!checkingModel.HasTargetInRange())
                {
                    if (checkingModel.currentModelState == SoldierModel.ModelState.Moving || checkingModel.currentModelState == SoldierModel.ModelState.Idle)
                    {
                        //checkingModel.CheckIfEnemyModelsNearby();
                        checkingModel.CheckIfDamageablesNearby();
                    }
                }
            }
            checkingModel.SaveFromFallingInfinitely();
            //checkingModel.UpdateDestination();
        }
        soldierModelToCheck++;
        int max = maxSoldiers;
        if (soldierModelToCheck >= max) //reset on 80 + 2
        {
            soldierModelToCheck = 0;
        }
        await Task.Yield();
    }
    private int pathfindingUpdateCurrentFrequency = 500; 

    public void RapidUpdateDestinations()
    {
        //pathfindingUpdateCurrentFrequency = pathfindingUpdateFrequencyMin;
    }
    public void SetDestAndSearchPath()
    {
        //Debug.Log("Searching: " + team);
        aiPath.destination = aiTarget.position;
        aiPath.SearchPath();
    }
    /*private void AISearchPath() 
    { 
        if (!aiPath.reachedDestination)
        {
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        model.pathfindingAI.SearchPath();
                    }
                }
            }
        } 
    }*/
    /*private async void PathfindingUpdate(CancellationToken cancelToken)
    {
        UpdatePathsOfSoldierModels();
        await Task.Delay(pathfindingUpdateCurrentFrequency, cancelToken);
        *//*if (pathfindingUpdateCurrentFrequency < pathfindingUpdateFrequencyCap)
        {
             pathfindingUpdateCurrentFrequency += pathfindingUpdateFrequencyIncrease;
             pathfindingUpdateCurrentFrequency = Mathf.Clamp( pathfindingUpdateCurrentFrequency, pathfindingUpdateFrequencyMin, pathfindingUpdateFrequencyCap);
            if (team == GlobalDefines.Team.Altgard)
            {
                Debug.Log(pathfindingUpdateCurrentFrequency);
            }
        }*//*
        PathfindingUpdate(cancelToken);
        await Task.Yield();
    }*/
    /*public void ForceUpdateSoldiersDestinations()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateDestinationPosition();
                }
            }
        }
    }*/
    /*private async void UpdatePathsOfSoldierModels()
    {
        //float threshold = 1;
        *//*Parallel.For(0, soldierBlock.modelsArray.Length, i =>
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) //  && model.CheckIfRemainingDistanceOverThreshold(threshold) //&& !model.pathfindingAI.pathPending
                {
                    model.UpdateDestinationPosition();
                }
            }
        });*//*
        if (!aiPath.reachedDestination)
        {
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        model.UpdateDestinationPosition();
                    }
                }
            }
        }
        *//*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) // && model.CheckIfRemainingDistanceOverThreshold(threshold)
                {
                    model.UpdateDestinationPosition();
                }
            }
        }*//*
        await Task.Yield();
    }*/
    /*private async void ModelRotationUpdate(int time, CancellationToken cancelToken)
    {
        FixModelRotation();
        await Task.Delay(time, cancelToken);
        ModelRotationUpdate(time, cancelToken);
    }
    private async void FixModelRotation()
    {
        Parallel.For(0, soldierBlock.modelsArray.Length, i => {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.FixRotation();
                }
            }
        });
        await Task.Yield();
    }*/
    private async void ReinforceUpdate(int time, CancellationToken cancelToken)
    {
        ReinforceEmptyPositions();
        await Task.Delay(time, cancelToken);
        ReinforceUpdate(time, cancelToken);
    }
    private async void ReinforceEmptyPositions()
    {
        Position position = soldierBlock.formationPositions[positionToCheck];
        positionToCheck++;
        //PlacePositionOnGround(position);
        int cap = soldierBlock.formationPositions.Length;
        if (positionToCheck >= cap) //reset on 80 + 2
        {
            positionToCheck = 0;
        }

        if (position != null) //found
        {
            //position.PlaceOnGround();
            if (position.assignedSoldierModel == null) //if null, then dead
            {
                position.SeekReplacement();
            }
        }
        if (movingSpeed != 0)
        { 
            Parallel.For(0, soldierBlock.formationPositions.Length, i =>
            {
                Position position = soldierBlock.formationPositions[i];
                if (position != null)
                {
                    position.PlaceOnGround();
                    /*if (position.assignedSoldierModel == null) //if null, then dead
                    {
                        position.SeekReplacement();
                    }*/
                }
            });
        }
        await Task.Yield();
    }
    #endregion 
    #region SlowUpdate
    private async void SlowUpdate(int time, CancellationToken cancelToken)
    {
        if (!routing)
        {
            CheckIfInMeleeRange();
        }

        //LockSoldiersToTerrain();
        await Task.Delay(time, cancelToken);
        SlowUpdate(time, cancelToken);
    }

    /*private void LockSoldiersToTerrain() //called periodically
    {
        if (showSoldierModels)
        {
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel soldier = soldierBlock.modelsArray[i];
                if (soldier != null && soldier.alive && !soldier.pathfindingAI.enabled)
                {
                    soldier.modelPosition.PlaceOnGround();
                }
            }
        }
    }*/
    private async void CheckIfInMeleeRange()
    {
        if (enemyFormationToTarget != null)
        {
            float dist = Vector3.Distance(transform.position, enemyFormationToTarget.gameObject.transform.position);
            if (dist <= stoppingDistance)
            {
                engagedInMelee = true;
            }
            else
            {
                engagedInMelee = false;
            }
        }
        else
        {
            engagedInMelee = false;
        }
        await Task.Yield();
    }
    #endregion
    #region VerySlowUpdate
    private void CalculateShaker()
    {
        if (aiPath.canMove)
        {
            float mod = 0.1f;
            if (shaker != null)
            {
                shaker.shakeIntensity = movingSpeed * mod * shaker.shakeModifier;
            }
        }
        else if (modelAttacked)
        {
            if (shaker != null)
            {
                shaker.shakeIntensity = shaker.shakeModifier;
            }
        }
        else
        {
            if (shaker != null)
            {
                shaker.shakeIntensity = 0;
            }
        }
    }
    private void UpdateMagicTimer()
    {
        if (timeUntilAllowedToCastMagicAgain > 0)
        {
            timeUntilAllowedToCastMagicAgain--;
            if (timeUntilAllowedToCastMagicAgain <= 0)
            {
                allowedToCastMagic = true;
                fightManager.UpdateGUI();
            }
        }
    }
    private float staminaRegain = 3;
    private float staminaLoss = 15;
    private void UpdateStaminaFormation(int time) //time in ms
    {
        if (!charging && !routing) //regain stamina if not moving fast
        {
            stamina += staminaRegain;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
        else //charging or routing
        {
            //Debug.Log("Changing stamina");
            stamina -= staminaLoss;
            stamina = Mathf.Clamp(stamina, 0, maxStamina); 
        }
        //out of stamina consequences 
        if (stamina <= 0) //if out of stamina must stop
        { 
            if (routing)
            {
                StopRoutingDueToExhaustion();
            }
            if (charging)
            {
                StopCharging(); 
            }
        }
    }
    private async void VerySlowUpdate(int time, CancellationToken cancelToken)
    {
        //CheckModelsBurst();
        /*if (numberOfAliveSoldiers <= 0) //if all soldiers dead, then goodbye
        {
            *//*foreach (FormationPosition item in listOfNearbyEnemies)
            {
                item.listOfNearbyEnemies.Remove(this);

            }*//*
            gameObject.SetActive(false);
            return;
        }*/
        if (formationType == FormationType.RangedInfantry && swapRowsAfterFiring) //only for musketeers
        {
            GeneralCheckIfSwapRows();
        } 
        //CalculateShaker();
        UpdateMagicTimer();
        
         
        if (!routing)
        {
            if (AIControlled || formationType == FormationType.RangedInfantry) //if AI, or a rangedu unit on our side
            {
                Debug.Log("Checking for nearby forms");
                CheckForNearbyEnemyFormations(); //probably expensive
            }
            /*if (enemyFormationToTarget == null || enemyFormationToTarget.numberOfAliveSoldiers <= 0)
            { 
            }*/
            RegainCohesion();
            CheckIfLowSoldiersRout();
            foreach (SoldierModel model in soldierBlock.listMageModels)
            {
                if (model.alive)
                {
                    model.UpdateMageTimer();
                }
            }
            CheckIfInCombat(); //
            UnfreezeThis();
        }
        else //routing!
        { 
            FullUnfreeze();
            BreakCohesion();
            GetMeOutOfHere();
        }
        if (!abilityCharged)
        {
            currentAbilityRechargeTime += 1;

            if (currentAbilityRechargeTime >= abilityRechargeTime)
            {
                abilityCharged = true;
                fightManager.UpdateGUI();
                currentAbilityRechargeTime = 0;
            }
        }
        VerySlowSoldierUpdate(); // not expensive
        UpdateDeployment();
        UpdateSpeed(); // 
        UpdateCollider(); //  
        await Task.Delay(time, cancelToken);
        VerySlowUpdate(time, cancelToken);
    }
    private async void CheckForNearbyEnemyFormations()
    {
        //this block seems unnecessary; uses physics so expensive + getting closest formation doesn't even use the list
        /* listOfNearbyEnemies.Clear();
         LayerMask layerMask = LayerMask.GetMask("Formation");
         int maxColliders = 40;
         Collider[] hitColliders = new Collider[maxColliders];
         int numColliders = Physics.OverlapSphereNonAlloc(transform.position, engageEnemyRadius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage

         numberOfFriendlyFormationsNearby = 0;
         for (int i = 0; i < numColliders; i++)
         {
             if (hitColliders[i].gameObject.tag != "Formation")
             {
                 continue;
             }
             else
             {
                 if (hitColliders[i].gameObject == gameObject) //ignore own collider
                 {
                     continue;
                 }
                 FormationPosition form = hitColliders[i].gameObject.GetComponent<FormationPosition>();
                 if (form.team == team)
                 {
                     numberOfFriendlyFormationsNearby++;
                     continue;
                 }
                 if (form.routing || form.numberOfAliveSoldiers <= 0) //don't count routing or dead formations
                 {
                     continue;
                 }
                 listOfNearbyEnemies.Add(form);
             }
         }*/

        if (obeyingMovementOrder && !AIControlled)
        {
            return;
        }
        enemyFormationToTarget = GetClosestFormationWithinRange(1, team, false, engageEnemyRadius); //grab enemy that isn't routing
        //enemyFormationToTarget = CycleThroughEnemyFormationsAndReturnClosestWithinRangeNotRouting(engageEnemyRadius); //grab enemy that isn't routing

        if (chaseDetectedEnemies && enemyFormationToTarget != null)
        {
            EngageFoe(); //chase enemies
        }
        await Task.Yield();
    }
    public bool AIControlled = false;  
    private FormationPosition GetClosestFormationWithinRange(int targetType = 1, GlobalDefines.Team ourTeam = GlobalDefines.Team.Altgard, bool targetRouting = false, float range = 100) //targettype 0: any, targettype 1: enemy, targettype 2: ally
    {
        if (fightManager.allArray.Length <= 0)
        {
            return null;
        }
        FormationPosition closest = null;

        if (targetType == 0)
        {
            closest = fightManager.allArray[0];
        }
        else if (targetType == 1 || targetType == 2)
        {
            for (int i = 0; i < fightManager.allArray.Length; i++) //go through until we get one that matches criteria
            {
                if (targetType == 1 && fightManager.allArray[i].team != ourTeam)
                {
                    closest = fightManager.allArray[i];
                }
                else if (targetType == 2 && fightManager.allArray[i].team == ourTeam)
                {
                    closest = fightManager.allArray[i];
                }
            }
        }
        if (closest == null)
        {
            return null;
        }
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, closest.transform.position);
        float compareDist = initDist;

        for (int i = 0; i < fightManager.allArray.Length; i++)
        {
            FormationPosition item = fightManager.allArray[i];
            if (targetType == 1 && item.team == ourTeam) //finding enemies but on ally
            {
                continue;
            }
            else if (targetType == 2 && item.team != ourTeam) //finding allies but on enemy
            {
                continue;
            }
            if (!targetRouting && item.routing)
            {
                continue;
            }
            if (!item.enabled || item.numberOfAliveSoldiers <= 0) //skip dead formations
            {
                continue;
            }
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist > range)
            {
                continue;
            }

            //float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
            if (dist < compareDist)
            {
                closest = item;
                compareDist = dist;
            }
        }
        return closest;
    }
    private async void EngageFoe()
    {
        if (routing)
        {
            return;
        }
        float distance = Vector3.Distance(transform.position, enemyFormationToTarget.transform.position);
        if (!soldierBlock.melee && distance <= rangedRadius)
        {
            StopInPlace();
        }
        else if (distance <= stoppingDistance)
        { //stop
            //Debug.Log("stopping");
            ResetAITarget();
        }
        else if (enemyFormationToTarget != null)
        {   //chase
            ChaseFoe();
        }
        await Task.Yield();
    }
    #region oldCode
    /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    //model.UpdateAttackTimer();
                    //model.UpdateLoadTimer();
                    *//*if (model.melee)
                    {
                        *//*if (listOfNearbyEnemies.Count > 0) //only check if enemies are nearby and we're melee
                        {
                            //model.CheckIfEnemyModelsNearby();
                        }*//*
                        //model.StopAttackingWhenNoEnemiesNearby();
                        
                        
                        //model.UpdateDeploymentStatus();
                    }*//*
                    model.UpdateAndCullAnimations();
                    if (model.attackType == SoldierModel.AttackType.Ranged)
                    { 
                        model.rangedModule.LineOfSightUpdate();
                    }
                    //model.CheckIfIdle();
                    //model.CheckIfAlive();
                }
            }
        }*/
    #endregion
    private async void VerySlowSoldierUpdate()
    { 
        Parallel.For(0, soldierBlock.modelsArray.Length, i =>
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) //  && model.CheckIfRemainingDistanceOverThreshold(threshold) //&& !model.pathfindingAI.pathPending
                {
                    //model.UpdateAndCullAnimations();
                    model.CheckIfTargetIsDead(); 

                    if (model.attackType == SoldierModel.AttackType.Ranged)
                    {
                        model.rangedModule.RepeatingUpdateTargetPosition();
                        if (!routing && movingSpeed <= 0)
                        { 
                            model.rangedModule.LineOfSightUpdate();
                        }
                    }
                }
            }
        });
        await Task.Yield();
    }
    #endregion 
    private void UpdateSoldierMovements()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateDestinationPosition();
                    model.pathfindingAI.UpdateMovement();
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (updatesBegun)
        { 
            aiPath.UpdateMovementInFixedUpdate();
        }
    }

    private async void IndicatorUpdateBurst()
    {
        if (selected)
        {
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null && model.alive && model.attackType == SoldierModel.AttackType.Ranged)
                {
                    if (model.lineOfSightIndicator != null)
                    {
                        model.lineOfSightIndicator.transform.LookAt(model.lineOfSightIndicator.transform.position + fightManager.cam.transform.forward);
                    }
                    if (model.reloadingIndicator != null)
                    {
                        model.reloadingIndicator.transform.LookAt(model.reloadingIndicator.transform.position + fightManager.cam.transform.forward);
                    }
                    if (model.rangedModule != null)
                    {
                        model.reloadingIndicator.enabled = model.rangedModule.loadingRightNow;
                    }
                }
            }
        } 
        await Task.Yield();
    }
    private void Update()
    { 
        if (updatesBegun)
        {
            UpdateSoldierMesh();
            UpdateSoldierMovements();
            CheckModelsIndividually();
            UpdateLineRenderer();
            IndicatorUpdateBurst();
            SoldiersFaceEnemyUpdate();
        }
    }
    private void UpdateSoldierMesh()
    { 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.animatedMesh.ManualUpdate();
                }
            }
        }
    }
    private async void SoldiersFaceEnemyUpdate()
    {
        if (formationType != FormationType.Cavalry)
        {
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        if (model.currentModelState != SoldierModel.ModelState.Routing && model.attackType != SoldierModel.AttackType.CavalryCharge)
                        {
                            model.FaceEnemy();
                        }
                    }
                }
            }
        } 
        await Task.Yield();
    }
    private int fastModelCheck = 0; 

    public void SetVisibleInFrustum(bool val) //pretty expensive
    {
        showSoldierModels = val;
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null && soldier.alive)
            {
                soldier.UpdateVisibility(val);
            }
            /*else if (soldier != null)
            {
                soldier.UpdateVisibility(true);
            }*/
        }
        //Debug.Log("showing");
        /*if (showSoldierModels == false && val)
        { 
            TeleportSoldiers();
        }
        */
    }

    /*private void TeleportSoldiers()
    {
        if (!isCavalry && !charging) //nfantry that is not charging gets teleported
        { 
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel soldier = soldierBlock.modelsArray[i];
                if (soldier != null && soldier.alive && soldier.modelPosition != null) //terrain check but don't teleport to position
                { 
                    soldier.PlaceOnGround(); 
                    //soldier.pathfindingAI.enabled = true;
                }
            }
        } 
    } 
    public void ResetFormationPositionsAndSoldiers()
    {
        FixPositions();
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null && soldier.modelPosition != null) //terrain check but don't teleport to position
            {
                soldier.PlaceOnGround(); 
            }
        }
    }*/
    /*public void ToggleFormationSoldiersPathfinding(bool val)
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null) //terrain check but don't teleport to position
            {
                soldier.pathfindingAI.enabled = val; 
            }
        }
    }*/
    /*private void LockSoldiers() //soldiers should use performant mode if not visible
    { 
        //float distanceFromCameraThreshold = QualitySettings.lodBias * 25;
        //float dist = Vector3.Distance(transform.position, fightManager.cam.transform.position);
        //bool farAway = dist > distanceFromCameraThreshold;
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null && soldier.alive && soldier.modelPosition != null)
            {
                //soldier.farAway = farAway;
                if (soldier.formPos.showSoldierModels) //use rich ai
                { 
                    soldier.animator.enabled = true; 
                }
                else //performant
                {
                    //soldier.SwitchAI(SoldierModel.AIToUse.AILerp); 
                    soldier.animator.enabled = false; 
                } 
            }
        } 
    }*/
    private async void UpdateFarAwayIconPos()
    { 
        float height = 0;
        float num = 0;
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null && soldier.alive)
            {
                height += soldier.transform.position.y;
                num++;
            }
        }
        if (num == 0) //prevent NaN
        {
            num = 1;
        }
        height = height / num;
        float avgHeight = height; 
        
        averagePositionBasedOnSoldierModels = height;

        formationPositionBasedOnSoldierModels = new Vector3(transform.position.x, averagePositionBasedOnSoldierModels, transform.position.z);

        formationIconsParent.transform.position = new Vector3(formationIconsParent.transform.position.x, avgHeight+25, formationIconsParent.transform.position.z); //set to average height
        /*if (shatteredIcon != null)
        {

            shatteredIcon.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight + 0.01f, farAwayIconMask.transform.position.z);
        }
        if (farAwayIconMask != null)
        {

            farAwayIconMask.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight, farAwayIconMask.transform.position.z);
        }
        if (routingIcon != null)
        {

            routingIcon.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight + 0.01f, farAwayIconMask.transform.position.z);
        }
        if (selectedSprite != null)
        {
            selectedSprite.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight, farAwayIconMask.transform.position.z);
        }*/
        await Task.Yield();
    } 
    /*private void SetAnimationsModels(bool val)
    { 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            if (soldierBlock.modelsArray[i] != null)
            {
                soldierBlock.modelsArray[i].animate = val; 
            }
        }
    }*/

    
    public bool modelHasAShot = false;
    private async void FollowFormation()
    {
        if (formationToFollow != null && !routing)
        {
            aiTarget.transform.position = formationToFollow.transform.position;
            PlaceAITargetOnTerrain();
            SetDestAndSearchPath();
        }
        await Task.Yield();
    }
    /*public void PlacePositionOnGround(Position itemPos)
    {
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(itemPos.transform.position.x, 100, itemPos.transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            itemPos.transform.position = hit.point;
        }
        //Debug.DrawRay(transform.position, Vector3.down*100, Color.yellow, 1);
    }*/
    private void UpdateDeployment()
    {
        /*if (listOfNearbyEnemies.Count == 0) //no enemy
        {
            weaponsDeployed = false;
        }*/
        if (1 == 1) //yes enemy
        {
            weaponsDeployed = true;
        }
    }
    private async void UpdateSpeed()
    {
        if (routing)
        {
            if (stamina > 0)
            { 
                currentSpeed = chargeSpeed;
            }
            else
            {
                currentSpeed = walkingSpeed;
            }
        }
        else if (frozen)
        {
            currentSpeed = 0;
        }
        else if (tangledUp && soldierBlock.melee)
        {
            float slowAmount = .5f;
            currentSpeed = walkingSpeed * slowAmount; //reduce speed by 25%
            /*float current = numberOfAliveSoldiers;
            float maxSol = maxSoldiers;
            float ratio = current / maxSol;

            currentSpeed *= ratio;*/
            aiPath.rotationSpeed = slowRotate;
        }
        else
        {
            if (!charging)
            {
                currentSpeed = walkingSpeed;
                aiPath.rotationSpeed = normRotate;
            }
            else
            {
                currentSpeed = chargeSpeed;
                aiPath.rotationSpeed = slowRotate;
            }
        }
        float min = 0.1f;
        float max = 100f;
        currentSpeed = Mathf.Clamp(currentSpeed, min, max);

        aiPath.maxSpeed = currentSpeed;
        await Task.Yield();
    }
    private bool frozen = false;
    private bool canBeFrozenAgain = true;
    public void FreezeMovement(float time)
    {
        if (canBeFrozenAgain)
        {
            frozen = true;
            canBeFrozenAgain = false;
            Invoke("UnfreezeMovement", time);
        }
    }
    private void UnfreezeMovement()
    {
        frozen = false;
        float time = 3;
        Invoke("AllowFreezing", time);
    }
    private void AllowFreezing()
    {
        canBeFrozenAgain = true;
    }
    private int numberOfFriendlyFormationsNearby;
    
    #endregion


    public void LoseMorale()
    {
        if (numberOfAliveSoldiers <= shatterThreshold)
        {
            HardRout();
        }
        else
        { 
            deathsThisTimeFrame++;
            if (deathsThisTimeFrame > maxToleratedDeaths + numberOfFriendlyFormationsNearby * 5)
            {  
                SoftRout();
                /*if (numberOfAliveSoldiers <= hardRoutOnSoftRoutThreshold)
                {
                    HardRout();
                }
                else
                { 
                } */
            }
        }
    }
    private void HardRout()
    {
        if (!canRout)
        {
            return;
        }
        shatteredIcon.gameObject.SetActive(true);
        routingIcon.gameObject.SetActive(false);
        StartFleeing();
        //float disappearTime = 30;
        //Invoke("SelfDestruct", disappearTime);
    }
    /*private void SelfDestruct()
    {
        soldierBlock.SelfDestruct();
    }*/
    public void SoftRout(int time = 20) //rout in a direction for some time. after time, check if no enemies melee attacking us. if so, then become controllable again
    {
        if (!canRout)
        {
            return;
        }
        shatteredIcon.gameObject.SetActive(false);
        routingIcon.gameObject.SetActive(true);
        StartFleeing(); 
        Invoke("StopFleeing", time);
    } //maybe stop fleeing if no enemies nearby?
    public bool canRout = true;
    public async void StopFleeing()
    {
        if (numberOfAliveSoldiers <= shatterThreshold && canRout)
        {
            HardRout();
        }
        else
        {
            shatteredIcon.gameObject.SetActive(false);
            routingIcon.gameObject.SetActive(false);
            selectable = true;
            routing = false;
            ShowFormIcon(true);

            aiTarget.transform.position = transform.position;
            PlaceAITargetOnTerrain();
            SetDestAndSearchPath();


            CheckIfRotateOrNot();
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        model.StopRout();
                    }
                }
            }
        }
        await Task.Yield();
    } 
    private void StartFleeing()
    {
        if (!canRout)
        {
            return;
        }
        formationToFollow = null;
        fightManager.DeselectFormation(this);
        selectable = false;
        routing = true;
        ShowFormIcon(true);

        GetMeOutOfHere();

        CheckIfRotateOrNot(); 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.ModelStartRouting();
                }
            }
        }
        if (!triggeredAPanic)
        {
            triggeredAPanic = true;
            AlliedFormationsPanic();
        }
    }
    private bool triggeredAPanic = false;
    private void AlliedFormationsPanic()
    {
        FormationPosition[] array = FightManager.Instance.allArray;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].team == team) //if allied
            { 
                array[i].maxToleratedDeaths = array[i].maxToleratedDeaths * .99f; //reduce tolerated deaths by a little
            }
        }
    }

    private void GetMeOutOfHere()
    {
        //Debug.Log("Fleeing");
        float detectionRange = 999;
        FormationPosition closestEnemy = GetClosestFormationWithinRange(1, team, false, detectionRange);
        if (closestEnemy != null)
        { 
            Vector3 heading = transform.position - closestEnemy.transform.position;
            heading = heading.normalized;
            float distanceToTravel = 100;
            Vector3 pos = transform.position + (heading * distanceToTravel);
            aiTarget.transform.position = pos;
            PlaceAITargetOnTerrain(); 
            SetDestAndSearchPath();
        }
    } 
    public void StartCharging()
    {
        if (stamina > 0 && !charging && !tangledUp)
        {
            //currentChargeTime = 0;
            //currentChargeRechargeTime = 0;
            //chargeRecharged = false; 
            //selectable = false;
            //SetSelected(false);
            //RapidUpdateDestinations();
            /*if (formationToFocusFire != null) 
            {
                aiTarget.transform.position = formationToFocusFire.transform.position;
            }
            else
            {
                aiTarget.transform.position = focusFirePos;
            }*/
            charging = true;
            movementManuallyStopped = false;
            UpdateCollider(); //make collider smaller while charging
            SetMoving(true); 
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null && soldierBlock.modelsArray[i].alive)
                {
                    soldierBlock.modelsArray[i].SwitchState(SoldierModel.ModelState.Charging);
                    soldierBlock.modelsArray[i].attackBox.Rearm(); 
                }
            }
            //embolden
            maxToleratedDeaths = maxToleratedDeaths * 2;
        } 
    }
    private float originalToleratedDeaths;
    public void StopCharging()
    {
        if (charging)
        { 
            //Debug.Log("charge stopping");
            charging = false;
            //selectable = true;
            //chargeRecharged = false;
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null && soldierBlock.modelsArray[i].alive)
                {
                    soldierBlock.modelsArray[i].SwitchState(SoldierModel.ModelState.Idle);
                    soldierBlock.modelsArray[i].ToggleAttackBox(false); 
                }
            }
            maxToleratedDeaths = originalToleratedDeaths;
        } 
    }
    private void StopRoutingDueToExhaustion()
    { 
        //also stop routing
        shatteredIcon.gameObject.SetActive(false);
        routingIcon.gameObject.SetActive(false);
        routing = false;
        ShowFormIcon(true);

        aiTarget.transform.position = transform.position;
        PlaceAITargetOnTerrain();
        CheckIfRotateOrNot();
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.StopRout();
                }
            }
        }
    }
    #region Fixers
    private float groundY = 0;
    private void PlaceThisOnGround()
    { 
        //aiTarget.transform.position
        LayerMask layerMask = LayerMask.GetMask("GroundForm");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
            groundY = hit.point.y;
        }
    }
    public async void FixPositions()
    {
        for (int i = 0; i < soldierBlock.formationPositions.Length; i++)
        {
            if (soldierBlock.formationPositions[i] != null)
            { 
                soldierBlock.formationPositions[i].PlaceOnGround();
            }
        }
        if (soldierBlock.mageType != SoldierBlock.MageTypes.None) //if we have mages
        {
            foreach (Position item in soldierBlock.magePositions)
            {
                item.PlaceOnGround();
            }
        }

        await Task.Yield();
    }
    private void PlaceAITargetOnTerrain()
    { 
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(aiTarget.position.x, 100, aiTarget.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            aiTarget.position = hit.point;
        } 
    }
    #endregion
    private void SetStandardEngagementRanges()
    { 
        if (soldierBlock.melee)
        {
            float defaultMeleeRadius = 20;
            engageEnemyRadius = defaultMeleeRadius;
        }
        else
        {
            //float defaultRangedRadius = 160;
            engageEnemyRadius = soldierBlock.modelAttackRange;
        }
        startingPursueRadius = engageEnemyRadius;
    }
    #region SetStates
    private void SetDefaultRigidConstraints()
    {
        //rigid.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    } 
    public void SetBrace(bool val)
    {
        if (formationCohesive && !charging)
        {
            if (val && !braced)
            {  
                rigid.constraints = RigidbodyConstraints.FreezeAll;
                rigid.collisionDetectionMode = CollisionDetectionMode.Continuous; 
                SetNavMeshCutters(val);
            }
            else if (!val && braced)
            { 
                SetDefaultRigidConstraints();
                rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
                SetNavMeshCutters(val);
            }
        } 
    }
    private void SetNavMeshCutters(bool val)
    {
        navCutter.enabled = val;
        braced = val;
        movementManuallyStopped = val;
        /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.SetBrace(val);
                }
            }
        }*/
        /*AstarPath.active.navmeshUpdates.ForceUpdate();
        AstarPath.active.FlushGraphUpdates();*/ 
    }
    #endregion
    public void AICheckIfNeedToBrace()
    {
        bool enemyInBraceRadius = false;
        float dist = 0; 
        for (int i = 0; i < fightManager.allArray.Length; i++) //go through until we get one that matches criteria
        { 
            if (((fightManager.allArray[i].formationType == FormationType.SpearInfantry && fightManager.allArray[i].charging) || (fightManager.allArray[i].formationType != FormationType.Cavalry)) && fightManager.allArray[i].team != team)
            { 
                float distance = Vector3.Distance(fightManager.allArray[i].transform.position, transform.position);
                if (distance <= aiBraceRadius)
                {
                    enemyInBraceRadius = true;
                    dist = distance;
                    break;
                }
            }
        }
        float intelligence = AIIntelligence;
        float chance;
        float normalizedDistance = Mathf.Abs(Mathf.Clamp(aiBraceRadius - dist, aiBraceRadius * 0.1f, aiBraceRadius)) / aiBraceRadius;
        if (enemyInBraceRadius)
        {
             chance = intelligence * normalizedDistance;
        }
        else
        {
            chance = intelligence * 100 * normalizedDistance;
        }
        int rand = Random.Range(0, 100); //chance to do the optimal action
        if (rand <= chance)
        {
            //Debug.Log("Brace!");
            SetBrace(enemyInBraceRadius);
        }
    }
    public void GetTangledUp()
    {
        if (routing)
        {
            FullUnfreeze();
            /*ShowFormIcon(false);
            Color color = farAwayIcon.color;
            color.a = 0;
            farAwayIcon.color = color;*/
            return;
        }
        freezeFormPos = true;
        tangledUp = true;
        freezeTimer++;
        freezeTimer = Mathf.Clamp(freezeTimer, 0, 3);
        StopCharging();
        //Invoke("StopCharging", 1);
    } 
    private void FullUnfreeze()
    { 
        freezeFormPos = false;
        tangledUp = false;
        freezeTimer = 0;
    }
    private void UnfreezeThis()
    {
        freezeTimer--;
        if (freezeTimer <= 0)
        {
            freezeFormPos = false;
            tangledUp = false;
            freezeTimer = 0;
        }
    }
    private bool formationCohesive = true;

    public void BreakCohesion() //maybe make this immediately splinter instead
    {
        if (formationCohesive)
        { 
            formationCohesive = false;
            if (formationCollider != null)
            {
                formationCollider.enabled = false;
            }
            cohesionTimer = 3;
            cohesionTimer = Mathf.Clamp(cohesionTimer, 0, 3);
            /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++) //oh god we're getting charged!!! spread out
            {
                if (soldierBlock.modelsArray[i] != null && soldierBlock.modelsArray[i].alive)
                {
                    soldierBlock.modelsArray[i].dispersalLevel = soldierBlock.modelsArray[i].oldDispersalLevel * 8;
                    soldierBlock.modelsArray[i].GenerateDispersalVector(soldierBlock.modelsArray[i].dispersalLevel);
                }
            }*/
        }
    }
    private int splinterCounter = 0;
    private int splinterThreshold = 10;
    [SerializeField] private int splinterLevel = 0;
    private int maxSplinterLevel = 8;
    private int splinterTimer = 0;
    public void SplinterCohesion(int amount)
    {
        splinterTimer = 10; 
        splinterCounter += amount;
        if (splinterCounter >= splinterThreshold)
        {
            splinterCounter = 0;
            splinterLevel = Mathf.Clamp(splinterLevel + 1, 0, 8);
        }
    }
    private void RegainCohesion()
    {
        cohesionTimer--;
        splinterTimer--;
        if (cohesionTimer <= 0)
        {
            if (!formationCohesive)
            {
                cohesionTimer = 0;
                formationCohesive = true;
                if (formationCollider != null)
                {
                    formationCollider.enabled = true;
                } 
            }
        }
        if (splinterTimer <= 0)
        { 
            if (splinterLevel > 0)
            {
                splinterTimer = 10;
                splinterLevel -= 1; 
            }
        }
    } 
    public void DisableIdleChatterForSeconds(float sec)
    {
        playingIdleChatter = true;
        Invoke("EnableIdleChatter", sec);
    }
    public void DisableAttackChatterForSeconds(float sec)
    {
        playingAttackChatter = true;
        Invoke("EnableAttackChatter", sec);
    }
    public void DisableMarchChatterForSeconds(float sec)
    {
        playingMarchChatter = true;
        Invoke("EnableMarchChatter", sec);
    }
    public void DisableDeathReactionForSeconds(float sec)
    {
        playingDeathReactionChatter = true;
        Invoke("EnableDeathReactionChatter", sec);
    }
    public void EnableDeathReactionChatter()
    {
        playingDeathReactionChatter = false;
    }
    private void EnableAttackChatter()
    {
        playingAttackChatter = false;
    }
    private void EnableMarchChatter()
    {
        playingMarchChatter = false;
    }
    private void EnableIdleChatter()
    {
        playingIdleChatter = false;
    }
    /*private void SimultaneousPositionCheck()
    {
        if (simultaneousPositionCheck)
        { 
            for (int i = 0; i < soldierBlock.formationPositions.Length; i++)
            {
                if (soldierBlock.formationPositions[i] != null)
                { 
                    soldierBlock.formationPositions[i].PlaceOnGround();
                }
            }
        }
    }*/
    private void GeneralCheckIfSwapRows()
    {
        if (modelsInFrontRowThatFired.Count >= requiredModelsThatFiredInRow)
        {
            modelsInFrontRowThatFired.Clear();
            SwapRows();
            RedefineFrontRow();
            ApplyRowTransforms();
        }
    }
    public void CheckIfSwapRows(SoldierModel model)
    {
        if (!modelsInFrontRowThatFired.Contains(model))
        {
            modelsInFrontRowThatFired.Add(model); 
        }
        if (modelsInFrontRowThatFired.Count >= requiredModelsThatFiredInRow)
        { 
            modelsInFrontRowThatFired.Clear();
            SwapRows();
            RedefineFrontRow(); 
            ApplyRowTransforms();
        }
    }
    private void SwapRows()
    { //shunt front to back
        Row storedRow = soldierBlock.rows[0]; //first 
        soldierBlock.rows.RemoveAt(0);
        soldierBlock.rows.Add(storedRow);

        foreach (Row item in soldierBlock.rows)
        {
            item.rowPositionInList = soldierBlock.rows.IndexOf(item);
        }
    }
    private void RedefineFrontRow()
    {
        soldierBlock.frontRow = soldierBlock.rows[0]; //next first row
    }
    private void ApplyRowTransforms()
    {
        for (int i = 0; i < soldierBlock.rows.Count; i++)
        {
            soldierBlock.rows[i].transform.localPosition = new Vector3(i, 0, 0);
        }
    }
    [SerializeField] private int positionToCheck = 0;
    
    private void SetMoving(bool val)
    { 
        if (aiPath != null)
        {
            aiPath.canMove = val;
        }
        
        //aiPath.enableRotation = val;
        if (val == false && charging)
        { 
            StopCharging();
        }
    }
    private void CheckIfLowSoldiersRout()
    {
        if (numberOfAliveSoldiers <= 10 && !routing)
        {
            StartFleeing();  
        }
    }
    public SpriteRenderer frontIcon;
    private void ShowFormIcon(bool val)
    {
        farAwayIcon.enabled = val;
        frontIcon.enabled = val;
        if (farAwayIconMask != null)
        {
            farAwayIconMask.SetActive(val); 
        }
        selectedSprite.enabled = val;
    }

    private void CheckIfInCombat()
    {
        //if (listOfNearbyEnemies.Count > 0)
        //{ 
            if (modelAttacked || modelTookDamage)
            {
                inCombat = true;
                modelAttacked = false;
                modelTookDamage = false;
            }
            else
            {
                inCombat = false;
            }
        //} 
    }
    
    public void TriggerSelectionCircles(bool on)
    { 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.selectionCircle.SetActive(on);
                }
                else
                {
                    model.selectionCircle.SetActive(false);
                }
            }
        }
    }

    public void PursueCommand()
    {
        chaseDetectedEnemies = true;
    }

    public void StopChaseCommand()
    {
        chaseDetectedEnemies = false;
    }
    public float ratioOfAliveToMax = 1;
    public float math = 8;
    private void UpdateCollider()
    {
        float z = 8f;

        float soldiers = numberOfAliveSoldiers;
        float max = maxSoldiers;

        ratioOfAliveToMax = soldiers / max;

        float num = 8f - (8f*ratioOfAliveToMax); //8 -7 = 1 
        if (formationType != FormationType.Cavalry)
        {
            int chargeOffset = 0;
            if (charging)
            {
                chargeOffset = -2;
            }
            //float centerOffset = 16.24f; 
            float offset = 0;
            posParentTransform.localPosition = new Vector3(-4.5f, offset, 3.5f - num * .5f);
            int buffer = 1;
            int x = 10 + buffer + chargeOffset - splinterLevel;
            x = Mathf.Clamp(x, 0, 20);
            int y = 4;
            math = buffer + chargeOffset + z - num - splinterLevel;
            math = Mathf.Clamp(math, 0, 20);
            if (formationCollider != null)
            {
                formationCollider.size = new Vector3(x, y, math);
                if (math <= 0)
                {
                    formationCollider.enabled = false;
                }
                else
                {
                    formationCollider.enabled = true;
                }
            }
            float defSize = 10;
            float remedy = 1.25f;
            float calc = math * remedy;

            /*if (farAwayIconMask != null)
            {
                farAwayIconMask.gameObject.transform.localScale = new Vector3(defSize, calc, 1);
                farAwayIconMask.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            }
            if (farAwayIcon != null)
            {

                farAwayIcon.gameObject.transform.localScale = new Vector3(defSize, calc, 1);
                farAwayIcon.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }
            if (selectedSprite != null)
            {
                selectedSprite.gameObject.transform.localScale = new Vector3(defSize + 1, calc + 1, 1);

                selectedSprite.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }
            if (routingIcon != null)
            {

                routingIcon.gameObject.transform.localScale = new Vector3(defSize + 1, calc + 1, 1);
                routingIcon.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }
            if (shatteredIcon != null)
            {

                shatteredIcon.gameObject.transform.localScale = new Vector3(defSize + 1, calc + 1, 1);

                shatteredIcon.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }*/


            /*if (!soldierBlock.canBeRanged)
            { 
                if (listOfNearbyEnemies.Count > 0)
                {
                    offsetSecondRow.localPosition = new Vector3(-secondRowOffsetAmount, 0, .5f);
                }
                else
                {
                    offsetSecondRow.localPosition = new Vector3(0, 0, 0);
                }
            }*/
        } 
    }

    private void UpdateLineRenderer()
    {
        if (!aiPath.reachedDestination)
        {
            lineRenderer.enabled = selected;
            lineRenderer2.enabled = selected;

            if (lineRenderer.enabled)
            {

                lineRenderer.SetPosition(0, new Vector3(formationPositionBasedOnSoldierModels.x, formationPositionBasedOnSoldierModels.y, formationPositionBasedOnSoldierModels.z)); //offsetting pos4 +0.585001f
                
                if (formationToFollow == null)
                {
                    int count = 1;
                    lineRenderer.positionCount = destinationsList.Count + 1;
                    foreach (Vector3 item in destinationsList)
                    {
                        lineRenderer.SetPosition(count, item);
                        count++;
                    }
                }
                else
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(1, aiTarget.transform.position);
                }
                
            }
            if (lineRenderer2.enabled)
            {
                if (destinationsList.Count > 0) //if there are still destinations to go
                {

                }
            }
            /*if (destinationsList.Count > 0)
            {
                Debug.Log("setting target to destination 2");
                aiTarget.transform.position = destinationsList[0];
            }*/

        }
        else
        {
            lineRenderer.enabled = false;
        }
        //rotation fixed?

        lineRenderer2.enabled = !finishedChangedFacing;
    } 
    public void CheckIfRotateOrNot()
    {
        if (braced)
        {
            aiPath.enableRotation = false;
        } 
        else if (!alwaysRotateTowardMovementPos)
        { 
            Vector3 heading = aiPath.destination - transform.position;
            float threshold = 50;
            if (Vector3.Angle(heading, -transform.forward) <= threshold)
            {
                aiPath.enableRotation = false;
            }
            else
            {
                aiPath.enableRotation = true;
            }
        }
    } 

    private void OffsetPositions()
    {
        if (enemyFormationToTarget == null)
        {
            if (aiPath.remainingDistance > offsetThreshold)
            {
                if (weaponsDeployed)
                {
                    posParentTransform.localPosition = posParentStartingPos;
                    if (Mathf.Abs(aiPath.velocity.x) > requiredVelocity || Mathf.Abs(aiPath.velocity.z) > deployedRequiredVelocity)
                    { 
                        posParentTransform.localPosition = posParentStartingPos + deployedOffsetAmount;
                    }

                }
                else
                { 
                    posParentTransform.localPosition = posParentStartingPos;
                    if (Mathf.Abs(aiPath.velocity.x) > requiredVelocity || Mathf.Abs(aiPath.velocity.z) > requiredVelocity)
                    { 
                        posParentTransform.localPosition = posParentStartingPos + offsetAmount;
                    }
                } 
            }
            else
            {
                posParentTransform.localPosition = posParentStartingPos;
            }
        }

    }
    
    public void CastMagic(Vector3 targetPos, int abilityNum)
    {
        switch (soldierBlock.mageType)
        {
            case SoldierBlock.MageTypes.None:
                break;
            case SoldierBlock.MageTypes.Pyromancer:

                if (abilityNum == 0)
                {
                    foreach (SoldierModel mage in soldierBlock.listMageModels)
                    {
                        if (mage.magicCharged && mage.alive && mage.rangedModule != null)
                        {
                            mage.rangedModule.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                            break;
                        }
                    }
                }
                break;
            case SoldierBlock.MageTypes.Gallowglass: 
                if (abilityNum == 0)
                {
                    float chance = 25;
                    foreach (SoldierModel model in soldierBlock.listSoldierModels)
                    {
                        if (model.alive && model.magicModule != null && model.currentModelState != SoldierModel.ModelState.Attacking)
                        {
                            int rand = Random.Range(0, 100);
                            if (rand <= chance)
                            {
                                model.magicModule.CastMagic(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                            }
                        }
                    }
                    abilityCharged = false;
                }
                break;
            case SoldierBlock.MageTypes.Eldritch:
                break;
            case SoldierBlock.MageTypes.Flammen:
                break;
            case SoldierBlock.MageTypes.Seele:
                break;
            case SoldierBlock.MageTypes.Torches: 
                if (abilityNum == 0)
                {
                    float chance = 10;
                    foreach (SoldierModel model in soldierBlock.listSoldierModels)
                    {
                        if (model.alive && model.magicModule != null && model.currentModelState != SoldierModel.ModelState.Attacking)
                        {
                            int rand = Random.Range(0, 100);
                            if (rand <= chance)
                            {
                                model.magicModule.CastMagic(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                            }
                        }
                    }
                    abilityCharged = false;
                }
                break;
            default:
                break;
        } 
    }
    public void SetSelected(bool val)
    {
        if (selectable)
        { 
            selected = val;
            ShowSelected(val);
        }
        else
        { 
            selected = false;
            ShowSelected(false);
            TriggerSelectionCircles(false);
            if (fightManager == null)
            {
                fightManager = FindObjectOfType<FightManager>();
            } 
            fightManager.UpdateGUI();
        }
        if (val == false)
        {
            OnDeselected();
        }
    }
    private void OnDeselected()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    if (!model.melee)
                    {
                        model.lineOfSightIndicator.enabled = false;
                        model.reloadingIndicator.enabled = false;
                    }
                }
            }
        }
    }

    private void ShowSelected(bool val)
    {
        if (selectedSprite != null)
        {
            selectedSprite.enabled = val;
        }
    }


    public void ClearOrders()
    {
        SetMoving(false); 
        destinationsList.Clear();
        obeyingMovementOrder = false;
        enemyFormationToTarget = null; 
        ResetAITarget();
        focusFire = false;
        formationToFocusFire = null;
        focusFirePos = new Vector3(999,999,999);
    }
    private void ResetAITarget()
    { 
        aiTarget.transform.position = transform.position;
        rotTarget.transform.position = transform.position;
    }
    private void StopInPlace()
    {
        ResetAITarget(); 
        obeyingMovementOrder = false;
        SetMoving(false);
    }

    private void ChaseFoe()
    {  
        if (enemyFormationToTarget == null)
        {
            return;
        }
        Debug.Log("chasing foe");
        aiTarget.transform.position = enemyFormationToTarget.transform.position;
        SetDestAndSearchPath();
        rotTarget.transform.position = enemyFormationToTarget.transform.position;
        CheckIfRotateOrNot(); 
    }
    public void StopCommand()
    {
        movementManuallyStopped = true;
    }
    public void RoutCommand()
    {
        int time = 10;
        SoftRout(time);
    }
    public void ResumeCommand()
    {
        movementManuallyStopped = false;
    }
    
    void OnDrawGizmosSelected()
    {
        /*Gizmos.DrawWireSphere(transform.position, engageEnemyRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, aiBraceRadius);*/
    }
}
