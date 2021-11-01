using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MultiplayerChessGameController : ChessGameController, IOnEventCallBack //use events to update things in multiplayer
{
    private NetworkManager networkManager;

    public void SetNetworkDependencies(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override bool CanPerformMove()
    {
        //use this when testing against real people
        if (!IsLocalPlayersTurn() || !networkManager.IsRoomFull())
        {
            return false;
        }
        return true;
    }

    public bool IsLocalPlayersTurn()
    {
        return localPlayer == activePlayer;
    }

    public void SetLocalPlayer(TeamColor team) 
    {
        localPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        Debug.Log(localPlayer.team);
    }

    public override void TryToStartCurrentGame()
    {
        //Debug.LogError("trying to start the game!");
        if (networkManager.IsRoomFull())
        {
            SetGameState(GameState.Play);
            
            //Debug.LogError("Set game state to play");
        }
    }

    protected override void SetGameState(GameState state)
    {
        object[] content = new object[] { (int)state };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(SET_GAME_STATE_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if(eventCode == SET_GAME_STATE_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameState state = (GameState)data[0];
            this.state = state;
        }
    }

}
