using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLoreController : MonoBehaviour
{
    public Animator unitAnimator;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAnimationIdle()
    {
        unitAnimator.Play("BaseIdle");
    }
    public void UpdateAnimationWalk()
    {
        unitAnimator.Play("BaseMove");
    }
    public void UpdateAnimationAttack()
    {
        unitAnimator.Play("BaseAttack");
    }
}
