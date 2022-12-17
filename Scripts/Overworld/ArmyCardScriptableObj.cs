using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "newArmyCard", menuName = "ArmyCard")]
public class ArmyCardScriptableObj : ScriptableObject
{
    public string cardName = "Conscript";
    public GlobalDefines.SoldierTypes cardType = GlobalDefines.SoldierTypes.conscript;
    public GlobalDefines.Team cardTeam = GlobalDefines.Team.Altgard;
    public Sprite cardIcon;
    public int cardTroops = 80;
    public int cardMaxTroops = 80;
    public Color cardColor;
    public int spoilsCost = 0;
    public float threatValuePerIndividual = 1;
}
