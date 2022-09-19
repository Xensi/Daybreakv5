using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    [Header("Assign these! If null then something will break")]
    public RichAI richAI;  
    public Animator animator;
    public Transform target;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public CharacterController characterController;
    [Header("Assign these if ranged")]
    [SerializeField] private bool isMagic = false;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectileFromSoldier projectile;

    [Header("Change these per unit")] 
    [SerializeField] private float health = 10;
    [SerializeField] float damage = 1;
    [SerializeField] private float armor = 0;

    [SerializeField] private float timeUntilDamage = .8f;
    [SerializeField] private float currentDamageTime = 0;
    [SerializeField] private float timeUntilFinishedAttacking = 1f;
    [SerializeField] private float currentFinishedAttackingTime = 0;
    [SerializeField] private float timeUntilRecovery = .1f;
    [SerializeField] private float currentRecoveryTime = 0;

    [SerializeField] private float magicRechargeTime = 60f;
    [SerializeField] private float currentMagicRecharge = 0;
    public bool magicCharged = true;

    [Header("Loading vars")]
    [SerializeField] private bool rangedNeedsLoading = false;
    [SerializeField] private int ammo = 0; //start unloaded typically.
    [SerializeField] private int maxAmmo = 1;
    [SerializeField] private bool loadingRightNow = false; 
    [SerializeField] private float currentFinishedLoadingTime = 0;
    [SerializeField] private float timeUntilFinishedLoading = 1f;


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
    public float movingSpeed = 0; 
    public bool alive = true;
    [Header("Exposed")]
    [SerializeField] private float sprintSpeed = 6;
    [SerializeField] private float settledRotationSpeed = 1;
    [SerializeField] private float threshold = .1f;
    [SerializeField] private SoldierModel targetEnemy;
    [SerializeField] private float catchUpThreshold = .5f; //not used
    [SerializeField] private float defaultAccel = 5;
    [SerializeField] private float sprintAccel = 10;


    public GameObject self; 
    [SerializeField] private float finishedPathRotSpeed = 1;

    [SerializeField] private List<SoldierModel> nearbyEnemyModels; 
    [SerializeField] private float reqAttackTime = 3;
    [SerializeField] private float currentAttackTime = 3;


    [SerializeField] private int currentIdleTimer = 0;

    [SerializeField] private int reqIdleTimer = 20;
    public Position position;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> deathSounds;
    [SerializeField] private List<AudioClip> deathReactionSounds;
    [SerializeField] private List<AudioClip> attackSounds;
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

    [SerializeField] private float defaultStoppingDistance = 0.5f;
    [SerializeField] private float combatStoppingDistance = 0.1f;

    public bool attacking = false;
    [SerializeField] private bool moving = false;
    [SerializeField] private bool damaged = false;
    [SerializeField] private bool deployed = false;
    [SerializeField] private bool idle = false;

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

    [SerializeField] private bool allowedToDealDamage = true;
    [SerializeField] private float projectileDeviationAmount = 5;


    public GameObject selectionCircle;

    public CapsuleCollider hurtbox;



    private void Start()
    { 
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool("walking", true);
        animator.SetInteger("row", position.row);
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

        if (rangedNeedsLoading && ammo <= 0)
        { 
            Reload();
        }
        if (!rangedNeedsLoading && ammo == 0) //give ammo if we dont use it at all
        {
            ModifyAmmo(1);
        }
    }
    public void UpdateCharController()
    { 
        characterController.enabled = position.activeController;
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

            float threshold = .1f;
            movingSpeed = Mathf.Sqrt(Mathf.Pow(richAI.velocity.x, 2) + Mathf.Pow(richAI.velocity.z, 2)); //calculate speed vector

            float adjustedSpeed = movingSpeed;

            if (formPos.soldierBlock.useActualMaxSpeed)
            {
                if (deployed)
                {
                    adjustedSpeed /= richAI.maxSpeed * 2; //at max speed = 1;
                }
                else
                {
                    adjustedSpeed /= richAI.maxSpeed; //actual speed divided by max speed normalizes it to 0-1
                }
            }
            else
            {
                if (deployed)
                {
                    adjustedSpeed /= formPos.soldierBlock.forcedMaxSpeed * 2; //at max speed = 1;
                }
                else
                {
                    adjustedSpeed /= formPos.soldierBlock.forcedMaxSpeed; //actual speed divided by max speed normalizes it to 0-1
                }
            }
            

            if (adjustedSpeed > threshold)
            {
                animator.SetFloat("speed", adjustedSpeed, dampTime, deltaTime); 
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
        if (formPos.listOfNearbyEnemies.Count > 0) //check if enemies nearby
        {
            if (position.row <= 2) //check if we're in second row and 
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
                                voiceSource.PlayOneShot(exhaustedVoiceLines[UnityEngine.Random.Range(0, exhaustedVoiceLines.Count)]);
                            }
                            else
                            {
                                voiceSource.PlayOneShot(incomingVoiceLines[UnityEngine.Random.Range(0, incomingVoiceLines.Count)]);
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
        if (formPos.obeyingMovementOrder)
        {
            if (!formPos.playingMarchChatter)
            {
                formPos.playingMarchChatter = true;
                formPos.DisableMarchChatterForSeconds(10);
                voiceSource.PlayOneShot(movingVoiceLines[UnityEngine.Random.Range(0, movingVoiceLines.Count)]);
            }
        }
        if (attacking)
        {
            SetMoving(false);
        }
        else if (damaged)
        {
            SetMoving(false); 
        }
        else if (loadingRightNow)
        {
            SetMoving(false);
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
    private void SetMoving(bool val)
    {
        moving = val;
        richAI.canMove = val; //we can move
        animator.SetBool("moving", val); //and animations will match
    }
    public void UpdateMageTimer()
    {
        if (!magicCharged)
        { 
            currentMagicRecharge += 1;

            if (currentMagicRecharge >= magicRechargeTime)
            {
                magicCharged = true;
                currentMagicRecharge = 0;
            }
        }
    }
    public void UpdateAttackTimer()
    {
        if (!attacking && !damaged && !loadingRightNow && !isMagic) //increment if not attacking and not damaged not reloading not magic
        {
            currentAttackTime += .1f; //increment timer

            if (melee)
            {
                if (currentAttackTime >= reqAttackTime && targetEnemy != null && targetEnemy.alive && !formPos.holdFire) //if we reach attack time, and we have a valid target
                {  //start an attack
                    AttackCodeChecks();
                }
                else if (currentAttackTime >= reqAttackTime && formPos.focusFire && !formPos.holdFire)
                {
                    AttackCodeChecks();
                }
            }
            else
            {
                if (currentAttackTime >= reqAttackTime && formPos.enemyFormationToTarget != null && formPos.enemyFormationToTarget.alive && !formPos.holdFire) //if we reach attack time, and we have a valid target
                {  //start an attack
                    AttackCodeChecks();
                }
                else if (currentAttackTime >= reqAttackTime && formPos.focusFire && !formPos.holdFire)
                {
                    AttackCodeChecks();
                }
            }

            
        }
    }
    private void AttackCodeChecks()
    {
        if (melee) //if melee, we can attack while moving
        {
            SetDeployed(true);
            AttackCodeContinued();
        }
        else if (!formPos.obeyingMovementOrder && !formPos.aiPath.canMove) //ranged
        { 
            if (CanRangedHitWithAngle())
            { 
                AttackCodeContinued();
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
        if (formPos.soldierBlock.arcingProjectiles)
        {
            angle = dist * 0.5f;
        }
        float clamped = Mathf.Clamp(angle, 0, 45);
        float angleTester = clamped / 5;
        //angle consideration; the lower the angle, the lower your row must be to fire. otherwise it cancels
        Debug.Log(angleTester);
        if (angleTester >= position.row)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void AttackCodeContinued()
    {  
        SetAttacking(true); //attacking starts here
        SetMoving(false); //stop moving while attacking.
        currentDamageTime = 0;

        allowedToDealDamage = true;

        if (!formPos.playingAttackChatter) //play some attack voice line
        {
            formPos.playingAttackChatter = true;
            formPos.DisableAttackChatterForSeconds(10);
            voiceSource.PlayOneShot(attackVoiceLines[UnityEngine.Random.Range(0, attackVoiceLines.Count)]);
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
    public void UpdateDamageTimer() //ATTACK CODE
    {
        if (attacking && !loadingRightNow) //increment only if attacking and not reloading
        {
            currentDamageTime += .1f;

            if (currentDamageTime >= timeUntilDamage && allowedToDealDamage)
            {
                allowedToDealDamage = false; 
                currentAttackTime = 0;

                if (melee)
                { 
                    DealDamage();
                }
                else //ranged
                {
                    FireProjectile(); 
                } 
            }
        }
    }
    private void Reload()
    {
        SetLoading(true);
    }
    private void SetLoading(bool val)
    {
        animator.SetBool("loading", val);
        loadingRightNow = val;
    }
    public void UpdateLoadTimer()
    {
        if (loadingRightNow) //increment if loading
        {
            if (damaged)
            {
                CancelLoading();
            }


            currentFinishedLoadingTime += .1f; //increment timer 
            if (currentFinishedLoadingTime >= timeUntilFinishedLoading)
            {
                FinishReload();
            }
        }
    }
    private void CancelLoading()
    {
        SetLoading(false);
        currentFinishedLoadingTime = 0;
    }
    private void FinishReload()
    {
        SetLoading(false);
        currentFinishedLoadingTime = 0;
        ModifyAmmo(1);
    }
    public void DealDamage()
    { //check if in range, otherwise whiff  
        formPos.modelAttacked = true;
        if (targetEnemy != null && targetEnemy.alive)
        {
            targetEnemy.impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]); //play impact sound at enemy position
            targetEnemy.SufferDamage(damage, this); 
        }
    }
    public void MageCastProjectile(Vector3 targetPos, int abilityNum, string mageType) //let's fire projectiles at a target
    {
        magicCharged = false;
        formPos.modelAttacked = true;   
        impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]); 
        ProjectileFromSoldier missile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity); //spawn the projectile
        missile.formPosParent = formPos; //communicate some info to the missile
        missile.soldierParent = this;
        missile.damage = damage;

        formPos.soldierBlock.listProjectiles.Add(missile);

        float dist = Vector3.Distance(transform.position, targetPos);
        float angle = 0;
        if (formPos.soldierBlock.arcingProjectiles)
        {
            angle = dist * 0.5f;
        }
        float clamped = Mathf.Clamp(angle, 0, 45); 
        if (mageType == "Pyromancer")
        {
            if (abilityNum == 0)
            {
                clamped = 70;
            }
        }
        float deviation = projectileDeviationAmount * dist * 0.01f;

        float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
        float adjusted = clamped / 45; //for anim 
        animator.SetFloat("angle", adjusted);

        missile.LaunchProjectile(targetPos, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
    }
    private void FireProjectile() //let's fire projectiles at a target
    {
        if (targetEnemy != null || formPos.focusFire || formPos.enemyFormationToTarget != null)
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
            if (formPos.soldierBlock.arcingProjectiles)
            {
                angle = dist * 0.5f;
            }

            float clamped = Mathf.Clamp(angle, 0, 45);
            float deviation = projectileDeviationAmount * dist * 0.01f;

            float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
            float adjusted = clamped / 45; //for anim   

            impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]); 
            ProjectileFromSoldier missile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity); //spawn the projectile
            missile.formPosParent = formPos; //communicate some info to the missile
            missile.soldierParent = this;
            missile.damage = damage;

            formPos.soldierBlock.listProjectiles.Add(missile);

            
            animator.SetFloat("angle", adjusted);
             
            missile.LaunchProjectile(targetPos, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
        }

        if (rangedNeedsLoading) //if we need reloading and we're out
        {
            ModifyAmmo(-1);
            if (ammo <= 0)
            {
                Reload();
            }
        }
        formPos.modelAttacked = true;
    }

    public void UpdateFinishedAttackingTimer()
    {
        if (attacking) //increment only if attacking
        {
            currentFinishedAttackingTime += .1f;

            if (currentFinishedAttackingTime >= timeUntilFinishedAttacking)
            {
                currentFinishedAttackingTime = 0;
                SetAttacking(false);
                SetMoving(true);
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
                SetMoving(true);
            }
        }
    }
    public void SufferDamage(float dmg, SoldierModel origin)
    {
        float val = dmg - armor;
        if (val < 0)
        {
            val = 0;
        }

        health -= val;

        SetAttacking(false);
        SetMoving(false);
        SetDamaged(true);
        //reset timers so that our animations dont desync 
        currentDamageTime = 0;
        currentFinishedAttackingTime = 0;

        hurtSource.PlayOneShot(hurtVoiceLines[UnityEngine.Random.Range(0, hurtVoiceLines.Count)]);
           
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
        }

        formPos.modelTookDamage = true;
        
        if (origin.melee)
        { 
            formPos.GetTangledUp();
        }
        
    } 
    public void KillThis()
    {
        richAI.gravity = Vector3.zero;
        characterController.enabled = false;
        selectionCircle.SetActive(false);
        animator.enabled = true;
        SetAlive(false);
        richAI.canMove = false;
        richAI.enableRotation = false; 
        position.assignedSoldierModel = null;
        formPos.numberOfAliveSoldiers -= 1;
        animator.Play("WeaponUpDie");
        if (voiceSource.enabled)
        { 
            voiceSource.PlayOneShot(deathSounds[UnityEngine.Random.Range(0, deathSounds.Count)]);
        }

        if (!formPos.playingDeathReactionChatter)
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
                if (colliders[i].gameObject.tag == team + "Model") //if our team
                {
                    SoldierModel model = colliders[i].GetComponent<SoldierModel>();
                    if (model != null)
                    {
                        if (model.alive)
                        {
                            model.voiceSource.PlayOneShot(deathReactionSounds[UnityEngine.Random.Range(0, deathReactionSounds.Count)]);
                            break;
                        }
                    }
                } 
            } 
        } 
        if (formPos.numberOfAliveSoldiers <= 0)
        {
            formPos.soldierBlock.SelfDestruct();
        }
        richAI.enabled = false;
        aiDesSet.enabled = false;
        hurtbox.enabled = false;
        Invoke("DelayedDisable", 2);
    }
    private void DelayedDisable()
    {
        voiceSource.enabled = false;
        impactSource.enabled = false; 
        enabled = false;
    }
    public void CheckIfEnemyModelsNearby()
    {
        nearbyEnemyModels.Clear(); //wipe the list 
        targetEnemy = null;
        //grab nearby models
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 160;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++) //go for hurtboxes
        {
            if (colliders[i].gameObject == self || colliders[i].gameObject.tag == team + "Model") //ignore character controller
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
        targetEnemy = nearbyEnemyModels[0]; 
        float initDist = GetDistance(transform, nearbyEnemyModels[0].transform);
        float compareDist = initDist;
        targetEnemy = nearbyEnemyModels[0];
        foreach (SoldierModel item in nearbyEnemyModels) //doesn't work yet
        {
            float dist = GetDistance(transform, item.gameObject.transform); 
            if (dist < compareDist)
            {
                targetEnemy = item;
                compareDist = dist;
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
                        voiceSource.PlayOneShot(idleVoiceLines[UnityEngine.Random.Range(0, idleVoiceLines.Count)]);
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
        animator.SetBool("idle", val);
        animator.SetInteger("randomIdle", UnityEngine.Random.Range(0, numRandIdleAnims));
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "DamagingObject")
        {
            SufferDamage(10, null);
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
        if (animate || richAI.canMove || attacking || !alive) //if we're in range or we can move
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


    public void FixRotation()
    {
        richAI.enableRotation = true;
        if (formPos.focusFire && !formPos.holdFire && !formPos.obeyingMovementOrder)
        {
            richAI.enableRotation = false;

            Vector3 targetDirection = new Vector3(0, 0, 0);
            if (formPos.formationToFocusFire != null)
            {
                targetDirection = formPos.formationToFocusFire.transform.position - transform.position;
            }
            else
            {
                targetDirection = formPos.focusFirePos - transform.position;
            }


            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else if (targetEnemy != null) // && !formPos.holdFire && !formPos.obeyingMovementOrder
        {
            richAI.enableRotation = false;
            Vector3 targetDirection = targetEnemy.transform.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else if (targetEnemy == null && formPos.enemyFormationToTarget != null)
        {
            richAI.enableRotation = false;
            Vector3 targetDirection = formPos.enemyFormationToTarget.transform.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        /*if (formPos.listOfNearbyEnemies.Count > 0) //makes guys face forward
        {
            richAI.enableRotation = false;
            Vector3 targetDirection = formPos.soldierBlock.target.transform.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            transform.rotation = Quaternion.LookRotation(newDirection);
        }*/
        /*if (!richAI.canMove)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, formPos.gameObject.transform.rotation, finishedPathRotSpeed * Time.deltaTime);
        }*/
    }
}
