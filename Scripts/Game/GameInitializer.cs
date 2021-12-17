using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameInitializer : MonoBehaviour
{
    [Header("Game mode dependent objects")]
    [SerializeField] private SingleplayerChessGameController singleplayerChessGameControllerPrefab;
    [SerializeField] private MultiplayerChessGameController multiplayerChessGameControllerPrefab;
    [SerializeField] private SinglePlayerBoard singleplayerBoardPrefab;
    [SerializeField] private MultiplayerBoard multiplayerBoardPrefab;

    [Header("Scene references")]

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private Transform boardAnchor;
    [SerializeField] private CameraSetup cameraSetup;
    [SerializeField] private LevelGenerator levelGen;

    [SerializeField] public GameObject dropDownParent;
    [SerializeField] public TMP_Dropdown actionDropdown;
    [SerializeField] public GameObject executeButtonParent;
    [SerializeField] public GameObject unreadyButtonParent;
    [SerializeField] public GameObject deselectButtonParent;
    [SerializeField] public GameObject formationDropDownParent;
    [SerializeField] public TMP_Dropdown formationDropDown;
    [SerializeField] public TMP_Dropdown attitudeDropDown;
    [SerializeField] public GameObject attitudeDropDownParent;
    Camera cam;
    private ChessGameController chessController;

    //[Header("Team colors")]
    public Material[] teamColors;
    public Material[] unlitTeamUI;

    public TeamColor[] teamColorDefinitions;

    public Material disengageMaterial;
    public Material attackMaterial;
    //[SerializeField] private Material sprintMaterial;
    public Material turnMaterial;
    public Material orangeMaterial;
    public Material defaultMaterial;
    public Material red;
    public Material yellow;

    public string action = "move";
    public string formation = "rectangle";
    public Board board;

    public Sprite[] icons;

    public GameObject modelsParent;

    public SaveInfo saveInfoObject;
    public UIButton unitButtonTemplate;

    public GameObject unitOptionsParent;
    public UIButton cancelPlaceUnitButton;

    public GameObject placingUnitsAlertText;
    public UIButton confirmButton;
    public GameObject image;

    public GameObject dirButtonParent;
    public BoardLayout[] levels;
    public BoardLayout boardLevel;

    public GameObject strafeCam;
    public GameObject cinematicCam;

    public void SelectLevel(string strLevel)
    {
        foreach (var level in levels)
        {
            Debug.Log(level.ToString());
            if (level.ToString() == strLevel + " (BoardLayout)")
            {
                boardLevel = level;
            }
        }
    }
    public void CancelPlacement()
    {
        board.readyToPlaceUnit = false;
        cancelPlaceUnitButton.gameObject.SetActive(false);
        foreach (var button in board.unitButtonsList)
        {
            button.gameObject.SetActive(true);
        }
        dirButtonParent.SetActive(false);
    }

    public void ConfirmPlacement()
    {
        placingUnitsAlertText.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        image.SetActive(false);
        foreach (var item in board.unitButtonsList)
        {
            item.gameObject.SetActive(false);
        }
        executeButtonParent.SetActive(true);
        unreadyButtonParent.SetActive(true);
        board.placingPieces = false;
        dirButtonParent.SetActive(false);
        foreach (var item in levelGen.placementTilesList)
        {
            item.SetActive(false);
        }
    }

    public void ChangeOrientation(int dir)
    {
        board.tempDir = dir;
    }


    private void Start()
    {
        var camObj = GameObject.Find("Main Camera");
        cam = camObj.GetComponent(typeof(Camera)) as Camera;
        StartCoroutine(SlowUpdate());
    }
    public void CreateMultiplayerBoard()
    {
        executeButtonParent.SetActive(true);
        unreadyButtonParent.SetActive(true);
        if (!networkManager.IsRoomFull())
        {
            //only first player instantiates the board
            PhotonNetwork.Instantiate(multiplayerBoardPrefab.name, boardAnchor.position, boardAnchor.rotation);
            levelGen.FindBoard();
            levelGen.GenerateLevel();
        }
        //both players need to find this board, then do level generation stuff
        //FindBoard();
        //disabled because it seems like it has problems finding it with this;
    }
    private void FindBoard()
    {

        board = FindObjectOfType<SinglePlayerBoard>();
        if (board == null) //couldn't find a singleplayer board
        {
            Debug.Log("SP board not found");
            board = FindObjectOfType<MultiplayerBoard>();
        }

        if (board != null)
        {
            Debug.Log("Board found");
        }
        //once you've found the board do level generation
        //levelGen.FindBoard();
        //levelGen.GenerateLevel();

    }

    public void CreateSinglePlayerBoard()
    {
        executeButtonParent.SetActive(true);
        //unreadyButtonParent.SetActive(true);


        Instantiate(singleplayerBoardPrefab, boardAnchor);
        levelGen.FindBoard();
        levelGen.GenerateLevel();
        FindBoard();
    }
    public void InitializeMultiplayerController()
    {

        Debug.Log("instantiating multiplayer controller");
        MultiplayerChessGameController controller = Instantiate(multiplayerChessGameControllerPrefab);
        if (controller != null)
        {
            Debug.Log("instantiated");
        }

        controller.SetDependencies(uiManager, board, cameraSetup);
        controller.CreatePlayers();
        controller.SetNetworkDependencies(networkManager);
        networkManager.SetDependencies(controller);

        if (board != null)
        {
            Debug.Log("Board" + board + "controller" + controller);
        }
        if (board == null)
        {
            Debug.Log("Board missing");
        }
        if (controller == null)
        {
            Debug.Log("controller missing");
        }
        board.SetDependencies(controller, true);
        chessController = controller;
    }

    /*private void Update()
    {
        FindBoard();
    }*/
    private IEnumerator SlowUpdate()
    {
        if (board == null) //if no board find it!
        {
            FindBoard();
        }
        yield return new WaitForSeconds(0.1f);
        if (board == null) //if still no board try again
        {
            StartCoroutine(SlowUpdate());
        }
        else
        {
            levelGen.FindBoard();
            levelGen.GenerateLevel();
        }
    }

    public void InitializeSinglePlayerController()
    {
        SingleplayerChessGameController controller = Instantiate(singleplayerChessGameControllerPrefab);
        controller.startingBoardLayout = boardLevel;
        controller.SetDependencies(uiManager, board, cameraSetup);
        controller.CreatePlayers();
        board.SetDependencies(controller, false);
        controller.StartNewGame();

        chessController = controller;
    }
    public void DeselectPiece()
    {
        board.DeselectPiece();
    }
    public void SubmitExecute() //prevent further inputs from the one that clicked this
    {
        //then check if both players have clicked button
        //then tell board to update pieces positions
        //then clear any errant selectors

        if (board.selectedPiece != null) //do not allow unless we do not have a unit selected.
        {
            return;
        }

        Debug.Log("Executing moves (SP)");
        board.squareSelector.ClearSelection(); //clear selector squares
        board.selectedPiece = null; //so that you can't queue movement erroneously
        board.ExecuteMoveForAllPieces();
    }

    public void Unexecute()
    {
        board.Unready();
    }


    public void PieceSprint()
    {

        board.ChangeStance(board.selectedPiece.unitID, "sprint");
        Debug.Log("Sprint");

    }
    public void PieceMove()
    {

        board.ChangeStance(board.selectedPiece.unitID, "move");
        Debug.Log("move");

    }
    public void PieceDisengage()
    {


        board.ChangeStance(board.selectedPiece.unitID, "disengage");
        Debug.Log("disengage");


    }
    public void PieceAttack()
    {

        board.ChangeStance(board.selectedPiece.unitID, "attack");
        Debug.Log("attack");

    }
    public void PieceMoveAttack()
    {

        board.ChangeStance(board.selectedPiece.unitID, "MoveAttack");
        Debug.Log("MoveAttack");

    }

    public void PieceTurn()
    {
        board.ChangeStance(board.selectedPiece.unitID, "turn");
        Debug.Log("turn");

    }
    public void ShowDropDown()
    {
        deselectButtonParent.SetActive(true);
        dropDownParent.SetActive(true);
        actionDropdown.value = 0;
        board.selectedPiece.DisplayFormation(board.selectedPiece.queuedFormation);

        List<TMP_Dropdown.OptionData> dropData = new List<TMP_Dropdown.OptionData>();


        //List<string> dropOptions = new List<string> { "Move", "Attack", "Switch Formation", "Sprint", "Disengage" };
        if (board.selectedPiece.attackType == "ranged")
        {
            //dropOptions = new List<string> { "Move", "Steady Attack", "Move and Attack", "Switch Formation", "Sprint" }; 
            var option1 = new TMP_Dropdown.OptionData("Move", icons[0]);
            dropData.Add(option1);
            var option2 = new TMP_Dropdown.OptionData("Steady Attack", icons[5]);
            dropData.Add(option2);
            var option6 = new TMP_Dropdown.OptionData("Move and Attack", icons[6]);
            dropData.Add(option6);
            var option4 = new TMP_Dropdown.OptionData("Switch Formation", icons[3]);
            dropData.Add(option4);
            var option3 = new TMP_Dropdown.OptionData("Sprint", icons[2]);
            dropData.Add(option3);
        }
        else if (board.selectedPiece.attackType == "melee" && board.selectedPiece.unitType == "cavalry")
        {
            //dropOptions = new List<string> { "Move", "Attack", "Switch Formation", "Sprint", "Disengage" };
            var option1 = new TMP_Dropdown.OptionData("Move", icons[0]);
            dropData.Add(option1);
            var option2 = new TMP_Dropdown.OptionData("Attack", icons[1]);
            dropData.Add(option2);
            var option4 = new TMP_Dropdown.OptionData("Switch Formation", icons[3]);
            dropData.Add(option4);
            var option3 = new TMP_Dropdown.OptionData("Sprint", icons[2]);
            dropData.Add(option3);
            var option5 = new TMP_Dropdown.OptionData("Disengage", icons[4]);
            dropData.Add(option5);
        }
        else if (board.selectedPiece.attackType == "melee")
        {
            //dropOptions = new List<string> { "Move", "Attack", "Switch Formation", "Sprint" };
            var option1 = new TMP_Dropdown.OptionData("Move", icons[0]);
            dropData.Add(option1);
            var option2 = new TMP_Dropdown.OptionData("Attack", icons[1]);
            dropData.Add(option2);
            var option4 = new TMP_Dropdown.OptionData("Switch Formation", icons[3]);
            dropData.Add(option4);
            var option3 = new TMP_Dropdown.OptionData("Sprint", icons[2]);
            dropData.Add(option3);
        }

        actionDropdown.ClearOptions();
        actionDropdown.AddOptions(dropData);

    }
    public void HideDropDown()
    {
        deselectButtonParent.SetActive(false);
        dropDownParent.SetActive(false);
        formationDropDownParent.SetActive(false);
        attitudeDropDownParent.SetActive(false);

    }
    public void ChooseAction()
    {
        if (board.selectedPiece == null)
        {
            return;
        }
        int val = actionDropdown.value;
        if (board.selectedPiece.attackType == "melee")
        {
            if (val == 0)
            {
                action = "move";
            }
            else if (val == 1)
            {
                action = "attack"; //becomes steady attack when ranged
            }
            else if (val == 2)
            {
                action = "switchFormation";
            }
            else if (val == 3)
            {
                action = "sprint";
            }
            else if (val == 4)
            {
                action = "disengage";
            }
        }
        else if (board.selectedPiece.attackType == "ranged")
        {
            if (val == 0)
            {
                action = "move";
            }
            else if (val == 1)
            {
                action = "attack"; //becomes steady attack when ranged
            }
            else if (val == 2)
            {
                action = "rangedMoveAndAttack"; //move and attack
            }
            else if (val == 3)
            {
                action = "switchFormation";
            }
            else if (val == 4)
            {
                action = "sprint";
            }
            else if (val == 5)
            {
                action = "disengage";
            }
        }

        //Debug.Log(action);
        ProcessAction(action);
    }
    public void ChooseFormation()
    {
        if (board.selectedPiece == null)
        {
            return;
        }
        int val = formationDropDown.value;
        if (val == 0)
        {
            formation = "nothing";
        }
        else if (val == 1)
        {
            formation = "rectangle";
        }
        else if (val == 2)
        {
            formation = "staggered";
        }
        else if (val == 3)
        {
            formation = "circle";
        }
        else if (val == 4)
        {
            formation = "braced";
        }
        //board.selectedPiece.ChangeFormation(formation);
        //board.selectedPiece.queuedFormation = formation;

        if (val != 0)
        {
            board.ChangeFormation(board.selectedPiece.unitID, formation);
            board.selectedPiece.DisplayFormation(formation);



            //board.selectedPiece.DisplayFormation(formation);
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.DeselectPiece();
            dropDownParent.SetActive(false);
        }

    }

    public void ChooseAttitude()
    {
        if (board.selectedPiece == null)
        {
            return;
        }
        int val = attitudeDropDown.value;
        bool aggressive = true;
        if (val == 0)
        {
            aggressive = true;
        }
        else if (val == 1)
        {
            aggressive = false;
        }
        board.ChangeAttitude(board.selectedPiece.unitID, aggressive);

    }


    private void ProcessAction(string action)
    {
        if (action == null || action == "move")
        {
            PieceMove();
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.selectedPiece.queuedFormation = "nothing";
            attitudeDropDownParent.SetActive(false);
        }
        else if (action == "attack")
        {
            PieceAttack();
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.selectedPiece.queuedFormation = "nothing";
            if (board.selectedPiece.attackType == "melee")
            {

                attitudeDropDownParent.SetActive(true);

            }
        }
        else if (action == "rangedMoveAndAttack")
        {
            PieceMoveAttack();
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.selectedPiece.queuedFormation = "nothing";
            attitudeDropDownParent.SetActive(false);
        }
        else if (action == "switchFormation")
        {
            /*
                        List<string> dropOptions = new List<string> { "Select a formation", "Rectangle", "Staggered", "Circle", "Braced" };
                        if (board.selectedPiece.unitName == "Conscript")
                        {
                            dropOptions = new List<string> { "Select a formation", "Rectangle", "Staggered", "Circle", };
                        }
                        else if (board.selectedPiece.unitName == "Longbowman")
                        {
                            dropOptions = new List<string> { "Select a formation", "Rectangle", "Staggered" };
                        }
                        else if (board.selectedPiece.unitName == "Ritter")
                        {
                            dropOptions = new List<string> { "Select a formation", "Rectangle", "Staggered" };
                        }*/


            List<TMP_Dropdown.OptionData> dropData = new List<TMP_Dropdown.OptionData>();
            if (board.selectedPiece.unitType == "infantry" && board.selectedPiece.attackType == "melee") //not including brace yet
            {
                var option1 = new TMP_Dropdown.OptionData("Select a formation", icons[10]);
                dropData.Add(option1);
                var option2 = new TMP_Dropdown.OptionData("Rectangle", icons[7]);
                dropData.Add(option2);
                var option6 = new TMP_Dropdown.OptionData("Staggered", icons[8]);
                dropData.Add(option6);
                var option4 = new TMP_Dropdown.OptionData("Circle", icons[9]);
                dropData.Add(option4);
            }
            else if (board.selectedPiece.unitType == "infantry" && board.selectedPiece.attackType == "ranged")
            {
                var option1 = new TMP_Dropdown.OptionData("Select a formation", icons[10]);
                dropData.Add(option1);
                var option2 = new TMP_Dropdown.OptionData("Rectangle", icons[7]);
                dropData.Add(option2);
                var option6 = new TMP_Dropdown.OptionData("Staggered", icons[8]);
                dropData.Add(option6);
            }
            else if (board.selectedPiece.unitType == "cavalry")
            {
                var option1 = new TMP_Dropdown.OptionData("Select a formation", icons[10]);
                dropData.Add(option1);
                var option2 = new TMP_Dropdown.OptionData("Rectangle", icons[7]);
                dropData.Add(option2);
                var option6 = new TMP_Dropdown.OptionData("Staggered", icons[8]);
                dropData.Add(option6);
            }

            formationDropDown.ClearOptions();
            formationDropDown.AddOptions(dropData);

            formationDropDown.value = 0;
            formationDropDownParent.SetActive(true);
            attitudeDropDownParent.SetActive(false);


            if (board.selectedPiece != null)
            {
                board.selectedPiece.ClearQueuedMoves();
                board.selectedPiece.speed = 0;
                board.selectedPiece.remainingMovement = 0;
                board.ShowSelectionSquares(board.selectedPiece.SelectAvailableSquares(board.selectedPiece.occupiedSquare), board.selectedPiece);
            }
        }
        else if (action == "sprint")
        {
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.selectedPiece.queuedFormation = "nothing";
            PieceSprint();
            attitudeDropDownParent.SetActive(false);
        }
        else if (action == "disengage")
        {
            board.selectingAction = false;
            formationDropDownParent.SetActive(false);
            board.selectedPiece.queuedFormation = "nothing";
            PieceDisengage();
            attitudeDropDownParent.SetActive(false);
        }
    }
}
