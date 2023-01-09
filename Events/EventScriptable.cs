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
    public string effectDesc;
    public string choice1;
    public string choice2;
    public string choice3;

    public enum EventCommands
    {
        AddMorale,
        AddSupplies,
        AddSpoils,
        AddUnit
    }
    public EventCommands command;
    public int commandNum;

    public EventScriptable choice1Event;
    public EventScriptable choice2Event;
    public EventScriptable choice3Event;  
}
