using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PhotonView))]
public class MultiplayerBoard : Board
{
    private PhotonView photonView;
    public int random = 420;

    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        string tempString = "testing!";

        Random.InitState(42);
        if (PhotonNetwork.IsMasterClient) //should only call once
        {
            random = Random.Range(1, 100);
            photonView.RPC(nameof(RPC_SetVar), RpcTarget.AllBuffered, new object[] { tempString, random });
        }
        else
        {
            random = Random.Range(1, 100);
            photonView.RPC(nameof(RPC_SetVar), RpcTarget.AllBuffered, new object[] { tempString, random });
        }

    }

    [PunRPC]
    private void RPC_SetVar(string tempString, int randomGen)
    {

        random = randomGen;
        //Debug.LogError(tempString + " " + randomGen);
        //Debug.LogError("Now check to see if both have the same value");


    }

    /*public override void SelectPieceMoved(Vector2 coords) //called by onsquareselected
    {
        photonView.RPC(nameof(RPC_OnSelectedPieceMoved), RpcTarget.AllBuffered, new object[] { coords }); //objects are parameters
    }
    [PunRPC]
    private void RPC_OnSelectedPieceMoved(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSelectedPieceMoved(intCoords); //captures/moves pieces multiplayer
    }*/

    public override void SelectPieceMoved(Vector2 coords) //called by onsquareselected
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSelectedPieceMoved(intCoords); //captures/moves pieces multiplayer

    }

    /*public override void SetSelectedPiece(Vector2 coords)
    {
        photonView.RPC(nameof(RPC_OnSetSelectedPiece), RpcTarget.AllBuffered, new object[] { coords });
    }
    [PunRPC]
    private void RPC_OnSetSelectedPiece(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSetSelectedPiece(intCoords); //literally just selects a piece idk
    }*/

    public override void SetSelectedPiece(Vector2 coords)
    {
        Vector2Int intCoords = new Vector2Int(Mathf.RoundToInt(coords.x), Mathf.RoundToInt(coords.y));
        OnSetSelectedPiece(intCoords);
    }

    public override void ExecuteMoveForAllPieces()
    {
        chessController.AllowInput = false; //set locally, hopefully
        executeButton.interactable = false;
        string team = chessController.localPlayer.team.ToString();
        photonView.RPC(nameof(RPC_OnExecuteMoveForAllPieces), RpcTarget.AllBuffered, new object[] { team });
    }
    [PunRPC]
    private void RPC_OnExecuteMoveForAllPieces(string team)
    {
        OnExecuteMoveForAllPieces(team);
    }
    public override void ChangeStance(int id, string stance)
    {
        photonView.RPC(nameof(RPC_OnChangeStance), RpcTarget.AllBuffered, new object[] { id, stance });
    }
    [PunRPC]
    private void RPC_OnChangeStance(int id, string stance)
    {
        OnChangeStance(id, stance);
    }

    public override void ArbitrateConflict()
    {
        random = Random.Range(1, 3);
        photonView.RPC(nameof(RPC_OnArbitrateConflict), RpcTarget.AllBuffered, new object[] { random });
        
    }

    [PunRPC]
    private void RPC_OnArbitrateConflict(int randomGen)
    {
        OnArbitrateConflict(randomGen);

    }

    public override void CommunicateQueuedMoves(int id, int x, int y)
    {
        photonView.RPC(nameof(RPC_OnCommunicateQueuedMoves), RpcTarget.AllBuffered, new object[] { id, x, y });
    }

    [PunRPC]
    private void RPC_OnCommunicateQueuedMoves(int id, int x, int y)
    {
        OnCommunicateQueuedMoves(id, x, y);

    }


    public override void CommunicateMarkers(int id, float x2, float y2, float z2, int x, int y, string team, int remainingMovement)
    {
        photonView.RPC(nameof(RPC_OnCommunicateMarkers), RpcTarget.AllBuffered, new object[] { id, x2, y2, z2, x, y, team, remainingMovement});
    }

    [PunRPC]
    private void RPC_OnCommunicateMarkers(int id, float x2, float y2, float z2, int x, int y, string team, int remainingMovement)
    {
        OnCommunicateMarkers(id, x2, y2, z2, x, y, team, remainingMovement);

    }

    public override void ClearMoves(int id)
    {
        photonView.RPC(nameof(RPC_OnClearMoves), RpcTarget.AllBuffered, new object[] { id });
    }

    [PunRPC]
    private void RPC_OnClearMoves(int id)
    {
        OnClearMoves(id);

    }

    public override void PieceApplyDamage(int id) //for moving soldiers
    {
        photonView.RPC(nameof(RPC_OnPieceApplyDamage), RpcTarget.AllBuffered, new object[] { id });
    }

    [PunRPC]
    private void RPC_OnPieceApplyDamage(int id)
    {
        OnPieceApplyDamage(id);

    }

    public override void TriggerSlowUpdate() //for determining if soldiers should keep going or not
    {
        photonView.RPC(nameof(RPC_OnTriggerSlowUpdate), RpcTarget.AllBuffered, new object[] { });
    }

    [PunRPC]
    private void RPC_OnTriggerSlowUpdate()
    {
        OnTriggerSlowUpdate();

    }
    public override void PieceUpdateTerrainType(int id, int x, int y) //for moving soldiers
    {
        photonView.RPC(nameof(RPC_OnPieceUpdateTerrainType), RpcTarget.AllBuffered, new object[] { id, x, y });
    }

    [PunRPC]
    private void RPC_OnPieceUpdateTerrainType(int id, int x, int y)
    {
        OnPieceUpdateTerrainType(id, x , y);

    }
    public override void CommunicateTurnHoldTime(int id, int turnTime, int holdTime) //we need this for getting attacks fully working
    {
        photonView.RPC(nameof(RPC_OnCommunicateTurnHoldTime), RpcTarget.AllBuffered, new object[] { id, turnTime, holdTime });
    }

    [PunRPC]
    private void RPC_OnCommunicateTurnHoldTime(int id, int turnTime, int holdTime)
    {
        OnCommunicateTurnHoldTime(id, turnTime, holdTime);

    }

    public override void PieceTriggerAttacksForSoldiers(int id) //we need this for getting attacks fully working
    {
        photonView.RPC(nameof(RPC_OnPieceTriggerAttacksForSoldiers), RpcTarget.AllBuffered, new object[] { id });
    }

    [PunRPC]
    private void RPC_OnPieceTriggerAttacksForSoldiers(int id)
    {
        OnPieceTriggerAttacksForSoldiers(id);

    }
    public override void PieceCommunicateTargetToAttackPiece(int id, int x, int y) //we need this for getting attacks fully working
    {
        photonView.RPC(nameof(RPC_OnPieceCommunicateTargetToAttackPiece), RpcTarget.AllBuffered, new object[] { id, x, y });
    }

    [PunRPC]
    private void RPC_OnPieceCommunicateTargetToAttackPiece(int id, int x, int y)
    {
        OnPieceCommunicateTargetToAttackPiece(id, x, y);

    }
    public override void PieceMarkForDeath(int id, int damage) //we need this for deaths
    {
        photonView.RPC(nameof(RPC_OnPieceMarkForDeath), RpcTarget.AllBuffered, new object[] { id, damage });
    }

    [PunRPC]
    private void RPC_OnPieceMarkForDeath(int id, int damage)
    {
        OnPieceMarkForDeath(id, damage);

    }
    public override void PieceCommunicateAttackTile(int id, int x, int y) //we need this for getting attacks fully working
    {
        photonView.RPC(nameof(RPC_OnPieceCommunicateAttackTile), RpcTarget.AllBuffered, new object[] { id, x, y });
    }

    [PunRPC]
    private void RPC_OnPieceCommunicateAttackTile(int id, int x, int y)
    {
        OnPieceCommunicateAttackTile(id, x, y);

    }


    public override void ChangeFormation(int id, string formation) 
    {
        photonView.RPC(nameof(RPC_OnChangeFormation), RpcTarget.AllBuffered, new object[] { id, formation });
    }

    [PunRPC]
    private void RPC_OnChangeFormation(int id, string formation)
    {
        OnChangeFormation(id, formation);

    }

}
