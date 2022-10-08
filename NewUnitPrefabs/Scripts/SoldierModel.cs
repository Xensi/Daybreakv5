using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    [Header("Assign these! If null then something will break")]
    public SpriteRenderer lineOfSightIndicator;
    public RichAI richAI;
    public Animator animator;
    public Transform target;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public CharacterController characterController;
    [SerializeField] private GameObject modelProj;
    public GameObject selectionCircle; 
    private CapsuleCollider hurtbox; //deprecated
    public List<SkinnedMeshRenderer> renderers;
    [Header("Assign these if ranged")]
    [Range(0.0f, 45.0f)] [SerializeField] private float maxFiringAngle = 45;
    [Range(0.0f, 45)] [SerializeField] private float minFiringAngle = 10;
    [SerializeField] private bool isMagic = false;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectileFromSoldier projectile;
    [SerializeField] private bool directFire = false;
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private float power = 100;

    [SerializeField] private bool volleyFireAndRetreat = false; 
    [SerializeField] private Transform eyeline;
    [SerializeField] private float projectileDeviationAmount = 5;
    [SerializeField] private float projectileDeviationAmountVertical = 5;
    [Header("Assign this if needed (cavalry, braced inf)")]
    [SerializeField] private AttackBox attackBox;
    [Header("Change these per unit")]
    public Transform spine;
    public Transform head;

    [SerializeField] private float startingMaxSpeed = 0; //defined by richai starting maxspeed settings, typically walk speed
    [SerializeField] private float adjustWalkSpeedVisually = 1;
    [SerializeField] private bool playIdleAnims = false;
    [SerializeField] private float health = 10;
    [SerializeField] private float damage = 1;
    [SerializeField] private float armorPiercingDamage = 0;
    [SerializeField] private float armor = 0;

    [SerializeField] private float timeUntilDamage = .8f;
    [SerializeField] private float currentDamageTime = 0;
    [SerializeField] private float timeUntilFinishedAttacking = 1f;
    [SerializeField] private float currentFinishedAttackingTime = 0;
    [SerializeField] private float timeUntilRecovery = .1f;
    [SerializeField] private float currentRecoveryTime = 0;
    [SerializeField] private float reqAttackTime = 3;
    public float currentAttackTime = 3;

    [SerializeField] private float magicRechargeTime = 60f;
    [SerializeField] private float currentMagicRecharge = 0;
    public bool magicCharged = true;


    [SerializeField] private bool chargeDamage = false;
    [SerializeField] private bool stopWhenAttacking = true;
    [SerializeField] private bool stopWhenDamaged = true;
    [SerializeField] private bool stopWhenLoading = true;
    [SerializeField] private bool attacksCanLaunchEnemies = false;
    [SerializeField] private bool canOnlyAttackWhileMoving = false;
    [SerializeField] private bool attacksBreakEnemyCohesion = false;
    [SerializeField] private bool directionalAttacks = false;
    [SerializeField] private float angleOfAttack = 20;
    [SerializeField] private bool attacksDontFaceEnemy = false;
    [SerializeField] private bool impactAttacks = false;
    
    [SerializeField] private float defaultStoppingDistance = 0.01f; //could deprecate
    [SerializeField] private float combatStoppingDistance = 0.01f;

    [Header("Reloading vars")]
    [SerializeField] private bool rangedNeedsLoading = false;
    [SerializeField] private int ammo = 0; //start unloaded typically.
    [SerializeField] private int maxAmmo = 1;
    [SerializeField] private int internalAmmoCapacity = 100;
    public bool loadingRightNow = false;
    [SerializeField] private float currentFinishedLoadingTime = 0;
    [SerializeField] private float timeUntilFinishedLoading = 1f;
    [SerializeField] private bool justFired = false;

    [Header("Unit status")]
    public bool braced = false;
    [SerializeField] private bool allowedToDealDamage = true;
    public Position modelPosition;  
    public float movingSpeed = 0;
    public bool alive = true;
    public bool attacking = false;
    public bool moving = false;
    [SerializeField] private bool damaged = false;
    [SerializeField] private bool deployed = false;
    [SerializeField] private bool idle = false;
    public bool knockedDown = false;
    public bool airborne = false;
    [SerializeField] private SoldierModel targetEnemy;
    public bool pendingLaunched = false;


    [Header("Public")]

    public float walkSpeed = 3;
    public float runSpeed = 6;
    public string team = "Altgard";
    public float attackRange = 1;
    public float meleeAttackRange = 1;
    public float rangedAttackRange = 160;
    public FormationPosition formPos;
    public bool animate = false;
    public float currentSpeed = 0;
    [SerializeField] private float documentedMaxSpeed = 5;

    [Header("Exposed")]
    [SerializeField] private float sprintSpeed = 6;
    [SerializeField] private float settledRotationSpeed = 1;
    [SerializeField] private float threshold = .1f;
    [SerializeField] private float catchUpThreshold = .5f; //not used
    [SerializeField] private float defaultAccel = 5;
    [SerializeField] private float sprintAccel = 10;


    public GameObject self; 

    [SerializeField] private List<SoldierModel> nearbyEnemyModels;


    [SerializeField] private int currentIdleTimer = 0;

    [SerializeField] private int reqIdleTimer = 20;

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

    [SerializeField] private AudioSource voiceSource;

    public AudioSource impactSource;
    public AudioSource hurtSource;



    [SerializeField] private int numRandIdleAnims = 5;
    [SerializeField] private int numRandAttackAnims = 2;

    [SerializeField] private float waitForAttackChatterTime = 3;
    [SerializeField] private float waitForMarchChatterTime = 10;
    [SerializeField] private float waitForDeathReactionChatterTime = 3;
    [SerializeField] private float waitForIdleChatterTime = 3;


    [SerializeField] private bool exhausted = false;



    public List<SkinnedMeshRenderer> normalMeshes;
    public List<SkinnedMeshRenderer> veteranMeshes;
    public bool isVeteran = false;
    [SerializeField] private int numKills = 0;

    public bool melee = true;
    [Range(20.0f, 75.0f)] [SerializeField] private float LaunchAngle;




    public float pendingDamage = 0;
    public float pendingArmorPiercingDamage = 0; 
    [SerializeField] private float speedSlow = 0;

    public bool ignoreAsNearby = false;

    public float getUpTime = 0;
    [SerializeField] private float getUpTimeCap = 8;
    [SerializeField] private bool useOldWalkCalculations = true;

    public float normalizedSpeed = 0;

    public bool routing = false;

    public Transform pendingDamageSource;
    public void PlaceOnGround()
    {
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
        { 
            transform.position = hit.point; 
        }
        else if (Physics.Raycast(transform.position, Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
        //Debug.DrawRay(transform.position, Vector3.down*100, Color.yellow, 1);
    }
    private void Start()
    {
        lineOfSightIndicator.enabled = false;
        PlaceOnGround();
        startingMaxSpeed = richAI.maxSpeed;
        /*if (startingMaxSpeed <= 0)
        {
            startingMaxSpeed = 3;
        }*/
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool("walking", true);
        currentSpeed = walkSpeed;
        richAI.endReachedDistance = defaultStoppingDistance;

        if (melee)
        {
            SetDeployed(false);
            animator.SetBool("melee", true);
        }
        else
        {
            SetDeployed(true); //ranged is always deployed . . . ? for now.
            animator.SetBool("melee", false);
        }

        animator.SetFloat("angle", 0);
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (hurtbox == null)
        {
            hurtbox = GetComponentInChildren<CapsuleCollider>();
        }
        animator.SetInteger("ammo", ammo);

        if (rangedNeedsLoading && ammo <= 0 && internalAmmoCapacity > 0)
        {
            Reload();
        }
        if (!rangedNeedsLoading && ammo == 0) //give ammo if we dont use it at all
        {
            ModifyAmmo(1);
        }
    }
    public void SetBrace(bool val)
    {
        braced = val;
    }
    public void Rout()
    {
        routing = true;
        SetDeployed(false);
        SetAttacking(false);
        SetLoading(false);
        SetMoving(true); 
    }
    public void UpdateCharController()
    {
        characterController.enabled = modelPosition.activeController;
    }
    private void SetKnockedDown(bool val)
    {
        knockedDown = val;
        animator.SetBool("knockedDown", val);
    }
    private void SetDeployed(bool val)
    {
        deployed = val;
        animator.SetBool("deployed", val); //and animations will match 
    }
    private void SetAttacking(bool val)
    {
        attacking = val;
        animator.SetBool("attacking", val); //and animations will match 
        animator.SetInteger("randomAttack", UnityEngine.Random.Range(0, numRandAttackAnims));
        formPos.modelAttacking = val;
    }
    private void SetDamaged(bool val)
    {
        damaged = val;
        animator.SetBool("damaged", val); //and animations will match 
    }
    private void SetAlive(bool val)
    {
        alive = val;
        animator.SetBool("alive", val);
    }
    public void CheckIfAlive()
    {
        if (alive && health <= 0)
        {
            KillThis();
        }
    }

    public void UpdateVisibility()
    { 
        foreach (Renderer rend in renderers)
        {
            rend.enabled = formPos.showSoldierModels;
            //rend.material.color = formPos.farAwayIcon.color;
        }
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
                Vector3 heading = pendingDamageSource.position - transform.position;
                Vector3 pos = transform.position + (-heading * maxDistance); //launch them in the opposite direction please
                LaunchModel(pos, 1, pendingDamageSource.position);
                pendingLaunched = false;
            }  
        } 
    }
    public void UpdateSpeed()
    {
        float dampTime = .1f;
        float deltaTime = .1f;
        if (richAI.canMove)
        {
            if (formPos.listOfNearbyEnemies.Count == 0)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") || idle) //if we are idle or in idle anim state
                {
                    SetIdle(false);
                    animator.Play("WalkingBlend");
                }
            }
            if (!routing)
            {
                //richAI.maxSpeed = startingMaxSpeed * 0.5f - speedSlow;
                richAI.maxSpeed = startingMaxSpeed - speedSlow;
            }
            else //if routing
            {
                richAI.maxSpeed = startingMaxSpeed * 2;
            }
            speedSlow -= 0.1f;
            speedSlow = Mathf.Clamp(speedSlow, 0, documentedMaxSpeed * 0.5f);

            float threshold = .1f;
            movingSpeed = Mathf.Sqrt(Mathf.Pow(richAI.velocity.x, 2) + Mathf.Pow(richAI.velocity.z, 2)); //calculate speed vector 
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
                        normalizedSpeed /= richAI.maxSpeed * 2; //at max speed = 1;
                    }
                    else
                    {
                        normalizedSpeed /= richAI.maxSpeed; //actual speed divided by max speed normalizes it to 0-1
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
            else
            {
                normalizedSpeed /= documentedMaxSpeed;
                //Debug.Log(normalizedSpeed);
            }

            if (normalizedSpeed > threshold)
            {
                animator.SetFloat("speed", normalizedSpeed * adjustWalkSpeedVisually, dampTime, deltaTime);
            }
            else
            {
                animator.SetFloat("speed", 0, dampTime, deltaTime);
            }
        }
        else
        {
            animator.SetFloat("speed", 0, dampTime, deltaTime);
        }

    }

    public void UpdateDeploymentStatus()
    {
        if (routing)
        {
            return;
        }
        if (braced)
        {
            SetDeployed(true);
            return;
        }
        if (formPos.listOfNearbyEnemies.Count > 0) //check if enemies nearby
        {
            if (modelPosition != null)
            {
                if (modelPosition.row != null)
                {
                    if (modelPosition.row.rowNum <= 2) //check if we're in second row and 
                    {
                        if (!deployed)
                        {
                            if (melee)
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
            
        }
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
            SetMoving(false);
            return;
        }
        if (braced)
        {
            SetMoving(false);
            return;
        }
        if (loadingRightNow && stopWhenLoading && richAI.remainingDistance <= threshold) //if attacking, and needs to stop when attacking, and in formation position
        {
            SetMoving(false);
            return;
        }
        else if (attacking && stopWhenAttacking && richAI.remainingDistance <= threshold) //if attacking, and needs to stop when attacking, and in formation position
        {
            SetMoving(false);
            return;
        }
        else if (knockedDown || airborne)
        {
            SetMoving(false);
            return;
        }
        else //if not attacking, check
        {
            if (richAI.remainingDistance > threshold) // if there's still path to traverse 
            {
                SetMoving(true);

            }
            if (richAI.reachedDestination) //if we've reached destination
            {
                SetMoving(false);
            }
        }
    }
    public void UpdateLoadTimer()
    {
        if (routing)
        {
            return;
        }
        if (!rangedNeedsLoading)
        {
            CancelLoading();
            return;
        }
        if (richAI.remainingDistance > threshold)
        {
            CancelLoading();
            return;
        }
        if (airborne || knockedDown)
        {
            CancelLoading();
            return;
        }
        if (damaged)
        {
            CancelLoading();
            return;
        }
        if (formPos.movementManuallyStopped || !formPos.obeyingMovementOrder)
        {
            //if we're force stopped we can reload
        }
        else if (moving)
        {
            CancelLoading();
            return;
        }
        if (ammo <= 0 && !loadingRightNow && internalAmmoCapacity > 0)
        {
            Reload();
        }
        else if (loadingRightNow)
        {   //should we stop reloading? 
            //increment timer  
            currentFinishedLoadingTime += .1f;
            if (currentFinishedLoadingTime >= timeUntilFinishedLoading)
            {
                FinishReload();
            }
        }

    }
    private void Reload()
    {
        SetLoading(true); 
    }
    private void CancelLoading()
    {
        SetLoading(false); 
    }
    private void FinishReload()
    {
        SetLoading(false);
        currentFinishedLoadingTime = 0;
        ModifyAmmo(1);
        internalAmmoCapacity--;
        internalAmmoCapacity = Mathf.Clamp(internalAmmoCapacity, 0, 999);
    }
    private void SetMoving(bool val)
    {
        moving = val;
        richAI.canMove = val; //we can move
        animator.SetBool("moving", val); //and animations will match
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
        if (!melee && HasTarget())
        {
            if (LineOfSightObstructed(GetTarget()))
            {
                SetAttacking(false); 
                return;
            }
        }
        if (airborne || knockedDown)
        { 
            return;
        }
        if (!attacking && !damaged && !loadingRightNow && !isMagic) //increment if not attacking and not damaged not reloading not magic
        {

            if (currentAttackTime < reqAttackTime) //timer goes up
            {
                currentAttackTime += .1f; //increment timer
            }
            else //timer has reached
            {
                //Debug.Log("timer reached");
                if (impactAttacks && !attackBox.canDamage) //cavalry/braced inf
                {
                    AttackCodeChecks();
                }
                else
                {
                    if (melee)
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
    private bool CheckIfInAttackRange()
    {
        return (Vector3.Distance(transform.position, targetEnemy.transform.position) <= attackRange);
    }
    private void AttackCodeChecks() //called to see if we can make an attack
    {
        //Debug.Log("checks"); //reachable
        /*Vector3 heading;
        if (melee)
        {
            if (targetEnemy != null)
            {
                heading = targetEnemy.transform.position - transform.position;
            }
            else
            {
                return;
            }
        }
        else //ranged
        {
            if (formPos.enemyFormationToTarget != null)
            { 
                heading = formPos.enemyFormationToTarget.transform.position - transform.position;
            }
            else
            {
                return;
            }
        }  */
        //angle checks
        /*float angle = Vector3.Angle(heading, transform.forward);
        float threshold = 25;
        if (angle > threshold)
        {
            return;
        }*/

        if (routing)
        {
            //Debug.Log("rout");
            return;
        }
        if (canOnlyAttackWhileMoving && !moving)
        {
            //Debug.Log("still");
            return;
        }
        if (melee) //if melee, we can attack while moving
        {
            //Debug.Log("melee");
            SetDeployed(true);
            AttackCodeContinued();
        }
        else //ranged
        {
            //Debug.Log("ranged");
            if (!formPos.obeyingMovementOrder && !formPos.aiPath.canMove) //ranged; if not working check what conditions we are set to be able to move in
            {
                //Debug.Log("not moving");
                if (CanRangedHitWithAngle() || directFire)
                {
                    //Debug.Log("can fire");
                    AttackCodeContinued();
                }
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
        if (!melee && HasTarget())
        {
            if (LineOfSightObstructed(GetTarget()))
            {
                SetAttacking(false);
                return;
            }
        }
        SetAttacking(true); //attacking starts here
        SetMoving(false); //stop moving while attacking.
        currentDamageTime = 0; 
            
        allowedToDealDamage = true;

        if (impactAttacks)
        {
            if (attackBox != null)
            {
                attackBox.Rearm();
            }
        }
    }
    private bool CanRangedHitWithAngle()
    { 
        Vector3 targetPos = new Vector3(0, 0, 0);
        if (formPos.focusFire)
        {
            if (formPos.formationToFocusFire != null) //if we have a formation to focus on
            {
                targetPos = formPos.formationToFocusFire.transform.position;
            }
            else //otherwise use the terrain position.
            {
                targetPos = formPos.focusFirePos;
            }
        }
        else
        {
            //targetPos = targetEnemy.transform.position;
            targetPos = formPos.enemyFormationToTarget.transform.position;
        }

        //calculations
        float dist = Vector3.Distance(transform.position, targetPos);
        float angle = 0; 
        angle = dist * 0.5f;
       /* if (formPos.soldierBlock.arcingProjectiles)
        {
        }*/
        float clamped = Mathf.Clamp(angle, 0, 45);
        float angleTester = clamped / 5;
        //angle consideration; the lower the angle, the lower your row must be to fire. otherwise it cancels 
        if (angleTester >= modelPosition.row.rowNum)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void SetMelee(bool val)
    {
        melee = val;
        animator.SetBool("melee", val);

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
    private Vector3 GetTarget()
    { 
        Vector3 targetPos = new Vector3(999, 999, 999);
        FormationPosition formToFireAt = null;
        Vector3 spawn = projectileSpawn.transform.position;

        if (directFire)
        {
            //get unit reference height
           /* float referenceHeight = 0;
            if (formToFireAt != null)
            { 
                for (int i = 0; i < formToFireAt.soldierBlock.modelsArray.Length; i++)
                {
                    if (formToFireAt.soldierBlock.modelsArray[i] != null)
                    {
                        if (formToFireAt.soldierBlock.modelsArray[i].alive)
                        {
                            referenceHeight = formToFireAt.soldierBlock.modelsArray[i].transform.position.y;
                            break;
                        }
                    }
                }
            }
            else
            {
                referenceHeight = formPos.focusFirePos.y;
            }
            targetPos = new Vector3(targetPos.x, referenceHeight + projectileSpawn.transform.position.y, targetPos.z); // */
            //Debug.Log(targetPos);

            if (formPos.focusFire)
            { 
                if (formPos.formationToFocusFire != null)
                {
                    Vector3 vecFocus = formPos.formationToFocusFire.formationPositionBasedOnSoldierModels;
                    targetPos = new Vector3(vecFocus.x, vecFocus.y, vecFocus.z);
                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(formPos.focusFirePos.x, formPos.focusFirePos.y, formPos.focusFirePos.z);
                }
            }
            else
            {
                if (formPos.enemyFormationToTarget != null)
                {
                    Vector3 vec = formPos.enemyFormationToTarget.formationPositionBasedOnSoldierModels;
                    targetPos = new Vector3(vec.x, vec.y, vec.z);
                }
            }
        }
        else //arc
        {
            if (formPos.focusFire)
            {
                if (formPos.formationToFocusFire != null) //if we have a formation to focus on
                {
                    //targetPos = formPos.formationToFocusFire.transform.position;
                    targetPos = new Vector3(formPos.formationToFocusFire.transform.position.x, formPos.formationToFocusFire.averagePositionBasedOnSoldierModels, formPos.formationToFocusFire.transform.position.z);
                     
                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(formPos.focusFirePos.x, formPos.focusFirePos.y + 5, formPos.focusFirePos.z);
                }
            }
            else
            {
                if (formPos.enemyFormationToTarget != null)
                {
                    //targetPos = formPos.enemyFormationToTarget.transform.position;
                    targetPos = new Vector3(formPos.enemyFormationToTarget.transform.position.x, formPos.enemyFormationToTarget.averagePositionBasedOnSoldierModels, formPos.enemyFormationToTarget.transform.position.z);
                     
                }
            }
        }



        if (formPos.missileTarget != null)
        { 
            formPos.missileTarget.transform.position = targetPos;
        }
        return targetPos;
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
        if (!melee && HasTarget())
        {
            if (LineOfSightObstructed(GetTarget()))
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
        if (attacking && !loadingRightNow) //increment only if attacking and not reloading
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

                if (melee)
                { 
                    DealDamage(targetEnemy);
                }
                else //ranged
                {
                    if (ammo > 0)
                    {  
                        if (directFire)
                        {
                            LaunchBullet();
                        }
                        else
                        { 
                            FireProjectile();
                        }
                    }
                } 
            }
        }
    }
    private void SetLoading(bool val)
    {
        animator.SetBool("loading", val);
        loadingRightNow = val;
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
                if (launchEnemy) //attacksCanLaunchEnemies && launchEnemy
                {  
                    force = Mathf.Clamp(force, 0, 1);
                    float maxDistance = 5; 
                    Vector3 pos = enemy.transform.position + (transform.forward * force * maxDistance); 
                    SlowDown(1-force);  
                    enemy.LaunchModel(pos, force, transform.position); 
                }
                if (melee)
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
                if (knockDown)
                {
                    enemy.KnockDown();
                }
                if (trampling)
                {
                    float trampleDebuff = .25f;
                    force = normalizedSpeed * trampleDebuff;
                }

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
                origin.numKills++;
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
            if (origin.melee)
            {
                formPos.GetTangledUp();
            }
        }

    }
    private void KnockDown()
    {
        SetKnockedDown(true);
    }
    private void SlowDown(float slowAmount)
    {
        speedSlow += slowAmount;
    }
    private void LaunchModel(Vector3 targetPos, float force, Vector3 startingPos)
    {
        airborne = true;
        //higher force means higher angle and greater deviation. ideally decided by movement speed at time of attack
        //calculations 
        float angle = force * 45;
        angle = Mathf.Clamp(angle, 0, 45);
        float deviation = 0;
        GameObject proj = Instantiate(modelProj, new Vector3(transform.position.x, transform.position.y+0.2f, transform.position.z), Quaternion.identity); //spawn the projectile
        ProjectileFromSoldier missile = proj.GetComponent<ProjectileFromSoldier>();
        missile.formPosParent = formPos; //communicate some info to the missile
        missile.soldierParent = this;
        missile.startingPos = startingPos;
        richAI.enabled = false; //disable pathing for now 
        missile.LaunchProjectile(targetPos, angle, deviation); //fire at the position of the target with a clamped angle and deviation based on distance
    }
    public void MageCastProjectile(Vector3 targetPos, int abilityNum, string mageType) //let's fire projectiles at a target
    { 
        magicCharged = false;
        formPos.modelAttacked = true; 
        if (attackSounds.Count > 0)
        { 
            impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);
        }
        ProjectileFromSoldier missile = SpawnMissile(); 

        float dist = Vector3.Distance(transform.position, targetPos);
        float angle = 0;
        angle = dist * 0.5f; 
        float clamped = Mathf.Clamp(angle, 0, 45); 
        if (mageType == "Pyromancer")
        {
            if (abilityNum == 0)
            {
                clamped = 60;
            }
        }
        if (mageType == "Gallowglass")
        {
            if (abilityNum == 0)
            {
                clamped = 10;
            }
        }
        float deviation = projectileDeviationAmount * dist * 0.01f;

        float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
        float adjusted = clamped / 45; //for anim 
        animator.SetFloat("angle", adjusted);

        missile.LaunchProjectile(targetPos, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
    }

    private ProjectileFromSoldier SpawnMissile()
    {
        ProjectileFromSoldier missile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity); //spawn the projectile
        missile.formPosParent = formPos; //communicate some info to the missile
        missile.soldierParent = this;
        missile.damage = damage;
        missile.armorPiercingDamage = armorPiercingDamage;
        
        formPos.soldierBlock.listProjectiles.Add(missile);

        return missile;
    }
    private bool LineOfSightObstructed(Vector3 target)
    {
        LayerMask layerMask = LayerMask.GetMask("Model");

        RaycastHit hit;
        Vector3 heading = target - transform.position;
        //float nearRange = 20;
        float range = Vector3.Distance(transform.position, target);
        Vector3 sightLine = transform.position;

        if (eyeline != null)
        { 
            sightLine = eyeline.position;
        } 
        if (Physics.Raycast(sightLine, heading, out hit, range, layerMask))
        {
            Debug.DrawRay(sightLine, heading * Vector3.Distance(sightLine,hit.point), Color.white, Time.deltaTime, true);
            if (hit.collider.gameObject.tag == "Hurtbox")
            {
                SoldierModel model = hit.collider.gameObject.GetComponentInParent<SoldierModel>();
                if (model != null)
                {
                    if (model.team == team)
                    {
                        if (lineOfSightIndicator != null && formPos.selected)
                        {
                            lineOfSightIndicator.enabled = true;
                        }
                        else
                        {
                            lineOfSightIndicator.enabled = false;
                        }
                        return true;
                    } 
                } 
            }
            else if (hit.collider.gameObject.tag == "Terrain") //terrain blocks shots
            {
                if (lineOfSightIndicator != null && formPos.selected)
                {
                    lineOfSightIndicator.enabled = true;
                }
                else
                { 
                    lineOfSightIndicator.enabled = false;
                }
                return true;
            }
        }
        if (lineOfSightIndicator != null)
        {
            lineOfSightIndicator.enabled = false;
        } 
        return false;
    }
    private void LaunchBullet() //let's fire raycasts
    {
        if (ammo <= 0) //probably not necessary
        {
            if (internalAmmoCapacity > 0)
            {
                Reload();
            }
            return;
        } 
        if (targetEnemy != null || formPos.focusFire || formPos.enemyFormationToTarget != null)
        {
            Vector3 targetPos = GetTarget();

            //calculations
            float dist = Vector3.Distance(transform.position, targetPos);    

            if (attackSounds.Count > 0)
            {
                //Debug.Log("playing bulelt sound");
                impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);
            }  
            animator.SetFloat("angle", 0);

            ProjectileFromSoldier missile = SpawnMissile();

            //FIRE
            Vector3 heading = targetPos - transform.position;
            float deviation = UnityEngine.Random.Range(-projectileDeviationAmount, projectileDeviationAmount);
            float deviationUp = UnityEngine.Random.Range(-projectileDeviationAmountVertical, projectileDeviationAmountVertical);
            //Debug.Log(deviationUp);

            heading = Quaternion.AngleAxis(deviation, Vector3.up) * heading;
            heading = Quaternion.AngleAxis(deviationUp, Vector3.forward) * heading; 

            missile.FireBullet(heading, power);

            if (fireEffect != null)
            { 
                GameObject effect = Instantiate(fireEffect, projectileSpawn.position, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(heading);
            }

            if (rangedNeedsLoading) //if we need reloading and we're out
            {
                ModifyAmmo(-1);
                if (ammo <= 0 && internalAmmoCapacity > 0)
                {
                    Reload();
                }
            }
            formPos.modelAttacked = true;


            if (volleyFireAndRetreat && formPos.soldierBlock.frontRow.modelsInRow.Contains(this)) //if volley fire and we're in first row
            {
                waitingForAttackOver = true;
            }
        } 
    }
    private bool waitingForAttackOver = false;
    private int SortByDistance(Position p1, Position p2)
    {
        float distance = Vector3.Distance(p1.transform.position, transform.position);
        float distance2 = Vector3.Distance(p2.transform.position, transform.position);
        if (distance > distance2)
        {
            return 1;
        }
        if (distance == distance2)
        {
            return 0;
        }
        if (distance < distance2)
        {
            return -1;
        }
        return 0;
    }
    private void MoveToRetreatRank()
    {
        formPos.soldierBlock.retreatPositions.Sort(SortByDistance);
        foreach (Position item in formPos.soldierBlock.retreatPositions)
        {
            if (item.assignedSoldierModel == null)
            {
                modelPosition.assignedSoldierModel = null; //clear our existing pos
                item.assignedSoldierModel = this; //assign us to new pos
                aiDesSet.target = item.transform; //update our navigation
                break;
            }
        }
    }
    private void FireProjectile() //let's fire projectiles at a target
    {
        //Debug.Log("firing proj");
        if (targetEnemy != null || formPos.focusFire || formPos.enemyFormationToTarget != null)
        {
            Vector3 targetPos = GetTarget();

            //calculations
            float dist = Vector3.Distance(transform.position, targetPos);
            float angle = 10;
            angle = dist * 0.5f;  


            float clamped = Mathf.Clamp(angle, minFiringAngle, maxFiringAngle);
            float deviation = projectileDeviationAmount * dist * 0.01f;

            float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
            float adjusted = clamped / 45; //for anim   

            if (attackSounds.Count > 0)
            {
                //Debug.Log("playing proj sound");
                impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);
            }
            ProjectileFromSoldier missile = SpawnMissile();

            
            animator.SetFloat("angle", adjusted);
            //Debug.Log(targetPos + "angle" + clamped);
            missile.LaunchProjectile(formPos.missileTarget.transform.position, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, projectileSpawn.position, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(transform.forward);
            }
            if (rangedNeedsLoading) //if we need reloading and we're out
            {
                ModifyAmmo(-1);
                if (ammo <= 0 && internalAmmoCapacity > 0)
                {
                    Reload();
                }
            }
            formPos.modelAttacked = true;
        } 
    }



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
    private void ModifyAmmo(int num)
    {
        ammo += num;
        ammo = Mathf.Clamp(ammo, 0, maxAmmo);
        animator.SetInteger("ammo", ammo);
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
        if (attackBox != null)
        {
            attackBox.enabled = false; 
        }
        //richAI.gravity = Vector3.zero;
        characterController.enabled = false;
        selectionCircle.SetActive(false);
        animator.enabled = true;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        SetAlive(false);
        richAI.canMove = false;
        richAI.enableRotation = false;

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
        //simplify
        /*if (!formPos.playingDeathReactionChatter)
        {
            formPos.playingDeathReactionChatter = true;
            formPos.DisableDeathReactionForSeconds(10);

            LayerMask layerMask = LayerMask.GetMask("Model");
            int maxColliders = 5;
            Collider[] colliders = new Collider[maxColliders];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, layerMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < numColliders; i++)
            {
                if (colliders[i].gameObject == self) //if our own hitbox, ignore
                {
                    continue;
                }
                SoldierModel model = colliders[i].GetComponentInParent<SoldierModel>(); //find someone who will say something about their death
                if (model != null)
                {
                    if (model.team == team && model.alive && deathReactionSounds.Count > 0)
                    {
                        model.voiceSource.PlayOneShot(deathReactionSounds[UnityEngine.Random.Range(0, deathReactionSounds.Count)]);
                        break;
                    } 
                }
            } 
        } */
        if (formPos.numberOfAliveSoldiers <= 0)
        {
            formPos.soldierBlock.SelfDestruct();
        }
        richAI.enabled = false;
        aiDesSet.enabled = false;
        hurtbox.enabled = false;
        //Invoke("DelayedDisable", 2);
        if (renderers.Count > 0)
        { 
            foreach (SkinnedMeshRenderer item in renderers)
            {
                item.enabled = true;
                item.material.color = Color.gray;
            } 
        }
        DelayedDisable();
    }
    private void DelayedDisable()
    {
        voiceSource.enabled = false;
        impactSource.enabled = false; 
        enabled = false;
        animator.cullingMode = AnimatorCullingMode.CullCompletely;
    }
    public void CheckIfEnemyModelsNearby()
    {
        if (routing)
        {
            return;
        }
        nearbyEnemyModels.Clear(); //wipe the list 
        targetEnemy = null;
        //grab nearby models
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 160;
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
    private void TargetClosestEnemyModel()
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
        targetEnemy = nearbyEnemyModels[0]; 
        float initDist = GetDistance(transform, nearbyEnemyModels[0].transform);
        float compareDist = initDist;
        targetEnemy = nearbyEnemyModels[0];
        foreach (SoldierModel item in nearbyEnemyModels) 
        {
            if (!item.ignoreAsNearby)
            { 
                float dist = GetDistance(transform, item.gameObject.transform);
                if (dist < compareDist)
                {
                    targetEnemy = item;
                    compareDist = dist;
                }
            }
        }
    }  
    public void CheckIfIdle()
    {
        if (!richAI.canMove && formPos.listOfNearbyEnemies.Count == 0)
        { 
            currentIdleTimer += UnityEngine.Random.Range(0, 2);
            if (currentIdleTimer >= reqIdleTimer)
            {
                currentIdleTimer = 0;

                SetIdle(true);

                if (formPos.listOfNearbyEnemies.Count == 0)
                {
                    if (!formPos.playingIdleChatter)
                    {
                        formPos.playingIdleChatter = true;
                        formPos.DisableIdleChatterForSeconds(10);
                        if (idleVoiceLines.Count > 0)
                        { 
                            voiceSource.PlayOneShot(idleVoiceLines[UnityEngine.Random.Range(0, idleVoiceLines.Count)]);
                        }
                    } 
                }

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
            animator.SetBool("idle", val);
            animator.SetInteger("randomIdle", UnityEngine.Random.Range(0, numRandIdleAnims));
        }
    } 

    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    } 
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green; 
    }
    public void StopAttackingWhenNoEnemiesNearby()
    {
        if (formPos.listOfNearbyEnemies.Count <= 0 && nearbyEnemyModels.Count <= 0) //if no nearby enemies
        {
            SetAttacking(false);
        }
    }
    public void CullAnimations()
    {
        if (animate || richAI.canMove || attacking || !alive || knockedDown) //if we're in range or we can move
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
        }
    }

    private void PointTowards(Vector3 targetDirection)
    {
        richAI.enableRotation = false; 
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime, 0.0f);
        newDirection.y = 0; //keep level
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
    public void FixRotation()
    {
        float finishedPathRotSpeed = 1;
        richAI.enableRotation = true; //just to start
        if (!attacksDontFaceEnemy && !knockedDown && !airborne && !moving) //if our attacks face enemy, and we're not knocked down or in the air, and not moving
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
