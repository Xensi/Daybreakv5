using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //stores level after menu is left
    public static LevelManager Instance { get; private set; } 
    public enum Level
    {
        None, DayOfGlory
    }
    public Level currentLevel;
    private void Awake()
    {
        Instance = this;
    } 
    public void NoteLevel(int num)
    {
        currentLevel = (Level)num; //set level enum so we can remember it
        Debug.Log(currentLevel);
        if (currentLevel != Level.None)
        {
            Debug.Log("Starting level");
            OverworldToFieldBattleManager.Instance.StartFieldBattleFromLevel();
        }
    }
}
