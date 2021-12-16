using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class UpdateAgentDestination : MonoBehaviour
{
    public bool dead = false;
    public GameObject thisNavPoint;
    public int positionInUnitList = 0;
    public NavMeshAgent navAgent;
    private float defaultAgentSpeed = .5f;
    public Animator animator;
    public Piece parentPiece;
    public Piece targetPiece;
    public bool attacking = false;
    public int id = 0;
    public int targetID = 0;
    public bool ableToAttack = false;
    public int maxNumberOfAttacks = 6;
    public int numberOfAttacks = 0;
    public bool idleSet = false;
    public bool moveSet = false;
    public float animationSpeed = 1;
    public int queuedDamage; //this is set by parent piece
    public bool freeze = false;

    //public float distanceToNotAttack = 1f;

    [SerializeField] private float navOffset = .9f;
    public float navOffsetAdd = 0;
    public float navOffsetAddClamp = 1;
    //[SerializeField] private float notAttackOffset = 1.8f;
    public float attackerRadius = .06f;
    public float movingRadius = .03f;

    public Transform effectSpawnTransform;
    public GameObject attackEffect;
    public AudioClip[] attackSoundEffect;
    public AudioClip[] deathSoundEffect;
    private AudioSource _audioSource;

    public Collider[] AllColliders;
    public Rigidbody[] AllRigidbodies;

    public Collider nonRagdollCollider;

    public GameObject projectilePrefab;
    public float spread = 0;
    public float angle = 10;
    public bool animationsEnabled = false;

    public Vector3 navmeshDestination;

    public Vector3 debugVelocity = Vector3.zero;

    public GameObject targetedSoldierDebug;

    public bool setInPlace = false;
    public bool rangedAndNeedsToTurnToFaceEnemy = false;
    public Vector3 rotationGoal = Vector3.zero;
    public Piece enemy;

    //public bool marked = false;
    // Update is called once per frame  
    private void Awake()
    {

        AllColliders = GetComponentsInChildren<Collider>(true);
        AllRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (var collider in AllColliders)
        {
            collider.enabled = false;
        }
        foreach (var rigid in AllRigidbodies)
        {
            rigid.isKinematic = true;
            rigid.useGravity = false;
        }
        if (nonRagdollCollider != null)
        {

            nonRagdollCollider.enabled = true;
        }

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        defaultAgentSpeed = navAgent.speed;
        //DoRagdoll();

    }


    void Update()
    {
        debugVelocity = navAgent.velocity;
        if (animationsEnabled || !animator.GetCurrentAnimatorStateInfo(0).IsName("BaseIdle")) //if doing something other than idle
        {

            animator.speed = animationSpeed;
        }
        else
        {
            animator.speed = 0;
        }
        if (freeze) //if frozen, do not update
        {
            return;
        }
        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(thisNavPoint.transform.position, out closestHit, 500, 1))
        {
            thisNavPoint.transform.position = closestHit.position;

        }
        if (attacking && parentPiece.attackType == "melee" && navAgent.enabled)
        { 
            if (!moveSet) //play looping animation once
            {
                animator.Play("BaseMove");
                moveSet = true;
            }
            navAgent.stoppingDistance = navOffset + navOffsetAdd;
            navAgent.radius = attackerRadius;
            if (targetedSoldierDebug != null && navAgent.isActiveAndEnabled && !parentPiece.targetToAttackPiece.disengaging) //if we have a move target, then update our nav target to its position
            {
                navAgent.destination = targetedSoldierDebug.transform.position;

            }


            if (animator != null) //if animator exists
            {
                if (!navAgent.pathPending) //if path ready
                {
                    if (navAgent.velocity.magnitude < .1f && navOffsetAdd >= navOffsetAddClamp) //if frontline starts causing a ruckus
                    {
                        if (!setInPlace && navAgent.isActiveAndEnabled)
                        {
                            navAgent.destination = transform.position;
                            ableToAttack = true;
                            if (!idleSet)
                            {
                                animator.Play("BaseIdle");
                                idleSet = true;
                            }
                            setInPlace = true;
                        }
                    }
                    if (navAgent.remainingDistance <= navAgent.stoppingDistance) //if met stopping distance
                    {
                        if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f) //if has no path or velocity = 0
                        {
                            // now able to attack, having met destination
                            ableToAttack = true;
                            if (!idleSet)
                            {
                                if (animationsEnabled)
                                {

                                }
                                animator.Play("BaseIdle");
                                idleSet = true;
                            }
                        }
                    }
                }
            }
        }
        else if (attacking && parentPiece.attackType == "ranged" && navAgent.enabled && rangedAndNeedsToTurnToFaceEnemy)
        {

            if (!moveSet)
            {
                animator.SetBool("moving", true);
                animator.Play("BaseMove");
                moveSet = true;
            }
            navAgent.stoppingDistance = 0f;
            if (thisNavPoint != null && navAgent != null && navAgent.enabled)
            {

                navAgent.destination = thisNavPoint.transform.position;
            }
            if (navAgent.remainingDistance <= navAgent.stoppingDistance) //if met stopping distance
            {
                if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f) //if has no path or velocity = 0
                {
                    if (!idleSet)
                    {
                        animator.Play("BaseIdle");
                        animator.SetBool("moving", false);
                        idleSet = true;
                    }
                    /*var rotationSpeed = 1f; 
                    Vector3 lookPos = rotationGoal - transform.position;
                    lookPos.y = 0;
                    Quaternion rotation = Quaternion.LookRotation(lookPos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed);*/
                    StartCoroutine(rotateToFaceEnemy());
                }
            }
        }
        else if (!attacking) //if not attacking (ergo unit is moving)
        {
            navAgent.radius = movingRadius;
            navAgent.stoppingDistance = 0.1f;
            navOffsetAdd = 0;
            if (thisNavPoint != null && navAgent != null && navAgent.enabled)
            {

                navAgent.destination = thisNavPoint.transform.position;
            }
            MoveAnimate();
        }


    }
    private IEnumerator rotateToFaceEnemy()
    {

        Tween rotateTween = transform.DORotate(rotationGoal, 1);
        yield return new WaitForSeconds(rotateTween.Duration());
        ableToAttack = true; // now able to attack, having met destination and rotated to face enemy
    }

    private void MoveAnimate()
    {
        if (animator != null && navAgent.enabled)
        {
            if (!navAgent.pathPending) //check if agent is finished moving
            {
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        // Done
                        animator.Play("BaseIdle");
                        animator.SetBool("moving", false);
                        return;
                    }
                }
            }
        }
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("BaseMove"))
        {
            animator.SetBool("moving", true);
            animator.Play("BaseMove");
        }
    }

    public void Unfreeze()
    {
        freeze = false;
        animationSpeed = 1;
        navAgent.speed = defaultAgentSpeed;
    }

    public void KillThis(GameObject parent)
    {
        dead = true;
        int random = Random.Range(0, deathSoundEffect.Length);
        _audioSource.PlayOneShot(deathSoundEffect[random]); //bypass the limit on sound play
        //Debug.Log(random);
        DoRagdoll();
        //Destroy(parent, 2);
    }

    private void DoRagdoll()
    {
        GetComponent<Animator>().enabled = false;
        foreach (var rigid in AllRigidbodies)
        {
            rigid.isKinematic = false;
            float random = Random.Range(-.5f, .5f);
            float random2 = Random.Range(-.5f, .5f);
            float random3 = Random.Range(-.5f, .5f);
            rigid.velocity = new Vector3(random, 0, random3);
            //rigid.angularVelocity = Vector3.zero;
            rigid.useGravity = true;
        }
        foreach (var collider in AllColliders)
        {
            collider.enabled = true;
        }
        if (nonRagdollCollider != null)
        {

            nonRagdollCollider.enabled = false;
        }
        navAgent.enabled = false;

    }
    public IEnumerator AttackInterval()
    {
        parentPiece.animationsOver = false;
        if (parentPiece.attackType == "melee")
        {
            GameObject tMin = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position; //this soldier's position

            for (int i = 0; i < targetPiece.soldierObjects.Count; i++) //todo this process will probably be laggy with more soldiers
            {
                float dist = Vector3.Distance(targetPiece.soldierObjects[i].transform.position, currentPos);
                if (dist < minDist)
                {
                    tMin = targetPiece.soldierObjects[i];
                    minDist = dist;
                    ////Debug.LogError("min dist " + minDist);
                }
            }
            if (tMin != null && navAgent.isActiveAndEnabled && !parentPiece.targetToAttackPiece.disengaging)
            {
                navAgent.destination = tMin.transform.position;
                targetedSoldierDebug = tMin;



            }
            //basically, find the closest soldier and make that our move target
        }


        Debug.Log("Attempting to attack!" + parentPiece);

        if (parentPiece.attackType == "ranged")
        {

            yield return new WaitForSeconds(Random.Range(.1f, 2f)); //add attack delay if we are a ranged unit
        }

        if (ableToAttack && parentPiece.attackType == "ranged" && numberOfAttacks < maxNumberOfAttacks)
        {
            animator.Play("BaseAttack");
            numberOfAttacks++;

            if (!parentPiece.markedDeaths)
            {
                parentPiece.markedDeaths = true;

                targetPiece.modelBar.SetHealth(targetPiece.models); //tween hp bar
                //targetPiece.MarkForDeath(queuedDamage);
                Debug.Log("models" + targetPiece.models);

            }
        }
        else if (ableToAttack && numberOfAttacks < maxNumberOfAttacks && parentPiece.attackType == "melee")
        {

            animator.Play("BaseAttack");
            numberOfAttacks++;

            if (!parentPiece.markedDeaths) //only update health bar once
            {
                parentPiece.markedDeaths = true;
                //Debug.LogError("Attacking and updating enemy hp to" + targetPiece.models);
                targetPiece.modelBar.SetHealth(targetPiece.models); //update enemy's hp bar to actual value when attacking
                //targetPiece.MarkForDeath(queuedDamage);
                //Debug.Log("models" + targetPiece.models);
            }


            //maybe we can increase the stopping distance with each attack
            foreach (var soldier in parentPiece.soldierObjects)
            {
                soldier.GetComponent<UpdateAgentDestination>().navOffsetAdd += .05f;
                soldier.GetComponent<UpdateAgentDestination>().navOffsetAdd = Mathf.Clamp(soldier.GetComponent<UpdateAgentDestination>().navOffsetAdd, 0, navOffsetAddClamp);
            }
        }
        if (parentPiece.unitType == "infantry")
        { 
            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
        else
        { 
            yield return new WaitForSeconds(Random.Range(.1f, 1f));
        }
        if (numberOfAttacks < maxNumberOfAttacks) //if not met max attacks
        {
            StartCoroutine(AttackInterval()); //do another attempt
        }
        else //if finished
        {
            //attacking = false;
            ableToAttack = false;
            numberOfAttacks = 0;
            idleSet = false;
            setInPlace = false;
        }
    }
    public IEnumerator Freeze()
    {
        yield return new WaitForSeconds(6);
        freeze = true;
        DOTween.To(() => numberOfAttacks, x => numberOfAttacks = x, maxNumberOfAttacks, 3);
        //animationSpeed = 0;
        DOTween.To(() => animationSpeed, x => animationSpeed = x, 0, 3);
        DOTween.To(() => navAgent.speed, x => navAgent.speed = x, 0, 3);

        //do this just in case
        if (!parentPiece.markedDeaths)
        {
            parentPiece.markedDeaths = true;

            targetPiece.modelBar.SetHealth(targetPiece.models); //tween hp bar
            //targetPiece.MarkForDeath(queuedDamage);
            Debug.Log("models" + targetPiece.models);

        }
        //yield return new WaitForSeconds(3);
        parentPiece.soldierAttacked = true; //set this after freeze just in case
        parentPiece.animationsOver = true;
        //Debug.LogError("ANIMATIONS OVER." + parentPiece.animationsOver);
        //parentPiece.board.AllowExecution();
    }

    public void SpawnEffect() //called using animation events. generally signifies when attack hits
    { //requires an animation event calling this function on the Soldier prefab of a unit
        /*var updater = targetedSoldierDebug.GetComponent<UpdateAgentDestination>();
        if (ableToAttack && parentPiece.attackType == "melee" && parentPiece.inflictedDeaths < queuedDamage && !updater.dead) //if attacking in melee and we have more deaths to go and target is alive
        {
            //instead of using mark for death, just kill the soldier you attack (unless we've already killed enough)
            updater.KillThis(targetedSoldierDebug); //this will set the soldier to be dead
            parentPiece.inflictedDeaths++; //we killed one.

            targetPiece.soldierObjects.RemoveAt(updater.positionInUnitList); //remove the soldier from list

        }
        else if (ableToAttack && parentPiece.attackType == "melee" && parentPiece.inflictedDeaths < queuedDamage && updater.dead) //if our target is dead, find another to kill
        {

            var updater2 = targetPiece.soldierObjects[0].GetComponent<UpdateAgentDestination>();
            updater2.KillThis(targetPiece.soldierObjects[0]);
            parentPiece.inflictedDeaths++;
            targetPiece.soldierObjects.RemoveAt(0);
        }*/

        if (attackEffect != null && effectSpawnTransform != null)
        {
            var effect = Instantiate(attackEffect, new Vector3(effectSpawnTransform.position.x, effectSpawnTransform.position.y, effectSpawnTransform.position.z), Quaternion.Euler(0, 0, 0));
            effect.transform.localScale = new Vector3(.1f, .1f, .1f);
            Destroy(effect, 10);
        }
        if (projectilePrefab != null && effectSpawnTransform != null)
        {
            //Debug.LogError("Spawning projectile");
            CreateProjectile();
        }

        int random = Random.Range(0, attackSoundEffect.Length);
        PlayClip(attackSoundEffect[random]);

        //now tell parent we've attacked with a piece
        parentPiece.soldierAttacked = true;



    }
    float GetPlatformOffset()
    {
        float platformOffset = 0.0f;
        // 
        //          (SIDE VIEW OF THE PLATFORM)
        //
        //                   +------------------------- Mark (Sprite)
        //                   v
        //                  ___                                          -+-
        //    +-------------   ------------+         <- Platform (Cube)   |  platformOffset
        // ---|--------------X-------------|-----    <- TargetObject     -+-
        //    +----------------------------+
        //

        // we're iterating through Mark (Sprite) and Platform (Cube) Transforms. 

        platformOffset = enemy.transform.localPosition.y;

        return platformOffset;
    }
    public void CreateProjectile()
    {
        var projectile = Instantiate(projectilePrefab, effectSpawnTransform.position, Quaternion.Euler(90, transform.forward.y, 0));
        Destroy(projectile, 60);
        // think of it as top-down view of vectors: 
        //   we don't care about the y-component(height) of the initial and target position.
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        //var target = parentPiece.targetToAttackPiece.transform.position; //targeting the position of the overall unit
        var target = enemy.soldierObjects[Random.Range(0, enemy.soldierObjects.Count)].transform.position; //set target to a random soldier
        Vector3 targetXZPos = new Vector3(target.x + Random.Range(-spread, spread), 0.0f, target.z + Random.Range(-spread, spread));

        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        var LaunchAngle = angle;

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos) * 0.9f; //position distance 
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (target.y + GetPlatformOffset()) - target.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        Rigidbody rigid = projectile.GetComponent<Rigidbody>();
        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity;
    }
    public void PlayClip(AudioClip clip)
    {
        //if (!_audioSource.isPlaying)
        //{
        _audioSource.PlayOneShot(clip);
        //}
    }

}
