using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{

    private Vector2Int[] directions = new Vector2Int[] { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };

    public override List<Vector2Int> SelectAvailableSquares(Vector2Int startingSquare)
    {
        availableMoves.Clear();
        float range = board.BOARD_SIZE;
        foreach(var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = occupiedSquare + direction * i;
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
                {
                    break;
                }
                if(piece == null) //if space empty, this is a place we can move to
                {
                    TryToAddMove(nextCoords);
                    //Debug.Log(nextCoords);
                }
                else if (!piece.IsFromSameTeam(this)) //if an enemy, can move here, but stop searching in this direction
                {
                    TryToAddMove(nextCoords);
                    break;
                }
                else if (piece.IsFromSameTeam(this))
                {
                    break;
                }
            }
            
        }
        return availableMoves;
    }
}
