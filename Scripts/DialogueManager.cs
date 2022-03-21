using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{

    public Queue<string> sentences;
    public Queue<AudioClip> sentenceAudio;
    public TMP_Text dialogueText;
    public float textspeed = 0.1f;
    public AudioSource audioSource;
    public string currentSentence;
    public bool runningText = false;

    public DialogueScriptableObject loadedDialogue; //needs to be changed whenever you want to swap dialogues
    public TMP_Text speakerText;
    public GameObject dialogueParent;
    public Image speakerImage;
    public AudioClip currentAudio;
    public int sentenceCount = 0;
    public float startingYPos;
    public GameObject targetPosObj;
    public DialogueScriptableObject[] conversationStarters;
    public bool autoStart = false;
    public GameObject choicesParent;
    public List<TMP_Text> choiceList;
    public Image speakerBGImage;
    public Image speakerFancyBorder;
    public npcAnimController npcAnimController;
    public GameInitializer gameInit;
    public ChessUIManager chessUI;

    public List<UnitListScriptableObject> levelUnitLists;

    public Image fadeToBlack;

    public Transform cameraPosTutorial;

    public GameObject cinematicParent;
    public bool readingDialogue = false;

    //public List<UnitScriptableObject> tutorialUnitList;

    [SerializeField] private OverworldManager overworldManager;

    [SerializeField] private UnitManager unitManager;
    void Start()
    {
        startingYPos = dialogueParent.transform.position.y;
        audioSource = GetComponent<AudioSource>();
        sentences = new Queue<string>(); //first in last out?

        if (autoStart)
        {
            StartCoroutine(LateStart(1));

        }
    }

    IEnumerator LateStart(float waitTime)
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

    public void SelectDialogue(string dialogue)
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
    public void StartDialogue()
    {
        readingDialogue = true;
        ForceChangeSpeaker(loadedDialogue.forceChangeSpeaker);
        dialogueText.text = "";
        speakerText.text = loadedDialogue.speaker;

        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);

        if (loadedDialogue.isChoices)
        {
            choicesParent.SetActive(true);
            PresentChoices();
        }
        else
        {
            tween.OnComplete(StartDialogue2);

        }

    }
    public void StartDialogue2()
    {

        //Debug.Log("Starting conversation");
        sentences.Clear();

        foreach (string sentence in loadedDialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
        ProcessCommand();
    }
    public void StartNextDialogue()
    {

        loadedDialogue = loadedDialogue.nextDialogue;

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
        ProcessCommand();
    }

    void ProcessCommand()
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

    public void DisplayNextSentence()
    {
        if (loadedDialogue.isChoices)
        {
            return;
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

    IEnumerator TypeSentence(string sentence)
    {
        if (currentAudio != null)
        {

            audioSource.clip = currentAudio;
            audioSource.Play();
        }
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter; //appends each letter to the end of the string
            //yield return null;//waits a frame
            yield return new WaitForSeconds(textspeed);
        }
        //audioSource.Stop();
        runningText = false;
    }

    public void FinishSentence()
    {
        StopAllCoroutines();
        dialogueText.text = currentSentence;
        if (runningText == false)
        {
            DisplayNextSentence();
        }
        else
        {
            runningText = false;
        }

    }
    void EndDialogue()
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
        else
        {
            StartNextDialogue();
        }
    }

    public void ProcessEndCommand()
    {
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


    }

    void EndTutorial()
    {
        gameInit.chessController.AllowInput = false;
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(EndTutorial2);
    }
    void EndTutorial2()
    {
        LoadLevel("HemmedIn");
        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(EndTutorial3);
    }
    void EndTutorial3()
    {
        gameInit.chessController.AllowInput = true;
        //Debug.Log("tutorial over");
    }

    public void IgnoreTutorial()
    {
        chessUI.menuOptionsParent.SetActive(false);
        chessUI.BG.gameObject.SetActive(false);
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);
        tween.OnComplete(IgnoreTutorial2);
    }

    void IgnoreTutorial2()
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
        LoadLevelUnitList("HemmedIn");
        gameInit.board.GenerateButtonsFromSavedUnits();


        gameInit.chessController.AllowInput = false; //needs to show up after chesscontroller is made 

        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(IgnoreTutorial3);
    }

    void IgnoreTutorial3()
    {
        gameInit.chessController.AllowInput = true;
        //Debug.Log("tutorial ignored");
    }

    public void LoadLevel(string level)
    {
        gameInit.levelGen.DestroyLevel();
        gameInit.SelectLevel(level); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel(level); //loads correct map, and placement map
        //if board already exists then we should just generate the level
        gameInit.levelGen.GenerateLevel();
        //if controller already exists then don't make another one

        //save units on your team
        //gameInit.saveInfoObject.SaveExistingPieceInfoInScripObjs();
        LoadLevelUnitList(level);

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


        //create new units from loadout
        gameInit.chessController.CreatePiecesFromLayout(gameInit.boardLevel);
        //enable placement again
        ActivatePlacementScreen();
    }

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

    public void LoadLevelUnitList(string level) // will destroy saved units list. useful if a level doesn't require units from a previous level
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
    public void StartTutorial()
    {
        gameInit.inTutorial = true;
        chessUI.menuOptionsParent.SetActive(false);
        chessUI.BG.gameObject.SetActive(false);
        Tween tween = fadeToBlack.DOFade(1, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartTutorial2);

    }

    void StartTutorial2()
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
        LoadLevelUnitList("Tutorial");
        gameInit.board.GenerateButtonsFromSavedUnits();


        gameInit.chessController.AllowInput = false;
        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartTutorial3);
    }

    public void StartTestingGrounds()
    {
        LoadLevelSetup("Tutorial");
    }

    public void FastLaunchDayOfGlory()
    {
        LoadLevelSetup("DayOfGlory");
    }

    private void LoadLevelSetup(string level)
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
        LoadLevelUnitList(level);
        gameInit.board.GenerateButtonsFromSavedUnits();

        gameInit.chessController.AllowInput = true;
        gameInit.placingUnitsScreen.SetActive(true);
        gameInit.executeButtonParent.SetActive(true);
        gameInit.unreadyButtonParent.SetActive(true);
    }

    void StartTutorial3()
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
    public void PresentChoices()
    {
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
        ProcessCommand();
    }
    private void ForceChangeSpeaker(bool change)
    {
        if (change)
        {
            speakerBGImage.color = loadedDialogue.speakerColorBorder;
            speakerFancyBorder.color = loadedDialogue.speakerFancyBorder;
            textspeed = loadedDialogue.speakerSpeed;
            speakerImage.sprite = loadedDialogue.speakerImage;
            speakerText.text = loadedDialogue.speaker;
        }
    }
    public void ChooseChoice(int num)
    {
        loadedDialogue = loadedDialogue.choicePaths[num];
        if (overworldManager.localeArmy.currentSupplyPoint != null)
        {
            if (overworldManager.localeArmy.currentSupplyPoint.npcTalkedTo[num] == true)
            {
                if (loadedDialogue.talkedToDialogue != null)
                {
                    loadedDialogue = loadedDialogue.talkedToDialogue;
                }
            }
        }
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
        ProcessCommand();

        if (overworldManager.localeArmy.currentSupplyPoint != null)
        {
            if (loadedDialogue.isFirstInstanceNPC)
            {
                overworldManager.localeArmy.currentSupplyPoint.npcTalkedTo[num] = true;
            }
        }
    }
}
