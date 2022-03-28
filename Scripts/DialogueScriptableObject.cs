using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "newDialogue", menuName = "Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    public string speaker;
    public Sprite speakerImage;
    public float speakerSpeed = 0.01f;
    public Color speakerColorBorder;
    public Color speakerFancyBorder;



    public string commandToExecuteStart;
    public string commandToExecuteEnd;
    public int commandVar;
    public string commandString;
    public DialogueScriptableObject dialogueIfConditionTrue;
    public bool isChoices = false;
    public bool forceChangeSpeaker = true;
    public DialogueScriptableObject talkedToDialogueNPC;
    public bool isFirstInstanceNPC = false;


    //public SpeakerScriptable initialSpeaker;
    public List<bool> italicizedSentences;
    public List<SpeakerScriptable> speakerSentences;

    [TextArea(3, 10)]
    public string[] sentences;
    public AudioClip[] sentenceAudio;
    public DialogueScriptableObject nextDialogue;
    public DialogueScriptableObject[] choicePaths;
}
