using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    public string team = "Altgard";
    public RichAI aiPath;
    [SerializeField] private float threshold = .5f;
    public List<FormationPosition> listOfNearbyEnemies;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public Transform aiTarget;
    public Transform rotTarget;
    [SerializeField] private float velThreshold = .1f;
    [SerializeField] private float checkRadius = 10;
    [SerializeField] private BoxCollider rectangleCollider;



    public FormationPosition enemyFormationToTarget;
    public SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;

    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;

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
    public float maxSoldiers = 80;

    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;

    [SerializeField] private float normRotate = 30;
    [SerializeField] private Transform offsetSecondRow;

    private float colliderBoxRange = 9;
    private float colliderBoxNotDeployedRange = 8;
    [SerializeField] private float xsize = 10;
    [SerializeField] private float ysize = 4;
    [SerializeField] private float zsize = 8;

    [SerializeField] private float xoffset = 0;
    [SerializeField] private float yoffset = 2;
    [SerializeField] private float zoffset = 1;
    [SerializeField] private float zNotDeployedOffset = 0;

    [SerializeField] private LineRenderer selectionLine;
    private float zSelLine;

    public bool playingIdleChatter = false;
    public bool playingAttackChatter = false;
    public bool playingDeathReactionChatter = false;

    public bool deployedPikes = false;

    public List<SoldierModel> firstLineModels;
    public bool modelAttacked = false;
    public bool modelTookDamage = false;
    public bool inCombat = false;


    [SerializeField] private float secondRowOffsetAmount = 0f;

    [SerializeField] private bool chaseDetectedEnemies = true;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Formation")
        { 
            tangledUp = true;
            
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Formation")
        {
            tangledUp = true;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        tangledUp = false;
    } 
    private void Start()
    {
        currentSpeed = walkingSpeed;
        aiPath.maxSpeed = currentSpeed;
        colliderBoxRange = soldierBlock.modelAttackRange * 2;
        BeginUpdates();
        selectionLine.enabled = false;

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
    private void EnableIdleChatter()
    {
        playingIdleChatter = false;
    }
    public void BeginUpdates()
    {
        InvokeRepeating("VerySlowUpdate", 4f, 1f);
        InvokeRepeating("SlowUpdate", 4f, .5f); //normally .05f
        InvokeRepeating("FastUpdate", 4f, .1f); 
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
        UpdateSpeed();
        UpdateCollider();
        CheckNearbyFormations();
        UpdateSoldiers();
        CheckIfInCombat();
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
    
    private void UpdateSoldiers()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.CheckIfEnemyModelsNearby();
                model.CommonSense();
                model.CullAnimations();
                model.CheckIfIdle();
                model.DeployWeaponsInAdvance();
            }
        }
        foreach (Position item in soldierBlock.reinforceablePositions)
        {
            if (item.assignedSoldierModel == null)
            {
                item.SeekReplacement();
            }
        }
    }

    private void UpdateProjectiles()
    {
        foreach (ProjectileFromSoldier missile in soldierBlock.listProjectiles)
        {  
        }
    }

    private void SlowBasedOnNumberOfDistantSoldiers()
    {
        //the more alive soldiers that are not at their position by some threshold, the more reduction in speed

    }
    private void UpdateCollider()
    {
        int num = 0;
        if (numberOfAliveSoldiers <= 10) //if very few soldiers, unable to hold the front
        { //one row remaining
            //rectangleCollider.size = new Vector3(1, 0, 1);
        }
        else
        {
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
            selectionLine.SetPositions(array);
            if (listOfNearbyEnemies.Count > 0)
            {
                rectangleCollider.center = new Vector3(xoffset, yoffset, zoffset + .5f * num); //originally .5 7
                rectangleCollider.size = new Vector3(xsize, ysize, colliderBoxRange - 1 * num);
            }
            else
            {
                rectangleCollider.center = new Vector3(xoffset, yoffset, 0+.5f *num); //originally .5 7
                rectangleCollider.size = new Vector3(xsize, ysize, colliderBoxNotDeployedRange - 1 * num);

            }
        }
        
        if (listOfNearbyEnemies.Count > 0)
        {
            offsetSecondRow.localPosition = new Vector3(-secondRowOffsetAmount, 0, .5f);
        }
        else
        {
            offsetSecondRow.localPosition = new Vector3(0, 0, 0);
            tangledUp = false; 
        }
    }
    private void SlowUpdate()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            { 
                model.UpdateMovementStatus(); 
            }
        }
        FixRotation(); 
    } 
    private void FastUpdate()
    { 
        OffsetPositions(); 
        UpdateLineRenderer();
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.UpdateAttackTimer();
                model.UpdateDamageTimer();
                model.UpdateFinishedAttackingTimer();
                model.UpdateRecoveryTimer();
                model.FixRotation(); //make it so this doesn't do anything if rotation is good 
                model.UpdateSpeed(); //for animations update them more frequently if player is closer
                
            }
        }
        foreach (ProjectileFromSoldier missile in soldierBlock.listProjectiles)
        {
            if (missile.isFlying)
            { 
                missile.UpdateRotation();
            }
        }

        if (aiPath.remainingDistance > threshold) // if there's still path to traverse
        {
            aiPath.canMove = true;
        }
        if (aiPath.reachedDestination && aiPath.reachedEndOfPath && aiPath.remainingDistance < 2)
        {
            obeyingMovementOrder = false;
        }
        if (aiPath.reachedDestination) //if we've reached destination
        {
            aiPath.canMove = false;
            //obeyingMovementOrder = false;
        }
        if (enableAnimations != oldEnableAnimations)
        {

            oldEnableAnimations = enableAnimations; 
            foreach (SoldierModel item in soldierBlock.listSoldierModels)
            {
                item.animate = enableAnimations;
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
            currentSpeed = walkingSpeed/2;
            float v = Mathf.Sqrt(numberOfAliveSoldiers / maxSoldiers);
            currentSpeed *= v;
            aiPath.rotationSpeed = slowRotate;
        }
        else
        {
            currentSpeed = walkingSpeed;
            aiPath.rotationSpeed = normRotate;
        }

        aiPath.maxSpeed = currentSpeed;

    } 

    private void UpdateLineRenderer()
    {
        if (!aiPath.reachedDestination)
        {
            lineRenderer.enabled = selected;

            if (lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, aiTarget.position);
            }
        }
        else
        {
            lineRenderer.enabled = false;
        }
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
     
    public void SetSelected(bool val)
    {
        selected = val;
        //selectionBox.enabled = val;
        selectionLine.enabled = val;
    }
     
    private void CheckNearbyFormations()
    {
        listOfNearbyEnemies.Clear();
        LayerMask layerMask = LayerMask.GetMask("Formation");
        int maxColliders = 10;
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

        if (chaseDetectedEnemies)
        { 
            EngageFoe();
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

    private void FixRotation()
    {
        if (!aiPath.canMove   && obeyingMovementOrder && !tangledUp)
        {
            Vector3 targetDirection = rotTarget.position - transform.position;

            float singleStep = finishedPathRotSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            //Debug.DrawRay(transform.position, newDirection, Color.red);

            transform.rotation = Quaternion.LookRotation(newDirection);
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
