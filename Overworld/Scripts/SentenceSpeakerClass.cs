using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SentenceSpeakerClass
{
    [TextArea(3, 10)]
    public string sentence = ""; 
    public SpeakerScriptable speaker;
    public DialogueScriptableObject.Commands commandPrerequisite;
    public int commandNum;
}
