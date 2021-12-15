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
            Debug.Log(loadedDialogue.sentences[i]);
            item.text = loadedDialogue.sentences[i];
            i++;
        }
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

    }


}
