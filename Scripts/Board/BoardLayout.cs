using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    private class BoardSquareSetup
    {
        //public int boardSize;
        public Vector2Int position;
        public PieceType pieceType;
        public TeamColor teamColor;
        public int direction;
    }

    [SerializeField] private BoardSquareSetup[] boardSquares;

    public int GetPiecesCount()
    {
        return boardSquares.Length;
    }

    public Vector2Int GetSquareCoordsAtIndex(int index)
    {
        if(boardSquares.Length <= index)
        {
            Debug.LogError("index of piece is out of range");
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(boardSquares[index].position.x - 1, boardSquares[index].position.y - 1);
    }
    public int GetDirectionAtIndex(int index)
    {
        if (boardSquares.Length <= index)
        {
            Debug.LogError("index of piece is out of range");
            return 0;
        }
        return boardSquares[index].direction;
    }

    public string GetSquarePieceNameAtIndex(int index)
    {
        if(boardSquares.Length <= index)
        {
            Debug.LogError("Index of piece is out of range");
            return "";
        }
        return boardSquares[index].pieceType.ToString();
    }

    public TeamColor GetSquareTeamColorAtIndex(int index)
    {
        if (boardSquares.Length <= index)
        {
            Debug.LogError("Index of piece is out of range");
            return TeamColor.Black;
        }
        return boardSquares[index].teamColor;
    }


}
