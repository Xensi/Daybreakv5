using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    public string team = "Altgard"; //Whose team are we on?
    public RichAI aiPath;
    [SerializeField] private float threshold = .5f;
    public List<FormationPosition> listOfNearbyEnemies;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public Transform aiTarget;
    public Transform rotTarget;
    [SerializeField] private float velThreshold = .1f;
    [Tooltip("Checks nearby formations. Nearby formations can be moved towards automatically.")]
    [SerializeField] private float checkRadius = 10;
    [Tooltip("When to stop moving when auto-engaging.")]
    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;
    [SerializeField] private BoxCollider rectangleCollider;
    //
    public FormationPosition enemyFormationToTarget;
    public SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;
    //

    [SerializeField] private float currentSpeed = 0;
    [SerializeField] private float slowSpeed = .5f;
    public float walkingSpeed = 3.5f;
    public float sprintSpeed = 6.5f;

    public bool selected = false;

    [SerializeField] private bool running = false;

    [SerializeField] private MeshRenderer selectionBox;
    public LineRenderer lineRenderer;
    public LineRenderer lineRenderer2;

    [SerializeField] private bool weaponsDeployed = false;
    private bool oldWeaponsDeployed = false;
    [SerializeField] private float waitThreshold = 1;
    [SerializeField] private float offsetThreshold = 4; //when remaining distance above this threshold
    [SerializeField] private Transform posParentTransform;
    [SerializeField] private Vector3 posParentStartingPos;
    [SerializeField] private Vector3 offsetAmount;
    [SerializeField] private Vector3 deployedOffsetAmount;
    [SerializeField] private float requiredVelocity = 2;
    [SerializeField] private float deployedRequiredVelocity = 4;
    [SerializeField] private Transform compass;
    [SerializeField] private Transform highParent;
    [SerializeField] private float finishedPathRotSpeed = 1;
    public bool pathSet = false;

    public List<Position> frontlinePositions;

    public bool modelAttacking = false;

    public bool obeyingMovementOrder;

    public int numberOfAliveSoldiers = 80;
    private int oldNumAlive = 80;
    public int maxSoldiers = 80;

    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;

    [SerializeField] private float normRotate = 30;
    [SerializeField] private Transform offsetSecondRow;

    [SerializeField] private float colliderBoxRange = 9;
    private float colliderBoxNotDeployedRange = 8;
    [SerializeField] private float xsize = 10;
    [SerializeField] private float ysize = 4;
    [SerializeField] private float zsize = 8;

    [SerializeField] private float xoffset = 0;
    [SerializeField] private float yoffset = 2;
    [SerializeField] private float zoffset = 1;
    [SerializeField] private float zNotDeployedOffset = 0;
    private float zSelLine; //???

    public bool playingIdleChatter = false;
    public bool playingAttackChatter = false;
    public bool playingDeathReactionChatter = false;
    public bool playingMarchChatter = false;

    public bool deployedPikes = false;

    public List<SoldierModel> firstLineModels;
    public bool modelAttacked = false;
    public bool modelTookDamage = false;
    public bool inCombat = false;


    [SerializeField] private float secondRowOffsetAmount = 0f;

    [Tooltip("Should we try to attack nearby enemies?")]
    public bool holdFire = false;
    public bool chaseDetectedEnemies = true;

    public bool engagedInMelee = false;

    public bool focusFire = false; //should we pick targets automatically or fire on a specific place/unit
    public Vector3 focusFirePos = new Vector3(0, 0, 0);

    public FormationPosition formationToFocusFire;

    public bool movementManuallyStopped = false;

    public List<Vector3> destinationsList = new List<Vector3>();

    public bool finishedChangedFacing = true;
    [SerializeField] private CharacterController charController;
    [SerializeField] private float charRadius = 4.5f;

    [SerializeField] private bool isCavalry = false;
    [SerializeField] private bool freezeFormPos = false;

    [SerializeField] private float freezeTimer = 0;

    public bool shouldRotateToward = false;

    public bool alive = true;


    [SerializeField] private int soldierModelToCheck = 0;


    private void Start()
    {
        currentSpeed = walkingSpeed;
        aiPath.maxSpeed = currentSpeed;

        if (soldierBlock.melee)
        {
            colliderBoxRange = soldierBlock.modelAttackRange * 2;
        }
        BeginUpdates();

    }
    public void GetTangledUp()
    {
        freezeFormPos = true;
        tangledUp = true;
        freezeTimer++;
        freezeTimer = Mathf.Clamp(freezeTimer, 0, 3);
    }
    private void UnfreezeThis()
    {
        freezeTimer--;
        if (freezeTimer <= 0)
        {
            freezeFormPos = false;
            tangledUp = false;
            freezeTimer = 0;
        }
    }


    public void DisableIdleChatterForSeconds(float sec)
    {
        playingIdleChatter = true;
        Invoke("EnableIdleChatter", sec);
    }
    public void DisableAttackChatterForSeconds(float sec)
    {
        playingAttackChatter = true;
        Invoke("EnableAttackChatter", sec);
    }
    public void DisableMarchChatterForSeconds(float sec)
    {
        playingMarchChatter = true;
        Invoke("EnableMarchChatter", sec);
    }
    public void DisableDeathReactionForSeconds(float sec)
    {
        playingDeathReactionChatter = true;
        Invoke("EnableDeathReactionChatter", sec);
    }
    public void EnableDeathReactionChatter()
    {
        playingDeathReactionChatter = false;
    }
    private void EnableAttackChatter()
    {
        playingAttackChatter = false;
    }
    private void EnableMarchChatter()
    {
        playingMarchChatter = false;
    }
    private void EnableIdleChatter()
    {
        playingIdleChatter = false;
    }
    public void BeginUpdates()
    {
        InvokeRepeating("RapidUpdate", 0f, .01f);
        InvokeRepeating("FastUpdate", 0f, .1f);
        InvokeRepeating("SlowUpdate", 0f, .5f); //normally .05f
        InvokeRepeating("VerySlowUpdate", 0f, 1f);
    }
    private  void RapidUpdate()
    {
        AsynchronousSoldierUpdate();
    }
    private void FastUpdate()
    {
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, 10), transform.position.z);
        FixRotation();
        UpdateLineRenderer();
        FastSoldierUpdate();
        UpdateFormationMovementStatus();

        if (enableAnimations != oldEnableAnimations) //interrogate purpose
        {

            oldEnableAnimations = enableAnimations;
            foreach (SoldierModel item in soldierBlock.listSoldierModels)
            {
                item.animate = enableAnimations;
            }
        }
    }
    private void AsynchronousSoldierUpdate()
    {
        //staggered/async 
        SoldierModel checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
        if (checkingModel == null)
        {
            while (checkingModel == null) //let us skip over those that have died
            {
                soldierModelToCheck++;
                if (soldierModelToCheck >= maxSoldiers) //reset on 80
                {
                    soldierModelToCheck = 0;
                }
                checkingModel = soldierBlock.modelsArray[soldierModelToCheck];
            }
        }

        if (checkingModel.alive)
        {
            if (checkingModel.melee && listOfNearbyEnemies.Count > 0)
            {
                checkingModel.CheckIfEnemyModelsNearby();
            }
        }
        soldierModelToCheck++;
        int max = maxSoldiers;
        if (soldierBlock.mageType != "")
        {
            max += 2;
        }
        if (soldierModelToCheck >= max) //reset on 80 + 2
        {
            soldierModelToCheck = 0;
        }
    }
    private void FastSoldierUpdate()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateAttackTimer();
                    model.UpdateLoadTimer();
                    model.UpdateDamageTimer();
                    model.UpdateFinishedAttackingTimer();
                    model.UpdateRecoveryTimer();
                    model.FixRotation();
                    model.UpdateSpeed();

                }
            }
        } 
    }
    private void UpdateFormationMovementStatus()
    {
        if (movementManuallyStopped)
        {
            aiPath.canMove = false;
            aiPath.enableRotation = false;
            obeyingMovementOrder = false;
        }
        else
        {
            if (modelAttacking && soldierBlock.canBeRanged) //if ranged and attacking, freeze formation
            {
                aiPath.canMove = false;
            }
            else
            {
                if (aiPath.remainingDistance > threshold) // if there's still path to traverse
                {
                    aiPath.canMove = true;
                }
                else if (aiPath.reachedDestination && aiPath.reachedEndOfPath && aiPath.remainingDistance <= threshold)
                {
                    if (destinationsList.Count <= 1)
                    {
                        obeyingMovementOrder = false;
                        aiPath.canMove = false;
                    }
                    else if (destinationsList.Count > 1)
                    {
                        destinationsList.RemoveAt(0);
                        if (destinationsList.Count > 0)
                        {
                            aiTarget.transform.position = destinationsList[0];
                        }
                    }
                }
            }
        }
    }
    private void SlowUpdate()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.UpdateMovementStatus();
                }
            }
        }
        //FixRotation();
        CheckIfInMeleeRange();
    }
    private void VerySlowUpdate() 
    {
        if (numberOfAliveSoldiers <= 0)
        {
            foreach (FormationPosition item in listOfNearbyEnemies)
            {
                item.listOfNearbyEnemies.Remove(this);
            }
            gameObject.SetActive(false);
        }
        UpdateSoldiers(); ////expensive and so far causes hang ups
        UpdateSpeed(); // 
        UpdateCollider(); // 
        CheckNearbyFormations(); //
        CheckIfInCombat(); //
        UnfreezeThis();
        CheckIfLowSoldiersRout();
        foreach (SoldierModel model in soldierBlock.listMageModels)
        {
            if (model.alive)
            {
                model.UpdateMageTimer();
            }
        }
    }
    private void UpdateSoldiers()
    {
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    if (model.melee)
                    { 
                        if (listOfNearbyEnemies.Count > 0) //only check if enemies are nearby and we're melee
                        {
                            //model.CheckIfEnemyModelsNearby();
                        }
                        model.StopAttackingWhenNoEnemiesNearby();
                        model.UpdateDeploymentStatus();
                    }
                    model.CullAnimations();
                    model.CheckIfIdle(); 
                    model.CheckIfAlive();
                    model.UpdateCharController();
                }
            }
        }
        for (int i = 0; i < soldierBlock.reinforcePositionsArray.Length; i++)
        {
            Position item = soldierBlock.reinforcePositionsArray[i];
            if (item != null)
            {
                if (item.assignedSoldierModel == null)
                {
                    item.SeekReplacement();
                }
            }
        }
    }
    private void UpdateSpeed()
    {

        if (listOfNearbyEnemies.Count == 0) //no enemy
        {
            weaponsDeployed = false;
        }
        else //yes enemy
        {
            weaponsDeployed = true;
        }
        if (tangledUp)
        {
            currentSpeed = walkingSpeed;
            float current = numberOfAliveSoldiers;
            float maxSol = maxSoldiers;
            float ratio = current / maxSol;

            currentSpeed *= ratio;
            aiPath.rotationSpeed = slowRotate;
        }
        else
        {
            currentSpeed = walkingSpeed;
            aiPath.rotationSpeed = normRotate;
        }
        float min = 0.1f;
        float max = 100f;
        currentSpeed = Mathf.Clamp(currentSpeed, min, max);

        aiPath.maxSpeed = currentSpeed;

    }
    private void CheckNearbyFormations()
    {
        listOfNearbyEnemies.Clear();
        LayerMask layerMask = LayerMask.GetMask("Formation");
        int maxColliders = 24;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].gameObject.tag != "Formation")
            {
                continue;
            }
            else
            {
                if (hitColliders[i] == rectangleCollider) //ignore own collider
                {
                    continue;
                }
                FormationPosition form = hitColliders[i].gameObject.GetComponent<FormationPosition>();
                if (form.team == team)
                {
                    continue;
                }

                listOfNearbyEnemies.Add(form);
            }
        }

        if (obeyingMovementOrder)
        {
            return;
        }
        FindClosestFormation();
    }
    private void CheckIfLowSoldiersRout()
    {
        if (numberOfAliveSoldiers <= 10)
        { 
            for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
            {
                SoldierModel model = soldierBlock.modelsArray[i];
                if (model != null)
                {
                    if (model.alive)
                    {
                        model.KillThis();
                    }
                }
            }
            alive = false;
            //remove from selections
        }
    }


    private void CheckIfInCombat()
    {
        if (listOfNearbyEnemies.Count > 0)
        { 
            if (modelAttacked || modelTookDamage)
            {
                inCombat = true;
                modelAttacked = false;
                modelTookDamage = false;
            }
            else
            {
                inCombat = false;
            }
        }

    }
    
    public void TriggerSelectionCircles(bool on)
    { 
        for (int i = 0; i < soldierBlock.modelsArray.Length; i++)
        {
            SoldierModel model = soldierBlock.modelsArray[i];
            if (model != null)
            {
                if (model.alive)
                {
                    model.selectionCircle.SetActive(on);
                }
                else
                {
                    model.selectionCircle.SetActive(false);
                }
            }
        }
    }

    public void PursueCommand()
    {
        chaseDetectedEnemies = true;
    }

    public void StopChaseCommand()
    {
        chaseDetectedEnemies = false;
    }

    private void UpdateCollider()
    {
        int num = 0;
        if (numberOfAliveSoldiers >= 71) //80
        {
            num = 0;
            zSelLine = 4;
        }
        else if (numberOfAliveSoldiers >= 61) //70
        {
            num = 1;
            zSelLine = 3;
        }
        else if (numberOfAliveSoldiers >= 51) //70
        {
            num = 2;
            zSelLine = 2;
        }
        else if (numberOfAliveSoldiers >= 41) //70
        {
            num = 3;
            zSelLine = 1;
        }
        else if (numberOfAliveSoldiers >= 31) //70
        {
            num = 4;
            zSelLine = 0;
        }
        else if (numberOfAliveSoldiers >= 21) //70
        {
            num = 5;
            zSelLine = -1;
        }
        else if (numberOfAliveSoldiers >= 11) //70
        {
            num = 6;
            zSelLine = -2;
        }
        else if (numberOfAliveSoldiers >= 1) //70
        {
            num = 7;
            zSelLine = -3;
        }
        Vector3[] array = new Vector3[5];
        array[0] = new Vector3(-5, 0, -zSelLine);
        array[1] = new Vector3(5, 0, -zSelLine);
        array[2] = new Vector3(5, 0, 4);
        array[3] = new Vector3(-5, 0, 4);
        array[4] = new Vector3(-5, 0, -zSelLine);

        if (!isCavalry)
        {
            float centerOffset = 16.24f;
            charController.radius = Mathf.Clamp(charRadius - (num * 0.5f), .5f, 4.5f);
            posParentTransform.localPosition = new Vector3(-4.5f, 0, 3.5f - num * .5f);

            if (!soldierBlock.canBeRanged)
            { 
                if (listOfNearbyEnemies.Count > 0)
                {
                    offsetSecondRow.localPosition = new Vector3(-secondRowOffsetAmount, 0, .5f);
                }
                else
                {
                    offsetSecondRow.localPosition = new Vector3(0, 0, 0);
                }
            }
        }
        
    }

    private void CheckIfInMeleeRange()
    {
        if (enemyFormationToTarget != null)
        { 
            float dist = GetDistance(transform, enemyFormationToTarget.gameObject.transform);
            if (dist <= stoppingDistance)
            {
                engagedInMelee = true;
            }
            else
            {
                engagedInMelee = false;
            }
        }
        else
        {
            engagedInMelee = false;
        }
    }
    
   

    private void UpdateLineRenderer()
    {
        if (!aiPath.reachedDestination)
        {
            lineRenderer.enabled = selected;
            lineRenderer2.enabled = selected;

            if (lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y+1.5f, transform.position.z)); //offsetting pos4 +0.585001f
                int count = 1;
                lineRenderer.positionCount = destinationsList.Count + 1; 
                foreach (Vector3 item in destinationsList)
                { 
                    lineRenderer.SetPosition(count, item);
                    count++;
                }
            }
            if (lineRenderer2.enabled)
            {
                if (destinationsList.Count > 0) //if there are still destinations to go
                {

                }
            }
            if (destinationsList.Count > 0)
            {
                aiTarget.transform.position = destinationsList[0];
            }

        }
        else
        {
            lineRenderer.enabled = false;
        }
        //rotation fixed?

        lineRenderer2.enabled = !finishedChangedFacing;
    } 
    public void CheckDirectionOfMovement()
    {
        compass.LookAt(aiTarget);
        float rot = compass.localEulerAngles.y;
        float threshold = 10;
        Debug.LogError(rot);
        if (-threshold <= rot && rot <= threshold)
        {
            Debug.LogError("front");
            aiPath.enableRotation = false; 
        }
        else if (90 - threshold <= rot && rot <= 90 + threshold)
        {
            Debug.LogError("Side1");
            aiPath.enableRotation = false;
             
        }
        else if (270 - threshold <= rot && rot <= 270 + threshold)
        {
            Debug.LogError("Side2");
            aiPath.enableRotation = false; 
        }
        else if (180 - threshold <= rot && rot <= 180 + threshold)
        {
            Debug.LogError("back");
            aiPath.enableRotation = false; 
        }
        else
        {
            aiPath.enableRotation = true;
        }
    }

    private void OffsetPositions()
    {
        if (enemyFormationToTarget == null)
        {
            if (aiPath.remainingDistance > offsetThreshold)
            {
                if (weaponsDeployed)
                {
                    posParentTransform.localPosition = posParentStartingPos;
                    if (Mathf.Abs(aiPath.velocity.x) > requiredVelocity || Mathf.Abs(aiPath.velocity.z) > deployedRequiredVelocity)
                    { 
                        posParentTransform.localPosition = posParentStartingPos + deployedOffsetAmount;
                    }

                }
                else
                { 
                    posParentTransform.localPosition = posParentStartingPos;
                    if (Mathf.Abs(aiPath.velocity.x) > requiredVelocity || Mathf.Abs(aiPath.velocity.z) > requiredVelocity)
                    { 
                        posParentTransform.localPosition = posParentStartingPos + offsetAmount;
                    }
                } 
            }
            else
            {
                posParentTransform.localPosition = posParentStartingPos;
            }
        }

    }
    
    public void CastMagic(Vector3 targetPos, int abilityNum)
    { 
        if (soldierBlock.mageType == "Pyromancer")
        {
            if (abilityNum == 0)
            {
                foreach(SoldierModel mage in soldierBlock.listMageModels)
                {
                    if (mage.magicCharged && mage.alive)
                    { 
                        mage.MageCastProjectile(targetPos, abilityNum, soldierBlock.mageType); //magic charged equals false
                        break;
                    }
                }
            }
        }
    }
    public void SetSelected(bool val)
    {
        selected = val; 
    }
     

    private void FindClosestFormation()
    {
        if (listOfNearbyEnemies.Count <= 0)
        {
            enemyFormationToTarget = null;
            aiPath.endReachedDistance = moveStopDistance;
            aiPath.maxSpeed = currentSpeed;
            weaponsDeployed = false;
            tangledUp = false;
            return;
        }
        enemyFormationToTarget = listOfNearbyEnemies[0]; 
        float initDist = GetDistance(transform, listOfNearbyEnemies[0].transform);
        float compareDist = initDist;
        foreach (FormationPosition item in listOfNearbyEnemies) //doesn't work yet
        { 
            float dist = GetDistance(transform, item.gameObject.transform);
            //Debug.LogError(dist);
            if (dist < compareDist)
            {
                enemyFormationToTarget = item;
                compareDist = dist;
            }
            
        } 
        if (soldierBlock.canBeRanged)
        { 
        } 
        else
        { 
            if (chaseDetectedEnemies)
            {
                EngageFoe();
            }
        }

    }

    private void EngageFoe()
    {
        float dist = GetDistance(transform, enemyFormationToTarget.gameObject.transform);
        if (dist <= stoppingDistance)
        {
            aiTarget.transform.position = transform.position;
            rotTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
        }
        else if (enemyFormationToTarget != null)
        {
            aiTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
        }

    }
    public void StopCommand()
    {
        movementManuallyStopped = true;
    }
    public void ResumeCommand()
    {
        movementManuallyStopped = false;
    }
    private void FixRotation()
    {
        if (!aiPath.canMove && !obeyingMovementOrder && !tangledUp && shouldRotateToward)
        {
            Vector3 targetDirection = rotTarget.position - transform.position; 

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            newDirection.y = 0; //so that our rotation is not vertical
            //Debug.DrawRay(transform.position, newDirection, Color.red);

            transform.rotation = Quaternion.LookRotation(newDirection);

        }
        float angle = 5;
        if (Vector3.Angle(transform.forward, rotTarget.position - transform.position) < angle && !aiPath.canMove)
        {
            finishedChangedFacing = true;
        }
        else
        {
            finishedChangedFacing = false;
        }
    }
    private float GetDistance(Transform one, Transform two)
    {
        float dist = Vector3.Distance(one.position, two.position);
        return dist;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
