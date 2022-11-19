using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class BattleGroup : MonoBehaviour
{ 
    public enum controlStatus
    {
        PlayerControlled,
        EnemyControlled
    }
    public controlStatus controlledBy = controlStatus.PlayerControlled;
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;


    public List<UnitInfoClass> listOfUnitsInThisArmy;
    public Transform aiTarget;

    private bool onSupplyPoint = false;
    public SupplyPoint currentSupplyPoint = null;
    public LocaleInvestigatable currentLocale = null;

    public int provisions = 50;
    public int maxProvisions = 100;
    public int spoils = 0;
    public int maxSpoils = 100;
    public int morale = 100;
    public int maxMorale = 100;

    public List<ArmyCard> armyDisplayCards;
    [SerializeField]
    private LineRenderer lineRenderer;

    public float aiSightDistance = 100;

    public bool aiCanSeePlayer = false;
    public RichAI pathfindingAI;

    private void OnDrawGizmos()
    { 
        if (controlledBy == controlStatus.EnemyControlled)
        { 
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aiSightDistance);
        }
    }
    private void Awake()
    {
        pathfindingAI = GetComponent<RichAI>();
    }
    private void UpdateSpeedBasedOnNumberOfUnits() //bigger armies move slower
    {

    }
    private void Start()
    {
        //GenerateArmy();
    }
    private void UpdateLineRenderer()
    {
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y + .2f, transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(aiTarget.position.x, aiTarget.position.y + .2f, aiTarget.position.z));
    }
    private void Update()
    {
        UpdateLineRenderer();
    }
    public void GenerateArmy()
    {
        foreach (ArmyCard card in armyDisplayCards)
        {
            Destroy(card);
        }
        armyDisplayCards.Clear();
        var x = 0;
        var y = 0;
        foreach (UnitInfoClass unit in listOfUnitsInThisArmy)
        {
            ArmyCard newCard = Instantiate(UnitManager.Instance.armyCardPrefab, OverworldManager.Instance.leftAnchor.position, Quaternion.identity, OverworldManager.Instance.armyCompBoxParent);
            ArmyCardScriptableObj cardInfo = ConvertUnitToCard(unit);
            newCard.cardName = cardInfo.cardName; //take information 
            newCard.cardColor = cardInfo.cardColor;
            newCard.cardIcon = cardInfo.cardIcon;
            newCard.cardTroops = unit.troops;
            newCard.cardMaxTroops = cardInfo.cardMaxTroops;

            CardVisual visuals = newCard.GetComponent<CardVisual>(); //apply to visuals
            visuals.colorBG.color = newCard.cardColor;
            visuals.cardName.text = newCard.cardName;
            visuals.troopNum.text = newCard.cardTroops + "/" + newCard.cardMaxTroops;
            visuals.icon.sprite = newCard.cardIcon; 

            armyDisplayCards.Add(newCard);
            newCard.transform.localPosition += new Vector3(214 * x, -214f* y, 0);
            x++;
            if (x >= 5)
            {
                y++;
                x = 0;
            }
        } 
    }  
    public void AddUnitToArmy(ArmyCardScriptableObj card)
    {
        UnitInfoClass unit = new UnitInfoClass();
        unit.troops = card.cardTroops;
        unit.type = card.cardType;
        unit.team = card.cardTeam;
        listOfUnitsInThisArmy.Add(unit);
    }
    private ArmyCardScriptableObj ConvertUnitToCard(UnitInfoClass unit)
    {
        List<GlobalDefines.SoldierTypes> list = UnitManager.Instance.unitTypes;
        List<ArmyCardScriptableObj> cardList = UnitManager.Instance.cardsToInstantiateBasedOnUnitType;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == unit.type)
            {
                return cardList[i];
            }
        }
        return null;
    }
    private void UpdateSupplyStatus(SupplyPoint point, bool enterOrExit)
    {
        if (enterOrExit)
        {
            onSupplyPoint = true;
            currentSupplyPoint = point;
            point.battleGroupAtThisSupplyPoint = this;
            OverworldManager.Instance.PlayerBattleGroupEnteredSupplyPoint();
        }
        else
        {
            onSupplyPoint = false;
            currentSupplyPoint = null;
            point.armyOnThisSupplyPoint = null;
            OverworldManager.Instance.PlayerBattleGroupExitedSupplyPoint();
        }
    }
    private void UpdateLocaleStatus(LocaleInvestigatable locale, bool enterOrExit)
    {

        if (enterOrExit)
        { 
            currentLocale = locale; 
            OverworldManager.Instance.PlayerBattleGroupEnteredLocale();
        }
        else
        { 
            currentLocale = null; 
            OverworldManager.Instance.PlayerBattleGroupExitedLocale();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        #region OnEnterForAll 
        #endregion
        #region OnEnterForPlayerOnly
        if (controlledBy == controlStatus.PlayerControlled)
        { 
            SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>(); //see if we are close to a supply giver
            if (collidedSupplyPoint != null)
            {
                UpdateSupplyStatus(collidedSupplyPoint, true);
            }
            LocaleInvestigatable collidedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
            if (collidedLocale != null)
            {
                UpdateLocaleStatus(collidedLocale, true);//update screen to show locale options
            }
        }
        /*if (controlledBy == controlStatus.PlayerControlled)
       {
           *//*SurpriseEvent surprise = other.gameObject.GetComponent<SurpriseEvent>();
           if (surprise != null)
           {
               if (surprise.eventDialogue != null && surprise.eventTriggered == false)
               {
                   suddenStop = true;
                   numberOfMovementAttempts = 100; //stop the movement of player
                   overworldManager.dialogueEvent = true;
                   overworldManager.localeArmy = this;
                   DialogueManager.Instance.loadedDialogue = surprise.eventDialogue;
                   DialogueManager.Instance.StartDialogue();
                   surprise.eventTriggered = true;
               }
           }*//*
       }*/
        //Army collidedArmy = other.gameObject.GetComponent<Army>();
        /*if (!aiControlled && collidedArmy != null && collidedArmy.faction != faction)
        {
            Debug.Log("WAR");
            OverworldToFieldBattleManager.Instance.StartFieldBattleWithEnemyArmy(collidedArmy);
            //numberOfMovementAttempts = 100;
            //collidedArmy.numberOfMovementAttempts = 100;
        }*/
        #endregion
        #region OnEnterForEnemyOnly
        #endregion 
        /*
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = true;
        }*/
    }
    private void OnTriggerExit(Collider other)
    {
        if (controlledBy == controlStatus.PlayerControlled)
        {
            SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>();
            if (collidedSupplyPoint != null)
            {
                UpdateSupplyStatus(collidedSupplyPoint, false);
            }
            LocaleInvestigatable collidedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
            if (collidedLocale != null)
            {
                UpdateLocaleStatus(collidedLocale, false);//update screen to show locale options
            }
        }
        
        /*LocaleInvestigatable exitedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
        if (exitedLocale != null)
        {
            suddenStop = false;
            currentLocale = null;
        } 
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = false;
        }*/
    }

}
