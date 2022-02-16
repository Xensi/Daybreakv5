using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitStats
{
    public string name;
    public float models;
    public float morale;
    public float energy;

    public UnitStats (Piece piece)
    {
        name = piece.name;
        models = piece.models;
        morale = piece.morale;
        energy = piece.energy;
    }

}
