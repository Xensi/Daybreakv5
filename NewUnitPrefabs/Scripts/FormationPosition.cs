using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    [SerializeField] private float AIIntelligence = 50;
    public bool usesSpears = true;
    public ShakeSource shaker;
    private FightManager fightManager;
    public int shotsHit = 0;
    public bool showSoldierModels = true;
    public SpriteRenderer farAwayIcon;
    public GameObject farAwayIconMask;
    public enum FormationType
    {
        Infantry,
        RangedInfantry,
        Cavalry
    }
    public FormationType type = FormationType.Infantry; //just by default

    public bool abilityCharged = true; 
    [SerializeField] private float abilityRechargeTime = 60f;
    [SerializeField] private float currentAbilityRechargeTime = 0;
    public bool allowedToCastMagic = true;
    public float timeUntilAllowedToCastMagicAgain = 0;
    public bool canBrace = false;
    public bool braced = false;
    public string team = "Altgard"; //Whose team are we on?
    public RichAI aiPath;
    private float threshold = .5f;
    public List<FormationPosition> listOfNearbyEnemies; 
    public Transform aiTarget;
    public Transform rotTarget; 
    [Tooltip("Checks nearby formations. Nearby formations can be moved towards automatically.")]
    public float engageEnemyRadius = 10;
    public float rangedRadius = 100;

    [HideInInspector] public float startingPursueRadius = 0; //do not modify manually

    [Tooltip("When to stop moving when auto-engaging.")]
    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1; 
    //
    public FormationPosition enemyFormationToTarget;
    public SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;
    //

    public float movingSpeed = 0;
    [SerializeField] private float currentSpeed = 0; 
    public float walkingSpeed = 3.5f;
    public float sprintSpeed = 6.5f;

    public bool selected = false; 
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

    public bool modelAttacking = false;

    public bool obeyingMovementOrder;

    public int numberOfAliveSoldiers = 80;
    private int oldNumAlive = 80;
    public int maxSoldiers = 80;

    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;

    [SerializeField] private float normRotate = 30; 

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
    [SerializeField] private BoxCollider charCollider;
    [SerializeField] private BoxCollider forwardControlZone;
    [SerializeField] private float charRadius = 4.5f;

    public bool isCavalry = false;
    [SerializeField] private bool freezeFormPos = false;

    [SerializeField] private float freezeTimer = 0;
    [SerializeField] private float cohesionTimer = 0;

    public bool shouldRotateToward = false;

    public bool alive = true;


    [SerializeField] private int soldierModelToCheck = 0;
    [SerializeField] private bool checkForNearbyEnemies = true;

    [SerializeField] private bool swapRowsAfterFiring = false;

    //swapping rows vars

    [SerializeField] private int requiredModelsThatFiredInRow = 5; //all of them
    [SerializeField] private int matched = 0;
    [SerializeField] private int lowerRow = 0;
    [SerializeField] private int upperRow = 9;

    [SerializeField] private List<SoldierModel> modelsInFrontRowThatFired;

    public bool fleeing = false;

    public float averagePositionBasedOnSoldierModels = 0;

    public Vector3 formationPositionBasedOnSoldierModels;

    public GameObject missileTarget;
    [SerializeField] private bool alwaysRotateTowardMovementPos = false;
    public SpriteRenderer selectedSprite;
    [SerializeField] private float chargeSpeed = 7;
    public bool charging = false;
    public bool selectable = true;
    [SerializeField] private float maxChargeTime = 10;
    [SerializeField] private float currentChargeTime = 0;
    [SerializeField] private float chargeRechargeTime = 30;
    private float currentChargeRechargeTime = 0;
    public bool chargeRecharged = true;

    [SerializeField] private Rigidbody rigid;

    [SerializeField] private bool simultaneousPositionCheck = false;

    [SerializeField] private float aiBraceRadius = 75;

    [SerializeField] private NavmeshCut navCutter;

    private void Start()
    {
        //set collision mode
        rigid = GetComponent<Rigidbody>();
        rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigid.drag = 50;
        //
        

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
        /*if (forwardControlZone != null)
        {
            if (type == FormationType.Infantry)
            {
                forwardControlZone.enabled = true;
            }
        }*/

        SetStandardEngagementRanges();

        requiredModelsThatFiredInRow = 10;

        lineRenderer2.SetPosition(0, new Vector3(transform.position.x, -100, transform.position.z));
        lineRenderer2.SetPosition(1, new Vector3(transform.position.x, -100, transform.position.z));
        currentSpeed = walkingSpeed;
        chargeSpeed = walkingSpeed * 3;
        aiPath.maxSpeed = currentSpeed;
         
        BeginUpdates();

        //PlaceAITargetOnTerrain();
        if (shaker == null)
        {
            shaker = GetComponentInChildren<ShakeSource>();
        }
    }
    #region Updates
    public void BeginUpdates()
    {
        InvokeRepeating("RapidUpdate", 0f, .01f);
        InvokeRepeating("FastUpdate", 0f, .1f);
        InvokeRepeating("SlowUpdate", 0f, .5f); //normally .05f
        InvokeRepeating("VerySlowUpdate", 0f, 1f);
    }

    private void RapidUpdate()
    {
        FixRotationModel();
        AsynchronousSoldierUpdate();
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
    void LateUpdate()
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
                            if (fightManager == null)
                            {
                                fightManager = FindObjectOfType<FightManager>();
                            }
                            if (model.lineOfSightIndicator != null)
                            {
                                model.lineOfSightIndicator.transform.LookAt(model.lineOfSightIndicator.transform.position + fightManager.cam.transform.forward);
                            }
                        }
                    }
                }
            }
        }
    }
    private void AsynchronousSoldierUpdate()
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
            checkingModel.SaveFromFallingInfinitely(); 
        }
        if (soldierBlock.melee) //we only need to check if enemies are near us if we are melee. ranged does it on a formation basis
        {
            //staggered/async 
            if (checkForNearbyEnemies)
            {
                if (checkingModel.alive)
                {
                    if (checkingModel.melee && listOfNearbyEnemies.Count > 0)
                    {
                        checkingModel.CheckIfEnemyModelsNearby();
                    }
                }
            }
        }
        else
        {
            if (checkingModel.alive)
            {
                checkingModel.LineOfSightUpdate();
            }
        }
        soldierModelToCheck++;
        int max = maxSoldiers;
        if (soldierModelToCheck >= max) //reset on 80 + 2
        {
            soldierModelToCheck = 0;
        }

    }
    private void FastUpdate()
    {
        FixRotation();
        UpdateFormationMovementStatus();
        CheckForEmptyPositionsToFill(); 
        SimultaneousPositionCheck();

        movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector
        float magic = 15;
        transform.position = new Vector3(transform.position.x, magic, transform.position.z);
        //UpdateLineRenderer();
        FastSoldierUpdate();

        if (enableAnimations != oldEnableAnimations) //interrogate purpose
        { 
            oldEnableAnimations = enableAnimations;
            foreach (SoldierModel item in soldierBlock.listSoldierModels)
            {
                item.animate = enableAnimations;
            }
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
                        model.UpdateDamageTimer();
                        model.UpdateFinishedAttackingTimer();
                    }
                    model.UpdateMovementStatus();
                    model.UpdateRecoveryTimer();
                    model.UpdateSpeed();
                    model.UpdateVisibility();
                }
            }
        }
    }

    private void SlowUpdate()
    {
        if (!fleeing)
        {
            CheckIfInMeleeRange();
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
    private void VerySlowUpdate()
    {
        if (numberOfAliveSoldiers <= 0) //if all soldiers dead, then goodbye
        {
            foreach (FormationPosition item in listOfNearbyEnemies)
            {
                item.listOfNearbyEnemies.Remove(this);

            }
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

                if (fightManager == null)
                {
                    fightManager = FindObjectOfType<FightManager>();
                }
                fightManager.UpdateGUI();
            }
        }
        if (!chargeRecharged)
        {
            currentChargeRechargeTime += 1;
            if (currentChargeRechargeTime >= chargeRechargeTime)
            {
                chargeRecharged = true;
                if (fightManager == null)
                {
                    fightManager = FindObjectOfType<FightManager>();
                }
                fightManager.UpdateGUI();
                currentAbilityRechargeTime = 0;
            }
        }
        if (charging)
        {
            currentChargeTime += 1;
            if (currentChargeTime >= maxChargeTime)
            {
                StopCharging();
                currentChargeTime = 0;
            }
        }

        if (!abilityCharged)
        {
            currentAbilityRechargeTime += 1;

            if (currentAbilityRechargeTime >= abilityRechargeTime)
            {
                abilityCharged = true;

                if (fightManager == null)
                {
                    fightManager = FindObjectOfType<FightManager>();
                }
                fightManager.UpdateGUI();
                currentAbilityRechargeTime = 0;
            }
        }
        if (fleeing)
        {
            FullUnfreeze();
            BreakCohesion();
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
        UpdateSoldiers(); // 
        UpdateDeployment();
        UpdateSpeed(); // 
        UpdateCollider(); // 

    }
    private void UpdateSoldiers()
    {
        float height = 0;
        float num = 0;
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateAttackTimer();
                    model.UpdateLoadTimer();
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

                    height += model.transform.position.y;
                    num++;
                }
            }
        }

        height = height / num;
        float avgHeight = height;
        /*float offset = 5f;
        height += offset;*/
        averagePositionBasedOnSoldierModels = height;

        formationPositionBasedOnSoldierModels = new Vector3(transform.position.x, averagePositionBasedOnSoldierModels, transform.position.z);

        farAwayIcon.transform.position = new Vector3(farAwayIcon.transform.position.x, avgHeight, farAwayIcon.transform.position.z); //set to average height
        farAwayIconMask.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight, farAwayIconMask.transform.position.z);
        if (selectedSprite != null)
        { 
            selectedSprite.transform.position = new Vector3(farAwayIconMask.transform.position.x, avgHeight, farAwayIconMask.transform.position.z);
        }
    }
    private void UpdateDeployment()
    {
        if (listOfNearbyEnemies.Count == 0) //no enemy
        {
            weaponsDeployed = false;
        }
        else //yes enemy
        {
            weaponsDeployed = true;
        }
    }
    private void UpdateSpeed()
    { 
        if (frozen)
        {
            currentSpeed = 0;
        }
        else if (tangledUp)
        {
            currentSpeed = walkingSpeed * .75f; //reduce speed by 25%
            float current = numberOfAliveSoldiers;
            float maxSol = maxSoldiers;
            float ratio = current / maxSol;

            currentSpeed *= ratio;
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
    private void UnfreezeMovement() {
        frozen = false;
        float time = 3;
        Invoke("AllowFreezing", time);
    }
    private void AllowFreezing()
    {
        canBeFrozenAgain = true;
    }
    private void CheckForNearbyEnemyFormations()
    {
        listOfNearbyEnemies.Clear();
        LayerMask layerMask = LayerMask.GetMask("Formation");
        int maxColliders = 40;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, engageEnemyRadius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage

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
    #endregion
    public void StartCharging()
    {
        if (!isCavalry)
        {
            if (chargeRecharged)
            {
                chargeRecharged = false;
                //Debug.Log("charging");
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
                        soldierBlock.modelsArray[i].ToggleAttackBox(true);
                    }
                }
            }
        }
         
    }
    private void StopCharging()
    {
        if (!isCavalry)
        { 
            charging = false;
            selectable = true;
            chargeRecharged = false;
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                if (soldierBlock.modelsArray[i] != null)
                {
                    soldierBlock.modelsArray[i].ToggleAttackBox(false);
                }
            }
        }
    }
    #region Fixers
    private void PlaceThisOnGround()
    { 
        //aiTarget.transform.position
        LayerMask layerMask = LayerMask.GetMask("GroundForm");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
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
    }
    private void PlaceAITargetOnTerrain()
    {
        //aiTarget.transform.position
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
        rigid.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    } 
    public void SetBrace(bool val)
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
    #endregion
    public void AICheckIfNeedToBrace()
    {
        bool enemyInBraceRadius = false;
        float dist = 0;
        for (int i = 0; i < listOfNearbyEnemies.Count; i++)
        {
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
        } 
    }
    public void GetTangledUp()
    {
        if (fleeing)
        {
            FullUnfreeze();
            ShowFormIcon(false);
            Color color = farAwayIcon.color;
            color.a = 0;
            farAwayIcon.color = color;
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
    public void BreakCohesion()
    {
        if (charController != null)
        { 
            charController.enabled = false;
        }
        if (charCollider != null)
        {
            charCollider.enabled = false;
        }
        cohesionTimer++;
        cohesionTimer = Mathf.Clamp(cohesionTimer, 0, 3);
    }
    private void RegainCohesion()
    {
        cohesionTimer--;
        if (cohesionTimer <= 0)
        {
            cohesionTimer = 0;

            if (charController != null)
            {
                //charController.enabled = true;
            }
            if (charCollider != null)
            {
                charCollider.enabled = true;
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
    public void CheckForEmptyPositionsToFill()
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
        if (numberOfAliveSoldiers <= 10 && !fleeing)
        {
            BeginFleeing();  
        }
    }
    private void ShowFormIcon(bool val)
    {
        farAwayIcon.enabled = val;
        farAwayIconMask.SetActive(val);
        selectedSprite.enabled = val;
    }
    private void BeginFleeing()
    {
        fleeing = true;
        ShowFormIcon(false);

        if (fightManager == null)
        {
            fightManager = FindObjectOfType<FightManager>();
        }
        fightManager.DeselectFormation(this);

        Vector3 pos = transform.position + (-transform.forward * 200);
        //Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-999, 999), 0, UnityEngine.Random.Range(-999, 999));
        aiTarget.transform.position = pos;
        CheckIfRotateOrNot();
         
        if (fightManager.selectedFormations.Contains(this))
        {
            fightManager.selectedFormations.Remove(this);
        }
        if (fightManager.aiFormations.Contains(this))
        {
            fightManager.aiFormations.Remove(this);
        } 
        if (fightManager.allFormations.Contains(this))
        {
            fightManager.allFormations.Remove(this);
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
        int z = 8; 
        /*Vector3[] array = new Vector3[5];
        array[0] = new Vector3(-5, 0, -zSelLine);
        array[1] = new Vector3(5, 0, -zSelLine);
        array[2] = new Vector3(5, 0, 4);
        array[3] = new Vector3(-5, 0, 4);
        array[4] = new Vector3(-5, 0, -zSelLine);
*/
        if (!isCavalry)
        {
            //float centerOffset = 16.24f;
            if (charController != null)
            { 
                charController.radius = Mathf.Clamp(charRadius - (num * 0.5f), .5f, 4.5f);
            }
            float offset = 0;
            posParentTransform.localPosition = new Vector3(-4.5f, offset, 3.5f - num * .5f);
            int x = 10;
            int y = 4;
            int math = z - num;
            if (charCollider != null)
            {
                charCollider.size = new Vector3(x, y, math); 
            }
            int defSize = 10;
            float remedy = 1.25f;
            float calc = math * remedy;

            farAwayIconMask.gameObject.transform.localScale = new Vector3(defSize, calc, 1);
            farAwayIcon.gameObject.transform.localScale = new Vector3(defSize, calc, 1);
            selectedSprite.gameObject.transform.localScale = new Vector3(defSize+1, calc+1, 1);
            farAwayIconMask.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            farAwayIcon.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
            selectedSprite.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

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
    private void UpdateLineRenderer()
    {
        if (!aiPath.reachedDestination)
        {
            lineRenderer.enabled = selected;
            lineRenderer2.enabled = selected;

            if (lineRenderer.enabled)
            {

                lineRenderer.SetPosition(0, new Vector3(formationPositionBasedOnSoldierModels.x, formationPositionBasedOnSoldierModels.y, formationPositionBasedOnSoldierModels.z)); //offsetting pos4 +0.585001f
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
    }

    private void ShowSelected(bool val)
    {
        if (selectedSprite != null)
        {
            selectedSprite.enabled = val;
        }
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
        //float initDist = Vector3.Distance(transform.position, listOfNearbyEnemies[0].transform.position);
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, listOfNearbyEnemies[0].transform.position);
        float compareDist = initDist;
        foreach (FormationPosition item in listOfNearbyEnemies) //doesn't work yet
        {
            //float dist = GetDistance(transform, item.gameObject.transform);
            float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
            //Debug.LogError(dist);
            if (dist < compareDist)
            {
                enemyFormationToTarget = item;
                compareDist = dist;
            }

        }
        if (chaseDetectedEnemies)
        {
            EngageFoe();
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
    private void EngageFoe()
    {
        if (fleeing)
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, engageEnemyRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, aiBraceRadius);
    }
}
