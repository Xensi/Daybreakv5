using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileFromSoldier : MonoBehaviour
{
    [SerializeField] private Rigidbody rigid;
    public FormationPosition formPosParent;
    private Quaternion initialRotation;
    private Quaternion finalRotation;
    public bool isFlying = true;

    private void Start()
    {
        //Destroy(this, 10);
        Invoke("SelfDestruct", 20);
        initialRotation = transform.rotation;
    }

    private void SelfDestruct()
    {
        formPosParent.soldierBlock.listProjectiles.Remove(this);
        Destroy(gameObject);
    }
    public void LaunchProjectile(Transform target, float LaunchAngle, float deviationAmount)
    {
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        //Vector3 targetXZPos = new Vector3(target.position.x, 0.0f, target.position.z);
        Vector3 targetXZPos = new Vector3(target.position.x + Random.Range(-deviationAmount, deviationAmount), 0.0f, target.position.z + Random.Range(-deviationAmount, deviationAmount));
        transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (target.position.y) - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity; 
    }

    public void UpdateRotation()
    { 
        transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Terrain")
        {
            isFlying = false;
            finalRotation = transform.rotation;
            transform.rotation = finalRotation;
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            //Debug.Log("Collision");
            //first, save the rotation
            /* 
             //stop updating rotation

             //final set for rotation?*/
        }
    }

}
