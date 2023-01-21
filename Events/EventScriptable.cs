using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "newEvent", menuName = "Event")]
public class EventScriptable : ScriptableObject
{
    public string title;
    [TextArea(3, 10)]
    public string description; 

    public float weight = 1;
    public bool removeFromEventPoolAfterTriggering = false;

    public List<EventCommandClass> commands;
     
    public List<ChoiceClass> choices;
    public bool endOfEventChain = true;
}
