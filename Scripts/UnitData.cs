using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData //needs to be able to save multiple units
{
    /*public List<string> namesList;
    public List<int> modelsList;
    public List<float> moraleList;
    public List<float> energyList;

    public UnitData(List<Piece> pieces)
    {
        foreach (var piece in pieces)
        {

            namesList.Add(piece.unitName);
            modelsList.Add(piece.models);
            moraleList.Add(piece.morale);
            energyList.Add(piece.energy);

        }
    }*/

    public string name;
    public float models;
    public float morale;
    public float energy;

    public UnitData(Piece piece)
    {
        name = piece.unitName;
        models = piece.models;
        morale = piece.morale;
        energy = piece.energy;
    }

}
