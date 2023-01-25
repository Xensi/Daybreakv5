using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newDialogue", menuName = "Dialogue")] 
public class DialogueScriptableObject : ScriptableObject
{
    /*[Header("Manual speaker info")]
    public string speaker;
    public Sprite speakerImage;
    public float speakerSpeed = 0.01f;
    public Color speakerColorBorder;
    public Color speakerFancyBorder; */

    public enum Commands
    {
        None,
        GainMorale,
        GainSupply,
        GainMaxSupply,
        GainHorses,
        GiveUnit,
        MakeAvailableSupplyTown,
        HelpCharacter,
        ArrestCharacter,
        RevealLocation,
        CheckVisited,
        CheckHelped,
        TradeSutler,
        GainSutler,
        DestroyLocale,
        MultiplyVisionRange
    } 

    [Header("Sentences")]
    public SentenceSpeakerClass[] sentencesWithSpeakers;
    public AudioClip[] sentenceAudio;

    [TextArea(3, 10)]
    public string[] sentences;
     

    [Header("Paths")]
    public DialogueScriptableObject nextDialogue;
    public DialogueScriptableObject[] choicePaths; 

    [Header("Command info")]
    public Commands startCommand;
    public Commands endCommand;

    //public string commandToExecuteStart;
    //public string commandToExecuteEnd;
    public int commandNum;
    public string commandString;
    public DialogueScriptableObject dialogueIfConditionTrue;
}
