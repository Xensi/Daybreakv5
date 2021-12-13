using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public Queue<string> sentences;
    public TextMeshPro dialogueText;
    public float textspeed = 0.1f;
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Starting conversation");
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();

    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0) //end of queue
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        //dialogueText.text = sentence;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        audioSource.Play();
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter; //appends each letter to the end of the string
            //yield return null;//waits a frame
            yield return new WaitForSeconds(textspeed);
        }
        audioSource.Stop();
    }

    void EndDialogue()
    {
        Debug.Log("End of conversation");
    }

}
