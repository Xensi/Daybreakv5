using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class FormationPosition : MonoBehaviour
{
    public string team = "Altgard";
    public RichAI aiPath;
    [SerializeField] private float threshold = .5f;
    [SerializeField] private List<FormationPosition> listOfNearbyEnemies;
    [SerializeField] private AIDestinationSetter aiDesSet;
    public Transform aiTarget;
    public Transform rotTarget;
    [SerializeField] private float velThreshold = .1f;
    [SerializeField] private float checkRadius = 10;
    [SerializeField] private Collider rectangleCollider;
    [SerializeField] private FormationPosition enemyFormationToTarget;
    [SerializeField] private SoldierBlock soldierBlock;
    private bool resetTarget = true;
    public bool enableAnimations = false;
    private bool oldEnableAnimations = false;

    [SerializeField] private float stoppingDistance = 10;
    [SerializeField] private float moveStopDistance = 1;

    [SerializeField] private float currentSpeed = 0;
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
    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, 4);
    }
    private void Start()
    {
        //InvokeRepeating("CheckNearbyFormations", 1f, 1f);
        //InvokeRepeating("CheckNearbyModels", 1f, 1f);
        currentSpeed = walkingSpeed;
        aiPath.maxSpeed = currentSpeed;
        InvokeRepeating("UpdateSoldiers", 1f, 1f);
        InvokeRepeating("FastUpdateSoldiers", .05f, .05f);
        InvokeRepeating("FixRotation", .05f, .05f);

    } 
    private void Update()
    {
        SlowDown();
        OffsetPositions();

        UpdateLineRenderer();
        CatchDeployEvents();
        VeryFastUpdateSoldiers(); 
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
    private void UpdateSoldiers()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.CullAnimations();
            }
        }
    }
    private void FastUpdateSoldiers()
    {
        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.AnimatorUpdate();
                model.FixRotation(); //make it so this doesn't do anything if rotation is good
            }
        }
    }
    private void VeryFastUpdateSoldiers()
    {

        foreach (SoldierModel model in soldierBlock.listSoldierModels)
        {
            if (model.alive)
            {
                model.UpdateSpeed(); 
            }
        }
    }
    private void SlowDown()
    {
        if (soldierBlock.arbiter == null)
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
        }
        else
        {
            aiPath.maxSpeed = currentSpeed;
        }

    }

    public void CheckDirectionOfMovement()
    {
        compass.LookAt(aiTarget);
        float rot =  compass.localEulerAngles.y;
        float threshold = 10;
        Debug.LogError(rot);
        if (-threshold <= rot && rot <= threshold)
        {
            Debug.LogError("front");
            aiPath.enableRotation = false;
            //transform.LookAt(aiTarget); 
            //highParent.rotation = Quaternion.LookRotation(highParent.position - aiTarget.position);
        }
        else if (90- threshold <= rot && rot <= 90+ threshold)
        {
            Debug.LogError("Side");
            aiPath.enableRotation = false;

            transform.localRotation = Quaternion.Euler(0, 0, 0);
            highParent.localRotation = Quaternion.Euler(0, -90, 0);
        }
        else if (270 - threshold <= rot && rot <= 270 + threshold)
        {
            Debug.LogError("Side");
            aiPath.enableRotation = false;
            //transform.LookAt(aiTarget);
            //highParent.rotation = Quaternion.LookRotation(highParent.position - aiTarget.position);
        }
        else if (180 - threshold <= rot && rot <= 180 + threshold)
        {
            Debug.LogError("back"); 
            aiPath.enableRotation = false;
            //transform.LookAt(aiTarget);
            //highParent.rotation = Quaternion.LookRotation(highParent.position - aiTarget.position);
        }
        else
        {
            aiPath.enableRotation = true;
        }
    }

    private void OffsetPositions()
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

    private void DeployEvent(bool sprinting)
    {
        if (sprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkingSpeed;
        }
        aiPath.maxSpeed = currentSpeed;
        foreach (SoldierModel soldier in soldierBlock.listSoldierModels)
        {
            soldier.SetSpeed(sprinting);
        }
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
        if (aiPath.reachedDestination) //if we've reached destination
        {
            aiPath.canMove = false;
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius, layerMask, QueryTriggerInteraction.Ignore);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == rectangleCollider)
            {
                continue;
            }
            FormationPosition form = hitCollider.gameObject.GetComponent<FormationPosition>();
            if (form.team == team)
            {
                continue;
            }

            listOfNearbyEnemies.Add(form);

            //Debug.LogError("Detected other collider nearby" + hitCollider.name);
        }

        FindClosestFormation(); 
    }

    private void FindClosestFormation()
    {
        if (listOfNearbyEnemies.Count <= 0)
        {
            enemyFormationToTarget = null;
            return;
        }
        if (listOfNearbyEnemies.Count == 1)
        {
            enemyFormationToTarget = listOfNearbyEnemies[0];
        }
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
        if (enemyFormationToTarget == null) //no enemy
        {
            aiPath.endReachedDistance = moveStopDistance;
            aiPath.maxSpeed = currentSpeed;
            weaponsDeployed = false;
            return;
        }
        else //yes enemy
        {
            aiPath.endReachedDistance = stoppingDistance;
            aiPath.maxSpeed = sprintSpeed;
            weaponsDeployed = true;
        }

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
        if (!aiPath.canMove)
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
