using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LayerMask layerMask = LayerMask.GetMask("Model");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20, layerMask);
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.LogError("detected a soldier in radius");
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 20);
    }
}
