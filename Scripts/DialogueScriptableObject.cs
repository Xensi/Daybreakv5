using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newDialogue", menuName = "Dialogue")] 
public class DialogueScriptableObject : ScriptableObject
{
    [Header("Manual speaker info")]
    public string speaker;
    public Sprite speakerImage;
    public float speakerSpeed = 0.01f;
    public Color speakerColorBorder;
    public Color speakerFancyBorder;

     


    [Header("Command info")]
    public string commandToExecuteStart;
    public string commandToExecuteEnd;
    public int commandVar;
    public string commandString;
    public DialogueScriptableObject dialogueIfConditionTrue;

    [Header("NPC")]
    public DialogueScriptableObject talkedToDialogueNPC;
    public bool isFirstInstanceNPC = false;

    [Header("Settings")]
    public bool isChoices = false;
    public bool forceChangeSpeaker = true;
    public List<bool> italicizedSentences;
    public List<SpeakerScriptable> speakerSentences;
    public AudioClip[] sentenceAudio;

    [TextArea(3, 10)]
    public string[] sentences;
     
    public SentenceSpeakerClass[] sentencesWithSpeakers;



    [Header("Paths")]
    public DialogueScriptableObject nextDialogue;
    public DialogueScriptableObject[] choicePaths;
}
