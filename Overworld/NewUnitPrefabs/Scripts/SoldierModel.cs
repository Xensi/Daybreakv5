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

    //public SkinnedMeshRenderer[] re;
     
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
        if (!aiPath.canMove)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, formPos.gameObject.transform.rotation, finishedPathRotSpeed * Time.deltaTime);
        }
    }
}
