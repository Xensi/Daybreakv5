using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BattleGroupClass
{
    public Vector3 localPosition;
    public Vector3 aiTargetLocalPosition;
    public List<UnitInfoClass> unitClassList;
}