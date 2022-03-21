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
    [TextArea(3, 10)]
    public string[] sentences;
    public AudioClip[] sentenceAudio;
    public DialogueScriptableObject nextDialogue;
    public bool isChoices = false;
    public Color speakerColorBorder;
    public Color speakerFancyBorder;
    public DialogueScriptableObject[] choicePaths;
    public string commandToExecuteStart;
    public string commandToExecuteEnd;
    public int commandVar;
    public string commandString;
    public bool forceChangeSpeaker = false;
    public DialogueScriptableObject talkedToDialogue;
    public bool isFirstInstanceNPC = false;
}
