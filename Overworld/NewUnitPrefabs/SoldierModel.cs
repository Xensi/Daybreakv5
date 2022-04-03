using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class SoldierModel : MonoBehaviour
{
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Animator animator;
    [SerializeField] private float threshold = .1f;
    [SerializeField] private float walkSpeed = 3;

    [SerializeField] private float sprintSpeed = 6;
    public Transform target; 
    [SerializeField] private float settledRotationSpeed = 1;

    private void Start()
    {
        animator.SetBool("walking", true);
        animator.SetBool("deployed", false);
    }

    private void Update()
    { 
        if (aiPath.remainingDistance > threshold)
        {
            animator.SetBool("moving", true);
        }
        if (animator.GetBool("walking"))
        {
            aiPath.maxSpeed = walkSpeed;
        }
        else
        {
            aiPath.maxSpeed = sprintSpeed;
        }

        if (!animator.GetBool("moving"))
        {
            Vector3 targetDirection = target.position - transform.position;

            float singleStep = settledRotationSpeed * Time.deltaTime;
             
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
             
            Debug.DrawRay(transform.position, newDirection, Color.red);
             
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

    }


    public void FinishedMovement()
    {
        animator.SetBool("moving", false);
        //rotation

    }


}
