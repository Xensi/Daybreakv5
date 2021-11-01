using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleplayerChessGameController : ChessGameController
{
    //public TeamColor teamOverride = TeamColor.White;

    /*public void Start()
    {
        SetLocalPlayer(teamOverride);
    }*/
    public override bool CanPerformMove()
    {
        if (!IsGameInProgress())
            return false;
        return true;
    }

    public override void TryToStartCurrentGame()
    {
        SetGameState(GameState.Play);
    }

    protected override void SetGameState(GameState state)
    {
        this.state = state;
    }
    /*public void SetLocalPlayer(TeamColor team)
    {
        localPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        Debug.Log(localPlayer.team);
    }*/
}
