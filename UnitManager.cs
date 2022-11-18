using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; } 

    public List<GlobalDefines.SoldierTypes> unitTypes;
    public List<SoldierBlock> formationsToInstantiateBasedOnUnitType;

    public List<ArmyCardScriptableObj> cardsToInstantiateBasedOnUnitType;

    public List<UnitInfoClass> unitsInMainArmyList;
    public List<UnitInfoClass> unitsInTestArmyList;
    public List<UnitInfoClass> unitsInEnemyArmyList;

    public ArmyCard armyCardPrefab;


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    } 
    public void UpdateArmy()
    {
        unitsInMainArmyList.Clear();
        for (int i = 0; i < FightManager.Instance.playerControlledFormations.Count; i++)
        {
            if (FightManager.Instance.playerControlledFormations[i] != null){ 
                unitsInMainArmyList.Add(ConvertFormationToUnitInfoClass(FightManager.Instance.playerControlledFormations[i]));
            }
        } 
    }
    public void UpdateBattleGroupWithFormation(BattleGroup battleGroup, List<FormationPosition> listOfFormationPositions)
    {
        battleGroup.listOfUnitsInThisArmy.Clear();
        for (int i = 0; i < listOfFormationPositions.Count; i++)
        {
            if (listOfFormationPositions[i] != null)
            {
                battleGroup.listOfUnitsInThisArmy.Add(ConvertFormationToUnitInfoClass(listOfFormationPositions[i]));
            }
        }
    }
    private UnitInfoClass ConvertFormationToUnitInfoClass(FormationPosition form)
    {
        UnitInfoClass unit = new UnitInfoClass();
        unit.type = form.soldierType;
        unit.team = form.team;
        unit.troops = form.numberOfAliveSoldiers;
        //unit.maxTroops = form.maxSoldiers;
        return unit;
    }
}
