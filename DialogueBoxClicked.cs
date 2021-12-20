using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBoxClicked : MonoBehaviour
{
    public DialogueManager dialogueManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        Debug.Log("clicked");
        dialogueManager.FinishSentence();
    }
}
