using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conscript : Piece
{

    public override List<Vector2Int> SelectAvailableSquares(Vector2Int startingSquare)
    {
        //Debug.Log("Selecting available squares");
        availableMoves.Clear();
        Vector2Int[] movement = new Vector2Int[] { };

        if (speed == 1)
        {
            movement = adjacentTiles;
        }
        else if (speed == 2)
        {
            movement = speed2;
        }
        else if (speed == 3)
        {
            movement = speed3;
        }
        else if (speed == 4)
        {
            movement = speed4;
        }
        else if (speed == 5)
        {
            movement = speed5;
        }
        else if (speed == 6)
        {
            movement = speed6;
        }
        else if (speed == 7)
        {
            movement = speed7;
        }
        else if (speed == 8)
        {
            movement = speed8;
        }
        else if (speed == 9)
        {
            movement = speed9;
        }
        else if (speed == 10)
        {
            movement = speed10;
        }

        Debug.Log("speed" + speed);

        for (int i = 0; i < movement.Length; i++) //go through each tile in array
        {
            Vector2Int nextCoords = startingSquare + movement[i]; //coords to check are equal to the given tile + the added tile
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                //Debug.Log("Skipping");
                continue;
            }
            //maybe here we can get rid of all of the moves that are invalid
            //
            /*var gridMarker = board.markerGrid[nextCoords.x, nextCoords.y];
            if (gridMarker != null)
            {
                Debug.Log(gridMarker.team);
                Debug.Log(gridMarker.turnTime);
                Debug.Log(turnTime);
            }
            if (gridMarker != null && gridMarker.team == team && gridMarker.turnTime == turnTime) //if there is a marker on the same team on the same turn time, skip
            {
                //Debug.Log("detected a marker");
                continue;
            }*/
            if (piece == null || !piece.IsFromSameTeam(this))
            {
                //Debug.Log("adding move");
                TryToAddMove(nextCoords);
            }



        }


        return availableMoves;
    }
}
