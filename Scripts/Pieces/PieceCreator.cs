using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
public class PieceCreator : MonoBehaviour
{
    [SerializeField] private GameObject[] piecesPrefabs;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material whiteMaterial;

    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach(var piece in piecesPrefabs)
        {
            //nameToPieceDict.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
            nameToPieceDict.Add(piece.GetComponent<Piece>().unitName, piece);
        }
        /*foreach (var item in nameToPieceDict)
        {
            Debug.Log(item);
        }*/
    }

    public GameObject CreatePiece(string typeName)
    {
        GameObject prefab = nameToPieceDict[typeName];
        if (prefab)
        {
            GameObject newPiece = Instantiate(prefab);
            //GameObject newPiece = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            //Debug.Log("Photon network instantiate");
            return newPiece;
        }
        return null;
    }
    public Material GetTeamMaterial(TeamColor team)
    {
        return team == TeamColor.White ? whiteMaterial : blackMaterial;
    }




}
