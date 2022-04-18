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

    [SerializeField] private float attackRange = 1;

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
    [SerializeField] private float finishedPathRotSpeed = 1;

    [SerializeField] private List<SoldierModel> nearbyEnemyModels;

    //public SkinnedMeshRenderer[] re;
    [SerializeField] private float reqAttackTime = 3;
    [SerializeField] private float currentAttackTime = 3;

    [SerializeField] private float health = 10;
    [SerializeField] private float damage = 1;
    [SerializeField] private float armor = 0;


    private void Start()
    {
        animator.SetBool("walking", true);
        animator.SetBool("deployed", false);
        currentSpeed = walkSpeed;
        currentAccel = defaultAccel;
        //InvokeRepeating("CullAnimations", 1f, .1f);
        //re = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void UpdateSpeed()
    {
        if (aiPath.remainingDistance > catchUpThreshold)
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
        LayerMask layerMask = LayerMask.GetMask("SelfBody"); 
        int maxColliders = 80;
        Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, attackRange, colliders, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < numColliders; i++)
        {
            if (colliders[i].gameObject == self) //if our own hitbox, ignore
            {
                continue;
            }
            SoldierModel model = colliders[i].GetComponentInParent<SoldierModel>(); 

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
        if (nearbyEnemyModels.Count == 1)
        {
            targetEnemy = nearbyEnemyModels[0];
        }
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
        if (targetEnemy == null || animator.GetBool("damaged"))
        {
            return;
        }
        else
        { 
            if (currentAttackTime >= reqAttackTime) //if timer has met attack time
            {
                animator.SetBool("attacking", true);
                //aiPath.canMove = false;

            }
            else
            {
                animator.SetBool("attacking", false);
                //aiPath.canMove = true;
            }
        }
    }
    public void UpdateAttackTimer()
    {
        if (!animator.GetBool("attacking") && targetEnemy != null) //can't increment while attacking
        { 
            currentAttackTime += Random.Range(.1f, .5f);
        }
    }
    public void DealDamage()
    {
        //aiPath.canMove = true;
        if (targetEnemy != null)
        {
            currentAttackTime = 0;
            targetEnemy.health -= damage;
            targetEnemy.animator.SetBool("damaged", true);
            targetEnemy.animator.SetBool("attacking", false);

            if (targetEnemy.animator.GetBool("deployed"))
            {

                targetEnemy.animator.Play("WeaponDownDamaged");
            }
            else
            {

                targetEnemy.animator.Play("WeaponUpDamaged");
            }
            if (targetEnemy.health <= 0)
            {
                targetEnemy.alive = false;
                targetEnemy.animator.SetBool("alive", false);
                //targetEnemy.animator.Play("WeaponUpDie");
            }
        }
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
        if (animate || aiPath.canMove) //if we're in range or we can move
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
        if (alive != oldAlive)
        {
            oldAlive = alive;
            AliveEvent();
        }

    }
    public void AnimatorUpdate()
    {
        if (aiPath.remainingDistance > threshold) // if there's still path to traverse
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

    private void AliveEvent()
    {
        animator.SetBool("alive", alive);
        aiPath.canMove = alive;
        aiPath.enableRotation = alive;
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
