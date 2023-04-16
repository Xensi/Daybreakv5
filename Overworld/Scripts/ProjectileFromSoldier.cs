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
    public float armorPiercingDamage = 1;
    private bool canDamage = false;
    [SerializeField] private int penetrationNum = 1;

    [SerializeField] private GameObject explosionEffect;

    [SerializeField] private bool isModelProj = false;

    public Vector3 startingPos;

    [SerializeField] private GameObject blood;

    [SerializeField] private bool shouldAlwaysSpawnExplosion = false;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }
    private void Start()
    {
        //Destroy(this, 10);
        Invoke("SelfDestruct", 20);
        //Invoke("StartDamage", .25f);
        if (model != null)
        { 
            model.SetActive(true);
        }
         
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

    public void FireBullet(Vector3 direction, float power)
    {
        rigid.AddForce(direction * power);
        isFlying = true;
    }
    public void LaunchBullet(Vector3 direction, float velocity)
    { 
        float reqForce = rigid.mass * (velocity / Time.fixedDeltaTime);
        rigid.AddForce(direction.normalized * reqForce);
        isFlying = true;
    }
    public void LaunchProjectileRevised(ProjectileDataClass data)
    {
        if (!float.IsNaN(data.InitialVelocity.x))
        { 
            rigid.velocity = data.InitialVelocity;
        }
        else
        {
            SelfDestruct();
        }
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
        //Debug.Log(globalVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigid.velocity = globalVelocity;


        /*float distance = Vector3.Distance(targetPos, soldierParent.transform.position);
        float angle = distance;
        float clampedAngle = Mathf.Clamp(angle, soldierParent.minFiringAngle, soldierParent.maxFiringAngle);

        float xDev = Random.Range(-deviationAmount, deviationAmount);
        float zDev = Random.Range(-deviationAmount, deviationAmount);
        Vector3 modifiedTargetPos = new Vector3(targetPos.x + xDev, targetPos.y + clampedAngle, targetPos.z + zDev);
        Vector3 direction = modifiedTargetPos - soldierParent.transform.position;
        rigid.AddForce(direction * soldierParent.power);
        isFlying = true;*/
    }

    public void UpdateRotation()
    {
        if (isFlying)
        {
            transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;

            if (isModelProj)
            {
                soldierParent.transform.position = transform.position;
                soldierParent.transform.rotation = Quaternion.LookRotation(-rigid.velocity) * initialRotation;
            }
        }
    }
    private void Update()
    {
        if (isFlying)
        { 
            UpdateRotation();
            CheckIfCanDamage(1);
            MakeSureAboveTerrain();
        }
    }
    private void MakeSureAboveTerrain()
    { 
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 1000, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            if (transform.position.y < hit.point.y)
            {
                transform.position = hit.point;
            }
        }
    }
    private void CheckIfCanDamage(float req)
    {
        if (!canDamage)
        {
            float distance = Vector3.Distance(soldierParent.formPos.formationPositionBasedOnSoldierModels, transform.position); 
            if (distance >= req)
            {
                canDamage = true;
            }
        }
    }
    private void DisableThis()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        capsule.enabled = false;
        audioSource.enabled = false;
        Invoke("SelfDestruct", 5);
    }
    void OnTriggerEnter(Collider other)
    {
        if (isModelProj)
        {

            if (other.gameObject.tag == "Terrain" || other.gameObject.tag == "TreeTerrain")
            {
                isFlying = false;
                finalRotation = transform.rotation;
                transform.rotation = finalRotation;
                //soldierParent.pathfindingAI.enabled = true;
                soldierParent.transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, 999), transform.position.z);
                Vector3 heading = startingPos - transform.position;
                soldierParent.transform.rotation = Quaternion.LookRotation(heading, Vector3.up);
                soldierParent.airborne = false;
                DisableThis();
            }
        }
        else
        {
            if (other.gameObject.tag == "Hurtbox") //
            {
                if (canDamage)
                {
                    SoldierModel hitModel = other.GetComponentInParent<SoldierModel>();
                    if (hitModel != null && hitModel.alive && hitModel.formPos != soldierParent.formPos) //can't hit our own formation, but can hit any others
                    {
                        if (hitModel.team == soldierParent.team) //no friendly fire, but arrows won't go through our own units
                        {
                            SelfDestruct();
                            return;
                        }
                        //canDamage = false;

                        if (soldierParent != null)
                        {
                            if (soldierParent.projectileImpactSounds.Count > 0)
                            {
                                audioSource.PlayOneShot(soldierParent.projectileImpactSounds[Random.Range(0, soldierParent.projectileImpactSounds.Count)]); //play impact sound at enemy position
                            }
                        }
                        float damageMult = 1;
                        BodyPart bodyPart = other.GetComponent<BodyPart>();
                        /*if (bodyPart != null) //blood
                        {
                            damageMult = bodyPart.multiplierDamage;
                            float randomX = UnityEngine.Random.Range(-360, 360);
                            float randomY = UnityEngine.Random.Range(-360, 360);
                            float randomZ = UnityEngine.Random.Range(-360, 360);
                            float x = UnityEngine.Random.Range(-.1f, .1f);
                            float y = UnityEngine.Random.Range(-.1f, .1f);
                            float z = UnityEngine.Random.Range(-.1f, .1f);
                            Vector3 newVec = new Vector3(x, y, z);
                            float modDamage = damage * .5f;
                            Vector3 spawnVec;
                            Vector3 heading;
                            if (bodyPart.type == BodyPart.BodyType.Head)
                            {
                                spawnVec = hitModel.head.position;
                                heading = hitModel.head.position - transform.position;
                            }
                            else
                            {
                                spawnVec = hitModel.body.position;
                                heading = hitModel.body.position - transform.position;
                            }
                            *//*GameObject decal = Instantiate(blood, spawnVec + newVec, Quaternion.identity);

                            if (bodyPart.type == BodyPart.BodyType.Head)
                            {
                                decal.transform.parent = hitModel.head;
                            }
                            else
                            {
                                decal.transform.parent = hitModel.spine;
                            }
                            decal.transform.rotation = Quaternion.LookRotation(heading);
                            decal.transform.localScale = new Vector3(modDamage, modDamage, modDamage);*//*
                        }*/
                        hitModel.SufferDamage(damage, armorPiercingDamage, soldierParent, damageMult);

                        formPosParent.shotsHit++;

                        penetrationNum--;
                        if (penetrationNum <= 0)
                        {
                            SelfDestruct();
                        }

                        if (shouldAlwaysSpawnExplosion)
                        { 
                            if (explosionEffect != null)
                            {
                                GameObject exp = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                                //needs to hit ground?
                                //exp.transform.position = new Vector3(exp.transform.position.x, exp, exp.transform.position.z);
                                //gameObject.SetActive(false);
                            }
                            else
                            {

                                rigid.constraints = RigidbodyConstraints.FreezeAll;
                            }
                        }

                    }
                }
            }
            if (other.gameObject.tag == "Terrain" || other.gameObject.tag == "TreeTerrain" )
            {
                canDamage = false;
                //Debug.Log("collided w terrain");

                if (soldierParent.projectileImpactSounds.Count > 0)
                {
                    audioSource.PlayOneShot(soldierParent.projectileImpactSounds[Random.Range(0, soldierParent.projectileImpactSounds.Count)]); //play impact sound at enemy position
                }

                isFlying = false;
                finalRotation = transform.rotation;
                transform.rotation = finalRotation;
                if (explosionEffect != null)
                {
                    GameObject exp = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                    //exp.transform.position = new Vector3(exp.transform.position.x, 0, exp.transform.position.z);
                    //gameObject.SetActive(false);
                }
                else
                {

                    rigid.constraints = RigidbodyConstraints.FreezeAll;
                }
                DisableThis();
            }
        } 
    } 
}
