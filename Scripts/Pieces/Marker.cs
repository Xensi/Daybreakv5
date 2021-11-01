using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marker : MonoBehaviour
{
    public TeamColor team;
    public int turnTime = 0; //this determines what turn time this marker is being executed on.
    public enum markerType {Move, Attack}
    public Piece parentPiece;
    public Vector2Int coords;
}
