using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class BattleGroupManager : MonoBehaviour
{

    public static BattleGroupManager Instance { get; private set; }

    public BattleGroup[] allBattleGroupsArray;
    public SupplyPoint[] allSupplyPointsArray;
    public LocaleInvestigatable[] allLocalesArray;
    public bool movementPaused = false;
    public float timeScale = 1;
    public float savedSliderValue = 1;
    public Slider timeSlider;
    public TMP_Text pauseText;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateBattleGroupArray();
        UpdateSupplyPointArray();
        UpdateLocaleArray();
        timeSlider.value = timeScale;
        savedSliderValue = timeSlider.value;
    }
    private void UpdateLocaleArray()
    { 
        LocaleInvestigatable[] array = FindObjectsOfType<LocaleInvestigatable>();
        allLocalesArray = array;
        Array.Sort(allLocalesArray, CompareLocaleNames); //consistent ordering alphabetical
    }
    private int CompareLocaleNames(LocaleInvestigatable x, LocaleInvestigatable y)
    {
        return x.name.CompareTo(y.name);
    }
    public void UpdateBattleGroupArray()
    {
        BattleGroup[] array = FindObjectsOfType<BattleGroup>();
        allBattleGroupsArray = array;
        Array.Sort(allBattleGroupsArray, CompareBattleGroupNames); //consistent ordering alphabetical
    }
    public void UpdateSupplyPointArray()
    {
        SupplyPoint[] array = FindObjectsOfType<SupplyPoint>();
        allSupplyPointsArray = array;
        Array.Sort(allSupplyPointsArray, CompareObNames); //consistent ordering alphabetical
    }
    private int CompareBattleGroupNames(BattleGroup x, BattleGroup y)
    {
        return x.name.CompareTo(y.name);
    }
    private int CompareObNames(SupplyPoint x, SupplyPoint y)
    {
        return x.name.CompareTo(y.name);
    }
    public void UpdateTimeScale()
    {
        savedSliderValue = timeSlider.value;
        if (!overworldForcePaused)
        {
            timeScale = savedSliderValue;
        }
    }
    public bool overworldForcePaused = false;
    public void ForcePause()
    {
        timeScale = 0;
        overworldForcePaused = true;
        pauseText.text = "Resume"; 
    }
    public void ForceUnpause()
    {
        timeScale = savedSliderValue;
        overworldForcePaused = false;
        pauseText.text = "Pause";
        timeScale = savedSliderValue;
    }
    public void TogglePause()
    {
        if (timeScale > 0)
        {
            ForcePause();
        }
        else
        {
            ForceUnpause();
        }
    }
    private void CheckIfEnemiesCanSeePlayer()
    {
        for (int i = 0; i < allBattleGroupsArray.Length; i++)
        {
            BattleGroup group = allBattleGroupsArray[i];
            if (group.controlledBy == BattleGroup.controlStatus.AIControlled)
            {
                float distance = Vector3.Distance(group.transform.position, OverworldManager.Instance.playerBattleGroup.transform.position); //get distance between enemy and group
                if (distance <= group.aiSightDistance) //check if distance less or equal than ai distance
                {
                    group.aiCanSeePlayer = true; 
                    //in the future compare our estimated strength vs player.
                }
                else
                { 
                    if (group.aiCanSeePlayer) //only update last known position if we saw them and now cannot
                    {
                        group.aiLastKnownPlayerPosition = OverworldManager.Instance.playerBattleGroup.transform.position; 
                    }
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
            group.UpdateSpeedBasedOnNumberOfUnits(timeScale);
        }
    }
    // Update is called once per frame
    void Update()
    {
        CheckIfEnemiesCanSeePlayer();
        CheckMovementStatus();
    }
}
