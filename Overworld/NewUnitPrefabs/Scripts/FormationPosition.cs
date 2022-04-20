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
    [SerializeField] private SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;

    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;

    [SerializeField] private float currentSpeed = 0;
    [SerializeField] private float slowSpeed = .5f;
    [SerializeField] private float walkingSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6.5f;

    public bool selected = false;
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

    public float numberOfAliveSoldiers = 80;
    [SerializeField] private float maxSoldiers = 80;

    public bool tangledUp = false;
    [SerializeField] private float slowRotate = 15;

    [SerializeField] private float normRotate = 30;
    [SerializeField] private Transform offsetSecondRow;

    private float colliderBoxRange = 9;
    [SerializeField] private float xsize = 10;
    [SerializeField] private float ysize = 4;
    [SerializeField] private float zsize = 8;

    [SerializeField] private float xoffset = 0;
    [SerializeField] private float yoffset = 2;
    [SerializeField] private float zoffset = 1;
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
    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, 4);
    }
    private void Start()
    {
        currentSpeed = walkingSpeed;
        aiPath.maxSpeed = currentSpeed;
        InvokeRepeating("VerySlowUpdate", 1f, 1f);  
        InvokeRepeating("SlowUpdate", .5f, .5f); //normally .05f
        InvokeRepeating("FastUpdate", .1f, .1f);

        colliderBoxRange = soldierBlock.modelAttackRange * 2;

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
        if (listOfNearbyEnemies.Count > 0)
        {
            offsetSecondRow.localPosition = new Vector3(-.5f,0,.5f);

            int num = 0;
            if (numberOfAliveSoldiers >= 71) //80
            {
                num = 0;
            }
            else if (numberOfAliveSoldiers >= 61) //70
            {
                num = 1;
            }
            else if (numberOfAliveSoldiers >= 51) //70
            {
                num = 2;
            }
            else if (numberOfAliveSoldiers >= 41) //70
            {
                num = 3;
            }
            else if (numberOfAliveSoldiers >= 31) //70
            {
                num = 4;
            }
            else if (numberOfAliveSoldiers >= 21) //70
            {
                num = 5;
            }
            else if (numberOfAliveSoldiers >= 11) //70
            {
                num = 6;
            }
            else if (numberOfAliveSoldiers >= 1) //70
            {
                num = 7;
            }
            rectangleCollider.center = new Vector3(xoffset, yoffset, zoffset + .5f * num); //originally .5 7
            rectangleCollider.size = new Vector3(xsize, ysize, colliderBoxRange - 1 * num);

        }
        else
        {
            offsetSecondRow.localPosition = new Vector3(0, 0, 0);
            tangledUp = false;
            rectangleCollider.center = new Vector3(xoffset, yoffset, zoffset-1); //originally .5 7
            rectangleCollider.size = new Vector3(xsize, ysize, colliderBoxRange-1);
        }
        CheckNearbyFormations();
        UpdateSoldiers();
    }
    private void SlowUpdate()
    {
        UpdateAttackTimers();
        FixRotation();
    }
    private void UpdateAttackTimers()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.UpdateAttackTimer();
                model.TryToAttackEnemy();
            }
        }
    }
    private void FastUpdate()
    {
        //SlowDown();
        OffsetPositions();
        //CatchDeployEvents();
        //AdjustToHitEnemy();
        UpdateLineRenderer();
    }
    private void UpdateSoldiers()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.CullAnimations();
                model.CheckIfEnemyModelsNearby();
                model.CheckIfIdle();
                model.UpdateRow();

                model.AnimatorUpdate();
                model.FixRotation(); //make it so this doesn't do anything if rotation is good
                model.UpdateSpeed();
            }
        }
        foreach (Position position in soldierBlock.reinforceablePositions)
        {
            if (position.assignedSoldierModel == null)
            {
                position.SeekReplacement();
            }
        }
    }
    private void UpdateSpeed()
    {

        if (enemyFormationToTarget == null) //no enemy
        {
            aiPath.endReachedDistance = moveStopDistance;
            //aiPath.maxSpeed = currentSpeed;
            weaponsDeployed = false;
        }
        else //yes enemy
        {
            aiPath.endReachedDistance = stoppingDistance;
            //aiPath.maxSpeed = sprintSpeed;
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
    private void AdjustToHitEnemy() //fix this
    {
        if (enemyFormationToTarget != null && !obeyingMovementOrder)// 
        {

            float dist = GetDistance(transform, enemyFormationToTarget.gameObject.transform);
            if (dist <= stoppingDistance && modelAttacking) //stop
            {
                aiTarget.transform.position = transform.position;
                rotTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
            }
            else
            {
                aiTarget.transform.position = enemyFormationToTarget.gameObject.transform.position;
            }
        }
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
    private void CatchDeployEvents()
    {
        if (weaponsDeployed != oldWeaponsDeployed)
        {
            oldWeaponsDeployed = weaponsDeployed;
            DeployEvent(weaponsDeployed);
        }
    }
    private void SlowDown()
    {
        /*if (soldierBlock.arbiter == null)
        {
            return;
        }
        if (!soldierBlock.arbiter.aiPath.hasPath)
        {
            return;
        }
        if (soldierBlock.arbiter.aiPath.remainingDistance > waitThreshold)
        {
            aiPath.maxSpeed = currentSpeed - soldierBlock.arbiter.aiPath.remainingDistance;
            if (aiPath.maxSpeed <= slowSpeed)
            {
                aiPath.maxSpeed = slowSpeed;
            }
        }
        else
        {
            aiPath.maxSpeed = currentSpeed;
        }*/

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

    private void DeployEvent(bool sprinting)
    {
        /*if (sprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkingSpeed;
        }*/
        //aiPath.maxSpeed = currentSpeed;
        /*foreach (Position position in frontlinePositions) //only deploy spears for frontliners
        {
            position.assignedSoldierModel.SetSpeed(sprinting);
        }*/
    }
    public void SetSelected(bool val)
    {
        selected = val;
        selectionBox.enabled = val;
    }

    private void FixedUpdate()
    {
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
            //Debug.LogError("Animate" + enableAnimations);
            foreach (SoldierModel item in soldierBlock.listSoldierModels)
            {
                item.animate = enableAnimations;
            }
        }
    }

    private bool CheckIfVelocityUnderThreshold(float threshold)
    {
        Vector3 vel = aiPath.velocity;
        if (vel.x < threshold || vel.z < threshold)
        {
            return true;
        }
        return false;

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
        EngageFoe();

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
