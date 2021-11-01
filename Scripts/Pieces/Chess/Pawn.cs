using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> SelectAvailableSquares(Vector2Int startingSquare)
    {
        //Debug.Log("attempting to generate moves for pawn");
        availableMoves.Clear();
        Vector2Int direction = team == TeamColor.White ? Vector2Int.up : Vector2Int.down; //white pawns go up, black pawns go down
        float range = hasMoved ? 1 : 2; //range is higher if it hasn't moved before
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                break;
            }
            if (piece == null)
            {
                TryToAddMove(nextCoords);
                
            }
            else
            {
                break;
            }
        }

        Vector2Int[] takeDirections = new Vector2Int[] { new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y) };
        for (int i = 0; i < takeDirections.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + takeDirections[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                continue;
            }
            if (piece != null && !piece.IsFromSameTeam(this))
            {
                TryToAddMove(nextCoords);
            }
        }
        return availableMoves;
    }
}
