using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBoxClicked : MonoBehaviour
{
    public DialogueManager dialogueManager;
    void OnMouseDown()
    {
        Debug.Log("clicked");
        dialogueManager.FinishSentence();
    }
}
