using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class UnitInfoClass
{ 
    public GlobalDefines.SoldierTypes type = GlobalDefines.SoldierTypes.conscript;
    public GlobalDefines.Team team;
    public int troops = 80;
    [HideInInspector] public int maxTroops = 80;  
}
