using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    [SerializeField] private AILerp aiPath;
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

    private void Start()
    {
        animator.SetBool("walking", true);
        animator.SetBool("deployed", false);

        colliderList = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliderList)
        {
            col.enabled = false;
        }
        rigidBodyList = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in rigidBodyList)
        { 
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }
    } 
    private void Update()
    {
        AnimatorUpdate();

        /*if (animator.GetBool("deployed"))
        {
            if (targetEnemy != null && GetDistanceBetween(transform, targetEnemy.transform) > attackRange)
            {
                target.transform.position = targetEnemy.transform.position;
            }
            else if (targetEnemy != null && GetDistanceBetween(transform, targetEnemy.transform) <= attackRange)
            {
                FixRotation(targetEnemy.transform);
                target.transform.position = transform.position;
                animator.SetBool("attacking", true);
            }
            else
            {
                animator.SetBool("attacking", false);
            }
        }
        else
        {
            targetEnemy = null;
        }*/

    }
    private void AnimatorUpdate()
    {
        if (!animator.GetBool("deployed"))
        {
            animator.SetBool("walking", true);
        }
        else
        {
            animator.SetBool("walking", false);
        }
        if (aiPath.remainingDistance > threshold) // if there's still path to traverse
        {
            animator.SetBool("moving", true);
            aiPath.canMove = true;
        }
        if (aiPath.reachedDestination) //if we've reached destination
        {
            animator.SetBool("moving", false);
            aiPath.canMove = false;
            //aiDesSet.target = null;
        }

        if (animator.GetBool("walking")) //if walking set speed
        {
            aiPath.speed = walkSpeed;
        }
        else
        {
            aiPath.speed = sprintSpeed;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        SoldierModel soldierModel = other.gameObject.GetComponent<SoldierModel>();
        if (soldierModel != null && soldierModel != this && team != soldierModel.team)
        {
            //Debug.LogError("Collided");

            if (targetEnemy != null)
            {//if distance between new model is closer than target enemy
                if (GetDistanceBetween(transform, soldierModel.transform) < GetDistanceBetween(transform, targetEnemy.transform))
                {
                    targetEnemy = soldierModel;
                }
            }
            else
            {
                targetEnemy = soldierModel;
            }

        }
    }*/
    private float GetDistanceBetween(Transform transform1, Transform transform2)
    {
        float dist = Vector3.Distance(transform1.position, transform2.position);
        return dist;
    }

    private void FixRotation(Transform transform2)
    { 
        Vector3 targetDirection = transform2.position - transform.position;

        float singleStep = settledRotationSpeed * Time.deltaTime;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        Debug.DrawRay(transform.position, newDirection, Color.red);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }


}
