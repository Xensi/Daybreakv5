using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class savetester : MonoBehaviour
{
    public List<Piece> piecesToSave;
    public SaverSystem saver;
    //public Piece piece;

    public void Start()
    {
        //SaverSystem.pieces = piecesToSave;
    }

    public void SaveArmy()
    {
        Debug.Log("Saving army");
        //SaverSystem.SaveUnit(piece);
    }

    public void LoadArmy()
    {
        /*foreach (var item in data.namesList)
        {
            Debug.Log(item);
        }
        foreach (var item in data.modelsList)
        {
            Debug.Log(item);
        }
        foreach (var item in data.moraleList)
        {
            Debug.Log(item);
        }
        foreach (var item in data.energyList)
        {
            Debug.Log(item);
        }*/
    }
}
