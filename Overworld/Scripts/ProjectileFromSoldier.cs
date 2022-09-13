using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileFromSoldier : MonoBehaviour
{
    [SerializeField] private Rigidbody rigid;
    public FormationPosition formPosParent;
    public SoldierModel soldierParent;
    private Quaternion initialRotation;
    private Quaternion finalRotation;
    public bool isFlying = true;
    [SerializeField] private GameObject model;
    [SerializeField] private AudioSource audioSource;
    public float damage = 1;
    private bool canDamage = false;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    private void Start()
    {
        //Destroy(this, 10);
        Invoke("SelfDestruct", 20);
        Invoke("StartDamage", .5f);
        model.SetActive(true);

    }

    private void StartDamage()
    {
        canDamage = true;
    }

    private void SelfDestruct()
    {
        formPosParent.soldierBlock.listProjectiles.Remove(this);
        Destroy(gameObject);
    }
    public void LaunchProjectile(Transform target, float LaunchAngle, float deviationAmount)
    {
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);

        float xdiff = target.position.x - transform.position.x;

        float zdiff = target.position.z - transform.position.z;

        float desiredOffset = 5;

        float pythagoras = Mathf.Sqrt(Mathf.Pow(xdiff, 2) + Mathf.Pow(zdiff, 2));

        float scaling = desiredOffset / pythagoras;

        xdiff *= scaling;

        zdiff *= scaling;

        Vector3 targetXZPos = new Vector3(target.position.x + xdiff + Random.Range(-deviationAmount, deviationAmount), 0.0f, target.position.z + zdiff + Random.Range(-deviationAmount, deviationAmount));
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
        if (other.gameObject.tag == "AltgardModel" || other.gameObject.tag == "ZhanguoModel" )
        {
            if (canDamage)
            { 
                //Debug.Log("collided w model");
                SoldierModel hitModel = other.GetComponent<SoldierModel>();
                if (hitModel != null && hitModel.alive)
                {
                    canDamage = false;
                    audioSource.PlayOneShot(soldierParent.projectileImpactSounds[UnityEngine.Random.Range(0, soldierParent.projectileImpactSounds.Count)]); //play impact sound at enemy position
                    hitModel.SufferDamage(damage, soldierParent);

/*
                    isFlying = false;
                    finalRotation = transform.rotation;
                    transform.rotation = finalRotation;
                    rigid.constraints = RigidbodyConstraints.FreezeAll;


                    transform.parent = other.gameObject.transform;*/
                    SelfDestruct();

                }
            }
        }
        if (other.gameObject.tag == "Terrain" && canDamage)
        {
            canDamage = false;
            //Debug.Log("collided w terrain");
            audioSource.PlayOneShot(soldierParent.projectileImpactSounds[UnityEngine.Random.Range(0, soldierParent.projectileImpactSounds.Count)]); //play impact sound at enemy position
            isFlying = false;
            finalRotation = transform.rotation;
            transform.rotation = finalRotation;
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            

        }
    }
}
