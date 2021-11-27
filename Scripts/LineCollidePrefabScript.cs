using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineCollidePrefabScript : MonoBehaviour //this script is attached to a cylinder
{

    public TileData tileData;

    public Piece parentPiece;

    //public Collider tileHit;

    public Board board;

    public Piece unitOnTile;

    public bool finishedProcessing = false;

    private void Start()
    {
        board = FindObjectOfType<SinglePlayerBoard>();
        if (board == null) //couldn't find a singleplayer board
        {
            board = FindObjectOfType<MultiplayerBoard>();
        }
    }

    void OnTriggerEnter(Collider otherObject) //when cylinder enters object

    {

        //print("Just entered the trigger defined by the object " + otherObject.gameObject.name);
        /*if(tileData != null)
        {
            Debug.Log("found a tile with tile data" + otherObject);
        }*/
        //we should add this object to a hashset which can only hold 1 of each item


        //and then we go through the list of tiles; using coordinates that we have already fed into the tile data at level generation,
        //check to see if there is a unit on that tile

    }
    private void OnTriggerStay(Collider other)
    {

        if (tileData == null)
        {

            tileData = other.GetComponent(typeof(TileData)) as TileData;
        }
        /*if (tileHit == null)
        {

            tileHit = other;
        }*/
        if (tileData != null && unitOnTile == null)
        {
            Vector2Int vector = new Vector2Int(tileData.x, tileData.y);
            unitOnTile = board.GetPieceOnSquare(vector);
            /*if (unitOnTile != null)
            { 
                GameObject eventTextPrefab = Instantiate(board.eventPrefab1, transform.position, Quaternion.Euler(90, transform.forward.y, 0));
                //eventTextPrefab.transform.position = num + new Vector3(0, 1.75f, 0);
                var text = eventTextPrefab.GetComponentInChildren<TMP_Text>();
                text.text = unitOnTile.ToString();
                var targetPosition = eventTextPrefab.transform.position + new Vector3(0, .4f, 0);
            }*/

        }

        if (tileData != null && finishedProcessing == false)
        {
            finishedProcessing = true;
        }


    }

    /*void OnTriggerExit(Collider otherObject)

    {

        print("Just exited the trigger defined by the object " + otherObject.gameObject.name);

    }*/
}
