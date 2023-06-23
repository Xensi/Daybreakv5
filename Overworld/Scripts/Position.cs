using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
using System.Threading.Tasks;
using System.Threading;
using Unity.Jobs;
using Unity.Collections;
public class Position : MonoBehaviour
{
    public SoldierModel assignedSoldierModel;
    public Row row;
     
    [SerializeField] private List<SoldierModel> candidates;

    [SerializeField] private int numTimesSought = 0;
     
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;
    public FormationPosition formPos;

    public bool activeController = false;

    private void Start()
    {
        PlaceOnGround();
    }
    void Awake()
    {
        _raycastCommands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
        _raycastHits = new NativeArray<RaycastHit>(1, Allocator.Persistent);
    }
    private void OnDestroy()
    {
        _jobHandle.Complete();
        _raycastCommands.Dispose();
        _raycastHits.Dispose();
    }

    private NativeArray<RaycastCommand> _raycastCommands;
    private NativeArray<RaycastHit> _raycastHits;
    private JobHandle _jobHandle;

    public void PlaceOnGround()
    {
        // 1. Process raycast from last frame
        _jobHandle.Complete();
        RaycastHit raycastHit = _raycastHits[0];
        bool didHitYa = raycastHit.collider != null; 
        if (didHitYa)
        {
            transform.position = new Vector3(transform.position.x, raycastHit.point.y, transform.position.z);
        }

        LayerMask layerMask = LayerMask.GetMask("Terrain");
        Vector3 origin = new Vector3(transform.position.x, 100, transform.position.z);
        // 2. Schedule new raycast
        _raycastCommands[0] = new RaycastCommand(origin, Vector3.down, Mathf.Infinity, layerMask, 1);
        _jobHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, 1);
    
    /*// Perform a single raycast using RaycastCommand and wait for it to complete
    // Setup the command and result buffers
    var results = new NativeArray<RaycastHit>(1, Allocator.Temp);

    var commands = new NativeArray<RaycastCommand>(1, Allocator.Temp);

    // Set the data of the first command

    Vector3 direction = Vector3.down;

    commands[0] = new RaycastCommand(origin, direction, Mathf.Infinity, layerMask, 1);

    // Schedule the batch of raycasts
    JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));

    // Wait for the batch processing job to complete
    handle.Complete();

    // Copy the result. If batchedHit.collider is null there was no hit
    RaycastHit batchedHit = results[0];
    if (batchedHit.collider != null)
    { 
        transform.position = batchedHit.point;
    }

    // Dispose the buffers
    results.Dispose();
    commands.Dispose(); */
    }


    public void SeekReplacement(float range = 5f)
    {
        if (assignedSoldierModel != null)
        { 
            if (assignedSoldierModel.alive)
            {
                return;
            }
        }
        //safety check
        //first get a row behind us
        int behindUs = row.rowPositionInList + 1 + numTimesSought;
        if (behindUs == formPos.soldierBlock.rows.Count)
        {
            //out of bounds 
            return;
        }
        //get candidates
        candidates.Clear();
        //instead of this, just select a suitable replacement from a row that is behind us if possible 
        Row desiredRow = row.soldierBlock.rows[behindUs];
        foreach (Position item in desiredRow.positionsInRow)
        {
            if (item.assignedSoldierModel != null)
            {
                candidates.Add(item.assignedSoldierModel);
            }
        }    
        if (candidates.Count > 0)
        {
            GetClosestReplacement();
        }
        else
        { 
            numTimesSought++;
            //SeekReplacement(); //keep going until out of bounds?
        } 
    }

    private void GetClosestReplacement()
    {
        if (assignedSoldierModel != null)
        {
            if (assignedSoldierModel.alive)
            {
                return;
            }
        }
        //safety
        if (candidates.Count <= 0) //if no candidates
        { 
            return;
        }
        SoldierModel model = candidates[0];
        if (model != null)
        {
            if (candidates.Count == 1) //only 1 candidate, so let's do it
            {
                assignedSoldierModel = model;
                UpdateModelPosition();
            }
            else //multiple candidates
            {
                assignedSoldierModel = model; //set for now
                if (candidates.Contains(assignedSoldierModel))
                { 
                    candidates.Remove(assignedSoldierModel);
                    //Check if others are closer
                    float compareDist = Vector3.Distance(transform.position, assignedSoldierModel.transform.position);
                    foreach (SoldierModel modelCandidate in candidates) 
                    {
                        float dist = Vector3.Distance(assignedSoldierModel.transform.position, modelCandidate.transform.position); 
                        if (dist < compareDist)
                        { 
                            assignedSoldierModel = modelCandidate;
                            compareDist = dist;
                        }
                    }
                    UpdateModelPosition();
                } 
            }
        } 
    }

    private void UpdateModelPosition()
    { 
        if (assignedSoldierModel != null)
        {  
            assignedSoldierModel.target = transform; //it will go here
            if (assignedSoldierModel.modelPosition != null)
            { 
                assignedSoldierModel.modelPosition.assignedSoldierModel = null; //remove from original position
            }
            assignedSoldierModel.modelPosition = this; //update its assigned position 
            row.UpdateModelsInRow();


            numTimesSought = 0;
        } 
    } 
}
