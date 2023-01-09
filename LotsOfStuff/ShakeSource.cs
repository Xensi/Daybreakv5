using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeSource : MonoBehaviour
{
    public float shakeModifier = 1;
    public float shakeIntensity = 0;
    [SerializeField] private float endRadius = 30;
    public int id = 0;
    public bool shouldShake = true;

    private void OnDrawGizmos()
    { 
        Gizmos.DrawWireSphere(transform.position, endRadius);
    }
    private void Start()
    {
        InvokeRepeating("Repeat", 0, 1f);
    }

    private void Repeat()
    {
        if (shouldShake)
        { 
            CinemachineShake.Instance.ShakeCamera(shakeIntensity, 1, transform.position, endRadius, id);
        }
    }
}
