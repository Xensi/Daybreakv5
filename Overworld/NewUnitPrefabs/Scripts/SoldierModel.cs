using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    public RichAI aiPath;
    //[SerializeField] private AILerp aiPath;

    [SerializeField] private Animator animator;
    [SerializeField] private float threshold = .1f;
    [SerializeField] private float walkSpeed = 3;

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

    private float currentSpeed = 0;
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
    [SerializeField] private List<AudioClip> attackSounds;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float defaultStoppingDistance = 0.5f;
    [SerializeField] private float combatStoppingDistance = 0.1f;

    private void Start()
    {
        //currentAttackTime = UnityEngine.Random.Range(0, reqAttackTime);
        currentIdleTimer = UnityEngine.Random.Range(0, reqIdleTimer);
        animator.SetBool("walking", true);
        animator.SetBool("deployed", false);
        animator.SetInteger("row", position.row);
        currentSpeed = walkSpeed;
        currentAccel = defaultAccel;
        //InvokeRepeating("CullAnimations", 1f, .1f);
        //re = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
    public void UpdateRow()
    {

        animator.SetInteger("row", position.row);
    }
    public void UpdateSpeed()
    {
        if (aiPath.remainingDistance > catchUpThreshold && !formPos.tangledUp)
        {
            aiPath.acceleration = currentAccel + aiPath.remainingDistance;
            aiPath.maxSpeed = currentSpeed + aiPath.remainingDistance;
        }
        else
        {
            aiPath.acceleration = currentAccel;
            aiPath.maxSpeed = currentSpeed;
        }
        if (aiPath.canMove)
        {
            movingSpeed = Mathf.Sqrt(Mathf.Pow(aiPath.velocity.x, 2) + Mathf.Pow(aiPath.velocity.z, 2)); //might be slow
            float adjustedSpeed = movingSpeed / aiPath.maxSpeed;
            if (adjustedSpeed > 0.01f)
            {

                animator.SetFloat("speed", adjustedSpeed, 0.05f, Time.deltaTime); //damptime is 0.05
            }
            else
            {

                animator.SetFloat("speed", 0, 0.05f, Time.deltaTime); //damptime is 0.05
            }
        }
        else
        {
            animator.SetFloat("speed", 0, 0.05f, Time.deltaTime); //damptime is 0.05
        }
        if (formPos.listOfNearbyEnemies.Count > 0)
        {
            aiPath.endReachedDistance = combatStoppingDistance;
            //aiPath.enableRotation = false;
            if (position.row <= 3)
            { 
                animator.SetBool("deployed", true);
            }
        }
        else{

            aiPath.endReachedDistance = defaultStoppingDistance;
            //aiPath.enableRotation = true;
            animator.SetBool("deployed", false);
        }
    } 
    public void SetSpeed(bool sprinting)
    {
        animator.SetBool("deployed", sprinting);
        animator.SetBool("walking", !sprinting); //if deployeed, should be sprinting 

        if (sprinting)
        {
            currentSpeed = sprintSpeed;
            currentAccel = sprintAccel;

        }
        else
        {
            currentSpeed = walkSpeed;
            currentAccel = defaultAccel;
        }
    }

    public void CheckIfEnemyModelsNearby()
    {
        nearbyEnemyModels.Clear();
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
            SoldierModel model = colliders[i].GetComponent<SoldierModel>(); 

            if (model != null)
            {
                if (model.team == team || !model.alive)
                {
                    continue;
                }
                nearbyEnemyModels.Add(model);
                //Debug.LogError("hit one"); 
            }
        }
        FindClosestModel();
    }
    private void FindClosestModel()
    { 
        if (nearbyEnemyModels.Count <= 0)
        {
            targetEnemy = null; 
            return;
        } 
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
    public void TryToAttackEnemy()
    {
        if (targetEnemy == null || animator.GetBool("damaged") || !targetEnemy.alive)
        {
            return;
        }
        else
        {
            //raycast and see if we can hit the target. if we can't then raise spear. if we can then lower spear and attack

            /*LayerMask layerMask = LayerMask.GetMask("SelfBody");
            //Vector3 fwd = transform.TransformDirection(Vector3.forward);

            RaycastHit[] results = new RaycastHit[2];
            int hits = Physics.RaycastNonAlloc(transform.position, transform.forward, results, attackRange, layerMask, QueryTriggerInteraction.Ignore); 
            bool hitIt = false; 
            foreach (RaycastHit hit in results)
            {
                if (hit.collider == null)
                {
                    return;
                }
                else
                {
                    //Debug.LogError(hit.collider.gameObject.name);
                    SoldierModel model = hit.collider.gameObject.GetComponentInParent<SoldierModel>();
                    if (model == targetEnemy) //
                    {
                        hitIt = true;
                        break;
                    }
                    if (model == this) //ignore self
                    {
                        continue;
                    }
                    if (model.team == team || model.team != team) //something in the way
                    {
                        return; //stop
                    }
                }

            }
            if (hitIt)
            {
                animator.SetBool("deployed", true);
            }
            else
            {
                animator.SetBool("deployed", false);
            }*/
            animator.SetBool("deployed", true);

            if (currentAttackTime >= reqAttackTime) //if timer has met attack time
            {
                animator.SetBool("attacking", true);
                aiPath.canMove = false;
                formPos.modelAttacking = true;

            }
            else
            {
                animator.SetBool("attacking", false);
                aiPath.canMove = true;
                formPos.modelAttacking = false;
            }
        }
    }
    public void UpdateAttackTimer()
    {
        if (!animator.GetBool("attacking") && targetEnemy != null) //can't increment while attacking
        {
            currentAttackTime += .5f; 
        }
    }
    public void CheckIfIdle()
    {
        if (!aiPath.canMove)
        {

            currentIdleTimer += UnityEngine.Random.Range(0, 2);
            if (currentIdleTimer >= reqIdleTimer)
            {
                currentIdleTimer = 0;
                animator.SetBool("idle", true);
                animator.SetInteger("randomIdle", UnityEngine.Random.Range(0, 5));
            }
            else
            {
                animator.SetBool("idle", false);
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "DamagingObject")
        {
            SufferDamage(10, null);
        }
    }
    public void DealDamage()
    {
        currentAttackTime = 0;

        //aiPath.canMove = true;
        if (targetEnemy != null && targetEnemy.alive)
        {
            audioSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Count)]);
            targetEnemy.SufferDamage(damage, this);
        } 
    }

    private void SufferDamage(float dmg, SoldierModel origin)
    { 
         health -= dmg;
         animator.SetBool("damaged", true);
         animator.SetBool("attacking", false);

        if (animator.GetBool("deployed"))
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
            }
        }
    }
    private void KillThis()
    {
        //Debug.LogError("dead");
        alive = false;
        animator.SetBool("alive", false);
        aiPath.canMove = false;
        aiPath.enableRotation = false;

        selfCollider.enabled = false;
        position.assignedSoldierModel = null;
        formPos.numberOfAliveSoldiers -= 1; 
        audioSource.PlayOneShot(deathSounds[UnityEngine.Random.Range(0, deathSounds.Count)]);
        //formPos.tangledUp = true; //make this happen if killed by melee
    }
    public void TookDamage()
    {
        animator.SetBool("damaged", false);
        //currentAttackTime = 0;
    }

    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
    public void CullAnimations()
    {
        if (animate || aiPath.canMove || animator.GetBool("attacking") || formPos.listOfNearbyEnemies.Count > 0) //if we're in range or we can move
        {
            animator.speed = 1;
            animator.enabled = true;
        }
        else if (!animate || !aiPath.canMove)
        { //if we're out of range or we can't move

            animator.speed = 0;
            animator.enabled = false;
        }
        else
        {
            animator.speed = 0;
            animator.enabled = false;
        }

        if (formPos.listOfNearbyEnemies.Count == 0)
        {
            animator.SetBool("attacking", false);
        }
    }
    public void AnimatorUpdate()
    {
        if (!animator.GetBool("attacking"))
        {
            aiPath.canMove = true;
        }
        else
        {
            aiPath.canMove = false;
        }
        if (aiPath.remainingDistance > threshold) // if there's still path to traverse and we're not in the middle of attacking
        {
            if (!animator.GetBool("moving"))
            {
                animator.SetBool("moving", true);
            }
            if (!aiPath.canMove)
            { 
                aiPath.canMove = true;
            }
        }
        if (aiPath.reachedDestination) //if we've reached destination
        {
            if (animator.GetBool("moving"))
            { 
                animator.SetBool("moving", false);
            }
            if (aiPath.canMove)
            {
                aiPath.canMove = false;
            }
        }
    }


    public void FixRotation()
    {
        if (targetEnemy != null)
        {
            Vector3 targetDirection = targetEnemy.transform.position - transform.position;

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
