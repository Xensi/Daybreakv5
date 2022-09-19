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
    [SerializeField] private int penetrationNum = 1;

    [SerializeField] private GameObject explosion;
    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    private void Start()
    {
        //Destroy(this, 10);
        Invoke("SelfDestruct", 20);
        Invoke("StartDamage", .25f);
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
    public void LaunchProjectile(Vector3 targetPos, float LaunchAngle, float deviationAmount)
    {
        Vector3 projectileXZPos = new Vector3(transform.position.x, 0.0f, transform.position.z);

        float xdiff = targetPos.x - transform.position.x;

        float zdiff = targetPos.z - transform.position.z;

        float desiredOffset = 5;

        float pythagoras = Mathf.Sqrt(Mathf.Pow(xdiff, 2) + Mathf.Pow(zdiff, 2));

        float scaling = desiredOffset / pythagoras;

        xdiff *= scaling;

        zdiff *= scaling;

        Vector3 targetXZPos = new Vector3(targetPos.x + xdiff + Random.Range(-deviationAmount, deviationAmount), 0.0f, targetPos.z + zdiff + Random.Range(-deviationAmount, deviationAmount));
        transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = (targetPos.y) - transform.position.y;

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
    private void Update()
    {
        if (isFlying)
        {
           UpdateRotation();
        }
    }
    private void DisableThis()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        capsule.enabled = false;
        audioSource.enabled = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hurtbox") //
        {
            if (canDamage)
            {  
                SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                if (hitModel != null && hitModel.alive)
                {
                    //canDamage = false;

                    if (soldierParent != null)
                    {
                        audioSource.PlayOneShot(soldierParent.projectileImpactSounds[UnityEngine.Random.Range(0, soldierParent.projectileImpactSounds.Count)]); //play impact sound at enemy position
                    }
                    hitModel.SufferDamage(damage, soldierParent); 
                    /*
                        isFlying = false;
                        finalRotation = transform.rotation;
                        transform.rotation = finalRotation;
                        rigid.constraints = RigidbodyConstraints.FreezeAll;


                        transform.parent = other.gameObject.transform;*/

                    penetrationNum--;
                    if (penetrationNum <= 0)
                    { 
                        SelfDestruct();
                    } 

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
            if (explosion != null)
            {
                GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);
                exp.transform.position = new Vector3(exp.transform.position.x, 0, exp.transform.position.z);
                gameObject.SetActive(false);
            }
            DisableThis();

        }
    } 
}
