using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Threading.Tasks;
using System.Threading;
public class FormationPosition : MonoBehaviour
{

    private CancellationTokenSource cancelToken;
    public FormationPosition formationToFollow;


    #region Generic 
    [HideInInspector] public enum FormationType
    {
        Infantry,
        RangedInfantry,
        Cavalry
    }
    public GlobalDefines.SoldierTypes soldierType = GlobalDefines.SoldierTypes.conscript;
    #endregion

    #region MustBeSet 

    public Rigidbody rigid;
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
    public bool usesSpears = true;
    public FormationType formationType = FormationType.Infantry; //just by default
    #endregion

    #region AutoSet
    public ShakeSource shaker;
    private FightManager fightManager; 

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
    [HideInInspector] public bool movementManuallyStopped = false;
    [HideInInspector] public List<Vector3> destinationsList = new List<Vector3>();
    [HideInInspector] public bool finishedChangedFacing = true;
    [HideInInspector] public float averagePositionBasedOnSoldierModels = 0;
    [HideInInspector] public Vector3 formationPositionBasedOnSoldierModels;
    [HideInInspector] public bool charging = false;
    [HideInInspector] public bool selectable = true;
    [HideInInspector] public bool braced = false;
    [HideInInspector] public bool enableAnimations = false;
    [HideInInspector] public float walkingSpeed = 3.5f;
    [HideInInspector] public float sprintSpeed = 6.5f;
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
    public bool isCavalry = false;
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
    [SerializeField] private float maxChargeTime = 15;
    [SerializeField] private float currentChargeTime = 0;
    [SerializeField] private float chargeRechargeTime = 60;
    private float currentChargeRechargeTime = 0;
    #endregion


    [SerializeField] private float maxToleratedDeaths = 30;
    [SerializeField] private int deathsThisTimeFrame = 0;
    private float timeFrame = 30;
    [SerializeField] private int extremeRoutThreshold = 15;
    [SerializeField] private int hardRoutOnSoftRoutThreshold = 40;

    public int numKills = 0;

    [SerializeField] private int stamina = 100;
    private int maxStamina = 100;
    private void Start()
    {
        Color color = farAwayIcon.color;
        color.a = 0;
        farAwayIcon.color = color;
        frontIcon.color = color;
        if (fightManager == null)
        {
            fightManager = FindObjectOfType<FightManager>();
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
        chargeSpeed = walkingSpeed * 3;
        aiPath.maxSpeed = currentSpeed;
        originalToleratedDeaths = maxToleratedDeaths;
        //PlaceAITargetOnTerrain(); 
    }
    private void OnEnable()
    {
        cancelToken = new CancellationTokenSource();
    }
    public void BeginUpdates()
    {
        PathfindingUpdate(250, cancelToken.Token); //in parallel
        FastUpdate(100, cancelToken.Token);
        ReinforceUpdate(100, cancelToken.Token); //cycles through positions
        CheckEnemyUpdate(50, cancelToken.Token); //cycles through soldiers 1 by one

        SlowUpdate(500, cancelToken.Token);
        VerySlowUpdate(1000, cancelToken.Token);
        InvokeRepeating("TimeFrameAdvance", 0, timeFrame);





        //InvokeRepeating("LockSoldiers", 0, lockTime);
        //InvokeRepeating("LockSoldiersToTerrain", 0, terrainLockTime);
        //InvokeRepeating("UpdateFarAwayIconPos", 0, .1f);
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
    private async void CheckEnemyUpdate(int time, CancellationToken cancelToken)
    {
        CheckModelsIndividually();
        await Task.Delay(time, cancelToken);
        CheckEnemyUpdate(time, cancelToken);
        await Task.Yield();
    }
    private async void CheckModelsIndividually()
    {
        SoldierModel checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
        if (checkingModel == null)
        {
            while (checkingModel == null || !checkingModel.alive) //let us skip over those that have died
            {
                soldierModelToCheck++;
                if (soldierModelToCheck >= maxSoldiers) //reset on 80
                {
                    soldierModelToCheck = 0;
                }
                checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
            }
        }
        if (checkingModel.alive)
        {
            if (checkingModel.melee) //if model has a target in range, don't need to check more
            {
                if (enemyFormationToTarget != null && !checkingModel.HasTargetInRange())
                { 
                    if (checkingModel.currentModelState == SoldierModel.ModelState.Moving || checkingModel.currentModelState == SoldierModel.ModelState.Idle)
                    { 
                        checkingModel.CheckIfEnemyModelsNearby();
                    }
                }
            }
            else
            { 
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
    private async void PathfindingUpdate(int time, CancellationToken cancelToken)
    {
        UpdatePathsOfSoldierModels();
        await Task.Delay(time, cancelToken);
        PathfindingUpdate(time, cancelToken);
        await Task.Yield();
    }
    private async void UpdatePathsOfSoldierModels()
    {
        float threshold = 1;
        Parallel.For(0, soldierBlock.modelsArray.Length, i =>
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) //  && model.CheckIfRemainingDistanceOverThreshold(threshold) //&& !model.pathfindingAI.pathPending
                {
                    model.UpdateDestinationPosition();
                }
            }
        });
        /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) // && model.CheckIfRemainingDistanceOverThreshold(threshold)
                {
                    model.UpdateDestinationPosition();
                }
            }
        }*/
        await Task.Yield();
    }
    private async void ModelRotationUpdate(int time, CancellationToken cancelToken)
    {
        FixModelRotation();
        await Task.Delay(time, cancelToken);
        ModelRotationUpdate(time, cancelToken);
    }
    private void FixModelRotation()
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
    }
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
            if (!simultaneousPositionCheck)
            {
                position.PlaceOnGround();
            }
            if (position.assignedSoldierModel == null) //if null, then dead
            {
                position.SeekReplacement();
            }
        }
        /*Parallel.For(0, soldierBlock.formationPositions.Length, i => {
            Position position = soldierBlock.formationPositions[i];
            if (position != null)
            { 
                position.PlaceOnGround();
                if (position.assignedSoldierModel == null) //if null, then dead
                {
                    position.SeekReplacement();
                }
            }
        });*/
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
        await Task.Delay(time, cancelToken);
        SlowUpdate(time, cancelToken);
    }
    private void CheckIfInMeleeRange()
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
    }
    #endregion
    #region FastUpdate 
    private async void FastUpdate(int time, CancellationToken cancelToken)
    {
        FixFormationRotation();
        UpdateFormationMovementStatus(); 

        movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector
        float magic = 15;
        transform.position = new Vector3(transform.position.x, magic, transform.position.z); 
        FastSoldierUpdate();
        FollowFormation();


        LockSoldiers();
        LockSoldiersToTerrain();

        UpdateFarAwayIconPos();

        await Task.Delay(time, cancelToken);
        FastUpdate(time, cancelToken);
    }
    private void FastSoldierUpdate()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateModelState(cancelToken.Token);
                    ////model.UpdateVisibility(); 
                }
            }
        }
    } 
    #endregion
    #region VerySlowUpdate
    private async void VerySlowUpdate(int time, CancellationToken cancelToken)
    {
        if (swapRowsAfterFiring)
        {
            GeneralCheckIfSwapRows();
        }
        if (numberOfAliveSoldiers <= 0) //if all soldiers dead, then goodbye
        {
            /*foreach (FormationPosition item in listOfNearbyEnemies)
            {
                item.listOfNearbyEnemies.Remove(this);

            }*/
            gameObject.SetActive(false);
            return;
        }

        movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector 
        float min = .01f;
        if (movingSpeed < min)
        {
            movingSpeed = 0;
        }
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

        if (timeUntilAllowedToCastMagicAgain > 0)
        {
            timeUntilAllowedToCastMagicAgain--;
            if (timeUntilAllowedToCastMagicAgain <= 0)
            {
                allowedToCastMagic = true;
                fightManager.UpdateGUI();
            }
        }
        if (charging)
        {
            currentChargeTime += 1;
            if (currentChargeTime >= maxChargeTime || stamina <= 0) //if out of stamina must stop
            {
                StopCharging();
                currentChargeTime = 0;
            }
        }
        else if (!chargeRecharged)
        {
            currentChargeRechargeTime += 1;
            if (currentChargeRechargeTime >= chargeRechargeTime)
            {
                chargeRecharged = true;
                fightManager.UpdateGUI();
                currentAbilityRechargeTime = 0;
            }
        }
        if (!charging && !routing) //regain stamina if not moving fast
        {
            stamina++;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
        }
        else //charging or routing
        {
            stamina -= 3;
            stamina = Mathf.Clamp(stamina, 0, maxStamina);
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
        if (routing)
        {
            FullUnfreeze();
            BreakCohesion();
            GetMeOutOfHere();
        }
        else //default
        {
            CheckForNearbyEnemyFormations(); //
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
        VerySlowSoldierUpdate(); // 
        UpdateDeployment();
        UpdateSpeed(); // 
        UpdateCollider(); // 

        await Task.Delay(time, cancelToken);
        VerySlowUpdate(time, cancelToken);
    }
    private async void VerySlowSoldierUpdate()
    {
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
        Parallel.For(0, soldierBlock.modelsArray.Length, i =>
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive) //  && model.CheckIfRemainingDistanceOverThreshold(threshold) //&& !model.pathfindingAI.pathPending
                {
                    model.UpdateAndCullAnimations();
                    model.CheckIfTargetIsDead();

                    if (model.attackType == SoldierModel.AttackType.Ranged)
                    {
                        model.rangedModule.GetTarget();
                        model.rangedModule.LineOfSightUpdate();
                    }
                }
            }
        });
        await Task.Yield();
    }
    #endregion 
    void Update()
    {
        UpdateLineRenderer();
        if (selected)
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
            }
        }
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.FaceEnemy(); 
                }
            }
        }
    }
    private int fastModelCheck = 0; 

    public void ShowHideSoldiers(bool val) 
    { 
        if (showSoldierModels == false && val)
        { 
            TeleportSoldiers();
        }
        showSoldierModels = val;
        /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel soldier = soldierBlock.modelsArray[i];
            if (soldier != null && soldier.alive)
            {
                soldier.UpdateVisibility(val);
            }
            else if (soldier != null)
            {
                soldier.UpdateVisibility(true);
            }
        }*/
    }
    private void TeleportSoldiers()
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
    }
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
    private void LockSoldiersToTerrain() //called periodically
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
    }
    private void LockSoldiers() //soldiers should use performant mode if not visible
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
    }
    private void UpdateFarAwayIconPos()
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

        formationIconsParent.transform.position = new Vector3(formationIconsParent.transform.position.x, avgHeight, formationIconsParent.transform.position.z); //set to average height
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
    } 
    private void SetAnimationsModels(bool val)
    { 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            if (soldierBlock.modelsArray[i] != null)
            {
                soldierBlock.modelsArray[i].animate = val; 
            }
        }
    }

    
    public bool modelHasAShot = false;
    private void FollowFormation()
    {
        if (formationToFollow != null && !routing)
        {
            aiTarget.transform.position = formationToFollow.transform.position;
            PlaceAITargetOnTerrain(); 
        }
    }
    public void PlacePositionOnGround(Position itemPos)
    {
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(itemPos.transform.position.x, 100, itemPos.transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            itemPos.transform.position = hit.point;
        }
        //Debug.DrawRay(transform.position, Vector3.down*100, Color.yellow, 1);
    }
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
    private void UpdateSpeed()
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
    private void CheckForNearbyEnemyFormations()
    {
        listOfNearbyEnemies.Clear();
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
                if (form.routing)
                {
                    continue;
                }
                listOfNearbyEnemies.Add(form);
            }
        }

        if (obeyingMovementOrder)
        {
            return;
        }
        FindClosestFormation();
    }
    private void FindClosestFormation()
    { 
        enemyFormationToTarget = GetClosestFormationWithinRange(1, team, true, engageEnemyRadius);
        if (chaseDetectedEnemies && enemyFormationToTarget != null)
        {
            EngageFoe();
        }

    }
    private void EngageFoe()
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
    }

    #endregion


    public void LoseMorale()
    {
        if (numberOfAliveSoldiers <= extremeRoutThreshold)
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
        shatteredIcon.gameObject.SetActive(true);
        routingIcon.gameObject.SetActive(false);
        BeginFleeing();
        float disappearTime = 30;
        Invoke("SelfDestruct", disappearTime);
    }
    private void TimeFrameAdvance()
    {
        deathsThisTimeFrame = 0;
    }
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
                if (targetType == 1  && fightManager.allArray[i].team != ourTeam)
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
    public void SoftRout(int time = 20) //rout in a direction for some time. after time, check if no enemies melee attacking us. if so, then become controllable again
    {
        shatteredIcon.gameObject.SetActive(false);
        routingIcon.gameObject.SetActive(true);
        BeginFleeing(); 
        Invoke("StopFleeing", time);
    } //maybe stop fleeing if no enemies nearby?
    private void StopFleeing()
    {
        if (numberOfAliveSoldiers <= extremeRoutThreshold)
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
    } 
    private void BeginFleeing()
    {
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
                    model.Rout();
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
    private void SelfDestruct()
    {
        soldierBlock.SelfDestruct();
    }

    private void GetMeOutOfHere()
    {
        //Debug.Log("Fleeing");
        float detectionRange = 50;
        FormationPosition closestEnemy = GetClosestFormationWithinRange(1, team, false, detectionRange);
        if (closestEnemy != null)
        { 
            Vector3 heading = transform.position - closestEnemy.transform.position;
            heading = heading.normalized;
            float distanceToTravel = 100;
            Vector3 pos = transform.position + (heading * distanceToTravel);
            aiTarget.transform.position = pos;
            PlaceAITargetOnTerrain();
        }
    } 
    public void StartCharging()
    {
        if (!isCavalry && chargeRecharged)
        {
            currentChargeTime = 0;
            currentChargeRechargeTime = 0;
            chargeRecharged = false;
            //Debug.Log("charging");
            movementManuallyStopped = false;
            selectable = false;
            SetSelected(false);
            charging = true;
            SetMoving(true);
            if (formationToFocusFire != null)
            {
                aiTarget.transform.position = formationToFocusFire.transform.position;
            }
            else
            {
                aiTarget.transform.position = focusFirePos;
            }
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null)
                {
                    soldierBlock.modelsArray[i].SwitchState(SoldierModel.ModelState.Charging);
                    soldierBlock.modelsArray[i].attackBox.Rearm();
                    soldierBlock.modelsArray[i].ToggleAttackBox(true);
                }
            }
            originalToleratedDeaths = maxToleratedDeaths;
            maxToleratedDeaths = maxToleratedDeaths * 2;
        } 
    }
    private float originalToleratedDeaths;
    private void StopCharging()
    {
        if (!isCavalry)
        {
            //Debug.Log("charge stopping");
            charging = false;
            selectable = true;
            chargeRecharged = false;
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null)
                {
                    soldierBlock.modelsArray[i].SwitchState(SoldierModel.ModelState.Idle);
                    soldierBlock.modelsArray[i].ToggleAttackBox(false);
                }
            }
            maxToleratedDeaths = originalToleratedDeaths;
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
    public void FixPositions()
    {
        for (int i = 0; i < soldierBlock.formationPositions.Length; i++)
        {
            if (soldierBlock.formationPositions[i] != null)
            { 
                soldierBlock.formationPositions[i].PlaceOnGround();
            }
        }
        if (soldierBlock.mageType != "") //if we have mages
        {
            foreach (Position item in soldierBlock.magePositions)
            {
                item.PlaceOnGround();
            }
        }
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
        if (formationCohesive)
        {
            if (val)
            {
                rigid.constraints = RigidbodyConstraints.FreezeAll;
                rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
                navCutter.enabled = true;
            }
            else
            {
                SetDefaultRigidConstraints();
                rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
                navCutter.enabled = false;
            }

            AstarPath.active.navmeshUpdates.ForceUpdate();
            AstarPath.active.FlushGraphUpdates();
            braced = val;
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        model.SetBrace(val);
                    }
                }
            }
            movementManuallyStopped = val;
        } 
    }
    #endregion
    public void AICheckIfNeedToBrace()
    {
        /*bool enemyInBraceRadius = false;
        float dist = 0;
        for (int i = 0; i < listOfNearbyEnemies.Count; i++)
        {
            if (!listOfNearbyEnemies[i].isCavalry)
            {
                continue;
            }
            float distance = Vector3.Distance(listOfNearbyEnemies[i].transform.position, transform.position);
            if (distance <= aiBraceRadius)
            {
                enemyInBraceRadius = true;
                dist = distance;
                break;
            }
        }
        float intelligence = AIIntelligence;
        float chance;
        if (enemyInBraceRadius)
        {
            float normalizedDistance = Mathf.Abs(aiBraceRadius - dist)/aiBraceRadius;  //dist 0 right on top of us, gives us ai brace radius value
            chance = intelligence * normalizedDistance;
        }
        else
        {
            chance = intelligence;
        }
        int rand = UnityEngine.Random.Range(0, 100); //chance to do the optimal action
        if (rand <= chance)
        { 
            SetBrace(enemyInBraceRadius);
        } */
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
        //StopCharging();
        Invoke("StopCharging", 1);
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
    public void BreakCohesion()
    {
        formationCohesive = false; 
        if (formationCollider != null)
        {
            formationCollider.enabled = false;
        }
        cohesionTimer = 3;
        cohesionTimer = Mathf.Clamp(cohesionTimer, 0, 3);
    }
    private void RegainCohesion()
    {
        cohesionTimer--;
        if (cohesionTimer <= 0)
        {
            cohesionTimer = 0;
            formationCohesive = true; 
            if (formationCollider != null)
            {
                formationCollider.enabled = true;
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
    private void SimultaneousPositionCheck()
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
    }
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
        aiPath.canMove = val; 
        //aiPath.enableRotation = val;
        if (val == false && charging)
        { 
            StopCharging();
        }
    }
    private void UpdateFormationMovementStatus()
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
    }
    private void CheckIfLowSoldiersRout()
    {
        if (numberOfAliveSoldiers <= 10 && !routing)
        {
            BeginFleeing();  
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
        if (!isCavalry)
        {
            //float centerOffset = 16.24f; 
            float offset = 0;
            posParentTransform.localPosition = new Vector3(-4.5f, offset, 3.5f - num * .5f);
            int x = 10;
            int y = 4;
            math = z - num;
            if (formationCollider != null)
            {
                formationCollider.size = new Vector3(x, y, math); 
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
        if (!alwaysRotateTowardMovementPos)
        { 
            Vector3 heading = aiTarget.transform.position - transform.position;
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
        if (soldierBlock.mageType == "Pyromancer")
        {
            if (abilityNum == 0)
            {
                foreach(SoldierModel mage in soldierBlock.listMageModels)
                {
                    if (mage.magicCharged && mage.alive && mage.rangedModule != null)
                    { 
                        mage.rangedModule.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                        break;
                    }
                }
            }
        }
        if (soldierBlock.mageType == "Gallowglass")
        {
            if (abilityNum == 0)
            {
                foreach (SoldierModel model in soldierBlock.listSoldierModels)
                {
                    if (abilityCharged && model.alive && model.rangedModule != null)
                    {
                        model.rangedModule.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                        abilityCharged = false;
                        break;
                    }
                }
            }
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
        //Debug.Log("chasing foe");
        aiTarget.transform.position = enemyFormationToTarget.transform.position;
        rotTarget.transform.position = enemyFormationToTarget.transform.position;
        CheckIfRotateOrNot(); 
    }
    public void StopCommand()
    {
        movementManuallyStopped = true;
    }
    public void RoutCommand()
    {

    }
    public void ResumeCommand()
    {
        movementManuallyStopped = false;
    }
    private void FixFormationRotation()
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
