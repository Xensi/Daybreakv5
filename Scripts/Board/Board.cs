using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using Random = UnityEngine.Random;

public abstract class Board : MonoBehaviour
{

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    public Piece[,] grid;
    public Piece[,] routingGrid;
    public Marker[,] markerGrid; //white
    public Marker[,] markerGrid2; //black
    public String[,] terrainGrid;
    public Piece selectedPiece;
    public ChessGameController chessController;
    private bool isChessControllerMP = false;
    public SquareSelectorCreator squareSelector;

    public TeamColor[] teamColors;


    [SerializeField] private Material testMat;
    [SerializeField] private Material whiteMat;
    [SerializeField] private Material blackMat;
    public Button executeButton;
    private bool turnBeingExecuted = false;
    public bool whiteReady = false;
    public bool blackReady = false;
    //private bool damagePhaseCalled = false;
    private bool allMovesFinishedCalled;
    public int BOARD_SIZE = 100;
    public int secondsPassed = 0;

    public Piece defendant;
    public Piece prosecutor;

    private ChessUIManager ui;

    public List<Piece> UnitList;
    public int unitNumber = 0;

    public GameInitializer gameInit;

    public bool selectingAction = false;
    public GameObject selectionIndicatorPrefab;

    private List<GameObject> instantiatedSelectors = new List<GameObject>();

    private bool waitingToFinishTurn = false;

    public List<Piece> piecesReadyToAttack = new List<Piece>();
    public List<Piece> piecesReadyToAttackAfterMovement = new List<Piece>();
    public List<Piece> secondPassMoveWave = new List<Piece>();
    public List<Piece> allPieces = new List<Piece>();

    protected virtual void Awake()
    {
        UnitList = new List<Piece>();
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
        executeButton = GameObject.FindGameObjectWithTag("ExecuteButton").GetComponent<Button>();
        var uiObj = GameObject.Find("UI");
        ui = uiObj.GetComponent<ChessUIManager>();
    }
    public abstract void SelectPieceMoved(Vector2 coords);
    public abstract void SetSelectedPiece(Vector2 coords);
    public abstract void ExecuteMoveForAllPieces();
    public abstract void ArbitrateConflict();

    public abstract void CommunicateQueuedMoves(int id, int x, int y);
    public abstract void PieceCommunicateAttackTile(int id, int x, int y);
    public void OnPieceCommunicateAttackTile(int id, int x, int y)
    {
        UnitList[id].OnCommunicateAttackTile(x, y);
    }
    public abstract void CommunicateMarkers(int id, float x2, float y2, float z2, int x, int y, string team, int remainingMovement);

    public abstract void CommunicateTurnHoldTime(int id, int turnTime, int holdTime);

    public void OnCommunicateTurnHoldTime(int id, int turnTime, int holdTime)
    {
        UnitList[id].turnTime = turnTime;
        UnitList[id].holdTime = holdTime;
    }

    public abstract void ClearMoves(int id);

    public abstract void PieceApplyDamage(int id);
    public void OnPieceApplyDamage(int id)
    {
        UnitList[id].OnApplyDamage();
    }
    public abstract void PieceCalculateDamage(int id);
    public void OnPieceCalculateDamage(int id)
    {
        UnitList[id].OnCalculateDamage();
    }

    public abstract void PieceCheckFlankingDamage(int id);
    public void OnPieceCheckFlankingDamage(int id)
    {
        UnitList[id].OnCheckFlankingDamage();
    }

    public abstract void PieceUpdateTerrainType(int id, int x, int y);
    public abstract void PieceTriggerAttacksForSoldiers(int id);

    public abstract void PieceCommunicateTargetToAttackPiece(int id, int x, int y);


    public abstract void PieceMarkForDeath(int id, int damage);
    public void OnPieceMarkForDeath(int id, int damage)
    {
        UnitList[id].OnMarkForDeath(damage);
    }

    public void OnPieceCommunicateTargetToAttackPiece(int id, int x, int y)
    {
        UnitList[id].OnCommunicateTargetToAttackPiece(x, y);
    }

    public void OnPieceTriggerAttacksForSoldiers(int id)
    {
        UnitList[id].OnTriggerAttacksForSoldiers();
    }

    public void OnPieceUpdateTerrainType(int id, int x, int y)
    {
        UnitList[id].OnUpdateTerrainType(x, y);
    }


    public void OnClearMoves(int id)
    {
        foreach (var i in UnitList[id].instantiatedMarkers)
        {
            Destroy(i.gameObject);
        }
        foreach (var i in UnitList[id].markerVisuals)
        {
            Destroy(i.gameObject);
        }
        /*if(UnitList[id].attackType == "ranged") //todo come back to delete 
        {

            Debug.Log("not clearing lines" + id);
        }*/
        //else
        //{
        //Debug.Log("clearing lines" + id);
        //visually clear line prefabs and aesthetic cylinders
        foreach (var i in UnitList[id].instantiatedLines)
        {
            Destroy(i.gameObject);
        }
        UnitList[id].instantiatedLines.Clear();
        foreach (var i in UnitList[id].aestheticCylinders)
        {
            Destroy(i.gameObject);
        }
        UnitList[id].aestheticCylinders.Clear();
        //}

        UnitList[id].instantiatedMarkers.Clear();
        UnitList[id].markerVisuals.Clear();
        UnitList[id].queuedMoves.Clear();
        UnitList[id].remainingMovement = UnitList[id].speed; //if we clear moves, we need to reset to allow full movement again
        UnitList[id].turnTime = 0;
    }

    public void OnCommunicateMarkers(int id, float x2, float y2, float z2, int x, int y, string team, int remainingMovement) //used in mp
    {
        //Debug.Log(team);
        var coords = new Vector2Int(x, y);
        var targetPosition = new Vector3(x2, y2, z2);
        Marker marker = Instantiate(UnitList[id].markerPrefab, targetPosition, Quaternion.identity); //then place a marker there
        UnitList[id].instantiatedMarkers.Add(marker); //add it to the list so we can delete if need be
        marker.turnTime = UnitList[id].speed - remainingMovement;//set marker turn time. we can tell what turn time we're on by the remaining movement. for example, 2 speed - 1 remaining move: 1st turn time
        ////Debug.LogError("Speed" + UnitList[id].speed + remainingMovement);
        marker.parentPiece = UnitList[id];
        marker.coords = coords;
        int num = 0;
        foreach (var color in gameInit.teamColorDefinitions)
        {
            num++;
            if (team == color.ToString())
            {
                marker.team = color;
                if (num == 1)
                {
                    if (CheckIfCoordinatedAreOnBoard(coords)) //necessary to stop game from freezing up
                    {
                        markerGrid[coords.x, coords.y] = marker;
                    }
                }
                else if (num == 2)
                {
                    if (CheckIfCoordinatedAreOnBoard(coords))
                    {

                        markerGrid2[coords.x, coords.y] = marker;
                    }
                }
                else if (num == 3)
                {
                    if (CheckIfCoordinatedAreOnBoard(coords))
                    {

                        markerGrid[coords.x, coords.y] = marker;
                    }
                }
                else if (num == 4)
                {
                    if (CheckIfCoordinatedAreOnBoard(coords))
                    {

                        markerGrid2[coords.x, coords.y] = marker;
                    }
                }
            }
        }

        //Debug.Log(marker.team);
    }
    public void OnCommunicateQueuedMoves(int id, int x, int y)
    {
        var coords = new Vector2Int(x, y);
        UnitList[id].queuedMoves.Add(coords);
        //piece.queuedMoves.Add(coords);
    }

    public abstract void ChangeFormation(int id, string formation);

    public void OnChangeFormation(int id, string formation)
    {

        UnitList[id].queuedFormation = formation;
    }


    public abstract void ChangeStance(int id, string stance);
    public void OnChangeStance(int id, string stance) //this should not rely on selected piece, rather unit ID.
                                                      //using selectedpiece is preferable so you can control what parts of the function are multiplayer, like showing selection squares .. .unless, we can use unit id and check in the function if the
                                                      //unit is on the same team. if they're not we can prevent showing the squares while still updating the variables
    {
        if (stance == "sprint")
        {
            //selectedPiece.SetStanceSprint();
            UnitList[id].SetStanceSprint();
        }
        else if (stance == "disengage")
        {

            //selectedPiece.SetStanceDisengage(); //got error
            UnitList[id].SetStanceDisengage();
        }
        else if (stance == "move")
        {

            //selectedPiece.SetStanceMove(); //got error
            UnitList[id].SetStanceMove();
        }
        else if (stance == "turn")
        {

            //selectedPiece.SetStanceTurn();
            UnitList[id].SetStanceTurn();
        }
        else if (stance == "attack")
        {

            //selectedPiece.SetStanceAttack();
            UnitList[id].SetStanceAttack();
        }
        else if (stance == "MoveAttack")
        {

            //selectedPiece.SetStanceMoveAttack();
            UnitList[id].SetStanceMoveAttack();
        }
        else if (stance == "reset")
        {

            //selectedPiece.ResetStance();
            UnitList[id].ResetStance();
        }
    }
    public abstract void Unready();
    public void OnUnready(string team)
    {
        if (!whiteReady || !blackReady) //if at least one player is not ready yet
        {
            turnBeingExecuted = false; //tell slow update to start checking if we're done moving or not

            for (int i = 0; i < gameInit.teamColorDefinitions.Length; i++) //set values to be true based on team color
            {
                if (team == gameInit.teamColorDefinitions[i].ToString())
                {
                    if (i == 0)
                    {

                        whiteReady = false;
                    }
                    else if (i == 1)
                    {

                        blackReady = false;
                    }
                    else if (i == 2)
                    {

                        whiteReady = false;
                    }
                    else if (i == 3)
                    {

                        blackReady = false;
                    }
                }
            }
        }
        
    }
    public void OnExecuteMoveForAllPieces(string team) //execute moves for all pieces! using mp
    {
        //DeselectPiece();

        turnBeingExecuted = true; //tell slow update to start checking if we're done moving or not

        for (int i = 0; i < gameInit.teamColorDefinitions.Length; i++) //set values to be true based on team color
        {
            if (team == gameInit.teamColorDefinitions[i].ToString())
            {
                if (i == 0)
                {

                    whiteReady = true;
                }
                else if (i == 1)
                {

                    blackReady = true;
                }
                else if (i == 2)
                {

                    whiteReady = true;
                }
                else if (i == 3)
                {

                    blackReady = true;
                }
            }
        }
        //if singleplayer, both are set to true by singleplayerboard;

        //Debug.Log(whiteReady + "" + blackReady);
        if (whiteReady && blackReady) //if both players are ready
        {
            waitingToFinishTurn = false;
            whiteReady = false;
            blackReady = false;
            allMovesFinishedCalled = false;
            //damagePhaseCalled = false;
            //Debug.Log("Trying to move all pieces");
            chessController.AllowInput = false;
            executeButton.interactable = false;

            //start of preturn
            Piece[] AllPieces = FindObjectsOfType<Piece>();

            Preturn(AllPieces); //turn set up
            piecesReadyToAttack.Clear();
            piecesReadyToAttackAfterMovement.Clear();
            for (int i = 0; i < AllPieces.Length; i++) //go through each piece
            {
                ////Debug.LogError("allpieces" + AllPieces[i] + AllPieces[i].queuedMoves.Count);
                if (AllPieces[i].queuedMoves.Count == 1) //if exactly one moved queued and attacking, you are eligible to attack immediately  && AllPieces[i].attacking && AllPieces[i].attackedThisTurn == false
                {
                    ////Debug.LogError("I HATE YOU");
                    piecesReadyToAttack.Add(AllPieces[i]);
                    ////Debug.LogError("added to pieces ready to immediate attack" + AllPieces[i]);

                }

            }

            AttackPhaseSetup(piecesReadyToAttack, 0); //allows for immediate attacks by pieces that are ready to attack already //attack phase setup is adding all pieces to the list for some reason . . .


            secondPassMoveWave.Clear();
            //start of movement phase
            for (int i = 0; i < AllPieces.Length; i++) //Actually start moving
            {
                AllPieces[i].markForRemovalFromSecondWave = false;
                //Debug.Log(AllPieces[i]);
                AllPieces[i].StartMoveCoroutines(0); //this will start moves but the order is not something we can choose ourselves
                                                     //move coroutines will add to second pass

            }
            var num = secondPassMoveWave.Count;
            var overflow = 0;
            while (num > 0  && overflow < 50) //while there are still pieces to check
            {
                foreach (var piece in secondPassMoveWave) //for each piece in the list
                {
                    if (!piece.markForRemovalFromSecondWave) //if mark for removal = false
                    {
                        piece.StartMoveCoroutines(0); //try to move
                    }
                    else //if marked for removal, "remove" it
                    {
                        num--;
                    }
                }
                overflow++;
                Debug.Log("overflow" + overflow);
            }


        }

    }

    private void Preturn(Piece[] AllPieces)
    {
        for (int i = 0; i < AllPieces.Length; i++) //check if enemies adjacent. if so, can't move
        {
            //Debug.Log(AllPieces[i]);

            AllPieces[i].attackedThisTurn = false; //remind everyone that they have not attacked yet
            AllPieces[i].markedDeaths = false; //remind everyone that they have not attacked yet
            AllPieces[i].alreadyCalculatedDamage = false;
            AllPieces[i].alreadyAppliedDamage = false;
            AllPieces[i].animationsOver = false;
            AllPieces[i].movementStopped = false;
        }
        for (int i = 0; i < AllPieces.Length; i++) //check if enemies adjacent. if so, can't move
        {
            //Debug.Log(AllPieces[i]);
            AllPieces[i].CheckIfEnemiesAdjacent(); //just changes variables but useful for later to determine if movement is allowed
        }
        for (int i = 0; i < AllPieces.Length; i++) //check if any markers overlap between friendly and enemy
        {
            //Debug.Log(AllPieces[i]);
            AllPieces[i].CheckIfMarkersOverlap(); //important for tie breaking behavior
        }
        for (int i = 0; i < AllPieces.Length; i++)
        {
            if (AllPieces[i].attacking && AllPieces[i].attackType == "melee")
            {
                AllPieces[i].CheckIfEnemyInFirstQueuedMove(); //important to call before moving to see if we can immediate attack
            }
        }
        
    }

    private IEnumerator SlowUpdate(float speed) //calls the function responsible for checking if movement phase should be over or not
    {
        if (turnBeingExecuted) //if turn is being executed
        {
            OneStepFinished(); //check to see if all units have moved a single step. if so, then tell them to move another step
            CheckIfAllMovesFinished(); //check to see if all units are a done moving completely. if so, start processing attacks
        }
        yield return new WaitForSeconds(speed);
        StartCoroutine(SlowUpdate(speed)); //start it again  
    }


    public void OneStepFinished() //checks to see if one step has been finished. if so, start move coroutines again
    {
        Debug.Log("Checking if all steps finished");
        Piece[] AllPieces = FindObjectsOfType<Piece>();
        for (int i = 0; i < AllPieces.Length; i++) //go through all pieces and see if they're done yet.
        {
            if (AllPieces[i].oneStepFinished == false)
            {
                //Debug.Log("Found one that isn't finished one step");
                //Debug.Log("what is their finished moving status" + AllPieces[i].FinishedMoving);
                return; //if there's one that hasn't finished, stop checking and don't change input allowance
            }
        }
        Debug.Log("All steps finished");
        //if all steps finished
        //check to see if we have any cavalry that still need to finish a second move step
        List<Piece> cavalry = new List<Piece>();
        foreach (var piece in AllPieces)
        {
            if (piece.unitType == "cavalry" && piece.queueTime % 2 == 1 && piece.queueTime < piece.queuedMoves.Count)  //if we have any cavalry that's only on its first/third move and has more moves left to go
            {
                ////Debug.LogError("Knight queue time " + piece.queueTime + "queuedmovescount" + piece.queuedMoves.Count);
                cavalry.Add(piece); //add it to the list
            }
        }

        if (cavalry.Count > 0) //if we have any cavalry that still needs to move
        {

            for (int i = 0; i < cavalry.Count; i++)
            {
                cavalry[i].CheckIfEnemiesAdjacent();
            }
            for (int i = 0; i < cavalry.Count; i++)
            {
                cavalry[i].StartMoveCoroutines(1);
            }
        }
        else
        {

            for (int i = 0; i < AllPieces.Length; i++)
            {
                AllPieces[i].CheckIfEnemiesAdjacent();
            }
            for (int i = 0; i < AllPieces.Length; i++)
            {
                AllPieces[i].StartMoveCoroutines(1);
            }
        }
    }
    public void CheckIfAllMovesFinished() //this will be responsible for ending the movement phase
    {
        //Debug.Log("Checking ifAllMovesFinished");
        Piece[] AllPieces = FindObjectsOfType<Piece>();
        for (int i = 0; i < AllPieces.Length; i++) //go through all pieces and see if they're done yet.
        {
            if (AllPieces[i].FinishedMoving == false)
            {
                //Debug.Log("Found one that isn't finished");
                return; //if there's one that hasn't finished, stop checking and don't change input allowance
            }
        }

        if (!allMovesFinishedCalled)
        {
            allMovesFinishedCalled = true;
            foreach (var piece in AllPieces)
            {
                piece.movementStopped = true;
            }

            //this doesn't work because queued moves is cleared before this is called
            for (int i = 0; i < AllPieces.Length; i++)
            {
                if (AllPieces[i].attacking && AllPieces[i].attackType == "melee")
                {
                    AllPieces[i].CheckIfEnemyInRelativeStashedMove(); //call after moving to see if we can acquire new target
                }
            }
            for (int i = 0; i < AllPieces.Length; i++)
            {
                if (AllPieces[i].attacking && AllPieces[i].attackType == "melee")
                {
                    AllPieces[i].CheckIfEnemyNearUs(); //last chance to see if we can acquire new target
                }
            }

            for (int i = 0; i < AllPieces.Length; i++) //go through each piece
            {
                Debug.Log(AllPieces[i].attackedThisTurn + "attacked this turn" + AllPieces[i]);
                if (AllPieces[i].attacking && AllPieces[i].attackedThisTurn == false) //if attacking, you are eligible to attack after movement
                {
                    piecesReadyToAttackAfterMovement.Add(AllPieces[i]);
                }
            }

            /*foreach (var item in piecesReadyToAttackAfterMovement)
            {
                //Debug.LogError("Pieces ready to attack after movement" + item);
            }*/


            AttackPhaseSetup(piecesReadyToAttackAfterMovement, 1); //call this only once
        }
    }
    public void AttackPhaseSetup(List<Piece> pieces, int phaseNum) //start processing attacks because movement is done (or not in the case of the immediate attacks)
    {
        foreach (var item in pieces)
        {
            Debug.LogError(item + " " + phaseNum);
        }

        if (phaseNum == 1)
        {
            turnBeingExecuted = false; //this will disable the checks running in slow update to see if movement is done (because it is!) 
        }

        for (int i = 0; i < pieces.Count; i++)
        {

            if (pieces[i].attackedThisTurn == false)
            {
                pieces[i].CheckIfEnemyInAttackTile(); //sets target to unit in targeted tile if we have no target and no attack tile already (targeting an empty tile and waiting for a unit to enter it)
            }

        }


        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].attackedThisTurn == false)
            {
                pieces[i].CheckIfEnemiesAdjacent(); //check if enemies are adjacent
            }
        }


        var numRangedUnits = 0;
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].attacking && pieces[i].attackType == "ranged" && pieces[i].targetToAttackPiece != null && pieces[i].attackedThisTurn == false) // if attacking, ranged and has a target
            {
                numRangedUnits++;
                pieces[i].CalculateLineOfSight(); //create cylinders
            }
        }
        if (numRangedUnits <= 0) //if no ranged units
        {
            ExecuteAttacks(pieces, phaseNum); //skip over to the next part
        }
        else
        {
            StartCoroutine(CheckIfPhysicsCalculationsProcessed(pieces, phaseNum)); //if there are ranged units, start coroutine that will check to see if done processing physics
        }
    }

    public IEnumerator CheckIfPhysicsCalculationsProcessed(List<Piece> pieces, int phaseNum) //this exists so that we don't have to do all the processing in one frame
    {
        Debug.Log("Checking");
        var numNotFinished = 0;
        for (int i = 0; i < pieces.Count; i++) //go through each piece
        {
            if (pieces[i].attacking && pieces[i].attackType == "ranged" && pieces[i].attackedThisTurn == false) //that's attacking and ranged
            {
                foreach (var item in pieces[i].instantiatedCylinders) //go through each of their cylinders
                {
                    var script = item.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;
                    if (script.finishedProcessing == false) //basically if not finished processing
                    {
                        numNotFinished++;
                    }
                }
            }
        }
        if (numNotFinished == 0) //if all of them are finished
        {
            ExecuteAttacks(pieces, phaseNum);
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(.1f);
            StartCoroutine(CheckIfPhysicsCalculationsProcessed(pieces, phaseNum));
        }

    }

    public void ExecuteAttacks(List<Piece> pieces, int phaseNum) //should be called even if no line of sight calculations occurred
    {
        ////Debug.LogError("EXECUTING ATTACKS");
        //by this point, all physics calculations should be done.

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].attacking && pieces[i].attackType == "ranged" && pieces[i].targetToAttackPiece != null && pieces[i].attackedThisTurn == false)
            {
                pieces[i].RunThroughCylinders(); //sets new target to attack or cancels attack if blocked by friendly
            }
        }
        // check if being attacked
        Piece[] AllPieces = FindObjectsOfType<Piece>(); //this necessarily has to be checked by all units
        for (int i = 0; i < AllPieces.Length; i++)
        {
            if (AllPieces[i].movementStopped && !AllPieces[i].attacking && AllPieces[i].targetToAttackPiece == null && AllPieces[i].attackedThisTurn == false) //if we are not attacking, have no attack target and have not attacked, and have finished moving
            {
                Debug.Log("phase num" + phaseNum);
                AllPieces[i].CheckIfAttacked(); //check to see if we're being attacked. if we are and haven't attacked yet, defend yourself
                if (AllPieces[i].defensiveAttacking)
                {
                    pieces.Add(AllPieces[i]); //this is now an attacker >:)
                    if (AllPieces[i].attackType == "ranged") // accuracy calculations are incompatible with this, so set it manually
                    {
                        AllPieces[i].accuracy = .5f; //for point blank range
                    }
                }
            }

        }
        for (int i = 0; i < AllPieces.Length; i++)
        {
            AllPieces[i].flankingDamage = 1;
        }
        for (int i = 0; i < AllPieces.Length; i++) //should calculate for all?
        {
            AllPieces[i].CheckFlankingDamage(); //if enemies are on our flanks they get bonus damage against us //it;s fine to call this multiple times
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].attackedThisTurn == false)
            {

                pieces[i].CalculateDamage(); //calculate attack damage //there isn't a reason to call this more than once 
            }
        }

        for (int i = 0; i < AllPieces.Length; i++)
        {
            Debug.Log("old" + AllPieces[i].oldModels + "new" + AllPieces[i].models);
            AllPieces[i].oldModels = AllPieces[i].models;
            Debug.Log("NEWold" + AllPieces[i].oldModels + "new" + AllPieces[i].models);
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].attackedThisTurn == false)
            {
                pieces[i].ApplyDamage(); //set models -= queued damage and triggers attacks for models //very important to only call this once 
            }
        }

        for (int i = 0; i < AllPieces.Length; i++) //this is gettin called both times is the problem
        {
            var totalDamage = AllPieces[i].oldModels - AllPieces[i].models; //calculate total damage dealt to each unit
            if (totalDamage > 0)
            {
                Debug.Log("old" + AllPieces[i].oldModels + "new" + AllPieces[i].models + "total damage" + totalDamage);
                //AllPieces[i].MarkForDeath(totalDamage);
                AllPieces[i].OnMarkForDeath(totalDamage);
                //PieceMarkForDeath(AllPieces[i].unitID, totalDamage); //mp
            }
        }

        for (int i = 0; i < pieces.Count; i++) // all of the pieces called in this function have now attacked
        {
            if (pieces[i].attacking)
            {
                ////Debug.LogError("phase" + phaseNum + pieces[i]);
                pieces[i].attackedThisTurn = true; //so we can mark it as such
            }

        }

        List<Piece> AllPiecesList = AllPieces.ToList(); //this necessarily has to be checked by all units
        if (phaseNum == 1) //if second attack phase then we will call cleanup
        {
            CleanUpPhase(AllPiecesList); //apply morale, reset everyone's stance, check if units are dead, check if units are routing
        }

    }
    private void CleanUpPhase(List<Piece> pieces)
    {

        for (int i = 0; i < pieces.Count; i++) //check if any markers overlap between friendly and enemy
        {
            if (pieces[i].attacking)
            {
                pieces[i].animationsOver = false;
            }
            else
            {

                pieces[i].animationsOver = true;
            }
        }
        for (int i = 0; i < pieces.Count; i++) //should call this after all attacks are done
        {
            pieces[i].ApplyMorale(); //only applies model losses morale loss once per unit
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].ResetStance(); //reset a whole bunch of variables
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].CheckIfDead(); //if dead, remove it from the board position
        }
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].CheckIfRouting(); //if routing, remove it from the board position
        }
        /*for (int i = 0; i < AllPieces.Length; i++) //queue rout movement for routing units
        {
            //Debug.Log(AllPieces[i]);

            if (AllPieces[i].routing)
            {
                AllPieces[i].QueueRout();
            }
        }*/
        for (int i = 0; i < pieces.Count; i++) //reset pieces for movement next turn
        {
            pieces[i].FinishedMoving = false;
            pieces[i].oneStepFinished = false;

            pieces[i].startOfTurn = true;
        }
        //waitingToFinishTurn = true;
        secondsPassed = 0;
        StartCoroutine(WaitForAnimationsToBeOver(pieces));

    }

    public IEnumerator WaitForAnimationsToBeOver(List<Piece> pieces)
    {

        var num = 0;

        foreach (var piece in pieces)
        {
            Debug.LogError(piece + "piece animations over" + piece.animationsOver);
            if (piece.animationsOver)
            {
                num++; //if we find one that's done, add it to the count
            }
        }
        ////Debug.LogError("number done" + num);
        if (num >= pieces.Count || secondsPassed >= 5) //if all are done
        {
            secondsPassed = 0;
            AllowExecution();
        }
        else
        {
            yield return new WaitForSeconds(1);
            secondsPassed++;
            StartCoroutine(WaitForAnimationsToBeOver(pieces));
        }



    }

    public void AllowExecution()
    {

        //Debug.Log("Allowed Input again");
        chessController.AllowInput = true; //since turn is over, input is okay again
        executeButton.interactable = true; //and we can execute again

        Piece[] AllPieces = FindObjectsOfType<Piece>(); //this necessarily has to be checked by all units
        for (int i = 0; i < AllPieces.Length; i++) //needs to be set after we're done animation wise
        {
            AllPieces[i].targetToAttackPiece = null; // 
            //AllPieces[i].queuedMoves.Clear();
        }
    }

    public void SetDependencies(ChessGameController chessController, bool mp)
    {
        this.chessController = chessController;
        if (mp)
        {
            isChessControllerMP = true;
        }
        Debug.Log("Dependency set");
    }
    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
        routingGrid = new Piece[BOARD_SIZE, BOARD_SIZE];
        markerGrid = new Marker[BOARD_SIZE, BOARD_SIZE];
        markerGrid2 = new Marker[BOARD_SIZE, BOARD_SIZE];
        terrainGrid = new String[BOARD_SIZE, BOARD_SIZE];
    }

    private void Start()
    {/*
        Piece[] AllPieces = FindObjectsOfType<Piece>();
        for (int i = 0; i < AllPieces.Length; i++)
        {
            LinkGridsCallForward(AllPieces[i]);
        }*/
    }

    private void OnEnable()
    {
        //Debug.Log("Board enabled (used instead of start because networked");

        var gameInitObj = GameObject.Find("GameInitializer");
        gameInit = gameInitObj.GetComponent(typeof(GameInitializer)) as GameInitializer;

        TriggerSlowUpdate();

    }

    public abstract void TriggerSlowUpdate();

    public void OnTriggerSlowUpdate() //finished communicating with mp
    {
        ////Debug.LogError("triggered slow update");
        //StartCoroutine(SlowUpdate(1f));
        StartCoroutine(SlowUpdate(.5f));
    }


    public Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    public void OnSquareSelected(Vector3 inputPosition, int mouse) //called when player clicks on a board square
    {

        if (chessController == null)
        {
            return;
        }

        if (!chessController.AllowInput)
        {
            return;
        }
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition); //coords calculated from position
        Piece piece = GetPieceOnSquare(coords); //specific piece nabbed using new coords

        if (mouse == 0) //display unit info and select
        {
            Debug.Log("mouse 0");
            if (selectedPiece) //if selected piece exists
            {
                if (piece != null && selectedPiece == piece) //if we click on the same piece twice
                {
                    Debug.Log("same piece clicked");
                    var gridMarker = selectedPiece.thisMarkerGrid[coords.x, coords.y]; //fetch marker, if it's there
                    if (gridMarker != null) // if marker here
                    {
                        gridMarker.parentPiece.ClearQueuedMoves(); //clear the moves of that parent;
                    }

                    selectedPiece.ClearQueuedMoves(); //needs to come before because it will be deselected afterwards
                    DeselectPiece(); //deselect it and hide movement paths (deselected because we clicked it twice
                    return;
                }

                if (selectedPiece != null && selectingAction == true && !selectedPiece.moving) //if selected piece exists, and selecting action, and this piece is not moving. if we're still picking an action and we're not on default action MOVE
                {
                    return;
                }
                else if (selectedPiece != null && selectedPiece.attacking && selectedPiece.holdingPosition && selectedPiece.thisMarkerGrid[coords.x, coords.y] != null && selectedPiece.thisMarkerGrid[coords.x, coords.y].parentPiece != selectedPiece)
                {
                    Debug.Log("different marker found"); //if we click on a space with a marker not belonging to us

                    SelectPieceMoved(coords);

                }
                else if (selectedPiece != null && selectedPiece.thisMarkerGrid[coords.x, coords.y] != null && selectedPiece.thisMarkerGrid[coords.x, coords.y].parentPiece == selectedPiece ) //if you click on the same tile twice
                {// l  &&  && selectedPiece.attacking 
                    Debug.Log("clicked on a position where we already have a marker for movement and we're attacking"); //next check if marker belongs to us
                    if (selectedPiece.attacking && selectedPiece.attackType == "melee")
                    {
                        var lastMarkerVisual = selectedPiece.markerVisuals[selectedPiece.markerVisuals.Count - 1];

                        GameObject markerVisual = Instantiate(selectedPiece.arrowMarkerVisualPrefab, lastMarkerVisual.transform.position, Quaternion.identity);

                        Vector2Int directionVector = Vector2Int.zero;

                        if (selectedPiece.queuedMoves.Count == 1)
                        {
                            directionVector = coords - selectedPiece.occupiedSquare;
                        }
                        else if (selectedPiece.queuedMoves.Count > 1)
                        {
                            directionVector = coords - selectedPiece.queuedMoves[selectedPiece.queuedMoves.Count - 2];
                        }

                        var facingDirection = 0;
                        for (int t = 0; t < selectedPiece.adjacentTiles.Length; t++) //check cardinal directions to see if they match up
                        {
                            if (selectedPiece.adjacentTiles[t] == directionVector)
                            {
                                facingDirection = t;
                            }
                        }

                        Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0); //set rotation goal

                        markerVisual.transform.Rotate(rotationGoal);

                        Destroy(selectedPiece.markerVisuals[selectedPiece.markerVisuals.Count - 1]); //delete the last marker visual so we can replace it 

                        selectedPiece.markerVisuals.Add(markerVisual);
                    }
                    selectedPiece.holdingPosition = true;
                    //selectedPiece.holdTime = selectedPiece.turnTime;
                    DeselectPiece(); //deselect it and hide movement paths (deselected because we are ending our movement early)



                }
                else if (piece != null && selectedPiece != piece && !piece.IsFromSameTeam(selectedPiece)) //if we click on a different piece and it's an enemy and our selectedPiece is attacking
                {
                    if (selectedPiece.attacking || selectedPiece.speed == selectedPiece.sprintSpeed)
                    {
                        Debug.Log("Queueing move onto a position we know has an enemy"); //use selectPieceMoved instead of queuing directly
                        SelectPieceMoved(coords);
                        //selectedPiece.QueueMove(coords); //queue a move (but really an attack)
                        //DeselectPiece(); //deselect because attack should basically just stop

                    }

                }
                else if (piece != null && selectedPiece != piece && selectedPiece.turning)//if we're turning and there's a unit there, still queue
                {
                    Debug.Log("turning override");
                    SelectPieceMoved(coords);
                }
                else if (piece != null && selectedPiece != piece && piece.turnTime == 0) //if we click on a different piece, select that one and show movement paths
                {
                    if (isChessControllerMP) //if we're using a MP controller
                    {
                        if (chessController.localPlayer.team == piece.team) //, we need to check if this is on our team or not
                        {
                            SelectPiece(coords);
                            ChangeStance(piece.unitID, "move");
                        }
                    }
                    else //singleplayer
                    { //make it so you can only select the units on your team
                        SelectPiece(coords);
                        ChangeStance(piece.unitID, "move");
                    }

                }
                else if (piece != null && selectedPiece != piece && piece.turnTime > 0)//if we click on another piece with a higher turn time
                { //&& chessController.IsTeamTurnActive(piece.team) 
                  //allow us to queue a move to this position then
                    SelectPieceMoved(coords);
                    /*selectedPiece.QueueMove(coords);
                    if (selectedPiece.remainingMovement <= 0)
                    {
                        DeselectPiece();
                    }*/
                }
                else if (selectedPiece.CanMoveTo(coords)) //if we click somewhere we can move, 
                {
                    Debug.Log("clicked somewhere we can move");
                    SelectPieceMoved(coords); //place some sort of marker indicating that this piece will move there

                }
            }
            else
            {
                if (piece != null) //for clicking on a piece normally from empty. ie left click on piece with no piece already selected
                {
                    if (isChessControllerMP) //if we're using a MP controller
                    {
                        if (chessController.localPlayer.team == piece.team) //, we need to check if this is on our team or not
                        {
                            selectingAction = true;
                            SelectPiece(coords);
                            ChangeStance(piece.unitID, "move");
                        }
                    }
                    else //singleplayer
                    {
                        selectingAction = true;
                        SelectPiece(coords);
                        ChangeStance(piece.unitID, "move");
                    }
                }
            }
        }
        else if (mouse == 1) //display dropdown and change actions
        {
            Debug.Log("mouse 1");
        }






    }
    public void OnArbitrateConflict(int random)
    {
        var enemy = defendant;
        var friendly = prosecutor;
        if (enemy.arbitratedConflict || friendly.arbitratedConflict) //if either of these have already decided the conflict
        {
            return;
        }
        //var random = Random.Range(1, 3);//friendly.randomInitiative;
        ////Debug.LogError("Arbitrating conflict" + random);

        //var random = friendly.randomInitiative;
        //Debug.Log("Random value" + random);
        if (enemy.speed > friendly.speed) //start trying to break the tie, starting with speed
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else if (enemy.speed < friendly.speed)
        {
            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        if (enemy.attackType == "melee" && friendly.attackType == "ranged") //start trying to break the tie, starting with speed
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else if (enemy.attackType == "ranged" && friendly.attackType == "melee")
        {
            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        else if (enemy.energy > friendly.energy)
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else if (enemy.energy < friendly.energy)
        {
            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        else if (enemy.morale > friendly.morale)
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else if (enemy.morale < friendly.morale)
        {
            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        else if (enemy.models > friendly.models)
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else if (enemy.models < friendly.models)
        {
            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        else if (random == 1) //just a 50/50
        {

            friendly.wonTieBreak = true;
            enemy.wonTieBreak = false;
        }
        else if (random == 2) //just a 50/50
        {

            friendly.wonTieBreak = false;
            enemy.wonTieBreak = true;
        }
        else
        {
            friendly.wonTieBreak = false;
            enemy.wonTieBreak = false;
        }
        ////Debug.LogError("friendly " + friendly.wonTieBreak + "enemy " + enemy.wonTieBreak);
        friendly.arbitratedConflict = true;
        enemy.arbitratedConflict = true;
    }




    public void OnSelectedPieceMoved(Vector2Int coords) //this shows up in multiplayer
    {
        if (chessController.AllowInput)
        {
            selectedPiece.QueueMove(coords);//tell piece to remember these coords and place a marker there //got error
        }

        ////Debug.LogError("remainingmovement" + selectedPiece.remainingMovement + "turn time" + selectedPiece.turnTime);
        if (selectedPiece.attacking && selectedPiece.attackType == "ranged" && selectedPiece.moveAndAttackEnabled && selectedPiece.remainingMovement == 0) //&& selectedPiece.turnTime >= 1
        {//if move and attack enabled, disable after first movement queued
            //selectedPiece.moveAndAttackEnabled = false;
            selectedPiece.speed = selectedPiece.longRange; //increase speed so we can queue a second move
            selectedPiece.remainingMovement = selectedPiece.speed;

            //lets find that queued position
            Vector2Int queuedPosition = selectedPiece.occupiedSquare; //so this will let us determine the position after moves are applied
            for (int i = 0; i < selectedPiece.queuedMoves.Count; i++)
            {
                Vector2Int distance2 = queuedPosition - selectedPiece.queuedMoves[i]; //first find distance between current position and new position
                queuedPosition -= distance2; //then subtract this distance to get the new position again
            }
            //show selection squares from queued position
            ShowSelectionSquares(selectedPiece.SelectAvailableSquares(queuedPosition), selectedPiece);
        }
        else if (selectedPiece.attacking && selectedPiece.attackType == "ranged" && selectedPiece.moveAndAttackEnabled && selectedPiece.turnTime >= 2) //if ranged attacking piece is move and attacking, and turn time is >= 2
        { //deselect because we're done moving
            selectedPiece.markForDeselect = false;
            DeselectPiece();
        }
        else if (selectedPiece.attacking && selectedPiece.attackType == "ranged" && !selectedPiece.moveAndAttackEnabled)
        { //deselect after the first move is queued (steady attack)
            selectedPiece.markForDeselect = false;
            DeselectPiece();
        }
        
        else if (selectedPiece.remainingMovement <= 0 || selectedPiece.markForDeselect)
        {
            selectedPiece.markForDeselect = false;
            DeselectPiece();
        }

        /*
                var lastQueuedMoveNum = selectedPiece.queuedMoves.Count - 1;
                //if queued move is on hill
                var terrainTypeAtQueuedPos = terrainGrid[selectedPiece.queuedMoves[lastQueuedMoveNum].x, selectedPiece.queuedMoves[lastQueuedMoveNum].y];

                var penultimateQueuedMoveNum = selectedPiece.queuedMoves.Count - 2;

                var terrainTypeAtPenultimateQueuedPos = "grass";
                if (penultimateQueuedMoveNum < 0)
                {
                    terrainTypeAtPenultimateQueuedPos = selectedPiece.board.terrainGrid[selectedPiece.occupiedSquare.x, selectedPiece.occupiedSquare.y];
                    //Debug.LogError("terrain last" + terrainTypeAtQueuedPos + "terrain penult" + terrainTypeAtPenultimateQueuedPos);
                    //Debug.LogError("terrain last" + selectedPiece.queuedMoves[lastQueuedMoveNum] + "terrain penult" + selectedPiece.occupiedSquare);
                }
                else
                {
                    terrainTypeAtPenultimateQueuedPos = terrainGrid[selectedPiece.queuedMoves[penultimateQueuedMoveNum].x, selectedPiece.queuedMoves[penultimateQueuedMoveNum].y];
                    //Debug.LogError("terrain last" + terrainTypeAtQueuedPos + "terrain penult" + terrainTypeAtPenultimateQueuedPos);
                    //Debug.LogError("terrain last" + selectedPiece.queuedMoves[lastQueuedMoveNum] + "terrain penult" + selectedPiece.queuedMoves[penultimateQueuedMoveNum]);
                }

                if (terrainTypeAtQueuedPos == "hill" && terrainTypeAtQueuedPos != "hill") //if we queue a move onto a hill from non hill
                {

                }*/





        //TryToTakeOppositePiece(coords);
        //UpdateBoardOnPieceMove(coords, selectedPiece.occupiedSquare, selectedPiece, null);
        //selectedPiece.MovePiece(coords);
        //DeselectPiece();
        //EndTurn();
    }

    public void OnSetSelectedPiece(Vector2Int coords) //this shows up in multiplayer
    {
        Piece piece = GetPieceOnSquare(coords);


        if (chessController.localPlayer == null || chessController.localPlayer.team == piece.team) //if sp or matches team in mp
        {
            selectedPiece = piece;
            ui.ShowUnitInfoScreen(selectedPiece);

            foreach (var selector in instantiatedSelectors) //there can only be one
            {
                Destroy(selector);
            }
            instantiatedSelectors.Clear();
            GameObject selectionIndicator = Instantiate(selectionIndicatorPrefab, selectedPiece.transform.position, Quaternion.identity);
            Vector3 temp = new Vector3(0, 0.01f, 0);
            selectionIndicator.transform.position += temp;
            instantiatedSelectors.Add(selectionIndicator);
            //disable inputs until action selected
            //chessController.AllowInput = false;
            gameInit.ShowDropDown();
            //selectedPiece.randomInitiative = Random.Range(1, 3);
            //piece.SetMaterial(testMat);
        }



    }


    public void DeselectPiece()
    {
        selectedPiece.DisplayFormation(selectedPiece.queuedFormation);
        selectedPiece = null;
        squareSelector.ClearSelection();

        ui.HideUnitInfoScreen();
        gameInit.HideDropDown();
        foreach (var selector in instantiatedSelectors)
        {
            Destroy(selector);
        }
        instantiatedSelectors.Clear();
    }


    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsFromSameTeam(piece))
        {
            TakePiece(piece);
        }
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
        }
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece) //normal move means old piece is null
    {
        if (newCoords.x < 0 || newCoords.x > BOARD_SIZE || newCoords.y < 0 || newCoords.y > BOARD_SIZE)
        {
            return;
        }
        /*if (newPiece.routing)
        {
            routingGrid[oldCoords.x, oldCoords.y] = oldPiece;
            routingGrid[newCoords.x, newCoords.y] = newPiece;
        }*/
        //else
        //{

        //Debug.Log("Update");
        //we should take care not to overwrite pieces:
        if (grid[oldCoords.x, oldCoords.y] == newPiece) //if old piece was us (this prevents erasing units)
        {
            grid[oldCoords.x, oldCoords.y] = oldPiece;
        }


        //Debug.Log(oldCoords + " " + newCoords);
        grid[newCoords.x, newCoords.y] = newPiece;
        //}
    }


    private void SelectPiece(Vector2Int coords) //piece selection on left click
    {
        Piece piece = GetPieceOnSquare(coords);

        piece.PlaySelection();
        //chessController.RemoveMovesEnablingAttackOnPieceOfType<King>(piece);
        SetSelectedPiece(coords); //after this we can call selectedpiece
        List<Vector2Int> selection = selectedPiece.availableMoves;
        ShowSelectionSquares(selection, piece);

        foreach (var item in piece.instantiatedCylinders)
        {
            Destroy(item);
        }
        piece.instantiatedCylinders.Clear();
    }

    public void UpdateUIManager()
    {
        if (chessController.localPlayer == null) //if single player
        {

        }
        else if (selectedPiece != null && chessController.localPlayer.team == selectedPiece.team) //if mp and selectedpiece exists and on our team
        {

            ui.ShowUnitInfoScreen(selectedPiece);
        }
    }

    public void ShowSelectionSquares(List<Vector2Int> selection, Piece piece) //this function shows squares available for movement, attacking, etc. "selection" is defined as selectedPiece.availableMoves or something similar
    {
        if (chessController.localPlayer == null) //for single player
        {
            Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
            for (int i = 0; i < selection.Count; i++)
            {
                Vector3 position = CalculatePositionFromCoords(selection[i]);
                bool isSquareFree = GetPieceOnSquare(selection[i]) == null; //detects whether or not a square is free. if == null, then returns true and square is free. oddly, changing this code seems to do nothing
                squaresData.Add(position, isSquareFree); //updates data, basically just says whether or not squares are free at specific pos

            }
            squareSelector.ShowSelection(squaresData); //then show squares based on data
            squareSelector.UpdateSelection(squaresData);
        }
        else if (chessController.localPlayer.team == piece.team) //for multiplayer
        {
            Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
            for (int i = 0; i < selection.Count; i++)
            {
                Vector3 position = CalculatePositionFromCoords(selection[i]);
                bool isSquareFree = GetPieceOnSquare(selection[i]) == null; //detects whether or not a square is free. if == null, then returns true and square is free. oddly, changing this code seems to do nothing
                squaresData.Add(position, isSquareFree); //updates data, basically just says whether or not squares are free at specific pos

            }
            squareSelector.ShowSelection(squaresData); //then show squares based on data
            squareSelector.UpdateSelection(squaresData);
        }
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatedAreOnBoard(coords))
        {
            //Debug.Log(grid[coords.x, coords.y]);
            return grid[coords.x, coords.y];
        }
        return null;
    }

    public bool CheckIfCoordinatedAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
        {
            return false;
        }
        return true;
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + 4; //don't have this scale with board size, numpty
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + 4;
        Debug.Log(x + " " + y);
        return new Vector2Int(x, y);
    }

    internal void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordinatedAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

}
