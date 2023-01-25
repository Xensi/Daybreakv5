using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public float timeUntilNextRandomEvent = 120;
    public float timeUntilLinearObjectiveEvent = 5;
    [SerializeField] private float timer;
    public List<EventClass> randomEvents;
    public List<EventClass> linearObjectiveEvents;  

    public Canvas eventCanvas;
    public TMP_Text eventTitle;
    public TMP_Text eventDescription;
    public TMP_Text eventEffect;
    public List<TMP_Text> eventChoiceTexts;
    private void Start()
    {
        eventCanvas.enabled = false;
        TriggerSpecificLinearEvent(0); //penitent crusade event
    }
    
    private void Update()
    {
        if (BattleGroupManager.Instance.timeScale > 0 && OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.Overworld)
        {
            timer += Time.deltaTime * BattleGroupManager.Instance.timeScale;

            if (timer >= timeUntilNextRandomEvent)
            {
                TriggerRandomEvent();
            }
            /*else if (timer >= timeUntilLinearObjectiveEvent)
            {
                TriggerLinearObjectiveEvent();
            }*/
            
            
        } 
    }
    int linearEventInt = 0;
    private void TriggerSpecificLinearEvent(int id)
    {
        EventClass item = linearObjectiveEvents[id];
        currentEvent = item.eventScriptable;
        if (currentEvent != null && item.allowedToTrigger)
        {
            DisplayEvent(currentEvent);
        }
    }
    private void TriggerLinearObjectiveEvent()
    {
        currentEvent = GetLinearEvent();
        if (currentEvent != null)
        {
            DisplayEvent(currentEvent);
        }
        linearEventInt++;
    }
    private EventScriptable GetLinearEvent()
    {
        while (!linearObjectiveEvents[linearEventInt].allowedToTrigger)
        {
            linearEventInt++;
        }
        EventScriptable item = linearObjectiveEvents[linearEventInt].eventScriptable;
        if (item.removeFromEventPoolAfterTriggering)
        {
            linearObjectiveEvents[linearEventInt].allowedToTrigger = false;
        }
        return item;
    }
    private void TriggerRandomEvent() //calculate weights based on allowed events
    {
        CalculateWeights(); 
        currentEvent = GetRandomEvent();
        if (currentEvent != null)
        { 
            DisplayEvent(currentEvent);
        } 
    }
    private void DisplayEvent(EventScriptable triggeredEvent)
    {
        timer = 0;
        BattleGroupManager.Instance.ForcePause();
        currentEvent = triggeredEvent;
        eventCanvas.enabled = true;
        eventTitle.text = triggeredEvent.title; 
        eventDescription.text = triggeredEvent.description; 
        ProcessEventCommands(triggeredEvent);
        DisplayChoices(triggeredEvent);
    }
    private void DisplayChoices(EventScriptable triggeredEvent)
    {
        int highest = triggeredEvent.choices.Count-1; 


        if (triggeredEvent.choices.Count == 0)
        {
            for (int i = 0; i < eventChoiceTexts.Count; i++)
            {
                if (i == 0)
                {
                    eventChoiceTexts[i].text = "Acknowledged.";
                    eventChoiceTexts[i].transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    eventChoiceTexts[i].text = "";
                    eventChoiceTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }
        else if (highest >= 0)
        {
            for (int i = 0; i < eventChoiceTexts.Count; i++)
            {
                if (i <= highest)
                {
                    eventChoiceTexts[i].text = triggeredEvent.choices[i].choiceText;
                    eventChoiceTexts[i].transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    eventChoiceTexts[i].text = "";
                    eventChoiceTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }
        }
        

        
    }
    private void ProcessEventCommands(EventScriptable triggeredEvent)
    {
        BattleGroup player = OverworldManager.Instance.playerBattleGroup;
        eventEffect.text = "";
        foreach (EventCommandClass commandItem in triggeredEvent.commands)
        { 
            int commandNum = commandItem.commandNum;
            string plusMinus = "+";
            if (commandNum < 0)
            {
                plusMinus = "";
            }
            switch (commandItem.command)
            {
                case EventCommandClass.EventCommands.AddMorale:
                    eventEffect.text += plusMinus + commandNum + " morale. ";
                    player.morale += commandNum;
                    player.morale = Mathf.Clamp(player.morale, 0, player.maxMorale);
                    break;
                case EventCommandClass.EventCommands.AddSupplies:
                    eventEffect.text += plusMinus + commandNum + " supplies. ";
                    player.supplies += commandNum;
                    player.supplies = Mathf.Clamp(player.supplies, 0, player.maxSupplies);
                    break;
                case EventCommandClass.EventCommands.AddSpoils:
                    eventEffect.text += plusMinus + commandNum + " spoils. ";
                    player.spoils += commandNum;
                    player.spoils = Mathf.Clamp(player.spoils, 0, player.maxSpoils);
                    break;
                case EventCommandClass.EventCommands.AddUnit:
                    break;
                case EventCommandClass.EventCommands.RevealLocation:
                    ManualMapManager.Instance.ChangeLocationStatus(commandNum, MapStatusClass.MapStatus.Visible);
                    break;
                default:
                    break;
            } 
        }
        OverworldManager.Instance.ShowArmyInfoAndUpdateArmyBars();
    }
    public EventScriptable currentEvent;
    private EventScriptable GetRandomEvent()
    {
        float randomNum = Random.Range(0, randomEvents[randomEvents.Count - 1].calculatedWeight);
        Debug.Log(randomNum);
        for (int i = 0; i < randomEvents.Count; i++)
        {
            if (randomNum <= randomEvents[i].calculatedWeight)
            {
                if (!randomEvents[i].allowedToTrigger)
                {
                    continue;
                }
                EventScriptable item = randomEvents[i].eventScriptable;
                if (item.removeFromEventPoolAfterTriggering)
                {
                    randomEvents[i].allowedToTrigger = false;
                }
                return item;
            }
        }
        return null;
    }
    private void CalculateWeights()
    {
        float currentWeight = 0;
        for (int i = 0; i < randomEvents.Count; i++)
        {
            currentWeight += randomEvents[i].eventScriptable.weight;
            randomEvents[i].calculatedWeight = currentWeight;
        }
    }
    public void ChooseEventChoice(int choiceNum)
    { 
        if (currentEvent.endOfEventChain)
        {
            eventCanvas.enabled = false;
        }
        else
        {  
            DisplayEvent(currentEvent.choices[choiceNum].eventScriptable);
        }
    }
}
