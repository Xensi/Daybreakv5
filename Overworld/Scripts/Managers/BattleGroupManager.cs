using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGroupManager : MonoBehaviour
{

    public static BattleGroupManager Instance { get; private set; }

    public BattleGroup[] allBattleGroupsArray;
    public bool movementPaused = false;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateBattleGroupArray();
    }

    public void UpdateBattleGroupArray()
    {
        BattleGroup[] array = FindObjectsOfType<BattleGroup>();
        allBattleGroupsArray = array;
    }
    private void CheckIfEnemiesCanSeePlayer()
    {
        for (int i = 0; i < allBattleGroupsArray.Length; i++)
        {
            BattleGroup group = allBattleGroupsArray[i];
            if (group.controlledBy == BattleGroup.controlStatus.EnemyControlled)
            {
                float distance = Vector3.Distance(group.transform.position, OverworldManager.Instance.playerBattleGroup.transform.position); //get distance between enemy and group
                if (distance <= group.aiSightDistance) //check if distance less or equal than ai distance
                {
                    group.aiCanSeePlayer = true;
                    group.aiTarget.position = OverworldManager.Instance.playerBattleGroup.transform.position; 
                    //in the future compare our estimated strength vs player.
                }
                else
                { 
                    group.aiCanSeePlayer = false;
                } 
            }
        }
    }
    private void CheckMovementStatus()
    { 
        for (int i = 0; i < allBattleGroupsArray.Length; i++)
        {
            BattleGroup group = allBattleGroupsArray[i];
            group.pathfindingAI.canMove = !movementPaused;
        }
    }
    // Update is called once per frame
    void Update()
    {
        CheckIfEnemiesCanSeePlayer();
        CheckMovementStatus();
    }
}
