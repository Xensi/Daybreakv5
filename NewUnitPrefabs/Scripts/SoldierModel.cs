using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    public RichAI aiPath;
    //[SerializeField] private AILerp aiPath;

    public Animator animator;
    [SerializeField] private float threshold = .1f;
    public float walkSpeed = 3;
    public float runSpeed = 6;

    

    [SerializeField] private float sprintSpeed = 6;
    public Transform target;
    [SerializeField] private float settledRotationSpeed = 1;
    public string team = "Altgard";
    [SerializeField] private SoldierModel targetEnemy;

    public float attackRange = 1;

    [SerializeField] private Collider[] colliderList;
    [SerializeField] private Rigidbody[] rigidBodyList;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public FormationPosition formPos;
    public bool animate = false;
    [SerializeField] private float catchUpThreshold = .5f;
    //[SerializeField] private bool walking = false;
    [SerializeField] private float defaultAccel = 5;
    [SerializeField] private float sprintAccel = 10;

    public float currentSpeed = 0;
    private float currentAccel = 0;

    public float movingSpeed = 0;

    public bool alive = true;
    private bool oldAlive = true;

    public GameObject self;
    public Collider selfCollider;
    [SerializeField] private float finishedPathRotSpeed = 1;

    [SerializeField] private List<SoldierModel> nearbyEnemyModels;

    //public SkinnedMeshRenderer[] re;
    [SerializeField] private float reqAttackTime = 3;
    [SerializeField] private float currentAttackTime = 3;

    [SerializeField] private float health = 10;
    [SerializeField] private float damage = 1;
    [SerializeField] private float armor = 0;

    [SerializeField] private int currentIdleTimer = 0;

    [SerializeField] private int reqIdleTimer = 20;
    public Position position;

    [SerializeField] private List<AudioClip> deathSounds;
    [SerializeField] private List<AudioClip> deathReactionSounds;
    [SerializeField] private List<AudioClip> attackSounds;
    [SerializeField] private List<AudioClip> attackVoiceLines;
    [SerializeField] private List<AudioClip> idleVoiceLines;
    [SerializeField] private List<AudioClip> hurtVoiceLines;
    [SerializeField] private List<AudioClip> incomingVoiceLines;
    [SerializeField] private List<AudioClip> exhaustedVoiceLines;

    [SerializeField] private AudioSource voiceSource;

    public AudioSource impactSource;
    public AudioSource hurtSource;

    [SerializeField] private float defaultStoppingDistance = 0.5f;
    [SerializeField] private float combatStoppingDistance = 0.1f;

    [SerializeField] private float timeUntilDamage = .8f;
    [SerializeField] private float currentDamageTime = 0;
    [SerializeField] private float timeUntilFinishedAttacking = 1f;
    [SerializeField] private float currentFinishedAttackingTime = 0;
    [SerializeField] private float timeUntilRecovery = .1f;
    [SerializeField] private float currentRecoveryTime = 0;
    public bool attacking = false;
    [SerializeField] private bool moving = false;
    [SerializeField] private bool damaged = false;
    [SerializeField] private bool deployed = false;
    [SerializeField] private bool idle = false;

    [SerializeField] private int numRandIdleAnims = 5;
    [SerializeField] private int numRandAttackAnims = 2;

    [SerializeField] private float waitForAttackChatterTime = 3;
    [SerializeField] private float waitForDeathReactionChatterTime = 3;
    [SerializeField] private float waitForIdleChatterTime = 3;


    [SerializeField] private bool exhausted = false;



    public List<SkinnedMeshRenderer> nornalMeshes;
    public List<SkinnedMeshRenderer> veteranMeshes;
    public bool isVeteran = false;
    [SerializeField] private int numKills = 0;

    [SerializeField] private float minimumAttackRange = 1;

    [SerializeField] private bool melee = true;
    [SerializeField] private ProjectileFromSoldier projectile;
    [Range(20.0f, 75.0f)] [SerializeField] private float LaunchAngle;

    [SerializeField] private bool allowedToDealDamage = true;
    [SerializeField] private float projectileDeviationAmount = 5;

    [SerializeField] private Transform projectileSpawn;
    private void Start()
    { 
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool("walking", true);
        animator.SetInteger("row", position.row);
        currentSpeed = walkSpeed;
        currentAccel = defaultAccel;
        aiPath.endReachedDistance = defaultStoppingDistance;

        if (melee)
        {
            SetDeployed(false);
        }
        else
        {
            SetDeployed(true); //ranged is always deployed . . . ? for now.
        }
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
    public void UpdateSpeed()
    {
        float dampTime = .1f;
        float deltaTime = .1f;
        if (aiPath.canMove)
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
            movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //calculate speed vector

            float adjustedSpeed = movingSpeed;
            if (deployed)
            {
                adjustedSpeed /= aiPath.maxSpeed * 2; //at max speed = 1;
            }
            else
            {
                adjustedSpeed /= aiPath.maxSpeed;
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
    
    public void DeployWeaponsInAdvance()
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
                            formPos.DisableAttackChatterForSeconds(waitForAttackChatterTime);
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
            if (melee)
            {
                SetDeployed(false);
            }
        }
    }

    public void UpdateMovementStatus()
    { 
        if (attacking || damaged)
        {
            SetMoving(false);
        }
        else //if not attacking, check
        {
            if (aiPath.remainingDistance > threshold) // if there's still path to traverse 
            {
                SetMoving(true);
            }
            if (aiPath.reachedDestination) //if we've reached destination
            {
                SetMoving(false);
            } 
        }
    }
    private void SetMoving(bool val)
    {
        moving = val;
        aiPath.canMove = val; //we can move
        animator.SetBool("moving", val); //and animations will match
    }
    public void UpdateAttackTimer()
    {
        if (!attacking && !damaged) //increment if not attacking and not damaged
        {
            currentAttackTime += .1f; //increment timer
            //&& GetDistance(transform, targetEnemy.transform) >= minimumAttackRange
            if (currentAttackTime >= reqAttackTime && targetEnemy != null && targetEnemy.alive ) //if we reach attack time, and we have a valid target
            {  //start an attack
                if (melee)
                {
                    SetDeployed(true);
                }
                SetAttacking(true);
                currentDamageTime = 0;
                SetMoving(false);

                allowedToDealDamage = true;

                if (!formPos.playingAttackChatter)
                {
                    formPos.playingAttackChatter = true;
                    formPos.DisableAttackChatterForSeconds(waitForAttackChatterTime);
                    voiceSource.PlayOneShot(attackVoiceLines[UnityEngine.Random.Range(0, attackVoiceLines.Count)]);
                }
            }
        }
    }
    public void UpdateDamageTimer() //ATTACK CODE
    {
        if (attacking) //increment only if attacking
        {
            currentDamageTime += .1f;

            if (currentDamageTime >= timeUntilDamage && allowedToDealDamage)
            {
                allowedToDealDamage = false;
                //currentDamageTime = 0; //reset damage time
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

    public void DealDamage()
    { //check if in range, otherwise whiff  
        formPos.modelAttacked = true;
        if (targetEnemy != null && targetEnemy.alive)
        {
            targetEnemy.impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]); //play impact sound at enemy position
            targetEnemy.SufferDamage(damage, this);
            /*if (GetDistance(transform, targetEnemy.transform) <= attackRange) //if still within range of attack
            { 
            }*/
        }
    }
    private void FireProjectile()
    {
        formPos.modelAttacked = true;
        if (targetEnemy != null)
        {
            impactSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]); //play impact sound at enemy position
            ProjectileFromSoldier missile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity); //spawn the projectile
            missile.formPosParent = formPos;
            formPos.soldierBlock.listProjectiles.Add(missile);
            missile.LaunchProjectile(targetEnemy.transform, LaunchAngle, projectileDeviationAmount);
        }
        
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
    private void SufferDamage(float dmg, SoldierModel origin)
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
    } 
    private void KillThis()
    {
        SetAlive(false);
        aiPath.canMove = false;
        aiPath.enableRotation = false;

        selfCollider.enabled = false;
        position.assignedSoldierModel = null;
        formPos.numberOfAliveSoldiers -= 1;
        voiceSource.PlayOneShot(deathSounds[UnityEngine.Random.Range(0, deathSounds.Count)]);


        if (!formPos.playingDeathReactionChatter)
        {
            formPos.playingDeathReactionChatter = true;
            formPos.DisableDeathReactionForSeconds(waitForDeathReactionChatterTime);

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
         
    }
    public void CheckIfEnemyModelsNearby()
    {
        nearbyEnemyModels.Clear(); //wipe the list
        targetEnemy = null;
        //grab nearby models
        LayerMask layerMask = LayerMask.GetMask("Model");
        int maxColliders = 80;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++)
        {
            if (colliders[i].gameObject == self) //if our own hitbox, ignore
            {
                continue;
            }
            if (colliders[i].gameObject.tag != team + "Model") //if enemy team
            { 
                SoldierModel model = colliders[i].GetComponent<SoldierModel>();
                if (model != null)
                {
                    if (model.alive)
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
        if (!aiPath.canMove && formPos.listOfNearbyEnemies.Count == 0)
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
                        formPos.DisableIdleChatterForSeconds(waitForIdleChatterTime);
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
        Gizmos.DrawWireSphere(transform.position, minimumAttackRange);
    }
    public void CommonSense()
    { 
        if (formPos.soldierBlock.melee)
        { 
            if (formPos.listOfNearbyEnemies.Count <= 0 && nearbyEnemyModels.Count <= 0) //if no nearby enemies
            {
                SetAttacking(false);
            }
        }
    }
    public void CullAnimations()
    {
        if (animate || aiPath.canMove || attacking || !alive) //if we're in range or we can move
        { //fix this so that when death animation is over disables all components 
            animator.enabled = true;
        }
        else if (!animate || !aiPath.canMove)
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
        aiPath.enableRotation = true;
        if (targetEnemy != null)
        {
            aiPath.enableRotation = false;
            Vector3 targetDirection = targetEnemy.transform.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
             

            transform.rotation = Quaternion.LookRotation(newDirection);
        }  
        else if (formPos.listOfNearbyEnemies.Count > 0)
        { 
            aiPath.enableRotation = false;
            Vector3 targetDirection = formPos.soldierBlock.target.transform.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f); 

            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else if (!aiPath.canMove)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, formPos.gameObject.transform.rotation, finishedPathRotSpeed * Time.deltaTime);
        }
    }
}
