// Connected to the Cube and includes a DontDestroyOnLoad()
// LoadScene() is called by the first  script and switches to the second.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveInfo : MonoBehaviour
{
    private static bool created = false;

    public List<UnitScriptableObject> list = new List<UnitScriptableObject>();

    public UnitInformationScript unitPrefab;

    public List<UnitInformationScript> listOfSavedUnits = new List<UnitInformationScript>();
    public GameInitializer gameInit;

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
           //Debug.Log("Awake: " + this.gameObject);
            //GenerateModifiableScripObjsAsChildren();
        }
    }


    public void GenerateModifiableScripObjsAsChildren()
    {
        var i = 0;
        foreach (var item in list)
        {
            var newItem = Instantiate(unitPrefab);
            newItem.transform.parent = this.transform;
            newItem.name = item.name;
            newItem.models = item.models;
            newItem.morale = item.morale;
            newItem.energy = item.energy;

            newItem.maxModels = item.models;
            newItem.maxMorale = item.morale;
            newItem.maxEnergy = item.energy;
            newItem.placementID = i;

            listOfSavedUnits.Add(newItem);
            i++;
        }
    }

    public void SaveExistingPieceInfoInScripObjs()
    {
        Piece[] AllPieces = FindObjectsOfType<Piece>();
        List<Piece> listOfPiecesOnOurTeam = new List<Piece>();
        foreach (var piece in AllPieces)
        {
            if (piece.team == gameInit.board.ourTeamColor)
            {
                listOfPiecesOnOurTeam.Add(piece);
            }
        }

        var i = 0;
        foreach (var savedUnit in listOfSavedUnits)
        {
            if (i < listOfPiecesOnOurTeam.Count) //imagine there are 3 saved, and 2 actual. i will go up to 2 (0, 1, 2) and the count of actual is 2. if there were 2 on both i need to be less than the actual count
            {

                Piece boardPiece = listOfPiecesOnOurTeam[i];
                savedUnit.name = boardPiece.unitName;
                savedUnit.models = boardPiece.models;
                savedUnit.morale = boardPiece.morale;
                savedUnit.energy = boardPiece.energy;
                savedUnit.maxModels = boardPiece.startingModels;
                savedUnit.maxMorale = boardPiece.startingMorale;
                savedUnit.maxEnergy = boardPiece.startingEnergy;
                savedUnit.alreadyPlaced = false;
                savedUnit.placementID = i;
            }
            else
            {
                Destroy(savedUnit); //if exceed we should get rid of it. we're probably not going to have many situations where you are given new units . . . we'll cross that bridge.
            }
            i++;
        }

    }

    public void LoadScene()
    {
        if (SceneManager.GetActiveScene().name == "test")
        {
            SceneManager.LoadScene("test2", LoadSceneMode.Single);
        }
    }
}