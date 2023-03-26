using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using System.Threading;
using System.Threading.Tasks;
public class RangedModule : MonoBehaviour
{
    #region RangedUnits

    public enum RangedBehavior
    {
        FireAndRepeat,
        FireAndReload
    }
    public RangedBehavior behavior = RangedBehavior.FireAndRepeat;

    [Header("Assign these if ranged")]
    [SerializeField] private Transform eyeline;
    [SerializeField] private bool directFire = false;
    [SerializeField] private float directFireRadius = 80;
    [Range(0.0f, 45.0f)] [SerializeField] public float maxFiringAngle = 45;
    [Range(0.0f, 45)] [SerializeField] public float minFiringAngle = 10;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectileFromSoldier projectile;
    [Range(0.0f, 100)] [SerializeField] public float power = 100;

    [SerializeField] private bool volleyFireAndRetreat = false;
    [SerializeField] private float projectileDeviationAmount = 5;
    [SerializeField] private float projectileDeviationAmountVertical = 5;
    [Header("Ranged optional")]
    [SerializeField] private GameObject fireEffect;


    [Header("Reloading vars")]
    public bool rangedNeedsLoading = false;
    public int ammo = 0; //start unloaded typically.
    [SerializeField] private int maxAmmo = 1;
    public int internalAmmoCapacity = 100;
    public bool loadingRightNow = false;
    public float currentFinishedLoadingTime = 0;
    public float requiredLoadingTime = 1f;
    [HideInInspector] public SoldierModel model;

    //public bool useMovementPrediction = true;
    //[SerializeField]
    [Range(0, 1)] //0: low force high angle; 1: high force low angle
    [SerializeField] private float forceRatio = 1;
    #endregion
    float maxDistanceAllowReloads = 1;


    private void Start()
    {

        model.animator.SetInteger(AnimatorDefines.ammoID, ammo);

        if (rangedNeedsLoading && ammo <= 0 && internalAmmoCapacity > 0)
        {
            Reload();
        }
        if (!rangedNeedsLoading && ammo == 0) //give ammo if we dont use it at all
        {
            ModifyAmmo(1);
        }
    }


    public void TriggerRangedAttack()
    {
        if (ammo > 0)
        { 
            if (directFire)
            {
                LaunchBullet();
            }
            else
            {
                //Debug.Log("firing proj revised");
                FireProjectileRevised();
            }
            ModifyAmmo(-1);
        }
    }
    public void FinishReload()
    {
        SetLoading(false);
        currentFinishedLoadingTime = 0;
        ModifyAmmo(1);
        internalAmmoCapacity--;
        internalAmmoCapacity = Mathf.Clamp(internalAmmoCapacity, 0, 999);
    }
    public enum MoveTreatment
    {
        DoNotChange,
        Halt,
        Move
    }
    private void CancelLoading(MoveTreatment moveTreatment)
    {
        currentFinishedLoadingTime = 0; //reset the time
        SetLoading(false);

        switch (moveTreatment)
        {
            case MoveTreatment.DoNotChange:
                break;
            case MoveTreatment.Halt:
                model.SetMoving(false);
                break;
            case MoveTreatment.Move:
                model.SetMoving(true);
                break;
            default:
                break;
        }
    } 
    private void Reload()
    {
        //Debug.Log("RELOADING");
        SetLoading(true);
    }

    public void SetLoading(bool val)
    {
        model.animator.SetBool(AnimatorDefines.loadingID, val);
        loadingRightNow = val;
        //reloadingIndicator.enabled = val;
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
    private float AngleCalculation(Vector3 targetPos)
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        float angle = 10;
        angle = dist * 0.5f;
        float clamped = Mathf.Clamp(angle, minFiringAngle, maxFiringAngle);
        return clamped;
    }

    private void ModifyAmmo(int num)
    {
        ammo += num;
        ammo = Mathf.Clamp(ammo, 0, maxAmmo);
        model.animator.SetInteger(AnimatorDefines.ammoID, ammo);
    }
    private Vector3 targetPos; //set using GetTarget();
    public async void LineOfSightUpdate()
    {
        CheckIfLOSObstructed(targetPos);
        await Task.Yield();
    }
    private void ManualUpdateTargetPosition()
    {
        if (!directFire) //arrows
        {
            if (!model.formPos.focusFire) //not focus fire
            {
                if (model.formPos.enemyFormationToTarget != null)
                {
                    targetPos = new Vector3(model.formPos.enemyFormationToTarget.transform.position.x, model.formPos.enemyFormationToTarget.averagePositionBasedOnSoldierModels, model.formPos.enemyFormationToTarget.transform.position.z);
 
                    hasTarget = true;
                }
                else
                {
                    targetPos = new Vector3(999, 999, 999);
                    hasTarget = false;
                }
            }
            else //focus fire
            {
                if (model.formPos.formationToFocusFire != null) //if we have a formation to focus on
                {
                    //targetPos = formPos.formationToFocusFire.transform.position;
                    targetPos = new Vector3(model.formPos.formationToFocusFire.transform.position.x, model.formPos.formationToFocusFire.averagePositionBasedOnSoldierModels, model.formPos.formationToFocusFire.transform.position.z);

                    hasTarget = true;
                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(model.formPos.focusFirePos.x, model.formPos.focusFirePos.y + 1, model.formPos.focusFirePos.z);
                    hasTarget = true;
                }
            }
        }
        else //muskets, cannons
        {
            if (model.formPos.focusFire)
            {
                if (model.formPos.formationToFocusFire != null)
                {
                    float centerOfMassOffset = 1;
                    model.TargetClosestEnemyInFormation(model.formPos.formationToFocusFire);
                    Vector3 pos = model.targetEnemy.transform.position;
                    targetPos = new Vector3(pos.x, pos.y + centerOfMassOffset, pos.z);
                    //Vector3 vecFocus = formPos.formationToFocusFire.formationPositionBasedOnSoldierModels;
                    //targetPos = new Vector3(vecFocus.x, vecFocus.y, vecFocus.z);
                    hasTarget = true;
                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(model.formPos.focusFirePos.x, model.formPos.focusFirePos.y, model.formPos.focusFirePos.z);
                    hasTarget = true;
                }
            }
            else
            {
                if (model.formPos.enemyFormationToTarget != null)
                {
                    float centerOfMassOffset = 1;
                    model.TargetClosestEnemyInFormation(model.formPos.enemyFormationToTarget);
                    Vector3 pos = model.targetEnemy.transform.position;
                    targetPos = new Vector3(pos.x, pos.y + centerOfMassOffset, pos.z);
                    //Vector3 vec = formPos.enemyFormationToTarget.formationPositionBasedOnSoldierModels;
                    //targetPos = new Vector3(vec.x, vec.y, vec.z); 
                    hasTarget = true;
                }
                else
                {
                    hasTarget = false;
                }
            }
        }
        /*if (model.formPos.missileTarget != null)
        {
            model.formPos.missileTarget.transform.position = targetPosition;
        }*/
    }
    public async void RepeatingUpdateTargetPosition()
    {
        ManualUpdateTargetPosition(); 
        await Task.Yield();
    }
    public void MageCastProjectile(Vector3 targetPos, int abilityNum, string mageType) //let's fire projectiles at a target
    {
        model.magicCharged = false;
        model.formPos.modelAttacked = true;
        if (model.attackSounds.Count > 0)
        {
            model.impactSource.PlayOneShot(model.attackSounds[UnityEngine.Random.Range(0, model.attackSounds.Count)]);
        }
        ProjectileFromSoldier missile = SpawnMissile();

        float dist = Vector3.Distance(transform.position, targetPos);
        float angle = 0;
        angle = dist * 0.5f;
        float clamped = Mathf.Clamp(angle, 0, 45);
        if (mageType == "Pyromancer")
        {
            if (abilityNum == 0)
            {
                clamped = 60;
            }
        }
        if (mageType == "Gallowglass")
        {
            if (abilityNum == 0)
            {
                clamped = 10;
            }
        }
        float deviation = projectileDeviationAmount * dist * 0.01f;

        float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
        float adjusted = clamped / 45; //for anim 
        model.animator.SetFloat(AnimatorDefines.angleID, adjusted);

        missile.LaunchProjectile(targetPos, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
    }

    private async void CheckIfLOSObstructed(Vector3 target) 
    {
        Vector3 start = projectileSpawn.transform.position;
        float distance = Vector3.Distance(target, start);
        bool treatAsDirectFire = directFire; 
        /*if (distance <= directFireRadius && !directFire)
        {
            treatAsDirectFire = true; //if arcing and enemy is within radius, then direct fire
        }*/
        Vector3 heading = (target - start).normalized; //vector from here to there 
        LayerMask layerMask;
        float range;
        if (treatAsDirectFire)
        {
            layerMask = LayerMask.GetMask("Model", "Terrain");
            range = Vector3.Distance(start, target);
        }
        else
        {
            layerMask = LayerMask.GetMask("Model", "Terrain");
            range = 20;
            Vector3 displacement = new Vector3(target.x, start.y, target.z) - start;

            ProjectileDataClass data = CalculateProjectileInformation(start, target);
            data = CalculatePredictedPositionData(data, start, target);
            float angle = data.Angle;
            //float angle = AngleCalculation(target);
            //var a = angle;// * Mathf.Deg2Rad;
            //Vector3 dir = (transform.forward * Mathf.Cos(a) + transform.right * Mathf.Sin(a)).normalized;
            //Vector3 dir = (heading * Mathf.Cos(a) + transform.up * Mathf.Sin(a)).normalized;

            Vector3 dir = Mathf.Cos(angle) * range * displacement.normalized + Mathf.Sin(angle) * range * Vector3.up;
            heading = dir; 
        }
        Vector3 sightLine = transform.position;

        if (eyeline != null)
        {
            sightLine = eyeline.position;
        }

        RaycastHit hit;
        if (Physics.Raycast(sightLine, heading, out hit, range, layerMask)) //hit something
        {
            if (hit.collider.gameObject.tag == "Hurtbox") //if model
            {
                SoldierModel hitModel = hit.collider.gameObject.GetComponentInParent<SoldierModel>();
                if (hitModel != null)
                {
                    if (hitModel.team == model.team)
                    { 
                        UpdateLOSIndicator();
                        model.hasClearLineOfSight = false;
                        Debug.DrawRay(sightLine, heading * range, Color.red, 1, true);
                    }
                    else //enemies are fair game
                    {
                        UpdateLOSIndicator(true);
                        model.hasClearLineOfSight = true;
                        Debug.DrawRay(sightLine, heading * range, Color.green, 1, true);
                    }
                }
            }
            else if (hit.collider.gameObject.tag == "Terrain") //terrain blocks shots
            {
                UpdateLOSIndicator();
                model.hasClearLineOfSight = false;
                Debug.DrawRay(sightLine, heading * range, Color.red, 1, true);
            }
        }
        else
        { 
            Debug.DrawRay(sightLine, heading * range, Color.green, 1, true); 
            UpdateLOSIndicator(true);
            model.hasClearLineOfSight = true;
        }
        await Task.Yield();
    }
    private async void UpdateLOSIndicator(bool ForceHide = false)
    {
        if (model.lineOfSightIndicator != null)
        {
            if (ForceHide)
            { 
                model.lineOfSightIndicator.enabled = false;
            }
            else //enable if visible
            { 
                if (model.formPos.selected)
                {
                    model.lineOfSightIndicator.enabled = true;
                }
                else
                {
                    model.lineOfSightIndicator.enabled = false;
                }
            }
        }
        await Task.Yield();
    }
    private void LaunchBullet()
    {
        if (ammo <= 0) //probably not necessary
        {
            if (internalAmmoCapacity > 0)
            {
                Reload();
            }
            return;
        }
        if (model.targetEnemy != null || model.formPos.focusFire || model.formPos.enemyFormationToTarget != null)
        {
            //Vector3 targetPos = GetTarget();

            //calculations
            float dist = Vector3.Distance(transform.position, targetPos);

            if (model.attackSounds.Count > 0)
            {
                //Debug.Log("playing bulelt sound");
                model.impactSource.PlayOneShot(model.attackSounds[UnityEngine.Random.Range(0, model.attackSounds.Count)]);
            }
            model.animator.SetFloat(AnimatorDefines.angleID, 0);

            ProjectileFromSoldier missile = SpawnMissile();

            //FIRE
            Vector3 heading = targetPos - transform.position;
            float deviation = UnityEngine.Random.Range(-projectileDeviationAmount, projectileDeviationAmount);
            float deviationUp = UnityEngine.Random.Range(-projectileDeviationAmountVertical, projectileDeviationAmountVertical);
            //Debug.Log(deviationUp);

            heading = Quaternion.AngleAxis(deviation, Vector3.up) * heading;
            heading = Quaternion.AngleAxis(deviationUp, Vector3.forward) * heading;

            missile.FireBullet(heading, power);

            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, projectileSpawn.position, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(heading);
            }

            if (rangedNeedsLoading) //if we need reloading and we're out
            {
                ModifyAmmo(-1);
                if (ammo <= 0 && internalAmmoCapacity > 0)
                {
                    Reload();
                }
            }
            model.formPos.modelAttacked = true;


            if (volleyFireAndRetreat && model.formPos.soldierBlock.frontRow.modelsInRow.Contains(model)) //if volley fire and we're in first row
            {
                model.waitingForAttackOver = true;
            }
        }
    }

    public float maxProjectileForce = 100;
    public float maxRange = 200;

    void OnDrawGizmos()
    {
        if (model.alive && !model.formPos.routing && hasTarget && model.hasClearLineOfSight)
        { 
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPos, 1);
        }
    }
    private ProjectileDataClass CalculateProjectileInformation(Vector3 startPos, Vector3 targetPos)
    {
        Vector3 displacement = new Vector3(targetPos.x, startPos.y, targetPos.z) - startPos;
        float deltaXZ = displacement.magnitude;
        float deltaY = targetPos.y - startPos.y;

        //

        float grav = Mathf.Abs(Physics.gravity.y);
        float projectileStrength = Mathf.Clamp(Mathf.Sqrt(grav * (deltaY + Mathf.Sqrt(Mathf.Pow(deltaY, 2) + Mathf.Pow(deltaXZ, 2)))), 0.01f, maxProjectileForce);

        forceRatio = (1 - (deltaXZ/maxRange))/2;
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
    private ProjectileDataClass CalculatePredictedPositionData(ProjectileDataClass directData, Vector3 startPos, Vector3 targetPos)
    {
        Vector3 projectileVelocity = directData.InitialVelocity;
        projectileVelocity.y = 0;
        float time = directData.DeltaXZ / projectileVelocity.magnitude;

        IAstarAI targetRigid = model.formPos.enemyFormationToTarget.aiPath;

        Vector3 targetMovement = targetRigid.velocity * time;

        Vector3 newTargetPos = new Vector3(targetPos.x + targetMovement.x, targetPos.y, targetPos.z + targetMovement.z);

        ProjectileDataClass predictiveData = CalculateProjectileInformation(startPos, newTargetPos);
        predictiveData.InitialVelocity = Vector3.ClampMagnitude(predictiveData.InitialVelocity, maxProjectileForce);
        return predictiveData;
    }
    private bool hasTarget = false;
    private void FireProjectileRevised() //what causes targetpos to not sync up with formation?
    {
        if (targetPos.y > 500)
        {
            hasTarget = false;
            return;
        }

        if (model.targetEnemy != null || model.formPos.focusFire || model.formPos.enemyFormationToTarget != null || hasTarget)
        {
            #region Projectile Math
            //get target and apply random deviation based on distance
            
            ManualUpdateTargetPosition();

            float dist = Vector3.Distance(transform.position, targetPos);

            float deviation = projectileDeviationAmount * dist * 0.01f;
            Vector3 deviationVec = new Vector3(Random.Range(-deviation, deviation), Random.Range(-deviation, deviation), Random.Range(-deviation, deviation));
            targetPos += deviationVec;

            //do math to hit target
            ProjectileDataClass data = CalculateProjectileInformation(projectileSpawn.transform.position, targetPos);
            data = CalculatePredictedPositionData(data, projectileSpawn.transform.position, targetPos);

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
            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, projectileSpawn.position, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(transform.forward);
            }
            #endregion 
            //manage ammo
            if (rangedNeedsLoading) //if we need reloading and we're out
            {
                ModifyAmmo(-1);
                if (ammo <= 0 && internalAmmoCapacity > 0)
                {
                    Reload();
                }
            }
        }
    } 
}
