using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    [Header("Needs to be accessible to to other files")]
    public DialogueScriptableObject loadedDialogue; //needs to be changed whenever you want to swap dialogues
    public bool readingDialogue = false;

    [Header("Needs to be set manually")]
    [SerializeField] private OverworldManager overworldManager;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject targetPosObj;
    //[SerializeField] private GameInitializer gameInit;
    //[SerializeField] private ChessUIManager chessUI;
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
    public bool displayingChoices = false;
    private bool checkedCondition = false;

    [SerializeField] private char currentChar;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        dialogueParent.SetActive(true);
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
        if (HasChoices())
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
        slowTextSpeed = textspeed * 2;
        slowerTextSpeed = textspeed * 4;
    }
    public void SelectDialogue(string dialogue) //used in tutorial to load then read dialogue
    {
        foreach (var convo in conversationStarters)
        {

            //Debug.Log(convo.ToString());
            if (convo.ToString() == dialogue + " (DialogueScriptableObject)")
            {
                loadedDialogue = convo;

                if (HasChoices())
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
        if (loadedDialogue != null)
        {
            readingDialogue = true;
            ChangeToInitialSpeaker();
            dialogueText.text = ""; 

            Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad); //tweens dialogue up

            if (HasChoices())
            {
                choicesParent.SetActive(true);
                PresentChoices();
            }
            else
            {
                tween.OnComplete(ReadDialogue);

            }
        }
    }
    private bool HasChoices()
    {
        if (loadedDialogue.choicePaths.Length > 1) //2 or more
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void ReadDialogue() //begins actually reading dialogue
    {
        sentences.Clear();

        foreach (SentenceSpeakerClass item in loadedDialogue.sentencesWithSpeakers)
        {
            sentences.Enqueue(item.sentence);
        }

        DisplayNextSentence();
        ProcessStartCommand();
    }
    
    private void ProcessStartCommand() //if command needed at beginning of dialogue
    { 
    }
    
    private void DisplayNextSentence() //display next "sentence" (a chunk of dialogue)
    {
        if (HasChoices())
        {
            return;
        } 

        audioSource.Stop();
        runningText = true;
        if (sentences.Count == 0) //end of queue
        {
            Debug.Log("sentences.count = 0");
            runningText = false;
            EndDialogue();
            return;
        }
        /*if (loadedDialogue.sentenceAudio.Length > sentenceCount) //length 3, count 2
        {
            currentAudio = loadedDialogue.sentenceAudio[sentenceCount];
        }
        else
        {
            currentAudio = null;
        }*/
        SwitchSpeaker();
        sentenceCount++;
        currentSentence = sentences.Dequeue();

        //Debug.Log(currentSentence);
        //dialogueText.text = sentence;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }
    private IEnumerator TypeSentence(string sentence) //reveal characters of sentence one by one
    {
        /*if (currentAudio != null)
        {

            audioSource.clip = currentAudio;
            audioSource.Play();
        }*/

        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        char[] charArray = sentence.ToCharArray();
        
        bool skip = false;
        for (int i = 0; i < charArray.Length; i++)
        {
            char letter = charArray[i];
            //currentChar = letter;
            //Debug.LogError(letter);

            if (letter == '<')
            {
                skip = true;
            }
            else if (letter == '>')
            {
                skip = false;
                continue;
            }
            if (skip)
            {
                continue;
            }
            else
            {
                dialogueText.maxVisibleCharacters++;
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
                else if (letter == ',' || letter == '—')
                {

                    if (i + 1 < charArray.Length && charArray[i + 1] == '!')
                    {
                        yield return new WaitForSeconds(textspeed);

                    }
                    else
                    {
                        yield return new WaitForSeconds(slowTextSpeed);
                    }


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
        if (currentSentence != null)
        {
            dialogueText.text = currentSentence;
            dialogueText.maxVisibleCharacters = currentSentence.ToCharArray().Length; 
        }
        if (runningText == false)
        {
            DisplayNextSentence();
        }
        else
        {
            runningText = false;
        }

    } 
    private bool ProcessEndCommand(DialogueScriptableObject.Commands command, int num = 0)
    {
        //checkedCondition = false; 
        if (command == DialogueScriptableObject.Commands.UseDialogueCommand)
        { 
            command = loadedDialogue.endCommand;
            num = loadedDialogue.commandNum; 
        }

        switch (command)
        {
            case DialogueScriptableObject.Commands.None:
                return true; 
            case DialogueScriptableObject.Commands.GainMorale:
                break;
            case DialogueScriptableObject.Commands.GainSupply:
                break;
            case DialogueScriptableObject.Commands.GainMaxSupply:
                break;
            case DialogueScriptableObject.Commands.GainHorses:
                break;
            case DialogueScriptableObject.Commands.GiveUnit:
                break;
            case DialogueScriptableObject.Commands.MakeAvailableSupplyTown:
                break;
            case DialogueScriptableObject.Commands.HelpCharacter:
                CharactersManager.Instance.ImprisonCharacter(num);
                break;
            case DialogueScriptableObject.Commands.ArrestCharacter:
                CharactersManager.Instance.ImprisonCharacter(num);
                break;
            case DialogueScriptableObject.Commands.RevealLocation:
                ManualMapManager.Instance.ChangeLocationStatus(num, MapStatusClass.MapStatus.Visible);
                break;
            case DialogueScriptableObject.Commands.CheckVisited: 
                return ManualMapManager.Instance.HasLocationBeenVisited(num);   
            case DialogueScriptableObject.Commands.SetVisited:
                ManualMapManager.Instance.ChangeLocationStatus(num, MapStatusClass.MapStatus.Visited);
                break;
            case DialogueScriptableObject.Commands.CheckHelped: 
                return CharactersManager.Instance.HasHelpedCharacter(num); 
            case DialogueScriptableObject.Commands.CheckImprisoned:
                return CharactersManager.Instance.CharacterIsImprisoned(num);  
            case DialogueScriptableObject.Commands.PutPrisonerOnTrial:
                CharacterClass prisoner = null;
                foreach (CharacterClass item in CharactersManager.Instance.characters)
                {
                    if (item.imprisonedByPlayer)
                    {
                        prisoner = item;
                        break;
                    }
                }
                if (prisoner != null)
                { 
                    loadedDialogue.dialogueIfConditionTrue = prisoner.trialDialogue;
                    return true;
                } 

                break;
            case DialogueScriptableObject.Commands.TradeSutler:
                break;
            case DialogueScriptableObject.Commands.GainSutler:
                break;
            case DialogueScriptableObject.Commands.DestroyLocale:
                break;
            case DialogueScriptableObject.Commands.MultiplyVisionRange:
                break;
            case DialogueScriptableObject.Commands.HasAtLeastPrisonerCount:
                return CharactersManager.Instance.CheckPrisonerCount(num); 
            default:
                break;
        }
        return false;

/*
        if (commandEnd == "moraleGain")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.overallMorale += loadedDialogue.commandNum;
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
                overworldManager.localeArmy.provisions += loadedDialogue.commandNum;
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
                overworldManager.localeArmy.fowUnit.circleRadius = .5f + (overworldManager.localeArmy.sightRadius * loadedDialogue.commandNum);
            }
        }
        if (commandEnd == "gainMaxSupply")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.maxProvisions += loadedDialogue.commandNum;
            }
        }
        if (commandEnd == "gainHorses")
        {
            if (overworldManager.localeArmy != null)
            {
                overworldManager.localeArmy.horses += loadedDialogue.commandNum;
            }
        }
        if (commandEnd == "giveUnit")
        {
            if (overworldManager.localeArmy != null)
            {
                for (int i = 0; i < loadedDialogue.commandNum; i++)
                {
                    *//*foreach (ArmyCardScriptableObj card in unitManager.units)
                    {
                        if (card.cardName == loadedDialogue.commandString)
                        {
                            overworldManager.localeArmy.AddArmyCard(card);
                        }
                    }*//*
                }
            }
        }
        if (commandEnd == "makeAvailableSupplyTown")
        {
            if (overworldManager.localeArmy != null)
            {
                if (overworldManager.localeArmy.currentSupplyPoint != null)
                {
                    overworldManager.localeArmy.currentSupplyPoint.amountOfProvisionsToReserve -= loadedDialogue.commandNum;
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
            OverworldManager.Instance.sutlerParent.SetActive(true);
        } */
    }

    private void EndDialogue()
    { 
        if (loadedDialogue.endCommand != DialogueScriptableObject.Commands.None)
        {
            checkedCondition = false;
            checkedCondition = ProcessEndCommand(DialogueScriptableObject.Commands.UseDialogueCommand);
        }
        sentenceCount = 0;
        if (checkedCondition)
        {
            checkedCondition = false;
            StartNextDialogue(loadedDialogue.dialogueIfConditionTrue);
            //Debug.Log("condition checked");
        }
        else if (loadedDialogue.choicePaths.Length == 0)
        {
            readingDialogue = false;
            //Debug.Log("End of conversation");
            //dialogueParent.SetActive(false);
            Tween tween = dialogueParent.transform.DOMove(new Vector3(dialogueParent.transform.position.x, startingYPos, dialogueParent.transform.position.z), .5f).SetEase(Ease.InOutQuad);
            OverworldManager.Instance.DialogueOver();
        } 
        else
        {
            StartNextDialogue(loadedDialogue.choicePaths[0]);
            //Debug.Log("starting dialogue");
        }
    }
    private void StartNextDialogue(DialogueScriptableObject dialogue) //loads next dialogue and reads it
    {
        loadedDialogue = dialogue;
        //Debug.Log("next dialogue = " + dialogue.name);

        if (HasChoices())
        {
            PresentChoices();
        }
        else
        {
            choicesParent.SetActive(false);
            ChangeToInitialSpeaker();
            //Debug.Log("Starting conversation");
            sentences.Clear();

            foreach (SentenceSpeakerClass item in loadedDialogue.sentencesWithSpeakers)
            {
                sentences.Enqueue(item.sentence);
            }

            DisplayNextSentence();
        }
        ProcessStartCommand();
    }
    public List<bool> choiceAvailable;
    private void PresentChoices()
    {
        //option to display specific choices if condition is met
        displayingChoices = true;
        Debug.Log("presenting choices");
        Tween tween = dialogueParent.transform.DOMove(targetPosObj.transform.position, .5f).SetEase(Ease.InOutQuad);
        dialogueText.text = "";

        ChangeToInitialSpeaker();
        choicesParent.SetActive(true);

        int nextEligible = 0;
        choiceAvailable.Clear();
        foreach (var choiceText in choiceList) //go through choice blocks
        {
            choiceText.text = "";
            SentenceSpeakerClass sentenceClass = null; 
            for (int i = nextEligible; i < loadedDialogue.sentencesWithSpeakers.Length; i++) //first loop 0. +1 = 1. next loop 1;
            {
                if (ProcessEndCommand(loadedDialogue.sentencesWithSpeakers[i].commandPrerequisite, loadedDialogue.sentencesWithSpeakers[i].commandNum))
                {
                    sentenceClass = loadedDialogue.sentencesWithSpeakers[i];
                    nextEligible = i;
                    choiceAvailable.Add(true);
                    break;
                }
                else //skipping over the ineligible
                { 
                    choiceAvailable.Add(false);
                }
            }

            if (sentenceClass != null)
            { 
                choiceText.transform.parent.gameObject.SetActive(true);
                choiceText.text = sentenceClass.sentence;
                nextEligible += 1;

            }
            else
            {
                choiceText.transform.parent.gameObject.SetActive(false);
            }
        }
        ProcessStartCommand();
    }
    private void ChangeToInitialSpeaker()
    {
        SpeakerScriptable speaker; 
        if (loadedDialogue.sentencesWithSpeakers.Length > 0)
        {
            speaker = loadedDialogue.sentencesWithSpeakers[0].speaker;
            UpdateSpeakerUI(speaker);
        }
    }
    private void SwitchSpeaker()
    { 
        SpeakerScriptable speaker;
        if (loadedDialogue.sentencesWithSpeakers[sentenceCount] != null)
        {
            speaker = loadedDialogue.sentencesWithSpeakers[sentenceCount].speaker;
            UpdateSpeakerUI(speaker);
        }
    }
    private void UpdateSpeakerUI(SpeakerScriptable speaker)
    {
        if (speaker != null)
        { 
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
    public void ChooseChoice(int num) //triggered by clicking on a choice. num is simply the button's location, 0 1 2
    {
        while (!choiceAvailable[num] && num < loadedDialogue.choicePaths.Length-1)
        {
            num++;
        }
        loadedDialogue = loadedDialogue.choicePaths[num];
        displayingChoices = false; 
        ChangeToInitialSpeaker();

        if (HasChoices())
        {
            PresentChoices();
        }
        else
        {
            choicesParent.SetActive(false);
            //Debug.Log("Starting conversation");
            sentences.Clear();

            foreach (SentenceSpeakerClass item in loadedDialogue.sentencesWithSpeakers)
            {
                sentences.Enqueue(item.sentence);
            }
            DisplayNextSentence();
        }
        ProcessStartCommand(); 
    }
     
     
}
