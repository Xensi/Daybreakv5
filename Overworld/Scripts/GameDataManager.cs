// Add System.IO to work with files!
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    // Create a field for the save file.
    public string saveFile;

    // Create a GameData field.
    public GameData gameData = new GameData();

    void Awake()
    {
        Instance = this;
        // Update the path once the persistent path exists.
        saveFile = Application.persistentDataPath + "/gamedata.json";

    }

    public void readFile() //load
    {
        // Does the file exist?
        if (File.Exists(saveFile))
        {
            // Read the entire file and save its contents.
            string fileContents = File.ReadAllText(saveFile);

            // Deserialize the JSON data 
            //  into a pattern matching the GameData class.
            gameData = JsonUtility.FromJson<GameData>(fileContents);
            ApplyGameData();
        }
    }
    private void ApplyGameData() //use game data to set values in game
    { 
        for (int i = 0; i < BattleGroupManager.Instance.allSupplyPointsArray.Length; i++)
        {
            UpdateExistingSupplyPointWithClass(BattleGroupManager.Instance.allSupplyPointsArray[i], gameData.supplyPoints[i]);
        }
        for (int i = 0; i < BattleGroupManager.Instance.allBattleGroupsArray.Length; i++)
        {
            UpdateExistingBattleGroupWithClass(BattleGroupManager.Instance.allBattleGroupsArray[i], gameData.battleGroups[i]);
        }
        for (int i = 0; i < BattleGroupManager.Instance.allLocalesArray.Length; i++)
        {
            UpdateExistingLocaleWithClass(BattleGroupManager.Instance.allLocalesArray[i], gameData.locales[i]);
        }
        OverworldToFieldBattleManager.Instance.UnpauseGame();
        BattleGroupManager.Instance.ForcePause();
    }
    private void UpdateExistingLocaleWithClass(LocaleInvestigatable locale, LocaleClass localeClass)
    {
        locale.investigated = localeClass.investigated; 
    }
    private void UpdateExistingSupplyPointWithClass(SupplyPoint point, SupplyPointClass pointClass)
    {
        point.storedSupplies = pointClass.supplies;
    }
    private void UpdateExistingBattleGroupWithClass(BattleGroup battleGroup, BattleGroupClass battleGroupClass)
    {
        battleGroup.transform.localPosition = battleGroupClass.localPosition;
        battleGroup.listOfUnitsInThisArmy = battleGroupClass.unitClassList;
        battleGroup.aiTarget.transform.localPosition = battleGroupClass.aiTargetLocalPosition;
    }
    public void UpdateGameData() //tell game data what values to save
    {  
        gameData.supplyPoints.Clear();
        for (int i = 0; i < BattleGroupManager.Instance.allSupplyPointsArray.Length; i++)
        {
            gameData.supplyPoints.Add(ConvertSupplyPointObjectToClass(BattleGroupManager.Instance.allSupplyPointsArray[i]));
        }
        gameData.battleGroups.Clear();
        for (int i = 0; i < BattleGroupManager.Instance.allBattleGroupsArray.Length; i++)
        {
            gameData.battleGroups.Add(ConvertBattleGroupObjectToClass(BattleGroupManager.Instance.allBattleGroupsArray[i]));
        }
        gameData.locales.Clear();
        for (int i = 0; i < BattleGroupManager.Instance.allLocalesArray.Length; i++)
        {
            gameData.locales.Add(ConvertLocaleObjectToClass(BattleGroupManager.Instance.allLocalesArray[i]));
        }
    }
    private BattleGroupClass ConvertBattleGroupObjectToClass(BattleGroup battleGroup)
    {
        BattleGroupClass battleGroupClass = new BattleGroupClass();
        battleGroupClass.localPosition = battleGroup.transform.localPosition;
        battleGroupClass.unitClassList = battleGroup.listOfUnitsInThisArmy;
        battleGroupClass.aiTargetLocalPosition = battleGroup.aiTarget.transform.localPosition;
        return battleGroupClass;
    }
    private SupplyPointClass ConvertSupplyPointObjectToClass(SupplyPoint point)
    {
        SupplyPointClass pointClass = new SupplyPointClass();
        pointClass.supplies = point.storedSupplies;
        return pointClass;
    }
    private LocaleClass ConvertLocaleObjectToClass(LocaleInvestigatable locale)
    {
        LocaleClass localeClass = new LocaleClass();
        localeClass.investigated = locale.investigated;
        return localeClass;
    }
    public void writeFile() //save
    {
        UpdateGameData();


        // Serialize the object into JSON and save string.
        string jsonString = JsonUtility.ToJson(gameData);

        // Write JSON to file.
        File.WriteAllText(saveFile, jsonString);
    }
}