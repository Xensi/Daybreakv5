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

    //public List<UnitScriptableObject> tutorialUnitList;
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

            Debug.Log(convo.ToString());
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
        speakerBGImage.color = loadedDialogue.speakerColorBorder;
        speakerFancyBorder.color = loadedDialogue.speakerFancyBorder;
        textspeed = loadedDialogue.speakerSpeed;
        dialogueText.text = "";
        speakerText.text = loadedDialogue.speaker;
        speakerImage.sprite = loadedDialogue.speakerImage;
        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartDialogue2);

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
            textspeed = loadedDialogue.speakerSpeed;
            speakerText.text = loadedDialogue.speaker;
            speakerImage.sprite = loadedDialogue.speakerImage;
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
        Debug.LogError("Processing commands");
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
        if (loadedDialogue.commandToExecuteEnd == "startTutorial")
        {
            StartTutorial();
        }
        if (loadedDialogue.commandToExecuteEnd == "startDayOfGlory")
        {
            Debug.Log("Starting day of glory");
            gameInit.strafeCam.SetActive(true);
            gameInit.cinematicCam.SetActive(false);
        }
        if (loadedDialogue.commandToExecuteEnd == "startTutorialPlacement")
        {
            StartTutorialPlacement();
        }
        if (loadedDialogue.commandToExecuteEnd == "enableChessControllerInput")
        {
            gameInit.chessController.AllowInput = true;
        }
        if (loadedDialogue.commandToExecuteEnd == "enableExecution")
        {
            gameInit.executeButtonParent.SetActive(true);
        }
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

        //Debug.Log("Starting training course");
        gameInit.strafeCam.transform.position = cameraPosTutorial.position;
        gameInit.strafeCam.SetActive(true);
        gameInit.cinematicCam.SetActive(false);

        gameInit.SelectLevel("Tutorial"); //loads the correct board layout (pieces positioning)
        gameInit.levelGen.SelectLevel("Tutorial"); //loads correct map, and placement map
        gameInit.CreateSinglePlayerBoard(); //enables execute button, creates board, finds board for level gen, generates level, finds board
        chessUI.OnSingleplayerModeSelected(); //simply disables some screens
        gameInit.InitializeSinglePlayerController();
        

        //gameInit.saveInfoObject.list = tutorialUnitList;
        foreach (var levelUnitList in levelUnitLists)
        {
            Debug.Log(levelUnitList.ToString());
            if (levelUnitList.ToString() == "TutorialUnitList (UnitListScriptableObject)")
            {
                gameInit.saveInfoObject.list = levelUnitList.unitList;
            }
        }

        gameInit.saveInfoObject.GenerateModifiableScripObjsAsChildren();
        gameInit.board.GenerateButtonsFromSavedUnits();


        gameInit.chessController.AllowInput = false;
        Tween tween = fadeToBlack.DOFade(0, 1).SetEase(Ease.InOutQuad);//dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        tween.OnComplete(StartTutorial3);
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
        Debug.Log("presenting choices");
        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        dialogueText.text = "";
        speakerBGImage.color = loadedDialogue.speakerColorBorder;
        speakerFancyBorder.color = loadedDialogue.speakerFancyBorder;
        speakerText.text = loadedDialogue.speaker;
        speakerImage.sprite = loadedDialogue.speakerImage;
        choicesParent.SetActive(true);

        var i = 0;
        foreach (var item in choiceList)
        {
            if (i < loadedDialogue.sentences.Length) //say i = 2 and tere are 2 choices
            {
                item.transform.parent.gameObject.SetActive(true);
                Debug.Log(loadedDialogue.sentences[i]);
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

    public void ChooseChoice(int num)
    {
        loadedDialogue = loadedDialogue.choicePaths[num];

        if (loadedDialogue.isChoices)
        {
            PresentChoices();
        }
        else
        {
            choicesParent.SetActive(false);
            textspeed = loadedDialogue.speakerSpeed;
            speakerText.text = loadedDialogue.speaker;
            speakerImage.sprite = loadedDialogue.speakerImage;
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




}
