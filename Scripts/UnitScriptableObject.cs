using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newUnit", menuName = "Unit")]
public class UnitScriptableObject : ScriptableObject
{ //only includes critical data or data that should be carried over between battles
    public new string name; //needed to spawn in the correct prefab
    public float models;
    public float morale;
    public float energy;
    

}
