using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Threading;
using System.Threading.Tasks;
public class SoldierModel : DamageableEntity
{
    public MagicModule magicModule;
    #region AssignedAtStart

    private NavmeshCut navMeshCutter;
    [HideInInspector] public RichAI pathfindingAI; 
    private Seeker seeker;
    [HideInInspector] public Animator animator; 
    [HideInInspector] public FormationPosition formPos;
    [HideInInspector] public SpriteRenderer lineOfSightIndicator;
    [HideInInspector] public SpriteRenderer reloadingIndicator;

    [HideInInspector] public float walkSpeed = 3;
    [HideInInspector] public float runSpeed = 6; //not used
    [HideInInspector] public bool melee = true;
    public enum AttackType
    {
        Melee,
        Ranged,
        CavalryCharge,
        None
    }
    public AttackType attackType = AttackType.Melee;
    #endregion
    #region MustBeAssignedManually
    [Header("Assign these! If null then something will break")] 
    [SerializeField] private GameObject modelProj;
    public GameObject selectionCircle;  
    public List<Renderer> renderers;

    public Renderer[] renderersArray;

    [SerializeField] private AudioSource voiceSource;  
    public AudioSource impactSource;
    public AudioSource hurtSource;

    [Header("Assign this if needed (cavalry, braced inf)")]
    public AttackBox attackBox;
    #endregion

    #region Generic
    [Header("Change these per unit")]

    [SerializeField] private float knockDownForSecondsMax = 6;
    [SerializeField] public float startingMaxSpeed = 3; //defined by richai starting maxspeed settings, typically walk speed
    [SerializeField] private float adjustWalkSpeedVisually = 1;
    [SerializeField] private bool playIdleAnims = false;
    //[SerializeField] private float health = 10;
    public float damage = 1;
    public float armorPiercingDamage = 0;
    //[SerializeField] private float armor = 0;

    [SerializeField] private float requiredDamageTime = .8f;
    [SerializeField] private float currentDamageTime = 0;
    [SerializeField] private float requiredTimeUntilFinishedAttacking = 1f;
    private float currentFinishedAttackingTime = 0;
    [SerializeField] private float requiredTimeUntilRecovery = 1f;
    private float currentRecoveryTime = 0;
    public float requiredAttackTime = 3;
    public float currentAttackTime = 3;
    [HideInInspector] public float oldRequiredAttackTime = 3;

    //[SerializeField] private bool stopWhenAttacking = true;
    [SerializeField] private bool whenDamagedSwitchToDamagedState = true;
    //[SerializeField] private bool stopWhenLoading = true;  
    [SerializeField] private bool attacksBreakEnemyCohesion = false; //when hitting an enemy, can we then pass through them? //cavalry ability
    [SerializeField] private bool attacksSplinterEnemyCohesion = false; //hitting enemies lowers their collider size
    [SerializeField] private int inflictSplinterAmount = 1;
    //[SerializeField] private bool directionalAttacks = false;
    //[SerializeField] private float angleOfAttack = 20; 

    [SerializeField] private float defaultStoppingDistance = 0.01f; //could deprecate 
    public float meleeAttackRange = 1;
    public float rangedAttackRange = 160;
    [SerializeField] private float documentedMaxSpeed = 5;
    #endregion

    #region MagicUsers
    [Header("Magic")]
    [SerializeField] private bool isMagic = false;
    [SerializeField] private float magicRechargeTime = 60f;
    private float currentMagicRecharge = 0;
    [HideInInspector] public bool magicCharged = true;
    #endregion

    #region Status 
    [HideInInspector] public bool braced = false;
    private bool allowedToDealDamage = true;
    public Position modelPosition;
    public float movingSpeed = 0;
    //[HideInInspector] public bool alive = true;
    [HideInInspector] public bool attacking = false;
    //public bool moving = false;
    public bool damaged = false;
    private bool deployed = false;
    private bool idle = false;
    public bool knockedDown = false;
    public bool airborne = false;
    public DamageableEntity targetDamageable;
    public SoldierModel targetEnemy;
    [HideInInspector] public bool pendingLaunched = false;
    public bool hasClearLineOfSight = false; 
    //public GlobalDefines.Team team = GlobalDefines.Team.Altgard;
    [HideInInspector] public float attackRange = 1;
    [HideInInspector] public bool animate = false;
    [HideInInspector] public float currentSpeed = 0;
     public List<SoldierModel> nearbyEnemyModels;
    #endregion 

    #region Sound Effects
    [Header("Sounds")]
    [SerializeField] private List<AudioClip> deathSounds;
    [SerializeField] private List<AudioClip> deathReactionSounds;
    public List<AudioClip> attackSounds;
    public List<AudioClip> projectileImpactSounds;
    [SerializeField] private List<AudioClip> attackVoiceLines;
    [SerializeField] private List<AudioClip> idleVoiceLines;
    [SerializeField] private List<AudioClip> hurtVoiceLines;
    [SerializeField] private List<AudioClip> incomingVoiceLines;
    [SerializeField] private List<AudioClip> exhaustedVoiceLines;
    [SerializeField] private List<AudioClip> movingVoiceLines;
    #endregion

    #region Animation
    [Header("UnimportantSetters")]
    [SerializeField] private int reqIdleTimer = 20; 
    [SerializeField] private int numRandIdleAnims = 1;
    [SerializeField] private int numRandAttackAnims = 1; 
    //public List<SkinnedMeshRenderer> normalMeshes;
    //public List<SkinnedMeshRenderer> veteranMeshes;
    #endregion

    #region ShouldNotSet
    [HideInInspector] public GameObject self;
    private int currentIdleTimer = 0;
    private float remainingDistanceThreshold = 0.01f;
    [HideInInspector] public float getUpTimeCap = 8;
    [HideInInspector] private float speedSlow = 0;
    [HideInInspector] public float pendingDamage = 0;
    [HideInInspector] public float pendingArmorPiercingDamage = 0;
    [HideInInspector] public float normalizedSpeed = 0; 
    [HideInInspector] public Transform pendingDamageSource; 
    [HideInInspector] public bool isVeteran = false;
    [HideInInspector] public bool ignoreAsNearby = false;
    [HideInInspector] public float getUpTime = 0; 
    #endregion

    #region Unused 
    private float waitForAttackChatterTime = 10;
    private float waitForMarchChatterTime = 10;
    private float waitForDeathReactionChatterTime = 10;
    private float waitForIdleChatterTime = 10;
    private int numKills = 0;
    #endregion
    private float currentDeployTime = 0;
    private float requiredDeployTime = 1;

    private LayerMask modelLayerMask;

    [HideInInspector] public RangedModule rangedModule;
    public Transform target;

    [Range(0, 1f)]
    public float dispersalLevel;
    public float oldDispersalLevel;
    public Vector3 dispersalVector;

    public AnimatedMesh animatedMesh; 

    public void PlaceOnGround()
    {
        transform.rotation = Quaternion.Euler(0, transform.rotation.y, 0);
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        } 
    }

    private void Awake()
    {
        magicModule = GetComponent<MagicModule>();
        modelLayerMask = LayerMask.GetMask("Model");
        #region Initializations 
        if (attackType == AttackType.Ranged)
        {
            if (rangedModule == null)
            {
                rangedModule = GetComponent<RangedModule>();
                if (rangedModule == null)
                {
                    Debug.LogError("Ranged unit is missing RangedModule");
                }
            }
            if (rangedModule != null)
            {
                rangedModule.model = this;
            }
        } 

        if (renderers.Count <= 0)
        { 
            SkinnedMeshRenderer[] array = GetComponentsInChildren<SkinnedMeshRenderer>();
            renderers.AddRange(array);
            MeshRenderer[] meshArray = GetComponentsInChildren<MeshRenderer>();
            renderers.AddRange(meshArray);

            renderersArray = renderers.ToArray();
        }

        if (animatedMesh == null)
        {
            animatedMesh = GetComponentInChildren<AnimatedMesh>();
        } 
        if (navMeshCutter == null)
        {
            navMeshCutter = GetComponent<NavmeshCut>();
        }

        if (pathfindingAI == null)
        {
            pathfindingAI = GetComponent<RichAI>();
        } 

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (seeker == null)
        {
            seeker = GetComponent<Seeker>();
        } 
        if (lineOfSightIndicator == null)
        {
            lineOfSightIndicator = transform.Find("LOSIndicator").GetComponent<SpriteRenderer>();
        }
        if (reloadingIndicator == null)
        {
            reloadingIndicator = transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>();
        } 
        #endregion 

        if (lineOfSightIndicator != null)
        { 
            lineOfSightIndicator.enabled = false;
        }
        if (reloadingIndicator != null)
        {
            reloadingIndicator.enabled = false;
        }
        startingMaxSpeed = pathfindingAI.maxSpeed; 
        //
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool(AnimatorDefines.walkingID, true);
        currentSpeed = walkSpeed;
        //richAI.endReachedDistance = defaultStoppingDistance;

        if (attackType == AttackType.Melee)
        {
            attackRange = meleeAttackRange;
            SetDeployed(false);
            animator.SetBool(AnimatorDefines.meleeID, true);
            animator.SetInteger(AnimatorDefines.ammoID, 999);
        }
        else
        {
            attackRange = rangedAttackRange;
            SetDeployed(true); //ranged is always deployed . . . ? for now.
            animator.SetBool(AnimatorDefines.meleeID, false);
        }

        animator.SetFloat(AnimatorDefines.angleID, 0);  
        PlaceOnGround();

        animator.cullingMode = AnimatorCullingMode.CullCompletely;
        //animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        pathfindingAI.enableRotation = true;
        oldDispersalLevel = dispersalLevel;
        oldRequiredAttackTime = requiredAttackTime;
        //animator.enabled = false;
    } 
    /// <summary>
    /// Sets destination, that's all. use search path to actually calculate and go to 
    /// </summary>
    public void UpdateDestinationPosition() 
    {
        if (target != null)
        {
            //pathfindingAI.destination = formPos.destinationsList[0];
            pathfindingAI.destination = target.position + dispersalVector;
        }
    }
    public Vector3 GenerateDispersalVector(float dispersal)
    {
        return dispersalVector = new Vector3(UnityEngine.Random.Range(-dispersal, dispersal), 0, UnityEngine.Random.Range(-dispersal, dispersal));
    }
    public bool IsVisible()
    {
        for (int i = 0; i < renderersArray.Length; i++)
        {
            if (renderersArray[i].isVisible)
            {
                return true;
            }
        }
        return false;
    }
    public void SaveFromFallingInfinitely()
    {
        if (transform.position.y <= 0)
        {
            PlaceOnGround();
        }
    }
    public void SetBrace(bool val)
    {
        braced = val;
        if (navMeshCutter != null)
        {
            navMeshCutter.enabled = val;
        }
    }
    public void ModelStartRouting()
    {
        float dispersalModifier = 4;
        GenerateDispersalVector(dispersalLevel * dispersalModifier);
        SwitchState(ModelState.Routing);
        //routing = true;
        /*SetDeployed(false);
        SetAttacking(false);
        if (rangedModule != null)
        { 
            rangedModule.SetLoading(false);
        }
        SetMoving(true); */
    } 
    public void StopRout()
    {
        GenerateDispersalVector(dispersalLevel); 
        SwitchState(ModelState.Idle);
    }
    private void SetKnockedDown(bool val)
    {
        knockedDown = val;
        animator.SetBool(AnimatorDefines.knockedDownID, val);
    }
    private void SetDeployed(bool val)
    {
        deployed = val;
        animator.SetBool(AnimatorDefines.deployedID, val); //and animations will match 
    }
    private void SetAttacking(bool val)
    {
        attacking = val;
        animator.SetBool(AnimatorDefines.attackingID, val); //and animations will match 
        animator.SetInteger(AnimatorDefines.randomAttackID, UnityEngine.Random.Range(0, numRandAttackAnims-1));
        formPos.modelAttacking = val;
    } 
    private void SetAlive(bool val)
    {
        alive = val;
        animator.SetBool(AnimatorDefines.aliveID, val);
    }
    public enum ModelState
    {
        Idle,
        Attacking,
        Moving,
        Damaged,
        Braced,
        Deploying,
        KnockedDown,
        Airborne,
        Reloading,
        Charging,
        Routing,
        Dead,
        Undeploying,
        FinishingAttack,
        Throwing
    }
    public ModelState currentModelState = ModelState.Idle;
    private ModelState lastState;
    public void CheckIfTargetIsDead()
    {
        if (targetEnemy != null && !targetEnemy.alive)
        {
            targetEnemy = null;
        }
    } 
    public async void UpdateModelState(CancellationToken cancelToken)
    {
        switch (currentModelState)
        {
            case ModelState.Idle:
                //do nothing unless
                //if our destination is far enough away from us
                if (CheckIfRemainingDistanceOverThreshold(remainingDistanceThreshold))
                {  
                    SwitchState(ModelState.Moving);  //start moving
                } 
                if (!deployed)
                { 
                    CheckIfShouldDeploy();
                }
                CheckIfCanSwitchToAttacking();
                if (attackType == AttackType.Ranged)
                { 
                    CheckIfCanSwitchToReloading();
                }
                SetSpeedNull(); //force animation walking speed to zero;
                break;
            case ModelState.Attacking: //swinging
                CheckIfCanDealDamage(); 
                break;
            case ModelState.Moving: 
                //UpdateDestination();
                UpdateSpeed();
                //if destination close enough
                if (CheckIfRemainingDistanceUnderThreshold(remainingDistanceThreshold))
                {
                    SwitchState(ModelState.Idle);
                }
                if (!deployed)
                {
                    CheckIfShouldDeploy();
                } 
                CheckIfCanSwitchToAttacking();
                break;
            case ModelState.Damaged:
                CheckIfRecoveredFromDamage();
                break;
            case ModelState.Braced: 
                break;
            case ModelState.Deploying:
                if (lastState == ModelState.Moving)
                {
                    //UpdateDestination();
                    UpdateSpeed();
                }
                CheckIfFinishedDeploying();
                break;
            case ModelState.KnockedDown: 
                break;
            case ModelState.Airborne: 
                break;
            case ModelState.Reloading: //reloading ranged weapon
                CheckIfFinishedReloading();
                break;
            case ModelState.Charging:
                //UpdateDestination();
                UpdateSpeed();
                CheckIfCanSwitchToAttacking();
                break; 
            case ModelState.Routing: //just run!
                pathfindingAI.enableRotation = true;
                deployed = false;
                UpdateSpeed();
                break;
            case ModelState.Dead:
                break; 
            case ModelState.Undeploying:
                if (lastState == ModelState.Moving)
                {
                    //UpdateDestination();
                    UpdateSpeed();
                }
                CheckIfFinishedDeploying();
                break;
            case ModelState.FinishingAttack:
                CheckIfAttackFinished();
                break;
            default:
                break;
        }
        await Task.Yield();
    }
    public void UpdateModelStateManual()
    {
        switch (currentModelState)
        {
            case ModelState.Idle:
                //do nothing unless
                //if our destination is far enough away from us
                if (CheckIfRemainingDistanceOverThreshold(remainingDistanceThreshold))
                {
                    SwitchState(ModelState.Moving);  //start moving
                }
                if (!deployed)
                {
                    CheckIfShouldDeploy();
                }
                CheckIfCanSwitchToAttacking();
                if (attackType == AttackType.Ranged)
                {
                    CheckIfCanSwitchToReloading();
                }
                SetSpeedNull(); //force animation walking speed to zero;
                break;
            case ModelState.Attacking: //swinging
                CheckIfCanDealDamage();
                break;
            case ModelState.Moving:
                //UpdateDestination();
                UpdateSpeed();
                //if destination close enough
                if (CheckIfRemainingDistanceUnderThreshold(remainingDistanceThreshold))
                {
                    SwitchState(ModelState.Idle);
                }
                if (!deployed)
                {
                    CheckIfShouldDeploy();
                }
                CheckIfCanSwitchToAttacking();
                break;
            case ModelState.Damaged:
                CheckIfRecoveredFromDamage();
                break;
            case ModelState.Braced:
                break;
            case ModelState.Deploying:
                if (lastState == ModelState.Moving)
                {
                    //UpdateDestination();
                    UpdateSpeed();
                }
                CheckIfFinishedDeploying();
                break;
            case ModelState.KnockedDown:
                break;
            case ModelState.Airborne:
                break;
            case ModelState.Reloading: //reloading ranged weapon
                CheckIfFinishedReloading();
                break;
            case ModelState.Charging:
                //UpdateDestination();
                UpdateSpeed();
                CheckIfCanSwitchToAttacking();
                break;
            case ModelState.Routing: //just run!
                pathfindingAI.enableRotation = true;
                deployed = false;
                UpdateSpeed();
                break;
            case ModelState.Dead:
                break;
            case ModelState.Undeploying:
                if (lastState == ModelState.Moving)
                {
                    //UpdateDestination();
                    UpdateSpeed();
                }
                CheckIfFinishedDeploying();
                break;
            case ModelState.FinishingAttack:
                CheckIfAttackFinished();
                break;
            default:
                break;
        } 
    }
    private float deltaTime = 0.1f;
    private void CheckIfRecoveredFromDamage()
    {
        if (currentRecoveryTime < requiredTimeUntilRecovery)
        {
            currentRecoveryTime += deltaTime;
        }
        else
        {
            //Debug.Log("Recovered");
            currentRecoveryTime = 0;
            if (lastState == ModelState.Routing)
            {
                SwitchState(ModelState.Routing);
            }
            else
            { 
                SwitchState(ModelState.Idle);
            }
        }
    }

    public bool waitingForAttackOver = false;
    public void CheckIfAttackFinished()
    { 
        if (currentFinishedAttackingTime < requiredTimeUntilFinishedAttacking)
        { 
            currentFinishedAttackingTime += deltaTime;
        }
        else
        { 
            currentFinishedAttackingTime = 0;
            //formPos.CheckIfSwapRows(this);
            SwitchState(ModelState.Idle);
        } 
    }
    private void CheckIfCanSwitchToReloading()
    {
        if (rangedModule.ammo <= 0 && rangedModule.internalAmmoCapacity > 0)
        {
            rangedModule.currentFinishedLoadingTime = 0;
            SwitchState(ModelState.Reloading);
        }
    }
    private void CheckIfFinishedReloading()
    {
        if (rangedModule.currentFinishedLoadingTime < rangedModule.requiredLoadingTime)
        {
            rangedModule.currentFinishedLoadingTime += deltaTime;
        }
        else
        {
            rangedModule.FinishReload();
            SwitchState(ModelState.Idle);
        }
    }
    private void CheckIfFinishedDeploying()
    {
        if (currentDeployTime < requiredDeployTime)
        {
            currentDeployTime += deltaTime;
        }
        else
        {
            deployed = true;
            currentDeployTime = 0;
            SwitchState(lastState);
        }
    }
    private void CheckIfCanDealDamage()
    { 
        if (!DamageIsReady()) //if not ready
        { 
            currentDamageTime += .1f;
        }
        else
        { 
            PlayAttackChatter();
            BeginAttack(); 
        } 
    }
    private void CheckIfShouldDeploy()
    {
        if (HasTargetInRange())
        {
            SwitchState(ModelState.Deploying);
            if (!formPos.inCombat && !formPos.playingAttackChatter)
            {
                PlayDeployChatter();
            }
        } 
    }
    private void PlayDeployChatter()
    {
        formPos.playingAttackChatter = true;
        formPos.DisableAttackChatterForSeconds(10);
        if (formPos.numberOfAliveSoldiers <= formPos.maxSoldiers / 2)
        {
            if (exhaustedVoiceLines.Count > 0)
            {

                voiceSource.PlayOneShot(exhaustedVoiceLines[UnityEngine.Random.Range(0, exhaustedVoiceLines.Count)]);
            }
        }
        else
        {
            if (incomingVoiceLines.Count > 0)
            {
                voiceSource.PlayOneShot(incomingVoiceLines[UnityEngine.Random.Range(0, incomingVoiceLines.Count)]);
            }
        }
    }
    private SoldierModel targetDamageableModel;
    private void BeginAttack()
    {
        if (targetDamageable == null && attackType == AttackType.Melee) //if we don't have a target and we're melee go back
        {
            SwitchState(lastState);
            return;
        }
        currentDamageTime = 0;
        if (attackType == AttackType.Melee)
        {
            int rand = UnityEngine.Random.Range(1, 100);
            targetDamageableModel = targetDamageable.GetComponentInParent<SoldierModel>();
            if (targetDamageableModel != null)
            { 
                if (rand <= hitThreshold || targetDamageableModel.attackType == AttackType.Ranged)
                { 
                    DealDamageToModel(targetDamageableModel, formPos.charging); //if target is ranged guaranteed hit because no way for them to defend against
                }
            }
            else
            {
                DealDamageToDamageable(targetDamageable); //always hit damageables?
            } 
            targetDamageableModel = null; 
        }
        else if (rangedModule != null)
        {
            rangedModule.TriggerRangedAttack();
        }
        SwitchState(ModelState.FinishingAttack);
    }
    private void PlayAttackChatter()
    {
        if (!formPos.playingAttackChatter) //play some attack voice line
        {
            formPos.playingAttackChatter = true;
            formPos.DisableAttackChatterForSeconds(10);
            if (attackVoiceLines.Count > 0)
            {
                voiceSource.PlayOneShot(attackVoiceLines[UnityEngine.Random.Range(0, attackVoiceLines.Count)]);
            }
        } 
    }
    private void CheckIfCanSwitchToAttacking()
    {
        if (attackType == AttackType.CavalryCharge)
        {
            //if attack box is not armed, then get ready to rearm it
            if (!attackBox.canDamage && !IfAttackIsReady())
            {
                currentAttackTime += deltaTime;
            }
            else
            {
                currentAttackTime = 0;
                attackBox.Rearm();
            }
        }
        else if (attackType == AttackType.Melee && formPos.charging)
        {
            if (!attackBox.canDamage && !IfAttackIsReady())
            {
                currentAttackTime += deltaTime;
            }
            else
            {
                currentAttackTime = 0;
                attackBox.Rearm();
            }
        }
        //if melee
        else if (attackType == AttackType.Melee && deployed)
        {  
            if (!IfAttackIsReady()) //if not ready to attack, start getting ready
            {
                currentAttackTime += deltaTime;
            }
            else if (HasTargetInRange()) //if ready and enemy is nearby
            { 
                currentAttackTime = 0; //reset attack time
                SwitchState(ModelState.Attacking);
            }
        }
        else if (attackType == AttackType.Ranged) //ranged
        {
            if (!IfAttackIsReady()) //if not ready to attack, start getting ready
            {
                currentAttackTime += deltaTime;
            }
            else if (HasTarget() && hasClearLineOfSight && CheckIfRemainingDistanceUnderThreshold(remainingDistanceThreshold)) //has target, line of sight, and is at formation position
            {
                currentAttackTime = 0; //reset attack time
                SwitchState(ModelState.Attacking);
            } 
        }
    }
    public bool HasTargetInRange()
    {
        if (targetDamageable != null && targetDamageable.alive && CheckIfInAttackRange())
        {
            return true;
        }
        else
        {
            return false;
        }
    } 
    private bool CheckIfInAttackRange()
    {
        return Helper.Instance.GetSquaredMagnitude(transform.position, targetDamageable.transform.position) <= Mathf.Pow(attackRange, 2);
    }
    private bool HasTarget()
    {
        if (targetDamageable != null || formPos.focusFire || formPos.enemyFormationToTarget != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void SwitchPathfinderMovement(bool val)
    { 
        if (pathfindingAI.enabled)
        { 
            pathfindingAI.canMove = val;
        } 
    }
    public void SwitchState(ModelState state)
    {
        lastState = currentModelState;
        currentModelState = state;
        UpdateAnimationState(state);
        switch (state)
        {
            case ModelState.Idle:
                SwitchPathfinderMovement(false);
                SetSpeedNull();
                break;
            case ModelState.Attacking:
                SwitchPathfinderMovement(false);
                SetSpeedNull();
                break;
            case ModelState.Moving: 
                SwitchPathfinderMovement(true);
                break;
            case ModelState.Damaged:
                SwitchPathfinderMovement(false);
                SetSpeedNull(); 
                break;
            case ModelState.Braced:
                SwitchPathfinderMovement(false);
                SetSpeedNull();
                break;
            case ModelState.Deploying: 
                break;
            case ModelState.KnockedDown:
                break;
            case ModelState.Airborne:
                break;
            case ModelState.Reloading:
                SwitchPathfinderMovement(false);
                SetSpeedNull();
                break;
            case ModelState.Charging:
                if (modelPosition != null && modelPosition.row.rowNum <= 4)
                {
                    DeployWithoutState();
                }
                SwitchPathfinderMovement(true);
                break;
            case ModelState.Routing:
                SwitchPathfinderMovement(true);
                break; 
            case ModelState.Dead:
                SwitchPathfinderMovement(false);
                SetSpeedNull();
                break;
            case ModelState.Undeploying:
                break;
            case ModelState.FinishingAttack:
                break;
            default:
                break;
        }
    } 
    private void DeployWithoutState()
    {
        animator.SetBool(AnimatorDefines.deployedID, true);
    }
    private void UpdateAnimationState(ModelState state) //set all animation states to false, except for chosen, which is set to true
    {
        animator.SetBool(AnimatorDefines.idleID, false);
        animator.SetBool(AnimatorDefines.attackingID, false);
        animator.SetBool(AnimatorDefines.movingID, false);
        animator.SetBool(AnimatorDefines.damagedID, false);
        //animator.SetBool(AnimatorDefines.deployedID, false); //should not be reset on state change
        animator.SetBool(AnimatorDefines.knockedDownID, false);
        animator.SetBool(AnimatorDefines.loadingID, false);
        switch (state)
        {
            case ModelState.Idle:
                animator.SetBool(AnimatorDefines.idleID, true); 
                if (animatedMesh != null)
                {

                    if (deployed)
                    {
                        animatedMesh.Play("downIdle");
                    }
                    else
                    {
                        animatedMesh.Play("upIdle");
                    }
                }
                break;
            case ModelState.Attacking:
                animator.SetBool(AnimatorDefines.attackingID, true);
                if (animatedMesh != null)
                {

                    animatedMesh.Play("attack");
                }
                break;
            case ModelState.Moving:
                animator.SetBool(AnimatorDefines.movingID, true);
                if (animatedMesh != null)
                {

                    if (deployed)
                    {
                        animatedMesh.Play("downWalk");
                    }
                    else
                    {
                        animatedMesh.Play("upWalk");

                    }
                }
                break;
            case ModelState.Damaged:
                animator.SetBool(AnimatorDefines.damagedID, true);
                if (animatedMesh != null)
                {

                    if (deployed)
                    {
                        animator.Play("WeaponDownDamaged");
                        animatedMesh.Play("downOuch");
                    }
                    else
                    {
                        animator.Play("WeaponUpDamaged");
                        animatedMesh.Play("upOuch");
                    }
                }
                break;
            case ModelState.Braced:
                animator.SetBool(AnimatorDefines.deployedID, true);
                break;
            case ModelState.Deploying:
                animator.SetBool(AnimatorDefines.deployedID, true);
                if (animatedMesh != null)
                {

                    animatedMesh.Play("deploy");
                }
                break;
            case ModelState.KnockedDown:
                animator.SetBool(AnimatorDefines.knockedDownID, true);
                break;
            case ModelState.Airborne:
                animator.SetBool(AnimatorDefines.knockedDownID, true);
                break;
            case ModelState.Reloading:
                animator.SetBool(AnimatorDefines.loadingID, true);
                break;
            case ModelState.Charging:
                animator.SetBool(AnimatorDefines.movingID, true);
                if (animatedMesh != null)
                {

                    if (deployed)
                    {

                        animatedMesh.Play("downRun");
                    }
                    else
                    {
                        animatedMesh.Play("upRun");
                    }
                }
                break;
            case ModelState.Routing:
                animator.SetBool(AnimatorDefines.movingID, true);
                if (animatedMesh != null)
                {

                    if (deployed)
                    {
                        animatedMesh.Play("downRun");
                    }
                    else
                    {
                        animatedMesh.Play("upRun");
                    }
                }
                break;
            case ModelState.Dead:
                animator.SetBool(AnimatorDefines.aliveID, false);
                break;
            case ModelState.Undeploying:
                animator.SetBool(AnimatorDefines.deployedID, false); 
                break;
            case ModelState.FinishingAttack:
                animator.SetBool(AnimatorDefines.attackingID, true);
                if (animatedMesh != null)
                {
                    animatedMesh.Play("attack");

                }
                break;
            default:
                break;
        }
    } 
    private bool IfAttackIsReady()
    {
        if (currentAttackTime >= requiredAttackTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool DamageIsReady()
    {
        if (currentDamageTime >= requiredDamageTime)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public List<DamageableEntity> nearbyDamageables;
    public void CheckIfDamageablesNearby()
    {
        nearbyDamageables.Clear(); //wipe the list  
        int maxColliders = 320; //lower numbers stop working
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, modelLayerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++) //go for hurtboxes
        {
            if (colliders[i].gameObject == self)
            {
                continue;
            }
            if (colliders[i].gameObject.tag == "Hurtbox") //if is hurtbox
            {
                DamageableEntity entity = colliders[i].GetComponentInParent<DamageableEntity>();
                if (entity == null)
                {
                    entity = colliders[i].GetComponent<DamageableEntity>();
                }
                if (entity != null)
                {
                    if (entity.alive && entity.team != team) //alive and enemy
                    {
                        nearbyDamageables.Add(entity);
                        //Debug.Log("adding nearby enemy to damageables");
                    }
                }
            }
        }
        targetDamageable = null;
        if (nearbyDamageables.Count > 0)
        {
            TargetClosestDamageable(); //set target enemy to be closest model
        }
    }
    public void TargetClosestDamageable()
    {
        float initDist = Mathf.Infinity;
        float compareDist = initDist;
        foreach (DamageableEntity item in nearbyDamageables)
        {
            if (item.alive)
            { 
                float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
                if (dist < compareDist)
                {
                    targetDamageable = item;
                    compareDist = dist;
                }
            }
        }
    }
    public void CheckIfEnemyModelsNearby()
    { 
        nearbyEnemyModels.Clear(); //wipe the list 
        //grab nearby models 
        int maxColliders = 320; //lower numbers stop working
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, modelLayerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++) //go for hurtboxes
        {
            if (colliders[i].gameObject == self)
            {
                continue;
            }
            if (colliders[i].gameObject.tag == "Hurtbox") //if is hurtbox
            {
                SoldierModel model = colliders[i].GetComponentInParent<SoldierModel>();
                if (model != null)
                {
                    if (model.alive && model.team != team) //alive and enemy
                    {
                        nearbyEnemyModels.Add(model);
                    }
                }
            }
        }
        if (nearbyEnemyModels.Count > 0)
        {
            TargetClosestEnemyModel(); //set target enemy to be closest model
        }
    }
    public void TargetClosestEnemyModel()
    {
        /*if (directionalAttacks)
        {
            //rule out any that aren't in our viewing angle

            foreach (SoldierModel model in nearbyEnemyModels)
            {
                if (Vector3.Angle(transform.forward, model.transform.position - transform.position) > angleOfAttack)
                {
                    //mark as out of bounds
                    model.ignoreAsNearby = true;
                }
                else
                {
                    model.ignoreAsNearby = false;
                }
            }
        }*/
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, nearbyEnemyModels[0].transform.position);
        float compareDist = initDist;

        foreach (SoldierModel item in nearbyEnemyModels)
        {
            if (!item.ignoreAsNearby && item.alive)
            {
                //float dist = GetDistance(transform, item.gameObject.transform);
                //float dist = GetSquaredMagnitude(transform.position, item.gameObject.transform.position);
                float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
                if (dist < compareDist)
                {
                    targetEnemy = item;
                    compareDist = dist;
                }
            }
        }
    }
    #region Deprecated
    /*targetEnemy = null;
        targetAttackable = null;
        float initDist = 9999;
    SoldierModel closest = null;
    AttackableObject closestObject = null;
    float compareDist = initDist;
        foreach (SoldierModel item in nearbyEnemyModels)
        {
            //float dist = GetDistance(transform, item.gameObject.transform);
            //float dist = GetSquaredMagnitude(transform.position, item.gameObject.transform.position);
            float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
            if (dist<compareDist)
            {
                closest = item;
                compareDist = dist;
            }
        }
        compareDist = initDist;
foreach (AttackableObject item in nearbyAttackableObjects)
{
    float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
    if (dist < compareDist)
    {
        closestObject = item;
        compareDist = dist;
    }
}
if (closest != null && closestObject != null) // get closest
{
    float dist1 = Helper.Instance.GetSquaredMagnitude(transform.position, closest.transform.position);
    float dist2 = Helper.Instance.GetSquaredMagnitude(transform.position, closestObject.transform.position);
    if (dist1 < dist2)
    {
        targetEnemy = closest;
    }
    else
    {
        targetAttackable = closestObject;
    }
}
else if (closest != null)
{

    targetEnemy = closest;
}
else if (closestObject != null)
{
    targetAttackable = closestObject;
}*/
#endregion

    private void SetSpeedNull()
    {
        float dampTime = .1f;
        float deltaTime = .1f;
        animator.SetFloat(AnimatorDefines.speedID, 0, dampTime, deltaTime);
    }
    private float slowTime = 3;
    private float fasterSlowTime = 2f;
    private float veryFastSlowTime = 1f;
    private float farDistanceThreshold = 5f;
    private void UpdateSpeed()
    {
        if (CheckIfRemainingDistanceOverThreshold(farDistanceThreshold)) //if far away, slow time isn't a factor
        {
            pathfindingAI.slowdownTime = veryFastSlowTime;
        }
        else if (currentModelState == ModelState.Charging || currentModelState == ModelState.Routing)
        {
            pathfindingAI.slowdownTime = fasterSlowTime;
        }
        else
        {
            pathfindingAI.slowdownTime = slowTime;
        } 

        float dampTime = .1f;
        float deltaTime = .1f; 
        if (currentModelState != ModelState.Routing)
        { 
            if (formPos != null && formPos.charging)
            {
                newMaxSpeed = startingMaxSpeed * 2 - speedSlow;
            }
            else
            {
                newMaxSpeed = startingMaxSpeed - speedSlow;
            }
        }
        else //if routing
        {
            newMaxSpeed = startingMaxSpeed * 2;
        }
        SetPathfindingSpeed(newMaxSpeed); 
        speedSlow -= 0.1f;
        speedSlow = Mathf.Clamp(speedSlow, 0, documentedMaxSpeed * 0.5f);

        movingSpeed = Mathf.Sqrt(Mathf.Pow(pathfindingAI.velocity.x, 2) + Mathf.Pow(pathfindingAI.velocity.z, 2)); //calculate speed vector 
        float threshold = .01f; 
        float min = .01f;
        if (movingSpeed < min)
        {
            movingSpeed = 0;
        }

        normalizedSpeed = movingSpeed; 

        normalizedSpeed /= documentedMaxSpeed; 

        if (normalizedSpeed > threshold)
        {
            animator.SetFloat(AnimatorDefines.speedID, normalizedSpeed * adjustWalkSpeedVisually, dampTime, deltaTime);
        }
        else
        {
            animator.SetFloat(AnimatorDefines.speedID, 0, dampTime, deltaTime);
        } 
    }
    private void SetPathfindingSpeed(float speed)
    {
        pathfindingAI.maxSpeed = speed; 
    }
    private bool visibleInFrustum = true;
    public void UpdateVisibility(bool val) //true means visible. false is hidden
    {
        visibleInFrustum = val;
        /*foreach (Renderer rend in renderers)
        {
            rend.enabled = formPos.showSoldierModels;
            //rend.material.color = formPos.farAwayIcon.color;
        }*/
        for (int i = 0; i < renderersArray.Length; i++)
        {
            renderersArray[i].enabled = val;
        }
        /*if (val)
        {
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        }
        else
        {
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }*/
        //animator.enabled = val;  
    }

    public void CheckForPendingDamage()
    {
        if (pendingDamage > 0 || pendingArmorPiercingDamage > 0)
        {
            SufferDamage(pendingDamage, pendingArmorPiercingDamage, null, 1);
            pendingDamage = 0;
            pendingArmorPiercingDamage = 0;

            if (pendingLaunched)
            { 
                //float maxDistance = 0.01f;
                Vector3 heading = (transform.position-pendingDamageSource.position).normalized;
                //Vector3 pos = transform.position + (-heading * maxDistance); //launch them in the opposite direction please
                LaunchModel(heading, 1, pendingDamageSource.position);
                pendingLaunched = false;
            }  
        } 
    }
    public Vector3 previousLocation;
    public float newMaxSpeed; 
    public bool CheckIfRemainingDistanceUnderThreshold(float threshold)
    {
        if (pathfindingAI.enabled && pathfindingAI.remainingDistance <= threshold)
        {
            return true;
        } 
        return false;
    }
    public bool CheckIfRemainingDistanceOverThreshold(float threshold)
    {
        if (pathfindingAI.enabled && pathfindingAI.remainingDistance > threshold)
        {
            return true;
        } 
        return false; 
    } 
    public void ToggleAttackBox(bool val)
    {
        if (attackBox != null)
        {
            attackBox.ToggleAttackBox(val);
        }
    } 
    public void UpdateMageTimer()
    { 
        if (!magicCharged)
        {
            currentMagicRecharge += 1;

            if (currentMagicRecharge >= magicRechargeTime)
            {
                magicCharged = true;
                FightManager obj = FindObjectOfType<FightManager>();
                obj.UpdateGUI();
                currentMagicRecharge = 0;
            }
        }
    } 
    [Range(1, 100)]
    [SerializeField] int hitThreshold = 50; //higher numbers are better. wider range of good hits
    
    public void DealDamageToModel(SoldierModel model, bool launchEnemy = false, bool knockDown = false, bool trampling = false)
    { 
        if (formPos != null)
        { 
            formPos.modelAttacked = true;
        }
        if (model != null && model.alive)
        {
            float force = 1; 
            if (attackType == AttackType.Melee)
            {
                PlayAttackSound();
                if (model.attackType == AttackType.Ranged) //melee breaks ranged cohesion
                {
                    if (model.formPos != null)
                    { 
                        model.formPos.BreakCohesion();
                    }
                    if (formPos != null && formPos.charging) //rout them if charging
                    {
                        int time = 10;
                        if (model.formPos != null)
                        { 
                            model.formPos.SoftRout(time);
                        }
                    }
                }  
                if (formPos != null && formPos.charging)
                {
                    force = normalizedSpeed * 2; 
                } 
                
                /*if (braced) //if we're braced, and they're charging
                {
                    if (enemy.formPos.charging || enemy.formPos.formationType == FormationPosition.FormationType.Cavalry)
                    {
                        float stunTime = 3;
                        enemy.formPos.FreezeMovement(stunTime);
                    }
                }*/
            }
            else if (attackType == AttackType.CavalryCharge)
            {
                force = normalizedSpeed;
                if (knockDown && !model.formPos.braced) //can't knock down braced units
                {
                    float getUpTime = knockDownForSecondsMax;
                    model.KnockDown();
                    model.getUpTimeCap = getUpTime * force;
                }
                if (trampling)
                {
                    float trampleDebuff = .25f;
                    force = normalizedSpeed * trampleDebuff;
                    float slowMax = startingMaxSpeed * 0.1f;
                    float minSlow = 0.01f;
                    SlowDown(slowMax - force, minSlow);
                }
            }
            if (attacksBreakEnemyCohesion && !model.braced)
            {
                if (model.formPos != null)
                {  
                    model.formPos.BreakCohesion();
                }
            }
            if (attacksSplinterEnemyCohesion && !model.braced)
            {
                if (model.formPos != null)
                { 
                    model.formPos.SplinterCohesion(inflictSplinterAmount);
                }
            }
            if (launchEnemy && model.formPos != null && !model.formPos.braced) //attacksCanLaunchEnemies && launchEnemy
            {
                //Debug.Log("attempting to launch enemy");
                force = Mathf.Clamp(force, 0, 1);
                float maxDistance = 5;
                //Vector3 pos = enemy.transform.position + (transform.forward * force * maxDistance);
                Vector3 heading = transform.forward;
                float slowMax = startingMaxSpeed * 0.9f;
                float minSlow = 0.1f;
                SlowDown(slowMax - force, minSlow);
                Vector3 startPos = new Vector3(model.transform.position.x, model.transform.position.y + 1, model.transform.position.z);
                model.LaunchModel(heading, force * maxDistance, model.transform.position);
            } 
            model.SufferDamage(damage * force, armorPiercingDamage * force, this, 1);
        }
    }

    public void DealDamageToDamageable(DamageableEntity entity)
    {
        if (attackType == AttackType.Melee)
        {
            PlayAttackSound();
        }
        formPos.modelAttacked = true;
        //check if soldiermodel or not 
        if (entity != null && entity.alive)
        {
            entity.InflictDamageOnThis(damage, armorPiercingDamage);
        } 
    }
    private void PlayAttackSound()
    { 
        if (attackSounds.Count > 0)
        {
            impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);
        }
    }
    public void SufferDamage(float dmg, float armorPiercingDmg, SoldierModel origin = null, float damageMultiplier = 1)
    {  
        float damageAfterArmor = dmg - armor;
        damageAfterArmor = Mathf.Clamp(damageAfterArmor, 0, 999);
        health -= damageAfterArmor * damageMultiplier;
        health -= armorPiercingDmg * damageMultiplier;
        //Debug.Log(armorPiercingDamage * damageMultiplier);


        currentDamageTime = 0;
        currentFinishedAttackingTime = 0;
        if (whenDamagedSwitchToDamagedState)
        {
            SwitchState(ModelState.Damaged); 
        }

        if (hurtVoiceLines.Count > 0)
        {
            hurtSource.PlayOneShot(hurtVoiceLines[UnityEngine.Random.Range(0, hurtVoiceLines.Count)]);
        }

        if (health <= 0)
        {
            KillThis();
            if (origin != null)
            {
                origin.targetEnemy = null;
                origin.formPos.numKills++;
            }
            if (isMagic)
            { 
                FightManager obj = FindObjectOfType<FightManager>();
                obj.UpdateGUI();
            }
        }
        if (formPos != null)
        {  
            formPos.modelTookDamage = true;
        }
        if (origin != null)
        {
            if (origin.attackType == AttackType.Melee && attackType == AttackType.Melee) //if attacked by melee and we're melee
            {
                formPos.GetTangledUp();
            }
        } 
    } 
    private void KnockDown()
    {
        SetKnockedDown(true);
    }
    private void SlowDown(float slowAmount, float minSlow)
    {
        slowAmount = Mathf.Clamp(slowAmount, minSlow, 100);
        speedSlow += slowAmount;
    }
    private void LaunchModel(Vector3 direction, float power, Vector3 startingPos)
    {
        airborne = true;
        //higher force means higher angle and greater deviation. ideally decided by movement speed at time of attack
        //calculations 
        float angle = power; // * 45
        angle = Mathf.Clamp(angle, 0, 45);
        //float deviation = 0;

        var a = angle * Mathf.Deg2Rad; 
        Vector3 dir = (direction * Mathf.Cos(a) + transform.up * Mathf.Sin(a)).normalized;  

        GameObject proj = Instantiate(modelProj, new Vector3(transform.position.x, transform.position.y+0.2f, transform.position.z), Quaternion.identity); //spawn the projectile
        proj.transform.rotation = transform.rotation;
        ProjectileFromSoldier missile = proj.GetComponent<ProjectileFromSoldier>();
        missile.formPosParent = formPos; //communicate some info to the missile
        missile.soldierParent = this;
        missile.startingPos = startingPos;
        //pathfindingAI.enabled = false; //disable pathing for now 
        //missile.LaunchProjectile(targetPos, angle, deviation); //fire at the position of the target with a clamped angle and deviation based on distance 
        missile.LaunchBullet(dir, power);
    }  
    private void ClearReferencesInPositionAndRow()
    { 
        //remove from pos 
        if (modelPosition != null)
        {
            if (modelPosition.row != null)
            { 
                modelPosition.row.RemoveModelFromRow(this);
                modelPosition.assignedSoldierModel = null; 
                modelPosition.SeekReplacement();
                modelPosition = null;
            }
        } 
    }
    public enum AIToUse
    {
        RichAI,
        AILerp
    }
    public void SwitchAI(AIToUse ai)
    {
        switch (ai)
        {
            case AIToUse.RichAI:
                pathfindingAI.enabled = true; 
                break;
            case AIToUse.AILerp:
                pathfindingAI.enabled = false; 
                break;
            default:
                break;
        }
    }
    public void KillThis()
    {
        SwitchState(ModelState.Dead);

        if (animatedMesh != null)
        {
            if (deployed)
            {
                animatedMesh.Play("upDeath", false);
            }
            else
            {
                animatedMesh.Play("upDeath", false);
            }

        } 
        PlaceOnGround();
        if (formPos != null)
        {
            formPos.LoseMorale();
            formPos.numberOfAliveSoldiers = Mathf.Clamp(formPos.numberOfAliveSoldiers-1, 0, 999);

            if (formPos.numberOfAliveSoldiers <= 0)
            {
                //Destroy(formPos.soldierBlock);
                    //SelfDestruct();
            }
        }
        if (attackBox != null)
        {
            attackBox.enabled = false; 
        } 
        selectionCircle.SetActive(false);
        //animator.enabled = true;
        //animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        SetAlive(false);

        pathfindingAI.canMove = false; 
        pathfindingAI.enableRotation = false; 

        ClearReferencesInPositionAndRow();
        //
        animator.Play("WeaponUpDie");
        PlayDeathChatter();
        pathfindingAI.enabled = false; 
        if (renderers.Count > 0)
        { 
            foreach (Renderer item in renderers)
            {
                //item.enabled = true;
                item.material.color = Color.gray;
            } 
        }
        if (lineOfSightIndicator != null)
        {
            lineOfSightIndicator.enabled = false;
        }
        Invoke("DelayedDisable", 2);
    } 
    private void PlayDeathChatter()
    {
        if (voiceSource.enabled)
        {
            if (deathSounds.Count > 0)
            {
                voiceSource.PlayOneShot(deathSounds[UnityEngine.Random.Range(0, deathSounds.Count)]);
            }
        }
    }
    private void DelayedDisable() //called by killthis
    { 
        voiceSource.enabled = false;
        impactSource.enabled = false;
        animator.cullingMode = AnimatorCullingMode.CullCompletely;
        enabled = false;
    } 
    public void TargetClosestEnemyInFormation(FormationPosition form)
    {
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, form.formationPositionBasedOnSoldierModels);
        float compareDist = initDist;

        SoldierModel[] array = form.soldierBlock.modelsArray;

        for (int i = 0; i < form.soldierBlock.modelsArray.Length; i++)
        {
            if (array[i] != null && array[i].alive)
            {
                float dist = Helper.Instance.GetSquaredMagnitude(transform.position, array[i].transform.position);
                if (dist < compareDist)
                {
                    targetEnemy = array[i];
                    compareDist = dist;
                }
            }
        }
    }
    private SoldierModel GetClosestModelInFormation(FormationPosition form) //targettype 0: any, targettype 1: enemy, targettype 2: ally
    {
        SoldierModel[] array = form.soldierBlock.modelsArray;
        SoldierModel closest = array[0];
         
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, closest.transform.position);
        float compareDist = initDist;

        for (int i = 0; i < array.Length; i++)
        {
            SoldierModel item = array[i]; 
            if (item == null)
            {
                continue;
            }
            float dist = Helper.Instance.GetSquaredMagnitude(transform.position, item.transform.position);
            if (dist < compareDist)
            {
                closest = item;
                compareDist = dist;
            }
        }
        return closest;
    }
    public bool PathfindingCanMove()
    {
        if (pathfindingAI.enabled)
        {
            return pathfindingAI.canMove;
        } 
        return true;
    }  
    void OnDrawGizmos()
    {
        /*Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green; */
        if (attackType == AttackType.Melee)
        { 
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        }
        if (targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1);
        }
    }
    public void StopAttackingWhenNoEnemiesNearby()
    {
        if (nearbyDamageables.Count <= 0) //if no nearby enemies formPos.listOfNearbyEnemies.Count <= 0 && 
        {
            SetAttacking(false);
        }
    }
    public bool farAway = false; 
    public async void UpdateAndCullAnimations() //not fully working
    {
        bool shouldAnimate = false;
        for (int i = 0; i < renderersArray.Length; i++)
        { 
            if (renderersArray[i].isVisible && renderersArray[i].tag != "DoNotAnimate")
            {
                shouldAnimate = true;
            }
        }
        animator.enabled = shouldAnimate;
        /*if (visibleInFrustum)
        { 
        }
        else
        {
            animator.enabled = false;
        }*/
        /*if (formPos.showSoldierModels)
        {
            animator.enabled = !farAway; //if faraway, disable animator
        }
        else
        {
            animator.enabled = false;
        }*/
        await Task.Yield();
    }  
    public void FaceEnemy()
    {
        if (targetEnemy != null) //HasTargetInRange()
        {
            pathfindingAI.enableRotation = false;
            Vector3 dir = targetEnemy.transform.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime, 0.0f);
            newDirection.y = 0; //keep level
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            pathfindingAI.enableRotation = true;
            //pathfindingAI.enableRotation = false;
        }
    } 
}
