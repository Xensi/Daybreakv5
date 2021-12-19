using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "unitList", menuName = "UnitList")]
public class UnitListScriptableObject : ScriptableObject
{
    public List<UnitScriptableObject> unitList;
}
