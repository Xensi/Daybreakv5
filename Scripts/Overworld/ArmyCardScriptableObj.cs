using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "newArmyCard", menuName = "ArmyCard")]
public class ArmyCardScriptableObj : ScriptableObject
{
    public string cardName;
    public Sprite cardIcon;
    public int cardTroops;
    public int cardMaxTroops;
    public Color cardColor;
}
