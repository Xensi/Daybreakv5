using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    public bool abilityCharged = true; 
    [SerializeField] private float abilityRechargeTime = 60f;
    [SerializeField] private float currentAbilityRechargeTime = 0;
    public bool allowedToCastMagic = true;
    public float timeUntilAllowedToCastMagicAgain = 0;
    public bool canBrace = false;
    public bool braced = false;
    public string team = "Altgard"; //Whose team are we on?
    public RichAI aiPath;
    [SerializeField] private float threshold = .5f;
    public List<FormationPosition> listOfNearbyEnemies;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public Transform aiTarget;
    public Transform rotTarget;
    [SerializeField] private float velThreshold = .1f;
    [Tooltip("Checks nearby formations. Nearby formations can be moved towards automatically.")]
    [SerializeField] private float checkRadius = 10;
    [Tooltip("When to stop moving when auto-engaging.")]
    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;
    [SerializeField] private BoxCollider rectangleCollider;
    //
    public FormationPosition enemyFormationToTarget;
    public SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;
    //

    [SerializeField] private float movingSpeed = 0;
    [SerializeField] private float currentSpeed = 0;
    [SerializeField] private float slowSpeed = .5f;
    public float walkingSpeed = 3.5f;
    public float sprintSpeed = 6.5f;

    public bool selected = false;

    [SerializeField] private bool running = false;

    [SerializeField] private MeshRenderer selectionBox;
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
    [SerializeField] private Transform compass;
    [SerializeField] private Transform highParent;
    [SerializeField] private float finishedPathRotSpeed = 1;
    public bool pathSet = false;

    public List<Position> frontlinePositions;

    public bool modelAttacking = false;

    public bool obeyingMovementOrder;

    public int numberOfAliveSoldiers = 80;
    private int oldNumAlive = 80;
    public int maxSoldiers = 80;

    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;

    [SerializeField] private float normRotate = 30;
    //[SerializeField] private Transform offsetSecondRow;

    [SerializeField] private float colliderBoxRange = 9;
    private float colliderBoxNotDeployedRange = 8;
    [SerializeField] private float xsize = 10;
    [SerializeField] private float ysize = 4;
    [SerializeField] private float zsize = 8;

    [SerializeField] private float xoffset = 0;
    [SerializeField] private float yoffset = 2;
    [SerializeField] private float zoffset = 1;
    [SerializeField] private float zNotDeployedOffset = 0;
    private float zSelLine; //???

    public bool playingIdleChatter = false;
    public bool playingAttackChatter = false;
    public bool playingDeathReactionChatter = false;
    public bool playingMarchChatter = false;

    public bool deployedPikes = false;

    public List<SoldierModel> firstLineModels;
    public bool modelAttacked = false;
    public bool modelTookDamage = false;
    public bool inCombat = false;


    [SerializeField] private float secondRowOffsetAmount = 0f;

    [Tooltip("Should we try to attack nearby enemies?")]
    public bool holdFire = false;
    public bool chaseDetectedEnemies = true;

    public bool engagedInMelee = false;

    public bool focusFire = false; //should we pick targets automatically or fire on a specific place/unit
    public Vector3 focusFirePos = new Vector3(0, 0, 0);

    public FormationPosition formationToFocusFire;

    public bool movementManuallyStopped = false;

    public List<Vector3> destinationsList = new List<Vector3>();

    public bool finishedChangedFacing = true;
    [SerializeField] private CharacterController charController;
    [SerializeField] private float charRadius = 4.5f;

    [SerializeField] private bool isCavalry = false;
    [SerializeField] private bool freezeFormPos = false;

    [SerializeField] private float freezeTimer = 0;
    [SerializeField] private float cohesionTimer = 0;

    public bool shouldRotateToward = false;

    public bool alive = true;


    [SerializeField] private int soldierModelToCheck = 0;
    [SerializeField] private bool checkForNearbyEnemies = true;

    [SerializeField] private bool swapRowsAfterFiring = false;

    //swapping rows vars

    [SerializeField] private int required = 10; //all of them
    [SerializeField] private int matched = 0;
    [SerializeField] private int lowerRow = 0;
    [SerializeField] private int upperRow = 9;

    [SerializeField] private List<SoldierModel> modelsInFrontRowThatFired;

    public bool fleeing = false; 

    private void Start()
    {
        required = 10;

        lineRenderer2.SetPosition(0, new Vector3(transform.position.x, -100, transform.position.z));  
        lineRenderer2.SetPosition(1, new Vector3(transform.position.x, -100, transform.position.z));  
        currentSpeed = walkingSpeed;
        aiPath.maxSpeed = currentSpeed;

        if (soldierBlock.melee)
        {
            colliderBoxRange = soldierBlock.modelAttackRange * 2;
        }
        BeginUpdates();

    }

    public void SetBrace(bool val)
    {
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
    public void GetTangledUp()
    {
        if (fleeing)
        {
            FullUnfreeze();
            return;
        }
        freezeFormPos = true;
        tangledUp = true;
        freezeTimer++;
        freezeTimer = Mathf.Clamp(freezeTimer, 0, 3);
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
    public void BreakCohesion()
    {
        charController.enabled = false;
        cohesionTimer++;
        cohesionTimer = Mathf.Clamp(cohesionTimer, 0, 3);
    }
    private void RegainCohesion()
    {
        cohesionTimer--;
        if (cohesionTimer <= 0)
        {
            cohesionTimer = 0;
            charController.enabled = true;
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
    public void BeginUpdates()
    {
        InvokeRepeating("RapidUpdate", 0f, .01f);
        InvokeRepeating("FastUpdate", 0f, .1f);
        InvokeRepeating("SlowUpdate", 0f, .5f); //normally .05f
        InvokeRepeating("VerySlowUpdate", 0f, 1f);
    }
    private  void RapidUpdate()
    {
        AsynchronousSoldierUpdate();
    }
    private void FastUpdate()
    {
        movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, 10), transform.position.z);
        FixRotation();
        UpdateLineRenderer();
        FastSoldierUpdate();
        UpdateFormationMovementStatus();

        if (enableAnimations != oldEnableAnimations) //interrogate purpose
        {

            oldEnableAnimations = enableAnimations;
            foreach (SoldierModel item in soldierBlock.listSoldierModels)
            {
                item.animate = enableAnimations;
            }
        } 
    }

    public void CheckIfSwapRows(SoldierModel model)
    {
        if (!modelsInFrontRowThatFired.Contains(model))
        {
            modelsInFrontRowThatFired.Add(model); 
        }
        if (modelsInFrontRowThatFired.Count >= required)
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
    private void Update()
    { 
        FixRotationModel();
    }
    private void AsynchronousSoldierUpdate()
    {
        //staggered/async 
        if (checkForNearbyEnemies)
        { 
            CheckForNearbyEnemies();
        }

        CheckForEmptyPositionsToFill();

        soldierModelToCheck++;
        int max = maxSoldiers;
        if (soldierModelToCheck >= max) //reset on 80 + 2
        {
            soldierModelToCheck = 0;
        }
    }
    private void FixRotationModel()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.FixRotation();  
                }
            }
        }
    }
    private void CheckForNearbyEnemies()
    {
        SoldierModel checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
        if (checkingModel == null)
        {
            while (checkingModel == null) //let us skip over those that have died
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
            if (checkingModel.melee && listOfNearbyEnemies.Count > 0)
            {
                checkingModel.CheckIfEnemyModelsNearby();
            }
        }
    }
    private void CheckForEmptyPositionsToFill()
    {

        Position position = soldierBlock.formationPositions[positionToCheck];
        if (position != null) //found
        {
            if (position.assignedSoldierModel == null) //if null, then dead
            {
                position.SeekReplacement();
            }
        }
        positionToCheck++;
        int cap = soldierBlock.formationPositions.Length;
        if (positionToCheck >= cap) //reset on 80 + 2
        {
            positionToCheck = 0;
        }
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
                    
                    model.CheckForPendingDamage();
                    if (!model.routing)
                    { 
                        model.UpdateAttackTimer();
                        model.UpdateLoadTimer();
                        model.UpdateDamageTimer();
                        model.UpdateFinishedAttackingTimer();
                    }
                    model.UpdateMovementStatus();
                    model.UpdateRecoveryTimer(); 
                    model.UpdateSpeed();

                }
            }
        } 
    }
    private void SetMoving(bool val)
    { 
        aiPath.canMove = val;
        //aiPath.enableRotation = val;
    }
    private void UpdateFormationMovementStatus()
    {
        if (movementManuallyStopped)
        {
            SetMoving(false);
            obeyingMovementOrder = false;
        }
        else
        {
            if (chaseDetectedEnemies && enemyFormationToTarget != null)
            {
                SetMoving(true);
            }
            else
            {
                if (modelAttacking && soldierBlock.canBeRanged) //if ranged and attacking, freeze formation
                {
                    SetMoving(false);
                    obeyingMovementOrder = false;
                }
                else
                {
                    if (aiPath.remainingDistance > threshold) // if there's still path to traverse
                    {
                        SetMoving(true);
                    }
                    else if (aiPath.reachedDestination && aiPath.reachedEndOfPath && aiPath.remainingDistance <= threshold)
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
    private void SlowUpdate()
    {
        /*for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                }
            }
        }*/
        //FixRotation();
        if (!fleeing)
        { 
            CheckIfInMeleeRange();
        }
    }
    private void VerySlowUpdate()
    {
        if (timeUntilAllowedToCastMagicAgain > 0)
        {
            timeUntilAllowedToCastMagicAgain--;
            if (timeUntilAllowedToCastMagicAgain <= 0)
            {
                allowedToCastMagic = true;
                FightManager obj = FindObjectOfType<FightManager>();
                obj.UpdateGUI();
            }
        }

        if (!abilityCharged)
        {
            currentAbilityRechargeTime += 1;

            if (currentAbilityRechargeTime >= abilityRechargeTime)
            {
                abilityCharged = true;
                FightManager obj = FindObjectOfType<FightManager>();
                obj.UpdateGUI();
                currentAbilityRechargeTime = 0;
            }
        }

        /*if (fleeing)
        {
            float bounds = 25;
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-bounds, bounds), 0, UnityEngine.Random.Range(-bounds, bounds));
            aiTarget.transform.position = aiTarget.transform.position + randomPosition;
            Debug.Log(randomPosition);
        }*/
        if (numberOfAliveSoldiers <= 0) //if all soldiers dead, then goodbye
        {
            foreach (FormationPosition item in listOfNearbyEnemies)
            {
                item.listOfNearbyEnemies.Remove(this);
            }
            gameObject.SetActive(false);
            return;
        }
        if (fleeing)
        {
            FullUnfreeze();
            BreakCohesion();
        }
        else
        {

            CheckNearbyFormations(); //
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
        UpdateSoldiers(); // 
        UpdateSpeed(); // 
        UpdateCollider(); // 
    }
    private void UpdateSoldiers()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    if (model.melee)
                    { 
                        if (listOfNearbyEnemies.Count > 0) //only check if enemies are nearby and we're melee
                        {
                            //model.CheckIfEnemyModelsNearby();
                        }
                        model.StopAttackingWhenNoEnemiesNearby();
                        model.UpdateDeploymentStatus();
                    }
                    model.CullAnimations();
                    model.CheckIfIdle(); 
                    model.CheckIfAlive();
                    model.UpdateCharController();
                }
            }
        }
        
    }
    private void UpdateSpeed()
    {

        if (listOfNearbyEnemies.Count == 0) //no enemy
        {
            weaponsDeployed = false;
        }
        else //yes enemy
        {
            weaponsDeployed = true;
        }
        if (tangledUp)
        {
            currentSpeed = walkingSpeed;
            float current = numberOfAliveSoldiers;
            float maxSol = maxSoldiers;
            float ratio = current / maxSol;

            currentSpeed *= ratio;
            aiPath.rotationSpeed = slowRotate;
        }
        else
        {
            currentSpeed = walkingSpeed;
            aiPath.rotationSpeed = normRotate;
        }
        float min = 0.1f;
        float max = 100f;
        currentSpeed = Mathf.Clamp(currentSpeed, min, max);

        aiPath.maxSpeed = currentSpeed;

    }
    private void CheckNearbyFormations()
    {
        listOfNearbyEnemies.Clear();
        LayerMask layerMask = LayerMask.GetMask("Formation");
        int maxColliders = 24;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].gameObject.tag != "Formation")
            {
                continue;
            }
            else
            {
                if (hitColliders[i] == rectangleCollider) //ignore own collider
                {
                    continue;
                }
                FormationPosition form = hitColliders[i].gameObject.GetComponent<FormationPosition>();
                if (form.team == team)
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
    private void CheckIfLowSoldiersRout()
    {
        if (numberOfAliveSoldiers <= 10 && !fleeing)
        {
            BeginFleeing();  
        }
    }

    private void BeginFleeing()
    {
        Vector3 pos = transform.position + (-transform.forward * 200);
        //Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-999, 999), 0, UnityEngine.Random.Range(-999, 999));
        aiTarget.transform.position = pos;
        CheckIfRotateOrNot();
        fleeing = true;

        FightManager obj = FindObjectOfType<FightManager>();
        if (obj.selectedFormations.Contains(this))
        {
            obj.selectedFormations.Remove(this);
        }
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
    }

    private void CheckIfInCombat()
    {
        if (listOfNearbyEnemies.Count > 0)
        { 
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
        }

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

    private void UpdateCollider()
    {
        int num = 0;
        if (numberOfAliveSoldiers >= 71) //80
        {
            num = 0;
            zSelLine = 4;
        }
        else if (numberOfAliveSoldiers >= 61) //70
        {
            num = 1;
            zSelLine = 3;
        }
        else if (numberOfAliveSoldiers >= 51) //70
        {
            num = 2;
            zSelLine = 2;
        }
        else if (numberOfAliveSoldiers >= 41) //70
        {
            num = 3;
            zSelLine = 1;
        }
        else if (numberOfAliveSoldiers >= 31) //70
        {
            num = 4;
            zSelLine = 0;
        }
        else if (numberOfAliveSoldiers >= 21) //70
        {
            num = 5;
            zSelLine = -1;
        }
        else if (numberOfAliveSoldiers >= 11) //70
        {
            num = 6;
            zSelLine = -2;
        }
        else if (numberOfAliveSoldiers >= 1) //70
        {
            num = 7;
            zSelLine = -3;
        }
        Vector3[] array = new Vector3[5];
        array[0] = new Vector3(-5, 0, -zSelLine);
        array[1] = new Vector3(5, 0, -zSelLine);
        array[2] = new Vector3(5, 0, 4);
        array[3] = new Vector3(-5, 0, 4);
        array[4] = new Vector3(-5, 0, -zSelLine);

        if (!isCavalry)
        {
            //float centerOffset = 16.24f;
            charController.radius = Mathf.Clamp(charRadius - (num * 0.5f), .5f, 4.5f);
            posParentTransform.localPosition = new Vector3(-4.5f, 0, 3.5f - num * .5f);

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

    private void CheckIfInMeleeRange()
    {
        if (enemyFormationToTarget != null)
        { 
            float dist = GetDistance(transform, enemyFormationToTarget.gameObject.transform);
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
    
   

    private void UpdateLineRenderer()
    {
        if (!aiPath.reachedDestination)
        {
            lineRenderer.enabled = selected;
            lineRenderer2.enabled = selected;

            if (lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y+1.5f, transform.position.z)); //offsetting pos4 +0.585001f
                int count = 1;
                lineRenderer.positionCount = destinationsList.Count + 1; 
                foreach (Vector3 item in destinationsList)
                { 
                    lineRenderer.SetPosition(count, item);
                    count++;
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
        Vector3 heading = aiTarget.transform.position - transform.position;
        float threshold = 30;
        if (Vector3.Angle(heading, -transform.forward) <= threshold)
        {
            Debug.Log("good");
            aiPath.enableRotation = false;
        }
        else
        {
            aiPath.enableRotation = true;
        }
    }
    public void CheckDirectionOfMovement()
    {
        compass.LookAt(aiTarget);
        float rot = compass.localEulerAngles.y;
        float threshold = 10;
        Debug.LogError(rot);
        if (-threshold <= rot && rot <= threshold)
        {
            Debug.LogError("front");
            aiPath.enableRotation = false; 
        }
        else if (90 - threshold <= rot && rot <= 90 + threshold)
        {
            Debug.LogError("Side1");
            aiPath.enableRotation = false;
             
        }
        else if (270 - threshold <= rot && rot <= 270 + threshold)
        {
            Debug.LogError("Side2");
            aiPath.enableRotation = false; 
        }
        else if (180 - threshold <= rot && rot <= 180 + threshold)
        {
            Debug.LogError("back");
            aiPath.enableRotation = false; 
        }
        else
        {
            aiPath.enableRotation = true;
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
                    if (mage.magicCharged && mage.alive)
                    { 
                        mage.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
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
                    if (abilityCharged && model.alive)
                    {
                        model.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                        abilityCharged = false;
                        break;
                    }
                }
            }
        }
    }
    public void SetSelected(bool val)
    {
        selected = val; 
    }
     

    private void FindClosestFormation()
    {
        if (listOfNearbyEnemies.Count <= 0)
        {
            enemyFormationToTarget = null;
            aiPath.endReachedDistance = moveStopDistance;
            aiPath.maxSpeed = currentSpeed;
            weaponsDeployed = false;
            tangledUp = false;
            return;
        }
        enemyFormationToTarget = listOfNearbyEnemies[0]; 
        float initDist = GetDistance(transform, listOfNearbyEnemies[0].transform);
        float compareDist = initDist;
        foreach (FormationPosition item in listOfNearbyEnemies) //doesn't work yet
        { 
            float dist = GetDistance(transform, item.gameObject.transform);
            //Debug.LogError(dist);
            if (dist < compareDist)
            {
                enemyFormationToTarget = item;
                compareDist = dist;
            }
            
        } 
        if (soldierBlock.canBeRanged)
        { 
        } 
        else
        { 
            if (chaseDetectedEnemies)
            {
                EngageFoe();
            }
        }

    }

    public void ClearOrders()
    { 
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
    private void EngageFoe()
    {
        if (fleeing)
        {
            return;
        }
        float dist = GetDistance(transform, enemyFormationToTarget.gameObject.transform);
        if (dist <= stoppingDistance)
        { //stop
            //Debug.Log("stopping");
            ResetAITarget();
        }
        else if (enemyFormationToTarget != null)
        { //chase
            //Debug.Log("chasing foe");
            aiTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
            rotTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
            CheckIfRotateOrNot();
        }

    }
    public void StopCommand()
    {
        movementManuallyStopped = true;
    }
    public void ResumeCommand()
    {
        movementManuallyStopped = false;
    }
    private void FixRotation()
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
    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
