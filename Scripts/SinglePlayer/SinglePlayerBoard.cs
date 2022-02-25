using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerBoard : Board
{
    public override void MoveSelectedPiece(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y)); //determine coords of selected area
        OnMoveSelectedPiece(intCoords);

    }

    public override void SetSelectedPiece(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSetSelectedPiece(intCoords);
    }

    public override void ExecuteMoveForAllPieces()
    {
        whiteReady = true;
        blackReady = true;
        OnExecuteMoveForAllPieces("");
    }
    public override void Unready()
    {
        if (!whiteReady || !blackReady)
        { 
            whiteReady = false;
            blackReady = false;
            OnUnready("");
        }
    }

    public override void ChangeStance(int id, string stance)
    {
        OnChangeStance(id, stance);
    }

    public override void ArbitrateConflict()
    {

        var random = Random.Range(1, 3);
        OnArbitrateConflict(random);
    }

    public override void CommunicateQueuedMoves(int id, int x, int y)
    {
        OnCommunicateQueuedMoves(id, x, y);
    }

    public override void CommunicateMarkers(int id, float x2, float y2, float z2, int x, int y, string team, int remainingMovement)
    {

        OnCommunicateMarkers(id, x2, y2, z2, x, y, team, remainingMovement);
    }

    public override void ClearMoves(int id)
    {
        OnClearMoves(id);
    }

    public override void PieceApplyDamage(int id) //for moving soldiers
    {
        OnPieceApplyDamage(id);
    }
    public override void PieceCalculateDamage(int id) //for moving soldiers
    {
        OnPieceCalculateDamage(id);
    }
    public override void PieceCheckFlankingDamage(int id) //for moving soldiers
    {
        OnPieceCheckFlankingDamage(id);
    }

    public override void TriggerSlowUpdate()
    {
        OnTriggerSlowUpdate();
    }
    public override void PieceUpdateTerrainType(int id, int x, int y) //for moving soldiers
    {
        OnPieceUpdateTerrainType(id, x, y);
    }

    public override void CommunicateTurnHoldTime(int id, int turnTime, int holdTime)
    {
        OnCommunicateTurnHoldTime(id, turnTime, holdTime);
    }

    public override void PieceTriggerAttacksForSoldiers(int id)
    {
        OnPieceTriggerAttacksForSoldiers(id);
    }
    public override void PieceCommunicateTargetToAttackPiece(int id, int x, int y)
    {
        OnPieceCommunicateTargetToAttackPiece(id, x, y);
    }
    public override void PieceMarkForDeath(int id, float damage)
    {
        OnPieceMarkForDeath(id, damage);
    }
    public override void PieceCommunicateAttackTile(int id, int x, int y)
    {
        OnPieceCommunicateAttackTile(id, x, y);
    }

    public override void ChangeFormation(int id, string formation)
    {
        OnChangeFormation(id, formation);
    }
    public override void ChangeAttitude(int id, bool aggressive)
    {
        OnChangeAttitude(id, aggressive);
    }

    public override void PieceCalculateLineOfSight(int id)
    {
        OnPieceCalculateLineOfSight(id);
    }
    public override void PieceRunThroughCylinders(int id)
    {
        OnPieceRunThroughCylinders(id);
    }
}
