using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfIAmVisibleDisableAnimator : MonoBehaviour
{ 
    private Animator animator; 
    private void Start()
    { 
        animator = GetComponentInParent<Animator>(); 
    }
    private void OnBecameInvisible()
    {
        animator.enabled = true; 
    }
    private void OnBecameVisible()
    {
        animator.enabled = false; 
    }
}

