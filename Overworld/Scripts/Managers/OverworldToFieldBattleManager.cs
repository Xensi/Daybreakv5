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
    [SerializeField] private GameObject MenuParent;
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
        Time.timeScale = 0;
        paused = true;
        PauseParent.SetActive(true);
    }
    void UnpauseGame()
    {
        Time.timeScale = 1;
        paused = false;
        PauseParent.SetActive(false);
    }
    public void StartFieldBattle()
    {
        state = possibleGameStates.FieldBattle;
        OverworldParent.SetActive(false);
        FieldBattleParent.SetActive(true);
        //MenuParent.SetActive(false);
        LoadScenario(); 
        AllowPlacementOfPlayerTroops();
    }
    public void EndFieldBattle()
    {
        EraseAllTroopsAndClearArrays();
        state = possibleGameStates.Overworld;
        FieldBattleParent.SetActive(false);
        OverworldParent.SetActive(true);
    } 
    public void UpdateUnitManagerArmies()
    { 
        UnitManager.Instance.UpdateArmy();
    }
    private void EraseAllTroopsAndClearArrays()
    {
        for (int i = 0; i < FightManager.Instance.allArray.Length; i++)
        {
            Destroy(FightManager.Instance.allArray[i].soldierBlock.gameObject);
        }
        FightManager.Instance.allArray = new FormationPosition[0];
        FightManager.Instance.yourFormations.Clear();
        FightManager.Instance.aiFormations.Clear();
    }
    private void LoadScenario()
    {
        FightManager.Instance.LoadScenario();
    } 
    private void AllowPlacementOfPlayerTroops()
    {
        FightManager.Instance.StartPlacingSoldiers();
    }
}
