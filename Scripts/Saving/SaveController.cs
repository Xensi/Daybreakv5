using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public void SaveUnit(Piece piece)
    {
        SaveSystem.SaveUnit(piece);
    }

    public void LoadPlayer()
    {
        UnitStats data = SaveSystem.LoadUnit();

        var name = data.name;
        var models = data.models;
        var morale = data.morale;
        var energy = data.energy;


    }

}
