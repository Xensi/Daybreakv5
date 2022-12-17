using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{  
    #region AssignedAtStart

    private NavmeshCut navMeshCutter;
    [HideInInspector] public RichAI pathfindingAI;
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
        CavalryCharge
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
    [SerializeField] private AttackBox attackBox;
    #endregion

    #region Generic
    [Header("Change these per unit")]

    [SerializeField] private float knockDownForSecondsMax = 6;
    [SerializeField] public float startingMaxSpeed = 3; //defined by richai starting maxspeed settings, typically walk speed
    [SerializeField] private float adjustWalkSpeedVisually = 1;
    [SerializeField] private bool playIdleAnims = false;
    [SerializeField] private float health = 10;
    public float damage = 1;
    public float armorPiercingDamage = 0;
    [SerializeField] private float armor = 0;

    [SerializeField] private float timeUntilDamage = .8f;
    private float currentDamageTime = 0;
    [SerializeField] private float timeUntilFinishedAttacking = 1f;
    private float currentFinishedAttackingTime = 0;
    [SerializeField] private float timeUntilRecovery = .1f;
    private float currentRecoveryTime = 0;
    [SerializeField] private float reqAttackTime = 3;
    public float currentAttackTime = 3; 

    [SerializeField] private bool chargeDamage = false;
    [SerializeField] private bool stopWhenAttacking = true;
    [SerializeField] private bool stopWhenDamaged = true;
    [SerializeField] private bool stopWhenLoading = true; 
    [SerializeField] private bool canOnlyAttackWhileMoving = false;
    [SerializeField] private bool attacksBreakEnemyCohesion = false;
    [SerializeField] private bool directionalAttacks = false;
    [SerializeField] private float angleOfAttack = 20;
    [SerializeField] private bool attacksFaceEnemies = true;
    [SerializeField] private bool impactAttacks = false;
    
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
    public enum AnimationState
    {
        Idle,
        Attacking,
        Moving,
        Damaged,
        Braced,
        Deployed,
        KnockedDown,
        Airborne,
        Loading
    }
    public AnimationState currentAnimationState = AnimationState.Idle;
    [HideInInspector] public bool braced = false;
    private bool allowedToDealDamage = true;
    [HideInInspector] public Position modelPosition;
    public float movingSpeed = 0;
    [HideInInspector] public bool alive = true;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool moving = false;
    public bool damaged = false;
    private bool deployed = false;
    private bool idle = false;
    [HideInInspector] public bool knockedDown = false;
    [HideInInspector] public bool airborne = false;
    public SoldierModel targetEnemy;
    [HideInInspector] public bool pendingLaunched = false;
    public bool clearLineOfSight = false; 
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;
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
    [SerializeField] private bool useOldWalkCalculations = false;
    [SerializeField] private int numRandIdleAnims = 1;
    [SerializeField] private int numRandAttackAnims = 1; 
    //public List<SkinnedMeshRenderer> normalMeshes;
    //public List<SkinnedMeshRenderer> veteranMeshes;
    #endregion

    #region ShouldNotSet
    [HideInInspector] public GameObject self;
    private int currentIdleTimer = 0;
    public float remainingDistanceThreshold = .01f;
    [HideInInspector] public float getUpTimeCap = 8;
    [HideInInspector] private float speedSlow = 0;
    [HideInInspector] public float pendingDamage = 0;
    [HideInInspector] public float pendingArmorPiercingDamage = 0;
    [HideInInspector] public float normalizedSpeed = 0;
    [HideInInspector] public bool routing = false; 
    [HideInInspector] public Transform pendingDamageSource; 
    [HideInInspector] public bool isVeteran = false;
    [HideInInspector] public bool ignoreAsNearby = false;
    [HideInInspector] public float getUpTime = 0; 
    #endregion

    #region Unused
    private bool exhausted = false;
    private float waitForAttackChatterTime = 10;
    private float waitForMarchChatterTime = 10;
    private float waitForDeathReactionChatterTime = 10;
    private float waitForIdleChatterTime = 10;
    private int numKills = 0;
    #endregion  


    public RangedModule rangedModule;

    public void PlaceOnGround()
    {
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
        //Debug.DrawRay(transform.position, Vector3.down*100, Color.yellow, 1);
    }

    public Transform target;
    private void Awake()
    {
        #region Initializations
        if (rangedModule == null)
        {
            rangedModule = GetComponent<RangedModule>();
        }
        if (rangedModule != null)
        { 
            rangedModule.model = this;
        }


        if (renderers.Count <= 0)
        { 
            SkinnedMeshRenderer[] array = GetComponentsInChildren<SkinnedMeshRenderer>();
            renderers.AddRange(array);
            MeshRenderer[] meshArray = GetComponentsInChildren<MeshRenderer>();
            renderers.AddRange(meshArray);

            renderersArray = renderers.ToArray();
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
        //Debug.Log(startingMaxSpeed);
        /*if (startingMaxSpeed <= 0)
        {
            startingMaxSpeed = 3;
        }*/
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool(AnimatorDefines.walkingID, true);
        currentSpeed = walkSpeed;
        //richAI.endReachedDistance = defaultStoppingDistance;

        if (attackType == AttackType.Melee)
        {
            SetDeployed(false);
            animator.SetBool(AnimatorDefines.meleeID, true);
        }
        else
        {
            SetDeployed(true); //ranged is always deployed . . . ? for now.
            animator.SetBool(AnimatorDefines.meleeID, false);
        }

        animator.SetFloat(AnimatorDefines.angleID, 0);  
        PlaceOnGround();

        //animator.cullingMode = AnimatorCullingMode.CullCompletely;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        pathfindingAI.enableRotation = true;
    }
    private void OnEnable()
    {
        if (pathfindingAI != null)
        {
            pathfindingAI.onSearchPath += UpdatePath; //subscribe to event
        }
    }
    private void OnDisable()
    {

        if (pathfindingAI != null)
        {
            pathfindingAI.onSearchPath -= UpdatePath;
        }
    }
    public void UpdatePath() //call whenever you want path to be updated, comrade
    {
        if (target != null && pathfindingAI != null)
        {
            pathfindingAI.destination = target.position;
        }
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
    public void Rout()
    {
        routing = true;
        SetDeployed(false);
        SetAttacking(false);
        if (rangedModule != null)
        { 
            rangedModule.SetLoading(false);
        }
        SetMoving(true); 
    } 
    public void StopRout()
    {
        routing = false; 
        SetMoving(false);
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
    private void SetDamaged(bool val)
    {
        damaged = val;
        animator.SetBool(AnimatorDefines.damagedID, val); //and animations will match 
    }
    private void SetAlive(bool val)
    {
        alive = val;
        animator.SetBool(AnimatorDefines.aliveID, val);
    }
    public void CheckIfAlive()
    {
        if (alive && health <= 0)
        {
            KillThis();
        }
    }
    private void UpdateAnimationState()
    {
        switch (currentAnimationState)
        {
            case AnimationState.Idle:  
                break;
            case AnimationState.Attacking: 
                break;
            case AnimationState.Moving: 
                break;
            case AnimationState.Damaged: 
                break;
            case AnimationState.Braced: 
                break;
            case AnimationState.Deployed: 
                break;
            case AnimationState.KnockedDown: 
                break;
            case AnimationState.Airborne: 
                break;
            case AnimationState.Loading: 
                break;
            default:
                break;
        }
    }

    public void UpdateVisibility(bool val)
    { 
        /*foreach (Renderer rend in renderers)
        {
            rend.enabled = formPos.showSoldierModels;
            //rend.material.color = formPos.farAwayIcon.color;
        }*/
        for (int i = 0; i < renderersArray.Length; i++)
        {
            renderersArray[i].enabled = val;
        }
        if (val)
        {
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
        else
        { 
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
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
                float maxDistance = 0.01f;
                Vector3 heading = (pendingDamageSource.position - transform.position).normalized;
                //Vector3 pos = transform.position + (-heading * maxDistance); //launch them in the opposite direction please
                LaunchModel(heading, 1, pendingDamageSource.position);
                pendingLaunched = false;
            }  
        } 
    }
    public Vector3 previousLocation;
    public float newMaxSpeed;
    public void UpdateSpeed()
    {
        float dampTime = .1f;
        float deltaTime = .1f;
        if (pathfindingAI.canMove)
        {
            //if (formPos.listOfNearbyEnemies.Count == 0)
            //{
                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") || idle) //if we are idle or in idle anim state
                {
                    SetIdle(false);
                    animator.Play("WalkingBlend");
                }
            //}
            if (!routing)
            {
                //richAI.maxSpeed = startingMaxSpeed * 0.5f - speedSlow;
                if (formPos.charging)
                {
                    newMaxSpeed = startingMaxSpeed*2 - speedSlow;
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
            pathfindingAI.maxSpeed = newMaxSpeed;
            speedSlow -= 0.1f;
            speedSlow = Mathf.Clamp(speedSlow, 0, documentedMaxSpeed * 0.5f);

            movingSpeed = Mathf.Sqrt(Mathf.Pow(pathfindingAI.velocity.x, 2) + Mathf.Pow(pathfindingAI.velocity.z, 2)); //calculate speed vector 
            float threshold = .01f;
            /*if (formPos.showSoldierModels)
            { 
                movingSpeed = Mathf.Sqrt(Mathf.Pow(richAI.velocity.x, 2) + Mathf.Pow(richAI.velocity.z, 2)); //calculate speed vector 
            }
            else
            {
                movingSpeed = Mathf.Sqrt(Mathf.Pow(transform.position.x - previousLocation.x, 2) + Mathf.Pow(transform.position.z - previousLocation.z,2))/.01f;
            } */
            float min = .01f;
            if (movingSpeed < min)
            {
                movingSpeed = 0;
            }

            normalizedSpeed = movingSpeed;

            if (useOldWalkCalculations)
            { 
                if (formPos.soldierBlock.useActualMaxSpeed)
                {
                    if (deployed)
                    {
                        normalizedSpeed /= newMaxSpeed * 2; //at max speed = 1;
                    }
                    else
                    {
                        normalizedSpeed /= newMaxSpeed; //actual speed divided by max speed normalizes it to 0-1
                    }
                }
                else //default
                {
                    if (deployed)
                    {
                        normalizedSpeed /= formPos.soldierBlock.forcedMaxSpeed * 2; //at max speed = 1;
                    }
                    else
                    {
                        normalizedSpeed /= formPos.soldierBlock.forcedMaxSpeed; //actual speed divided by max speed normalizes it to 0-1
                    }
                }
            }
            else //new calculations
            {
                normalizedSpeed /= documentedMaxSpeed;
                //Debug.Log(normalizedSpeed);
            }

            if (normalizedSpeed > threshold)
            {
                animator.SetFloat(AnimatorDefines.speedID, normalizedSpeed * adjustWalkSpeedVisually, dampTime, deltaTime);
            }
            else
            {
                animator.SetFloat(AnimatorDefines.speedID, 0, dampTime, deltaTime);
            }
        }
        else
        {
            animator.SetFloat(AnimatorDefines.speedID, 0, dampTime, deltaTime);
        }

    }

    public void UpdateDeploymentStatus()
    {
        /*if (formPos.charging && !MeleeWeaponObstructed(transform.forward))
        {
            SetDeployed(true);
            return;
        }*/
        if (routing)
        {
            return;
        }
        if (braced)
        {
            SetDeployed(true);
            return;
        }
        //if (formPos.listOfNearbyEnemies.Count > 0) //check if enemies nearby
        //{
            if (modelPosition != null)
            {
                if (modelPosition.row != null)
                {
                    if (modelPosition.row.rowNum <= 2) //check if we're in second row and 
                    {
                        if (!deployed)
                        {
                            if (attackType == AttackType.Melee)
                            {
                                SetDeployed(true);
                            }

                            if (!formPos.inCombat)
                            {
                                if (!formPos.playingAttackChatter)
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
                            }
                        }
                    }
                }
            }
            
        //}
        else //if none don't be deployed
        {
            SetDeployed(false);
        }
    }

    public void UpdateMovementStatus()
    {
        if (formPos.obeyingMovementOrder && moving)
        {
            if (!formPos.playingMarchChatter)
            {
                formPos.playingMarchChatter = true;
                formPos.DisableMarchChatterForSeconds(10);
                if (movingVoiceLines.Count > 0)
                {
                    voiceSource.PlayOneShot(movingVoiceLines[UnityEngine.Random.Range(0, movingVoiceLines.Count)]);
                }
            }
        }
        if (routing)
        {
            SetMoving(true);
            return;
        }
        if (damaged && stopWhenDamaged)
        {
            //Debug.Log("damaged");
            SetMoving(false);
            return;
        }
        if (braced)
        {
            //Debug.Log("braced");
            SetMoving(false);
            return;
        }
        if (rangedModule != null && rangedModule.rangedNeedsLoading)
        {
            if (rangedModule.loadingRightNow && stopWhenLoading && pathfindingAI.remainingDistance <= remainingDistanceThreshold) //if attacking, and needs to stop when attacking, and in formation position
            {
                //Debug.Log("ranged module");
                SetMoving(false);
                return;
            }
        }
        else if (attacking && stopWhenAttacking && pathfindingAI.remainingDistance <= remainingDistanceThreshold) //if attacking, and needs to stop when attacking, and in formation position
        {
            //Debug.Log("attacking and stop");
            SetMoving(false);
            return;
        }
        else if (knockedDown || airborne)
        {
            //Debug.Log("knocked down");
            SetMoving(false);
            return;
        }
        else //if not attacking, check
        {
            if (pathfindingAI.remainingDistance > remainingDistanceThreshold) // if there's still path to traverse 
            {
                SetMoving(true);

            }
            else
            {
                //Debug.Log("reached destination");
                SetMoving(false);
            }
        }
        
    }
    public void UpdateLoadTimer()
    {
        if (rangedModule != null)
        { 
            rangedModule.UpdateLoadTimer();
        }
    }
    public void ToggleAttackBox(bool val)
    {
        if (attackBox != null)
        {
            attackBox.ToggleAttackBox(val);
        }
    }
    private void SetMoving(bool val)
    {
        moving = val;
        pathfindingAI.canMove = val; //we can move
        //animator.SetBool("moving", val); //and animations will match 
        animator.SetBool(AnimatorDefines.movingID, val);
    }
    public void UpdateMageTimer()
    {
        if (routing)
        {
            return;
        }
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
    public void UpdateAttackTimer()
    {
        //Debug.Log("timer");
        if (routing)
        {
            return;
        } 
        if (airborne || knockedDown)
        { 
            return;
        }
        if (rangedModule != null && rangedModule.loadingRightNow)
        {
            return;
        }
        if (rangedModule != null && HasTarget())
        {
            if (!clearLineOfSight)
            {
                SetAttacking(false);
                return;
            }
        }
        if (!attacking && !damaged && !isMagic) //increment if not attacking and not damaged not reloading not magic
        { 
            if (currentAttackTime < reqAttackTime) //timer goes up
            {
                currentAttackTime += 1; //increment timer
            }
            else //timer has reached
            {
                //Debug.Log("timer reached");
                
                if (impactAttacks || formPos.charging) //cavalry/braced inf
                {
                    if (!attackBox.canDamage)
                    { 
                        AttackCodeChecks();
                    }
                }
                else
                {
                    if (attackType == AttackType.Melee)
                    {
                        if (targetEnemy != null && targetEnemy.alive && CheckIfInAttackRange() && !formPos.holdFire) //if we reach attack time, and we have a valid target
                        {  //start an attack
                            AttackCodeChecks();
                        }
                    }
                    else //ranged
                    { 
                        //Debug.Log("ranged reached");
                        if (!formPos.holdFire)
                        {
                            //Debug.Log("not hold fire");
                            if (formPos.focusFire)
                            {

                                //Debug.Log("Focus fire");
                                AttackCodeChecks();
                            }
                            else if (formPos.enemyFormationToTarget != null )
                            { 
                                //Debug.Log("formation to target present");
                                if (formPos.enemyFormationToTarget.alive)
                                { 
                                    //Debug.Log("formation enemy alive");
                                    AttackCodeChecks();
                                }
                            }
                        } 
                    }
                }
            }  
        }
    }
    private bool CheckIfInAttackRange() //slow
    {
        //return (Vector3.Distance(transform.position, targetEnemy.transform.position) <= attackRange);
        return Helper.Instance.GetSquaredMagnitude(transform.position, targetEnemy.transform.position) <= Mathf.Pow(attackRange, 2);
    }
    private void AttackCodeChecks() //called to see if we can make an attack
    { 
        if (routing)
        {
            //Debug.Log("rout");
            return;
        }
        if (!formPos.charging) //if charging we can attack while moving
        { 
            if (canOnlyAttackWhileMoving && !moving)
            {
                //Debug.Log("still");
                return;
            }
        }
        if (attackType == AttackType.Melee) //if melee, we can attack while moving
        {
            //Debug.Log("melee");
            SetDeployed(true);
            AttackCodeContinued();
        }
        else //ranged
        {
            //Debug.Log("ranged");
            float threshold = 0.1f;
            if (formPos.movingSpeed < threshold) //ranged; if not working check what conditions we are set to be able to move in  && !formPos.aiPath.canMove !formPos.obeyingMovementOrder
            {
                //Debug.Log("not moving");
                AttackCodeContinued();
            }
        }
    }
    private void AttackCodeContinued()
    {
        //Debug.Log("continued");
        if (routing)
        {
            return;
        }
        if (attackType == AttackType.Ranged && HasTarget())
        {
            if (!clearLineOfSight)
            {
                SetAttacking(false);
                return;
            }
        }
        SetAttacking(true); //attacking starts here
        SetMoving(false); //stop moving while attacking.
        currentDamageTime = 0; 
            
        allowedToDealDamage = true;

        if (impactAttacks || formPos.charging)
        {
            if (attackBox != null)
            {
                attackBox.Rearm();
            }
        }
    } 
    private void SetMelee(bool val)
    {
        if (val)
        { 
            attackType = AttackType.Melee;
        }
        else
        { 
            attackType = AttackType.Ranged;
        }
        melee = val;
        animator.SetBool(AnimatorDefines.meleeID, val); 
    }
    public void UpdateMeleeEngagement()
    {
        if (formPos.engagedInMelee)
        {
            SetMelee(true);
            attackRange = meleeAttackRange;
        }
        else
        {
            SetMelee(false);
            attackRange = rangedAttackRange;
        }
    }
    private bool HasTarget()
    {
        if (targetEnemy != null || formPos.focusFire || formPos.enemyFormationToTarget != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    } 
    public void UpdateDamageTimer() //ATTACK CODE
    {
        if (routing)
        {
            return;
        }
        if (attackType == AttackType.Ranged && HasTarget())
        {
            if (!clearLineOfSight)
            {
                SetAttacking(false);
                return;
            }
        }
        if (impactAttacks)
        {
            return;
        }
        if (airborne  || knockedDown)
        {
            return;
        }
        if (rangedModule != null && rangedModule.loadingRightNow)
        {
            return;
        }
        if (attacking) //increment only if attacking and not reloading
        {
            currentDamageTime += .1f;

            if (currentDamageTime >= timeUntilDamage && allowedToDealDamage)
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
                allowedToDealDamage = false; 
                currentAttackTime = 0;

                if (attackType == AttackType.Melee)
                {   
                    DealDamage(targetEnemy, formPos.charging);
                }
                else if (rangedModule != null)
                {
                    rangedModule.TriggerRangedAttack();
                } 
            }
        }
    }
    
    public void DealDamage(SoldierModel enemy, bool launchEnemy = false, bool knockDown = false, bool trampling = false)
    { //check if in range, otherwise whiff  
        formPos.modelAttacked = true;
        if (enemy != null)
        { 
            if (enemy.alive)
            {
                float force = 1;
                if (chargeDamage)
                {
                    force = normalizedSpeed; 
                }
                else if (formPos.charging)
                {
                    force = normalizedSpeed * 2;
                    //Debug.Log(damage * force);
                }
                if (launchEnemy && !enemy.formPos.braced) //attacksCanLaunchEnemies && launchEnemy
                {
                    //Debug.Log("attempting to launch enemy");
                    force = Mathf.Clamp(force, 0, 1);
                    float maxDistance = 5; 
                    //Vector3 pos = enemy.transform.position + (transform.forward * force * maxDistance);
                    Vector3 heading = transform.forward;
                    float slowMax = startingMaxSpeed * 0.9f;
                    float minSlow = 0.1f;
                    SlowDown(slowMax-force, minSlow);
                    Vector3 startPos = new Vector3(enemy.transform.position.x, enemy.transform.position.y + 1, enemy.transform.position.z); 
                    enemy.LaunchModel(heading, force* maxDistance, enemy.transform.position); 
                }
                if (attackType == AttackType.Melee)
                { 
                    if (attackSounds.Count > 0)
                    { 
                        impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);  
                    }
                }
                if (attacksBreakEnemyCohesion && !enemy.braced)
                {
                    enemy.formPos.BreakCohesion();
                }
                if (knockDown && !enemy.formPos.braced) //can't knock down braced units
                { 
                    float getUpTime = knockDownForSecondsMax;
                    enemy.KnockDown();
                    enemy.getUpTimeCap = getUpTime * force; 
                }
                if (trampling)
                {
                    float trampleDebuff = .25f;
                    force = normalizedSpeed * trampleDebuff;
                    float slowMax = startingMaxSpeed * 0.1f;
                    float minSlow = 0.1f;
                    SlowDown(slowMax - force, minSlow);
                }
                /*if (braced) //if we're braced, and they're charging
                { 
                    if (enemy.formPos.charging || enemy.formPos.isCavalry)
                    {
                        float stunTime = 3;
                        enemy.formPos.FreezeMovement(stunTime);
                    }
                }*/
                enemy.SufferDamage(damage * force, armorPiercingDamage * force, this, 1); 
            } 
        }
    }

    public void SufferDamage(float dmg, float armorPiercingDmg, SoldierModel origin, float damageMultiplier = 1)
    { 

        float damageAfterArmor = dmg - armor;
        damageAfterArmor = Mathf.Clamp(damageAfterArmor, 0, 999);
        health -= damageAfterArmor * damageMultiplier;
        health -= armorPiercingDmg * damageMultiplier;

        SetAttacking(false);

        if (stopWhenDamaged)
        { 
            SetMoving(false);
        }

        SetDamaged(true);
        //reset timers so that our animations dont desync 
        currentDamageTime = 0;
        currentFinishedAttackingTime = 0;

        if (hurtVoiceLines.Count > 0)
        {
            hurtSource.PlayOneShot(hurtVoiceLines[UnityEngine.Random.Range(0, hurtVoiceLines.Count)]);
        }

        if (deployed)
        {
            animator.Play("WeaponDownDamaged");
        }
        else
        {
            animator.Play("WeaponUpDamaged");
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

        formPos.modelTookDamage = true;
        if (origin != null)
        {
            if (origin.attackType == AttackType.Melee)
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
        pathfindingAI.enabled = false; //disable pathing for now 
        //missile.LaunchProjectile(targetPos, angle, deviation); //fire at the position of the target with a clamped angle and deviation based on distance 
        missile.LaunchBullet(dir, power);
    }
    public void ApproximateCharge()
    {
        if (moving)
        { 
            if (attackBox != null)
            {
                if (attackBox.canDamage)
                {
                    attackBox.ApproximateCharge();
                }
            }
        }
    }
    public bool waitingForAttackOver = false;   
    public void UpdateFinishedAttackingTimer()
    {
        if (routing)
        {
            return;
        }
        if (attacking) //increment only if attacking
        {
            currentFinishedAttackingTime += .1f;

            if (currentFinishedAttackingTime >= timeUntilFinishedAttacking)
            {
                currentFinishedAttackingTime = 0;
                SetAttacking(false);
                //SetMoving(true);
                if (waitingForAttackOver)
                {
                    waitingForAttackOver = false; 
                    formPos.CheckIfSwapRows(this);
                }
            }
        }
    }
    public void UpdateRecoveryTimer()
    {
        if (damaged) //increment only if attacking
        {
            currentRecoveryTime += .1f;

            if (currentRecoveryTime >= timeUntilRecovery)
            {
                currentRecoveryTime = 0; //reset damage time 
                SetDamaged(false);
                //SetMoving(true);
            }
        }
        if (knockedDown)
        {
            getUpTime += .1f;

            if (getUpTime >= getUpTimeCap)
            {
                getUpTime = 0;
                SetKnockedDown(false);
            }
        }
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
    public void KillThis()
    {
        PlaceOnGround();
        formPos.LoseMorale();
        if (attackBox != null)
        {
            attackBox.enabled = false; 
        }
        //richAI.gravity = Vector3.zero; 
        selectionCircle.SetActive(false);
        animator.enabled = true;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        SetAlive(false);
        pathfindingAI.canMove = false;
        pathfindingAI.enableRotation = false;

        ClearReferencesInPositionAndRow();
        //
        formPos.numberOfAliveSoldiers -= 1;
        animator.Play("WeaponUpDie");
        if (voiceSource.enabled)
        {
            if (deathSounds.Count > 0)
            {
                voiceSource.PlayOneShot(deathSounds[UnityEngine.Random.Range(0, deathSounds.Count)]);
            }
        }
        if (formPos.numberOfAliveSoldiers <= 0)
        {
            formPos.soldierBlock.SelfDestruct();
        }
        pathfindingAI.enabled = false; 
        //Invoke("DelayedDisable", 2);
        if (renderers.Count > 0)
        { 
            foreach (Renderer item in renderers)
            {
                item.enabled = true;
                item.material.color = Color.gray;
            } 
        }
        if (lineOfSightIndicator != null)
        {
            lineOfSightIndicator.enabled = false;
        }
        Invoke("DelayedDisable", 2);
    }
    private void DelayedDisable()
    {
        voiceSource.enabled = false;
        impactSource.enabled = false; 
        enabled = false;
        animator.cullingMode = AnimatorCullingMode.CullCompletely;
        //SelfDestruct();
    }
    private void SelfDestruct()
    {
        Destroy(transform.parent.gameObject);
    }

    public void CheckIfEnemyModelsNearby()
    {
        if (routing)
        {
            return;
        }
        nearbyEnemyModels.Clear(); //wipe the list 
        //grab nearby models
        LayerMask layerMask = LayerMask.GetMask("Model"); 
        int maxColliders = 320; //lower numbers stop working
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, layerMask, QueryTriggerInteraction.Ignore);
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
        if (directionalAttacks)
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
        }
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, nearbyEnemyModels[0].transform.position);
        float compareDist = initDist;

        foreach (SoldierModel item in nearbyEnemyModels)
        {
            if (!item.ignoreAsNearby)
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
    public void TargetClosestEnemyInFormation(FormationPosition form)
    {
        float initDist = Helper.Instance.GetSquaredMagnitude(transform.position, form.formationPositionBasedOnSoldierModels);
        float compareDist = initDist;

        var array = form.soldierBlock.modelsArray;

        for (int i = 0; i < form.soldierBlock.modelsArray.Length; i++)
        {
            if (array[i] != null)
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
    
    public void CheckIfIdle()
    {
        if (!pathfindingAI.canMove) // && formPos.listOfNearbyEnemies.Count == 0
        { 
            currentIdleTimer += UnityEngine.Random.Range(0, 2);
            if (currentIdleTimer >= reqIdleTimer)
            {
                currentIdleTimer = 0;

                SetIdle(true);

                //if (formPos.listOfNearbyEnemies.Count == 0)
                //{
                    if (!formPos.playingIdleChatter)
                    {
                        formPos.playingIdleChatter = true;
                        formPos.DisableIdleChatterForSeconds(10);
                        if (idleVoiceLines.Count > 0)
                        { 
                            voiceSource.PlayOneShot(idleVoiceLines[UnityEngine.Random.Range(0, idleVoiceLines.Count)]);
                        }
                    } 
                //}

            }
            else
            {
                SetIdle(false);
            }
        }
    } 
    private void SetIdle(bool val)
    {
        idle = val;

        if (playIdleAnims)
        { 
            animator.SetBool(AnimatorDefines.idleID, val);
            animator.SetInteger(AnimatorDefines.randomIdleID, UnityEngine.Random.Range(0, numRandIdleAnims-1));
        }
    }  
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green; 
    }
    public void StopAttackingWhenNoEnemiesNearby()
    {
        if (nearbyEnemyModels.Count <= 0) //if no nearby enemies formPos.listOfNearbyEnemies.Count <= 0 && 
        {
            SetAttacking(false);
        }
    }
    public bool farAway = false;
    public void UpdateAndCullAnimations()
    {
        if (formPos.showSoldierModels)
        {
            animator.enabled = !farAway; //if faraway, disable animator
        }
        else
        {
            animator.enabled = false;
        }
        /*if (animate || richAI.canMove || attacking || !alive || knockedDown) //if we're in range or we can move
        {  
            animator.enabled = true;
        }
        else if (!animate || !richAI.canMove)
        { //if we're out of range or we can't move 
            animator.enabled = false;
        }
        else
        { 
            animator.enabled = false;
        }*/
    }
     

    private void PointTowards(Vector3 targetDirection)
    {
        pathfindingAI.enableRotation = false;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime, 0.0f);
        newDirection.y = 0; //keep level
        transform.rotation = Quaternion.LookRotation(newDirection);
        //transform.rotation = Quaternion.LookRotation(targetDirection);
    }
    public void FixRotation()
    {
        if (formPos.showSoldierModels)
        {
            float finishedPathRotSpeed = 1;
            //richAI.enableRotation = true; //just to start

            /* if (nearbyEnemyModels.Count > 0)
             {
                 richAI.enableRotation = false;
             }
             else
             {
                 richAI.enableRotation = true;
             }
     */
            if (attacksFaceEnemies && !knockedDown && !airborne) //if our attacks face enemy, and we're not knocked down or in the air, and not moving
            { 
                if (formPos.focusFire && !formPos.holdFire && !formPos.obeyingMovementOrder) //if we're focus firing
                {
                    Vector3 targetDirection = new Vector3(0, 0, 0);
                    if (formPos.formationToFocusFire != null)
                    {
                        targetDirection = formPos.formationToFocusFire.transform.position - transform.position;
                    }
                    else
                    {
                        targetDirection = formPos.focusFirePos - transform.position;
                    }
                    PointTowards(targetDirection);
                }
                else if (targetEnemy != null)  //target enemy
                {
                    Vector3 targetDirection = targetEnemy.transform.position - transform.position;
                    PointTowards(targetDirection);
                }
                else if (targetEnemy == null && formPos.enemyFormationToTarget != null) //formation
                {
                    Vector3 targetDirection = formPos.enemyFormationToTarget.transform.position - transform.position;
                    PointTowards(targetDirection);
                }
            }
            /*if (formPos.listOfNearbyEnemies.Count > 0) //makes guys face forward
            {
                richAI.enableRotation = false;
                Vector3 targetDirection = formPos.soldierBlock.target.transform.position - transform.position; 
            }*/
            /*if (!richAI.canMove)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, formPos.gameObject.transform.rotation, finishedPathRotSpeed * Time.deltaTime);
            }*/
        } 
    }
}
