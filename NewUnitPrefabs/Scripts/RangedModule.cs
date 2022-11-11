using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int ammo = 0; //start unloaded typically.
    [SerializeField] private int maxAmmo = 1;
    [SerializeField] private int internalAmmoCapacity = 100;
    public bool loadingRightNow = false;
    private float currentFinishedLoadingTime = 0;
    [SerializeField] private float timeUntilFinishedLoading = 1f;
    [HideInInspector] public SoldierModel model;
    #endregion


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
                FireProjectile();
            }
        }
    }
    private void FinishReload()
    {
        SetLoading(false);
        currentFinishedLoadingTime = 0;
        ModifyAmmo(1);
        internalAmmoCapacity--;
        internalAmmoCapacity = Mathf.Clamp(internalAmmoCapacity, 0, 999);
    }
    private void CancelLoading()
    {
        SetLoading(false);
    }
    public void UpdateLoadTimer()
    {
        if (model.routing)
        {
            return;
        }
        if (!rangedNeedsLoading)
        {
            CancelLoading();
            return;
        }
        if (model.richAI.remainingDistance > model.remainingDistanceThreshold)
        {
            CancelLoading();
            return;
        }
        if (model.airborne || model.knockedDown)
        {
            CancelLoading();
            return;
        }
        if (model.damaged)
        {
            CancelLoading();
            return;
        }
        if (model.formPos.movementManuallyStopped || !model.formPos.obeyingMovementOrder)
        {
            //if we're force stopped we can reload
        }
        else if (model.moving)
        {
            CancelLoading();
            return;
        }
        if (ammo <= 0 && !loadingRightNow && internalAmmoCapacity > 0)
        {
            Reload();
        }
        else if (loadingRightNow)
        {   //should we stop reloading? 
            //increment timer  
            currentFinishedLoadingTime += 1f;
            if (currentFinishedLoadingTime >= timeUntilFinishedLoading)
            {
                FinishReload();
            }
        }
    }
    private void Reload()
    {
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
    public void LineOfSightUpdate()
    {
        model.clearLineOfSight = !IsLineOfSightObstructed(GetTarget());
    }
    private Vector3 GetTarget()
    {
        Vector3 targetPos = new Vector3(999, 999, 999);
        Vector3 spawn = projectileSpawn.transform.position;

        if (directFire) //muskets
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
                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(model.formPos.focusFirePos.x, model.formPos.focusFirePos.y, model.formPos.focusFirePos.z);
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
                }
            }
        }
        else //arc
        {
            if (model.formPos.focusFire)
            {
                if (model.formPos.formationToFocusFire != null) //if we have a formation to focus on
                {
                    //targetPos = formPos.formationToFocusFire.transform.position;
                    targetPos = new Vector3(model.formPos.formationToFocusFire.transform.position.x, model.formPos.formationToFocusFire.averagePositionBasedOnSoldierModels, model.formPos.formationToFocusFire.transform.position.z);

                }
                else //otherwise use the terrain position.
                {
                    targetPos = new Vector3(model.formPos.focusFirePos.x, model.formPos.focusFirePos.y + 1, model.formPos.focusFirePos.z);
                }
            }
            else
            {
                if (model.formPos.enemyFormationToTarget != null)
                {
                    //targetPos = formPos.enemyFormationToTarget.transform.position;
                    targetPos = new Vector3(model.formPos.enemyFormationToTarget.transform.position.x, model.formPos.enemyFormationToTarget.averagePositionBasedOnSoldierModels, model.formPos.enemyFormationToTarget.transform.position.z);

                }
            }
        }

        if (model.formPos.missileTarget != null)
        {
            model.formPos.missileTarget.transform.position = targetPos;
        }
        return targetPos;
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

    private bool IsLineOfSightObstructed(Vector3 target)
    {
        float distance = Vector3.Distance(target, transform.position);
        bool treatAsDirectFire = directFire;
        if (distance <= directFireRadius && !directFire)
        {
            treatAsDirectFire = true; //if arcing and enemy is within radius, then direct fire
        }
        Vector3 heading = (target - transform.position).normalized; //vector from here to there 
        LayerMask layerMask;
        float range;
        if (treatAsDirectFire)
        {
            layerMask = LayerMask.GetMask("Model", "Terrain");
            range = Vector3.Distance(transform.position, target);
        }
        else
        {
            layerMask = LayerMask.GetMask("Model", "Terrain");
            range = 20;
            float angle = AngleCalculation(target);
            var a = angle * Mathf.Deg2Rad;
            //Vector3 dir = (transform.forward * Mathf.Cos(a) + transform.right * Mathf.Sin(a)).normalized;
            Vector3 dir = (heading * Mathf.Cos(a) + transform.up * Mathf.Sin(a)).normalized;
            heading = dir;
        }
        Vector3 sightLine = transform.position;

        if (eyeline != null)
        {
            sightLine = eyeline.position;
        }

        RaycastHit hit;
        if (Physics.Raycast(sightLine, heading, out hit, range, layerMask))
        {
            Debug.DrawRay(sightLine, heading * range, Color.white, Time.deltaTime, true);
            if (hit.collider.gameObject.tag == "Hurtbox")
            {
                SoldierModel hitModel = hit.collider.gameObject.GetComponentInParent<SoldierModel>();
                if (hitModel != null)
                {
                    if (hitModel.team == model.team)
                    {
                        if (model.lineOfSightIndicator != null)
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
                        return true;
                    }
                }
            }
            else if (hit.collider.gameObject.tag == "Terrain") //terrain blocks shots
            {
                if (model.lineOfSightIndicator != null)
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
                return true;
            }
        }
        else
        {

            Debug.DrawRay(sightLine, heading * range, Color.red, Time.deltaTime, true);
        }
        if (model.lineOfSightIndicator != null)
        {
            model.lineOfSightIndicator.enabled = false;
        }
        return false;
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
            Vector3 targetPos = GetTarget();

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
    private void FireProjectile() //let's fire projectiles at a target
    {
        //Debug.Log("firing proj");
        if (model.targetEnemy != null || model.formPos.focusFire || model.formPos.enemyFormationToTarget != null)
        {
            Vector3 targetPos = GetTarget();

            //calculations
            float dist = Vector3.Distance(transform.position, targetPos);
            float clamped = AngleCalculation(targetPos);
            float deviation = projectileDeviationAmount * dist * 0.01f;

            float clampedDeviation = Mathf.Clamp(deviation, 2, 999);
            float adjusted = clamped / 45; //for anim   

            if (model.attackSounds.Count > 0)
            {
                //Debug.Log("playing proj sound");
                model.impactSource.PlayOneShot(model.attackSounds[UnityEngine.Random.Range(0, model.attackSounds.Count)]);
            }
            ProjectileFromSoldier missile = SpawnMissile();


            model.animator.SetFloat(AnimatorDefines.angleID, adjusted);
            if (dist <= directFireRadius && !directFire) //if enemy is close
            {
                Vector3 heading = targetPos - transform.position;
                float newDeviation = UnityEngine.Random.Range(-projectileDeviationAmount, projectileDeviationAmount);
                float deviationUp = UnityEngine.Random.Range(-projectileDeviationAmountVertical, projectileDeviationAmountVertical);
                //Debug.Log(deviationUp);

                heading = Quaternion.AngleAxis(newDeviation, Vector3.up) * heading;
                heading = Quaternion.AngleAxis(deviationUp, Vector3.forward) * heading;

                float velocity = 37.5f;
                //missile.FireBullet(heading, power);
                missile.LaunchBullet(heading, velocity);
            }
            else
            {
                missile.LaunchProjectile(model.formPos.missileTarget.transform.position, clamped, clampedDeviation); //fire at the position of the target with a clamped angle and deviation based on distance
            }
            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, projectileSpawn.position, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(transform.forward);
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
        }
    }
}
