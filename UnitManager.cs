using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; } 

    public List<GlobalDefines.SoldierTypes> unitTypes;
    public List<SoldierBlock> formationsToInstantiateBasedOnUnitType;

    public List<UnitInfoClass> unitsInMainArmyList;
    public List<UnitInfoClass> unitsInTestArmyList;


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    } 
    public void UpdateArmy()
    {
        unitsInMainArmyList.Clear();
        for (int i = 0; i < FightManager.Instance.yourFormations.Count; i++)
        {
            if (FightManager.Instance.yourFormations[i] != null){ 
                unitsInMainArmyList.Add(ConvertFormationToUnitInfoClass(FightManager.Instance.yourFormations[i]));
            }
        } 
    }
    private UnitInfoClass ConvertFormationToUnitInfoClass(FormationPosition form)
    {
        UnitInfoClass unit = new UnitInfoClass();
        unit.type = form.soldierType;
        unit.team = form.team;
        unit.troops = form.numberOfAliveSoldiers;
        unit.maxTroops = form.maxSoldiers;
        return unit;
    }
}
