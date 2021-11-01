using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{

    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
    };
    public override List<Vector2Int> SelectAvailableSquares(Vector2Int startingSquare)
    {
        availableMoves.Clear();
        float range = 1;
        foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextCoords = occupiedSquare + direction * i;
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
                {
                    break;
                }
                if (piece == null) //if space empty, this is a place we can move to
                {
                    TryToAddMove(nextCoords);
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
