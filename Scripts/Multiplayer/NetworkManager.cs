using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string LEVEL = "level";
    private const string TEAM = "team";
    private const int MAX_PLAYERS = 2;

    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private GameInitializer gameInitializer;
    public MultiplayerChessGameController chessGameController;

    private ChessLevel playerLevel;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        uiManager.SetConnectionStatus(PhotonNetwork.NetworkClientState.ToString());
    }

    public void SetDependencies(MultiplayerChessGameController chessGameController)
    {
        this.chessGameController = chessGameController;
    }
    public void Connect()
    {

        if (PhotonNetwork.IsConnected)
        {
            Debug.LogError($"Connected to server. Looking for random room with level {playerLevel}");
            PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel } }, MAX_PLAYERS);
        }
        else
        {

            PhotonNetwork.ConnectUsingSettings();
        }

    }


    #region Photon Callbacks

    public override void OnConnectedToMaster() //called when we have connected to server
    {
        //whenever a client connects to a server
        Debug.LogError($"Connected to server. Looking for random room with level {playerLevel}");
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel } }, MAX_PLAYERS);

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError($"Joining random room failed because of {message}. Creating a new one with player level {playerLevel}");
        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            CustomRoomPropertiesForLobby = new string[] { LEVEL },
            MaxPlayers = MAX_PLAYERS,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel} },
        }) ;

    }
    public override void OnJoinedRoom() //local
    {
        Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room with level {(ChessLevel)PhotonNetwork.CurrentRoom.CustomProperties[LEVEL]}");
        gameInitializer.CreateMultiplayerBoard();
        PrepareTeamSelectionOptions();
        uiManager.ShowTeamSelectionScreen();
    }

    private void PrepareTeamSelectionOptions()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            var firstPlayer = PhotonNetwork.CurrentRoom.GetPlayer(1);
            if (firstPlayer.CustomProperties.ContainsKey(TEAM))
            {
                var occupiedTeam = firstPlayer.CustomProperties[TEAM];
                uiManager.RestrictTeamChoice((TeamColor)occupiedTeam);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //remote
    {
        Debug.LogError($"Player {newPlayer.ActorNumber} entered room");
    }

    public bool IsRoomFull()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    internal void SelectTeam(int team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { TEAM, team } });
        gameInitializer.InitializeMultiplayerController();
        
        chessGameController.SetLocalPlayer((TeamColor)team);
        chessGameController.StartNewGame();
        chessGameController.SetupCamera((TeamColor)team);
    }


    public void SetPlayerLevel(ChessLevel level)
    {
        playerLevel = level;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { LEVEL, level } });
    }
    #endregion

}
