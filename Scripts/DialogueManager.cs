using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    [Header("Needs to be accessible to to other files")]
    public DialogueScriptableObject loadedDialogue; //needs to be changed whenever you want to swap dialogues
    public bool readingDialogue = false;

    [Header("Needs to be set manually")]
    [SerializeField] private OverworldManager overworldManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject targetPosObj;
    [SerializeField] private GameInitializer gameInit;
    [SerializeField] private ChessUIManager chessUI;
    [SerializeField] private GameObject dialogueParent;
    [SerializeField] private List<UnitListScriptableObject> levelUnitLists;
    [SerializeField] private Transform cameraPosTutorial;
    [SerializeField] private bool runningText = false;
    [SerializeField] private DialogueScriptableObject[] conversationStarters;
    [SerializeField] private bool autoStart = false;
    [SerializeField] private GameObject choicesParent;
    [SerializeField] private List<TMP_Text> choiceList;
    //[SerializeField] private Image speakerBGImage;
    [SerializeField] private List<Image> speakerBorders;
    [SerializeField] private Image speakerFancyBorder;
    [SerializeField] private Image fadeToBlack;
    [SerializeField] private GameObject cinematicParent;
    [SerializeField] private Image speakerImage;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private LocationAndFactionPointsManager LFPManager;
    [SerializeField] private SutlerManager sutlerManager;

    //private variables
    private float slowTextSpeed;
    private float slowerTextSpeed;
    private float textspeed = 0.1f;
    private AudioSource audioSource;
    private Queue<string> sentences;
    private Queue<AudioClip> sentenceAudio;
    private string currentSentence;
    private AudioClip currentAudio;
    private int sentenceCount = 0;
    private float startingYPos;
    private npcAnimController npcAnimController;
    private bool displayingChoices = false;
    private bool checkedCondition = false;

    private void Start()
    {
        //define text speeds algorithmically
        UpdateTextSpeeds();
        startingYPos = dialogueParent.transform.position.y; //get starting y position
        audioSource = GetComponent<AudioSource>();
        sentences = new Queue<string>(); //A queue is first in last out

        if (autoStart) //start dialogue immediately
        {
            StartCoroutine(LateStart(1));
        }
    }
    private IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (loadedDialogue.isChoices)
        {
            PresentChoices();
        }
        else
        {
            StartDialogue();
        }
    }
    private void UpdateTextSpeeds()
    {
        slowTextSpeed = textspeed * 8;
        slowerTextSpeed = textspeed * 12;
    }
    public void SelectDialogue(string dialogue) //used in tutorial to load then read dialogue
    {
        foreach (var convo in conversationStarters)
        {

            //Debug.Log(convo.ToString());
            if (convo.ToString() == dialogue + " (DialogueScriptableObject)")
            {
                loadedDialogue = convo;

                if (loadedDialogue.isChoices)
                {
                    PresentChoices();

                }
                else
                {
                    StartDialogue();
                }
                break;
            }
        }
    }
    public void StartDialogue() //used to raise loaded dialogue and read it (assuming that dialogue screen is lowered)
    {
        readingDialogue = true;
        ForceChangeSpeaker(loadedDialogue.forceChangeSpeaker);
        dialogueText.text = "";
        speakerText.text = loadedDialogue.speaker;

        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad); //tweens dialogue up

        if (loadedDialogue.isChoices)
        {
            choicesParent.SetActive(true);
            PresentChoices();
        }
        else
        {
            tween.OnComplete(ReadDialogue);

        }

    }
    private void ReadDialogue() //begins actually reading dialogue
    {
        sentences.Clear();

        foreach (string sentence in loadedDialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
        ProcessStartCommand();
    }
    
    private void ProcessStartCommand() //if command needed at beginning of dialogue
    {
        //Debug.LogError("Processing commands");
        if (loadedDialogue.commandToExecuteStart == "nod")
        {
            npcAnimController.AnimNod();
        }
        if (loadedDialogue.commandToExecuteStart == "confused")
        {
            npcAnimController.AnimConfused();
        }
    }
    
    private void DisplayNextSentence() //display next "sentence" (a chunk of dialogue)
    {
        if (loadedDialogue.isChoices)
        {
            return;
        }

        if (loadedDialogue.italicizedSentences.Count > 0 && sentenceCount < loadedDialogue.italicizedSentences.Count) //change italicization
        {
            if (loadedDialogue.italicizedSentences[sentenceCount] == true)
            {
                dialogueText.fontStyle = FontStyles.Italic;
            }
            else
            {

                dialogueText.fontStyle = FontStyles.Normal;
            }
        }
        if (loadedDialogue.forceChangeSpeaker && loadedDialogue.speakerSentences.Count > 0 && sentenceCount < loadedDialogue.speakerSentences.Count) //change speaker
        {
            if (loadedDialogue.speakerSentences[sentenceCount] != null)
            {
                SpeakerScriptable speaker = loadedDialogue.speakerSentences[sentenceCount];
                //speakerBGImage.color = speaker.colorBorder;
                foreach (Image image in speakerBorders)
                {
                    image.color = speaker.colorBorder;
                }
                speakerFancyBorder.color = speaker.fancyBorder;
                textspeed = speaker.speed;
                speakerImage.sprite = speaker.image;
                speakerText.text = speaker.speakerName;
                UpdateTextSpeeds();
            }
        }

        audioSource.Stop();
        runningText = true;
        if (sentences.Count == 0) //end of queue
        {

            runningText = false;
            EndDialogue();
            return;
        }
        if (loadedDialogue.sentenceAudio.Length > sentenceCount) //length 3, count 2
        {
            currentAudio = loadedDialogue.sentenceAudio[sentenceCount];
        }
        else
        {
            currentAudio = null;
        }
        sentenceCount++;
        currentSentence = sentences.Dequeue();

        //Debug.Log(currentSentence);
        //dialogueText.text = sentence;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }
    private IEnumerator TypeSentence(string sentence) //reveal characters of sentence one by one
    {
        if (currentAudio != null)
        {

            audioSource.clip = currentAudio;
            audioSource.Play();
        }

        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        char[] charArray = sentence.ToCharArray();
        bool skip = false;
        for (int i = 0; i < charArray.Length; i++)
        {
            dialogueText.maxVisibleCharacters++;
            char letter = charArray[i];


            if (letter == '<')
            {
                skip = true;
            }
            else if (letter == '>')
            {
                skip = false;
            }

            if (skip)
            {
                yield return new WaitForSeconds(0);
            }
            else
            {
                if (letter == '.' || letter == ';' || letter == '?' || letter == '!')
                {
                    if (i + 1 < charArray.Length && charArray[i + 1] == ')')
                    {
                        yield return new WaitForSeconds(textspeed);

                    }
                    else
                    {
                        yield return new WaitForSeconds(slowerTextSpeed);
                    }
                }
                else if (letter == ',')
                {
                    yield return new WaitForSeconds(slowTextSpeed);

                }
                else
                {
                    yield return new WaitForSeconds(textspeed);
                }
            }
        }
        //audioSource.Stop();
        runningText = false;
    }
    public void FinishSentence() //this is triggered when the user clicks on the dialogue box
    {
        if (displayingChoices)
        {
            return;
        }
        StopAllCoroutines();
        dialogueText.text = currentSentence;
        dialogueText.maxVisibleCharacters = currentSentence.ToCharArray().Length;
        if (runningText == false)
        {
            DisplayNextSentence();
        }
        else
        {
            runningText = false;
        }

    }

    private void ProcessEndCommand()
    {
        checkedCondition = false;
        string commandEnd = loadedDialogue.commandToExecuteEnd;
        if (commandEnd == "startTutorial")
        {
            StartTutorial();
        }
        if (commandEnd == "startDayOfGlory")
        {
            //Debug.Log("Starting day of glory");
            gameInit.strafeCam.SetActive(true);
            gameInit.cinematicCam.SetActive(false);
        }
        if (commandEnd == "startTutorialPlacement")
        {
            StartTutorialPlacement();
        }
        if (commandEnd == "enableChessControllerInput")
        {
            gameInit.chessController.AllowInput = true;
        }
        if (commandEnd == "enableExecution")
        {
            gameInit.executeButtonParent.SetActive(true);
        }
        if (commandEnd == "ignoreTutorial")
        {
            IgnoreTutorial();
        }
        if (commandEnd == "endTutorial")
        {
            EndTutorial();
        }
        if (commandEnd == "moraleGain")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.overallMorale += loadedDialogue.commandVar;
                if (overworldManager.localeArmy.overallMorale > overworldManager.localeArmy.maxMorale)
                {
                    overworldManager.localeArmy.overallMorale = overworldManager.localeArmy.maxMorale;
                }
                if (overworldManager.localeArmy.overallMorale < 0)
                {
                    overworldManager.localeArmy.overallMorale = 0;
                }
            }
        }
        if (commandEnd == "supplyGain")
        {
            if (overworldManager.localeArmy.provisions < overworldManager.localeArmy.maxProvisions)
            {
                overworldManager.localeArmy.provisions += loadedDialogue.commandVar;
            }
        }
        if (commandEnd == "destroyLocale")
        {
            if (overworldManager.localeArmy != null && overworldManager.localeArmy.currentLocale != null)
            {
                overworldManager.localeArmy.currentLocale.destroyed = true;
                overworldManager.localeArmy.currentLocale.UpdateAppearance();
            }
        }
        if (commandEnd == "multVisionRange")
        {
            //Debug.LogError("MULT");
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.fowUnit.circleRadius = .5f + (overworldManager.localeArmy.sightRadius * loadedDialogue.commandVar);
            }
        }
        if (commandEnd == "gainMaxSupply")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.maxProvisions += loadedDialogue.commandVar;
            }
        }
        if (commandEnd == "gainHorses")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.horses += loadedDialogue.commandVar;
            }
        }
        if (commandEnd == "giveUnit")
        {
            if (overworldManager.localeArmy != null)
            {
                for (int i = 0; i < loadedDialogue.commandVar; i++)
                {
                    foreach (ArmyCardScriptableObj card in unitManager.units)
                    {
                        if (card.cardName == loadedDialogue.commandString)
                        {
                            overworldManager.localeArmy.AddArmyCard(card);
                        }
                    }
                }
            }
        }
        if (commandEnd == "makeAvailableSupplyTown")
        {
            if (overworldManager.localeArmy != null)
            {
                if (overworldManager.localeArmy.currentSupplyPoint != null)
                {
                    overworldManager.localeArmy.currentSupplyPoint.reservedProvisions -= loadedDialogue.commandVar;
                }
            }
        }
        if (commandEnd == "arrestDernoth")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.arrestedDernoth = true;
            }
        }

        if (commandEnd == "arrestButcher")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.arrestedButcher = true;
            }
        }

        if (commandEnd == "dernothMapAdvice")
        {

        }

        if (commandEnd == "checkVisited")
        {
            //Debug.LogError("checking");
            string location = loadedDialogue.commandString;
            bool visited = LFPManager.CheckIfVisited(location);
            //Debug.LogError("bool" + visited);
            if (visited && loadedDialogue.dialogueIfConditionTrue != null)
            {
                checkedCondition = true;
            }
        }
        if (commandEnd == "checkHelped")
        {
            string npcName = loadedDialogue.commandString;
            bool helped = LFPManager.CheckIfHelped(npcName);
            if (helped && loadedDialogue.dialogueIfConditionTrue != null)
            {
                checkedCondition = true;
            }
        }
        if (commandEnd == "tradeSutler")
        {
            sutlerManager.ShowSutlerScreen();
            sutlerManager.tradingInDialogue = true;
        }
        if (commandEnd == "helpNPC")
        {
            Debug.LogError(loadedDialogue.commandString);
            LFPManager.SetHelped(loadedDialogue.commandString);
        }
        if (commandEnd == "gainSutler")
        {
            overworldManager.sutlerParent.SetActive(true);
        }

    }
    private void EndDialogue()
    {
        
        ProcessEndCommand();
        sentenceCount = 0;
        if (loadedDialogue.nextDialogue == null)
        {
            readingDialogue = false;
            //Debug.Log("End of conversation");
            //dialogueParent.SetActive(false);
            Tween tween = dialogueParent.transform.DOMove(new Vector3(dialogueParent.transform.position.x, startingYPos, dialogueParent.transform.position.z), .5f).SetEase(Ease.InOutQuad);
        }
        else if (checkedCondition)
        {
            StartNextDialogue(loadedDialogue.dialogueIfConditionTrue);
        }
        else
        {
            StartNextDialogue(loadedDialogue.nextDialogue);
        }
    }
    private void StartNextDialogue(DialogueScriptableObject dialogue) //loads next dialogue and reads it
    {
        loadedDialogue = dialogue;

        if (loadedDialogue.isChoices)
        {
            PresentChoices();
        }
        else
        {
            choicesParent.SetActive(false);
            ForceChangeSpeaker(loadedDialogue.forceChangeSpeaker);
            //Debug.Log("Starting conversation");
            sentences.Clear();

            foreach (string sentence in loadedDialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }

            DisplayNextSentence();
        }
        ProcessStartCommand();
    }
    private void PresentChoices()
    {
        displayingChoices = true;
        //Debug.Log("presenting choices");
        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        dialogueText.text = "";

        ForceChangeSpeaker(loadedDialogue.forceChangeSpeaker);
        choicesParent.SetActive(true);

        var i = 0;
        foreach (var item in choiceList)
        {
            if (i < loadedDialogue.sentences.Length) //say i = 2 and tere are 2 choices
            {
                item.transform.parent.gameObject.SetActive(true);
                //Debug.Log(loadedDialogue.sentences[i]);
                item.text = loadedDialogue.sentences[i];
                i++;
            }
            else
            {
                item.transform.parent.gameObject.SetActive(false);
            }
        }
        ProcessStartCommand();
    }
    private void ForceChangeSpeaker(bool change)
    {
        if (change)
        {
            if (loadedDialogue.speakerSentences.Count > 0) //if there's at least one speaker
            {
                if (loadedDialogue.speakerSentences[0] != null)
                {
                    SpeakerScriptable speaker = loadedDialogue.speakerSentences[0];
                    foreach (Image image in speakerBorders)
                    {
                        image.color = speaker.colorBorder;
                    }
                    speakerFancyBorder.color = speaker.fancyBorder;
                    textspeed = speaker.speed;
                    speakerImage.sprite = speaker.image;
                    speakerText.text = speaker.speakerName;
                    UpdateTextSpeeds();
                }
            }
            else
            {
                foreach (Image image in speakerBorders)
                {
                    image.color = loadedDialogue.speakerColorBorder;
                }
                speakerFancyBorder.color = loadedDialogue.speakerFancyBorder;
                textspeed = loadedDialogue.speakerSpeed;
                speakerImage.sprite = loadedDialogue.speakerImage;
                speakerText.text = loadedDialogue.speaker;
            }
        }
    }
    public void ChooseChoice(int num) //triggered by clicking on a choice
    {
        loadedDialogue = loadedDialogue.choicePaths[num];
        displayingChoices = false;
        /*if (overworldManager.dialogueEvent == false) //weird npc code, fix later
        {
            if (overworldManager.localeArmy != null)
            {
                if (overworldManager.localeArmy.currentSupplyPoint != null)
                {
                    if (overworldManager.localeArmy.currentSupplyPoint.npcTalkedTo.Count > 0)
                    {
                        if (overworldManager.localeArmy.currentSupplyPoint.npcTalkedTo[num] == true)
                        {
                            if (loadedDialogue.talkedToDialogueNPC != null)
                            {
                                loadedDialogue = loadedDialogue.talkedToDialogueNPC;
                            }
                        }
                    }
                }
            }
        }*/
        ForceChangeSpeaker(loadedDialogue.forceChangeSpeaker);

        if (loadedDialogue.isChoices)
        {
            PresentChoices();
        }
        else
        {
            choicesParent.SetActive(false);
            //Debug.Log("Starting conversation");
            sentences.Clear();

            foreach (string sentence in loadedDialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
            DisplayNextSentence();
        }
        ProcessStartCommand();

        /*if (overworldManager.localeArmy != null) //weird npc code fix later
        {
            if (overworldManager.localeArmy.currentSupplyPoint != null)
            {
                if (loadedDialogue.isFirstInstanceNPC)
                {
                    overworldManager.localeArmy.currentSupplyPoint.npcTalkedTo[num] = true;
                }
            }
        }*/
    }

    //FIELD BATTLE STUFF; WILL BE MIGRATED TO FIELD BATTLE MANAGER

    public void ActivatePlacementScreen()
    {
        gameInit.board.placingPieces = true;
        gameInit.placingUnitsScreen.SetActive(true);
        var text = gameInit.placingUnitsAlertText.GetComponentInChildren<TMP_Text>();
        text.text = "Select a unit to place on the field.";
        gameInit.image.SetActive(true);
        gameInit.placingUnitsAlertText.SetActive(true);
        //var i = 0;

        List<UIButton> buttonsToDestroy = new List<UIButton>();
        foreach (var button in gameInit.board.unitButtonsList)
        {
            buttonsToDestroy.Add(button);
        }
        foreach (var item in buttonsToDestroy)
        {
            gameInit.board.unitButtonsList.Remove(item);
            Destroy(item);
        }

        gameInit.board.GenerateButtonsFromSavedUnits();
    }
    private void DestroyAndLoadLevelUnitList(string level) // will destroy saved units list. useful if a level doesn't require units from a previous level
    {
        List<UnitInformationScript> toDestroyList = new List<UnitInformationScript>();
        foreach (var item in gameInit.saveInfoObject.listOfSavedUnits)
        {
            toDestroyList.Add(item);
        }

        foreach (var item in toDestroyList)
        {
            gameInit.saveInfoObject.listOfSavedUnits.Remove(item);
            Destroy(item);
        }
        //this the list of saved units

        foreach (var levelUnitList in levelUnitLists)
        {
            //Debug.Log(levelUnitList.ToString());
            if (levelUnitList.ToString() == level + "UnitList (UnitListScriptableObject)")
            {
                gameInit.saveInfoObject.list = levelUnitList.unitList;
            }
        }
        //loads the correct unit list
        gameInit.saveInfoObject.GenerateModifiableScripObjsAsChildren(); //generates them
    }
    public void StartTestingGrounds()
    {
        LoadLevelSetup("Tutorial");
    }
    public void FastLaunchDayOfGlory()
    {
        LoadLevelSetup("DayOfGlory");
    }
    public void LoadLevelSetup(string level)
    {
        gameInit.strafeCam.transform.position = cameraPosTutorial.position;
        gameInit.strafeCam.SetActive(true);
        gameInit.cinematicCam.SetActive(false);

        cinematicParent.SetActive(false);

        gameInit.SelectLevel(level); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel(level); //loads correct map, and placement map

        StartCoroutine(WaitForASec(level));
    }

    private IEnumerator WaitForASec(string level)
    {
        yield return new WaitForSecondsRealtime(0.01f);

        FastLaunch(level);
    }

    private void FastLaunch(string level)
    {
        gameInit.CreateSinglePlayerBoard(); //enables execute button, creates board, finds board for level gen, generates level, finds board
        chessUI.OnSingleplayerModeSelected(); //simply disables some screens
        gameInit.InitializeSinglePlayerController(); //creates units
        DestroyAndLoadLevelUnitList(level);
        gameInit.board.GenerateButtonsFromSavedUnits();

        gameInit.chessController.AllowInput = true;
        gameInit.placingUnitsScreen.SetActive(true);
        gameInit.executeButtonParent.SetActive(true);
        gameInit.unreadyButtonParent.SetActive(true);
    }

    public void DestroyLevel(string level)
    {

        gameInit.levelGen.DestroyLevel(level);
    }

    public void LoadLevel(string level) //use to swap levels on the fly. 
    {
        //gameInit.levelGen.DestroyLevel();
        //gameInit.levelGen.DestroyLevel(level);

        gameInit.SelectLevel(level); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel(level); //loads correct map, and placement map
        //if board already exists then we should just generate the level
        gameInit.levelGen.GenerateLevel();
        //if controller already exists then don't make another one

        //save units on your team
        //gameInit.saveInfoObject.SaveExistingPieceInfoInScripObjs();
        DestroyAndLoadLevelUnitList(level);

        //destroy units on board
        Piece[] AllPieces = FindObjectsOfType<Piece>();
        foreach (var piece in AllPieces)
        {
            piece.ImmediateRemoval();
            foreach (var corpse in piece.deadSoldiers) //delete corpses
            {
                Destroy(corpse);
            }
            foreach (var projectile in piece.childProjectiles)
            {
                Destroy(projectile);
            }
        }

        if (gameInit.board == null)
        {
            gameInit.CreateSinglePlayerBoard();
        }

        gameInit.InitializeSinglePlayerController(); //creates units
        //create new units from loadout
        gameInit.chessController.CreatePiecesFromLayout(gameInit.boardLevel);
        //enable placement again
        ActivatePlacementScreen();
    }

    //TUTORIAL STUFF; WILL BE MIGRATED TO TUTORIAL MANAGER
    
    public void StartTutorial()
    {
        gameInit.inTutorial = true;
        chessUI.menuOptionsParent.SetActive(false);
        chessUI.BG.gameObject.SetActive(false);
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartTutorial2);

    }

    private void StartTutorial2()
    {
        gameInit.strafeCam.transform.position = cameraPosTutorial.position;
        gameInit.strafeCam.SetActive(true);
        gameInit.cinematicCam.SetActive(false);

        cinematicParent.SetActive(false);

        gameInit.SelectLevel("Tutorial"); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel("Tutorial"); //loads correct map, and placement map

        StartCoroutine(Tutorial3());
    }

    private IEnumerator Tutorial3()
    {
        yield return new WaitForSecondsRealtime(.01f);

        gameInit.CreateSinglePlayerBoard(); //enables execute button, creates board, finds board for level gen, generates level, finds board
        chessUI.OnSingleplayerModeSelected(); //simply disables some screens
        gameInit.InitializeSinglePlayerController();
        DestroyAndLoadLevelUnitList("Tutorial");
        gameInit.board.GenerateButtonsFromSavedUnits();


        gameInit.chessController.AllowInput = false;
        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartTutorial3);
    }

    private void StartTutorial3()
    {
        SelectDialogue("Actual1");
    }

    void StartTutorialPlacement()
    {

        gameInit.placingUnitsScreen.SetActive(true);
    }
    public void ConfirmedUnitPlacementTutorial()
    {
        SelectDialogue("Actual3");
    }

    public void MovementMade()
    {
        SelectDialogue("Actual5");
    }
    private void EndTutorial()
    {
        gameInit.chessController.AllowInput = false;
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(EndTutorial2);
    }
    private void EndTutorial2()
    {
        LoadLevel("HemmedIn");
        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(EndTutorial3);
    }
    private void EndTutorial3()
    {
        gameInit.chessController.AllowInput = true;
        //Debug.Log("tutorial over");
    }

    private void IgnoreTutorial()
    {
        chessUI.menuOptionsParent.SetActive(false);
        chessUI.BG.gameObject.SetActive(false);
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(IgnoreTutorial2);
    }

    private void IgnoreTutorial2()
    {
        gameInit.strafeCam.transform.position = cameraPosTutorial.position;
        gameInit.strafeCam.SetActive(true);
        gameInit.cinematicCam.SetActive(false);
        cinematicParent.SetActive(false);

        gameInit.SelectLevel("HemmedIn"); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel("HemmedIn"); //loads correct map, and placement map
        gameInit.CreateSinglePlayerBoard(); //enables execute button, creates board, finds board for level gen, generates level, finds board
        chessUI.OnSingleplayerModeSelected(); //simply disables some screens
        gameInit.InitializeSinglePlayerController();
        DestroyAndLoadLevelUnitList("HemmedIn");
        gameInit.board.GenerateButtonsFromSavedUnits();


        gameInit.chessController.AllowInput = false; //needs to show up after chesscontroller is made 

        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(IgnoreTutorial3);
    }

    private void IgnoreTutorial3()
    {
        gameInit.chessController.AllowInput = true;
        //Debug.Log("tutorial ignored");
    }

}
