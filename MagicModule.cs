using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using System.Threading;
using System.Threading.Tasks;
public class MagicModule : MonoBehaviour
{    
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectileFromSoldier projectile; 
    [HideInInspector] public SoldierModel model;
    [SerializeField] private float projectileDeviationAmount = 5;

    [Range(0, 1)] //0: low force high angle; 1: high force low angle
    [SerializeField] private float forceRatio = 1;
    private void Start()
    {
        model = GetComponent<SoldierModel>();
        if (projectileSpawn == null)
        {
            projectileSpawn = model.transform;
        }
    }
    private ProjectileFromSoldier SpawnMissile()
    {
        ProjectileFromSoldier missile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity); //spawn the projectile
        missile.formPosParent = model.formPos; //communicate some info to the missile
        missile.soldierParent = model;
        missile.damage = model.damage;
        missile.armorPiercingDamage = model.armorPiercingDamage;

        model.formPos.soldierBlock.listProjectiles.Add(missile);

        return missile;
    } 
    public void CastMagic(Vector3 targetPos, int abilityNum, SoldierBlock.MageTypes mageType) 
    {
        Debug.Log("casting magic");
        #region Projectile Math
        //get target and apply random deviation based on distance 
        float dist = Vector3.Distance(transform.position, targetPos);

        float deviation = projectileDeviationAmount * dist * 0.01f;
        Vector3 deviationVec = new Vector3(Random.Range(-deviation, deviation), Random.Range(-deviation, deviation), Random.Range(-deviation, deviation));
        targetPos += deviationVec;

        //do math to hit target
        ProjectileDataClass data = CalculateProjectileInformation(projectileSpawn.transform.position, targetPos);  
        float angle = data.Angle;
        #endregion

        //fire projectile
        ProjectileFromSoldier missile = SpawnMissile();
        missile.LaunchProjectileRevised(data);
        model.formPos.modelAttacked = true;

        #region Cosmetic Effects
        //cosmetic
        if (model.attackSounds.Count > 0)
        {
            AudioClip sound = model.attackSounds[Random.Range(0, model.attackSounds.Count)];
            model.impactSource.PlayOneShot(sound);
        }
        model.animator.SetFloat(AnimatorDefines.angleID, angle); 
        #endregion  
    }

    public float maxProjectileForce = 100;
    public float maxRange = 200;
    private ProjectileDataClass CalculateProjectileInformation(Vector3 startPos, Vector3 targetPos)
    {
        Vector3 displacement = new Vector3(targetPos.x, startPos.y, targetPos.z) - startPos;
        float deltaXZ = displacement.magnitude;
        float deltaY = targetPos.y - startPos.y;

        //

        float grav = Mathf.Abs(Physics.gravity.y);
        float projectileStrength = Mathf.Clamp(Mathf.Sqrt(grav * (deltaY + Mathf.Sqrt(Mathf.Pow(deltaY, 2) + Mathf.Pow(deltaXZ, 2)))), 0.01f, maxProjectileForce);

        forceRatio = (1 - (deltaXZ / maxRange)) / 2;
        projectileStrength = Mathf.Lerp(projectileStrength, maxProjectileForce, forceRatio);

        float angle;

        if (forceRatio == 0)
        {
            angle = Mathf.PI / 2f - (0.5f * (Mathf.PI / 2 - (deltaY / deltaXZ)));
        }
        else
        {
            angle = Mathf.Atan((Mathf.Pow(projectileStrength, 2) - Mathf.Sqrt(Mathf.Pow(projectileStrength, 4) - grav * (grav * Mathf.Pow(deltaXZ, 2) + 2 * deltaY * Mathf.Pow(projectileStrength, 2)))) / (grav * deltaXZ));
        }
        Vector3 initialVelocity = Mathf.Cos(angle) * projectileStrength * displacement.normalized + Mathf.Sin(angle) * projectileStrength * Vector3.up;


        //clamp angle based on distance, closer means lower ceiling
        return new ProjectileDataClass
        {
            InitialVelocity = initialVelocity,
            Angle = angle,
            DeltaXZ = deltaXZ,
            DeltaY = deltaY
        };
    }
}
