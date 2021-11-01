
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(PieceCreator))]
public abstract class ChessGameController : MonoBehaviour
{

    protected const byte SET_GAME_STATE_EVENT_CODE = 1;
    public enum GameState { Init, Play, Finished}

    [SerializeField] private BoardLayout startingBoardLayout;
    private Board board;
    private ChessUIManager uiManager;
    private CameraSetup cameraSetup;
    private PieceCreator pieceCreator;
    protected ChessPlayer whitePlayer;
    protected ChessPlayer blackPlayer;
    protected ChessPlayer activePlayer;
    public ChessPlayer localPlayer;

    protected GameState state;

    public bool AllowInput = true;


    protected abstract void SetGameState(GameState state);
    public abstract void TryToStartCurrentGame();
    public abstract bool CanPerformMove();


    private void Awake()
    {
        pieceCreator = GetComponent<PieceCreator>();
    }

    public void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    public void SetDependencies(ChessUIManager uiManager, Board board, CameraSetup cameraSetup)
    {
        this.uiManager = uiManager;
        this.board = board;
        this.cameraSetup = cameraSetup;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //StartNewGame();
    }

    // Update is called once per frame
    public void StartNewGame()
    {
        uiManager.OnGameStarted();
        SetGameState(GameState.Init);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(whitePlayer);
        GenerateAllPossiblePlayerMoves(blackPlayer);
        TryToStartCurrentGame();
        //Debug.LogError("starting game");
    }

    public void SetupCamera(TeamColor team)
    {
        cameraSetup.SetupCamera(team);
    }

    public bool IsGameInProgress()//i'm pretty sure this doesn't work, for whatever reason.
    {
        return state == GameState.Play; 
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);
            int direction = layout.GetDirectionAtIndex(i);

            //Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, typeName, direction);
        }
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    private void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, string typeName, int direction)
    {
        Piece newPiece = pieceCreator.CreatePiece(typeName).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board, direction);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        //newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(whitePlayer);
        GenerateAllPossiblePlayerMoves(blackPlayer);
        if (CheckIfGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    private bool CheckIfGameIsFinished()
    {
        Piece[] kingAttackingPieces = activePlayer.GetPiecesAttackingOppositePieceOfType<King>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttackOnPiece<King>(activePlayer, attackedKing);

            int availableKingMoves = attackedKing.availableMoves.Count;
            if(availableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(activePlayer);
                if (!canCoverKing)
                    return true;
            }
        }
        return false;
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void EndGame()
    {
        uiManager.OnGameFinished(activePlayer.team.ToString());
        SetGameState(GameState.Finished);
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public void RemoveMovesEnablingAttackOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(activePlayer), piece);
    }

}
