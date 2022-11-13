using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "newSpeaker", menuName = "Speaker")]
public class SpeakerScriptable : ScriptableObject
{
    public string speakerName;
    public Sprite image;
    public float speed = 0.025f;
    public Color colorBorder;
    public Color fancyBorder;
}
