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
    [SerializeField] private float timer;
    public EventClass[] events;

    public Canvas eventCanvas;
    public TMP_Text eventTitle;
    public TMP_Text eventDescription;
    public TMP_Text eventEffect;
    public List<TMP_Text> eventChoiceTexts;
    private void Start()
    {
        eventCanvas.enabled = false;
    }
    private void Update()
    {
        if (BattleGroupManager.Instance.timeScale > 0)
        {
            timer += Time.deltaTime * BattleGroupManager.Instance.timeScale;
            if (timer >= timeUntilNextRandomEvent)
            {
                TriggerRandomEvent();
            }
        } 
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
                default:
                    break;
            } 
        }
        OverworldManager.Instance.ShowArmyInfoAndUpdateArmyBars();
    }
    public EventScriptable currentEvent;
    private EventScriptable GetRandomEvent()
    {
        float randomNum = Random.Range(0, events[events.Length - 1].calculatedWeight);
        Debug.Log(randomNum);
        for (int i = 0; i < events.Length; i++)
        {
            if (randomNum <= events[i].calculatedWeight)
            {
                if (!events[i].allowedToTrigger)
                {
                    continue;
                }
                EventScriptable item = events[i].eventScriptable;
                if (item.removeFromEventPoolAfterTriggering)
                {
                    events[i].allowedToTrigger = false;
                }
                return item;
            }
        }
        return null;
    }
    private void CalculateWeights()
    {
        float currentWeight = 0;
        for (int i = 0; i < events.Length; i++)
        {
            currentWeight += events[i].eventScriptable.weight;
            events[i].calculatedWeight = currentWeight;
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
