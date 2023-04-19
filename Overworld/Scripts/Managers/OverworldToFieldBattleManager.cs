using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldToFieldBattleManager : MonoBehaviour
{ 
    public static OverworldToFieldBattleManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private GameObject FieldBattleParent;
    [SerializeField] private GameObject OverworldParent;
    //[SerializeField] private GameObject MenuParent;
    [SerializeField] private GameObject PauseParent;


    private bool paused = false;

    public enum possibleGameStates
    {
        Menu,
        Overworld,
        FieldBattle
    }
    public possibleGameStates state = possibleGameStates.FieldBattle;

    // Start is called before the first frame update
    void Start()
    {
        UnpauseGame();
        //StartFieldBattle();
    }

    private void Update()
    {
        switch (state)
        {
            case possibleGameStates.Menu:
                break;
            case possibleGameStates.FieldBattle:
            case possibleGameStates.Overworld:
                if (Input.GetKeyDown("escape"))
                {
                    if (paused)
                    {
                        UnpauseGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
                break;
            default:
                break;
        } 
    }
    void PauseGame()
    {
        if (state == possibleGameStates.Overworld)
        {
            OverworldManager.Instance.fleeBattleButton.interactable = false;
        }
        else
        {
            OverworldManager.Instance.fleeBattleButton.interactable = true;
        }
        Time.timeScale = 0;
        paused = true;
        PauseParent.SetActive(true);
        MusicManager.Instance.PauseMusic();
    }
    public void UnpauseGame()
    {
        Time.timeScale = 1;
        paused = false;
        PauseParent.SetActive(false);
        MusicManager.Instance.UnpauseMusic();
    }
    public void StartFieldBattleWithEnemyBattleGroup(BattleGroup enemyBattleGroup)
    {
        Debug.Log("Starting field battle");
        OverworldManager.Instance.enemyBattleGroup = enemyBattleGroup;
        UnitManager.Instance.unitsInEnemyArmyList = enemyBattleGroup.listOfUnitsInThisArmy;
        state = possibleGameStates.FieldBattle;
        OverworldParent.SetActive(false);
        FieldBattleParent.SetActive(true);
        ResetCamera();
        LoadScenario();
        AllowPlacementOfPlayerTroops(); //places player's troops randomly
    }
    public void StartFieldBattleFromLevel() //starting a battle that was loaded from preset level setup
    { 
        state = possibleGameStates.FieldBattle;
        OverworldParent.SetActive(false);
        FieldBattleParent.SetActive(true);
        ResetCamera();
        LoadScenario();
        Invoke("AllowPlacementOfPlayerTroops", 1);
        //AllowPlacementOfPlayerTroops(); //places player's troops randomly
    }
    private void AllowPlacementOfPlayerTroops()
    {
        if (LevelManager.Instance.currentLevel != LevelManager.Level.None)
        {
            FightManager.Instance.StartPlacingSoldiersFromLevel(); 
        }
        else //overworld
        { 
            FightManager.Instance.StartPlacingSoldiers();
        }
    }
    private void ResetCamera()
    {
        CinemachineShake.Instance.HaltShakes();
        FightManager.Instance.virtualCamTransform.SetPositionAndRotation(new Vector3(0, 50, -90), Quaternion.Euler(0, 0, 0));
    }
    public void EndFieldBattle()
    {
        EraseAllTroopsAndClearArrays();
        state = possibleGameStates.Overworld;
        BattleGroupManager.Instance.ForcePause();
        FieldBattleParent.SetActive(false);
        OverworldParent.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;

        if (FightManager.Instance.victorBattleGroup != null) //re-enable allowedToStartCombat for victor
        {
            FightManager.Instance.victorBattleGroup.allowedToStartCombat = true;
        }
        if (FightManager.Instance.loserBattleGroup != null) //
        { 
            FightManager.Instance.loserBattleGroup.BeginCombatDefeatTimer();
        }
    } 
    public void UpdateUnitManagerArmies()
    {
        //UnitManager.Instance.UpdateArmy();
        //update the player's battlegroup using the player's formations
        if (OverworldManager.Instance.playerBattleGroup != null)
        { 
            UnitManager.Instance.UpdateBattleGroupWithFormation(OverworldManager.Instance.playerBattleGroup, FightManager.Instance.playerControlledFormations);
        }
        //also update the opponent's army
        if (OverworldManager.Instance.enemyBattleGroup != null) 
        { 
            UnitManager.Instance.UpdateBattleGroupWithFormation(OverworldManager.Instance.enemyBattleGroup, FightManager.Instance.enemyControlledFormations);
        } 
    }
    private void EraseAllTroopsAndClearArrays()
    {
        for (int i = 0; i < FightManager.Instance.allArray.Length; i++)
        {
            if (FightManager.Instance.allArray[i] != null)
            { 
                if (FightManager.Instance.allArray[i].soldierBlock != null)
                {
                    Destroy(FightManager.Instance.allArray[i].soldierBlock.gameObject);
                }
            }
        }
        FightManager.Instance.allArray = new FormationPosition[0];
        FightManager.Instance.playerControlledFormations.Clear();
        FightManager.Instance.enemyControlledFormations.Clear();
        
    }
    private void LoadScenario()
    {
        FightManager.Instance.LoadScenario();
    } 
}
