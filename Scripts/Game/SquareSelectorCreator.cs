using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareSelectorCreator : MonoBehaviour
{

    [SerializeField] private Material freeSquareMaterial;
    [SerializeField] private Material opponentSquareMaterial;
    [SerializeField] private GameObject selectorPrefab;
    [SerializeField] private Board board;

    private List<GameObject> instantiatedSelectors = new List<GameObject>();

    private GameInitializer gameInit;

    private void Awake()
    {
        if (gameInit == null)
        {
            var game = GameObject.Find("GameInitializer");
            gameInit = game.GetComponent(typeof(GameInitializer)) as GameInitializer;
        }
    }
    public void ShowSelection(Dictionary<Vector3, bool> squareData)
    {
        ClearSelection();
        foreach(var data in squareData)
        {
            GameObject selector = Instantiate(selectorPrefab, data.Key, Quaternion.identity);
            instantiatedSelectors.Add(selector);
            //board.selectedPiece
            Vector3 temp = new Vector3 (0, 0.01f, 0);
            selector.transform.position += temp;


        }


    }

    public void UpdateSelection(Dictionary<Vector3, bool> squareData)
    {
        //Debug.Log("Attempt to update selection squares");
        for (int i = 0; i < instantiatedSelectors.Count; i++)
        {
            foreach (var matSetter in instantiatedSelectors[i].GetComponentsInChildren<MaterialSetter>()) //necessary to change all the pieces
            {
                //Debug.Log(board.selectedPiece.disengaging);
                if (board.selectedPiece.disengaging)
                {
                    //Debug.Log("Setting to yellow");
                    matSetter.SetSingleMaterial(gameInit.disengageMaterial);
                }
                else if (board.selectedPiece.attacking)
                {
                    matSetter.SetSingleMaterial(gameInit.attackMaterial);

                }
                else if (board.selectedPiece.turning)
                {
                    matSetter.SetSingleMaterial(gameInit.turnMaterial);

                }
                else
                {
                    matSetter.SetSingleMaterial(gameInit.defaultMaterial);
                }
            }
        }

        //section for updating squares based on accuracy
        //Dictionary<Vector3, bool> accuracyData = new Dictionary<Vector3, bool>();

        //foreach (KeyValuePair<Vector3, bool> definition in squareData) //cycle through each value definied in the dictionary

        //Debug.Log(definition.Key);
        //Debug.Log(definition.Value);


        var selectedPiece = board.selectedPiece;


        if (selectedPiece.attacking && selectedPiece.attackType == "ranged") //if ranged and attacking, set accuracy indicators
        {

            var effectiveRange = SetRanges(selectedPiece.effectiveRange);

            var midRange = SetRanges(selectedPiece.midRange);
            var longRange = SetRanges(selectedPiece.longRange);


            var piecePos = selectedPiece.occupiedSquare;
            if (selectedPiece.moveAndAttackEnabled)
            {

                Vector2Int queuedPosition = selectedPiece.occupiedSquare; //so this will let us determine the position after moves are applied
                for (int i = 0; i < selectedPiece.queuedMoves.Count; i++)
                {
                    Vector2Int distance2 = queuedPosition - selectedPiece.queuedMoves[i]; //first find distance between current position and new position
                    queuedPosition -= distance2; //then subtract this distance to get the new position again
                }
                piecePos = queuedPosition;
            }


            foreach (var selector in instantiatedSelectors) //cycle through each selector
            {
                for (int i = 0; i < midRange.Length; i++) //cycle through adjacent tile or other tiles
                {
                    Vector2Int nextCoords = piecePos + midRange[i]; //fetch a position relative to the selected piece

                    Vector3 position = board.CalculatePositionFromCoords(nextCoords); //convert to vector 3 world coords

                    if (selector.transform.position.x == position.x && selector.transform.position.z == position.z)
                    {
                        foreach (var matSetter in selector.GetComponentsInChildren<MaterialSetter>()) //necessary to change all the pieces
                        {
                            matSetter.SetSingleMaterial(gameInit.disengageMaterial);
                        }
                        break;
                    }

                }
                for (int i = 0; i < effectiveRange.Length; i++) //cycle through adjacent tile or other tiles
                {
                    Vector2Int nextCoords = piecePos + effectiveRange[i]; //fetch a position relative to the selected piece

                    Vector3 position = board.CalculatePositionFromCoords(nextCoords); //convert to vector 3 world coords

                    if (selector.transform.position.x == position.x && selector.transform.position.z == position.z)
                    {
                        foreach (var matSetter in selector.GetComponentsInChildren<MaterialSetter>()) //necessary to change all the pieces
                        {
                            matSetter.SetSingleMaterial(gameInit.turnMaterial);
                        }
                        break;
                    }

                }
            }
        }

        



    }


    public Vector2Int[] SetRanges(int range)
    {
        var selectedPiece = board.selectedPiece;
        
        Vector2Int[] movement = new Vector2Int[] { };
        if (range == 1)
        {
            movement = selectedPiece.adjacentTiles;
        }
        else if (range == 2)
        {
            movement = selectedPiece.speed2;
        }
        else if (range == 3)
        {
            movement = selectedPiece.speed3;
        }
        else if (range == 4)
        {
            movement = selectedPiece.speed4;
        }
        else if (range == 5)
        {
            movement = selectedPiece.speed5;
        }
        else if (range == 6)
        {
            movement = selectedPiece.speed6;
        }
        else if (range == 7)
        {
            movement = selectedPiece.speed7;
        }
        else if (range == 8)
        {
            movement = selectedPiece.speed8;
        }
        else if (range == 9)
        {
            movement = selectedPiece.speed9;
        }
        else if (range == 10)
        {
            movement = selectedPiece.speed10;
        }

        return movement;
    }

    public void ClearSelection()
    {
        foreach (var selector in instantiatedSelectors)
        {
            Destroy(selector.gameObject);
        }
        instantiatedSelectors.Clear();
    }
}
