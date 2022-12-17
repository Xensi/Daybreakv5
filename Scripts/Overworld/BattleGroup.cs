using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class BattleGroup : MonoBehaviour
{ 
    public enum controlStatus
    {
        PlayerControlled,
        AIControlled
    }
    public enum aiBehavior
    {
        Aggressive, //always attacks
        Opportunistic, //attacks if stronger, runs if weaker
        Fearful, //always runs
        Ignorant //acts as if it doesn't see the player
    }

    public enum aiPriority
    {
        Hunt, //chase down known player location
        Patrol, //go to set patrol points
        Wander, //move randomly
        FindSupplies, //go to home base and get new troops and supplies
        Flee, //just run in direction opposite 
        Idle //don't do anything
    }
    public aiPriority aiCurrentPriority = aiPriority.Patrol;
    public aiBehavior aiCurrentBehavior = aiBehavior.Opportunistic;

    public controlStatus controlledBy = controlStatus.PlayerControlled;
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard;


    public List<UnitInfoClass> listOfUnitsInThisArmy;
    public Transform aiTarget;

    private bool onSupplyPoint = false;
    public SupplyPoint currentSupplyPoint = null;
    public LocaleInvestigatable currentLocale = null;

    public float supplies = 100;
    public float maxSupplies = 100;
    public int aiLowSuppliesThreshold = 50;
    public int spoils = 2;
    public int maxSpoils = 100;
    public int morale = 100;
    public int maxMorale = 100;

    public List<ArmyCard> armyDisplayCards;
    [SerializeField]
    private LineRenderer lineRenderer;

    public float aiSightDistance = 21;
    public float aiPlayerTooCloseDistance = 10;

    public bool aiCanSeePlayer = false;
    public RichAI pathfindingAI;
    public float maxSpeed = 2;
    private float unitSpeedDebuff = 0.1f;
    private float horseSpeedBuff = 0.01f;
    public float horses = 0;

    public bool allowedToStartCombat = true;
    public Vector3 aiLastKnownPlayerPosition;

    public List<Transform> aiPatrolNodes;
    public int aiPatrolIndex = 0;
    public SupplyPoint aiNearestSupplyPoint;
    public int provisionsConsumptionRatePerUnit = 1;

    private float consumptionModifier = 0.25f;
    public List<SupplyPoint> aiVisitedSupplyPoints; 
    private bool aiNowResupplying = false;
    private void Patrol()
    {
        if (aiPatrolNodes.Count > 0)
        {
            float threshold = .1f;
            if (Vector3.Distance(transform.position, aiPatrolNodes[aiPatrolIndex].position) < threshold)
            {
                aiPatrolIndex++;
                if (aiPatrolIndex >= aiPatrolNodes.Count)
                {
                    aiPatrolIndex = 0;
                }
            }
            aiTarget.position = aiPatrolNodes[aiPatrolIndex].position;
        }
    }
    private void ConsumeProvisions()
    {
        if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.Overworld && BattleGroupManager.Instance.timeScale > 0)
        { 
            supplies -= provisionsConsumptionRatePerUnit * listOfUnitsInThisArmy.Count * BattleGroupManager.Instance.timeScale * consumptionModifier;
            OverworldManager.Instance.ShowArmyInfoAndUpdateArmyBars();
        }
    }
    private void Awake()
    {
        pathfindingAI = GetComponent<RichAI>();
    }
    private void Start()
    {
        //GenerateArmy();
        InvokeRepeating("ConsumeProvisions", 1, 1);
        threatValue = CalculateThreatValueOfArmy(); 
    }
    private bool IsPlayerWeakerOrStronger()
    {
        if (threatValue > OverworldManager.Instance.playerBattleGroup.threatValue)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private float triumphMultiplier = 0.25f;
    public float aiTriumphTimer = 0; //higher the more casualties inflicted
    public void SetTriumphTimer()
    {
        aiTriumphTimer = casualtiesInflictedThisBattle * triumphMultiplier;
        allowedToStartCombat = false;
        InvokeRepeating("UpdateTriumphTimer", 1, 1);
    }
    public int casualtiesInflictedThisBattle = 0;
    private void UpdateTriumphTimer()
    {
        if (aiTriumphTimer > 0)
        { 
            aiTriumphTimer -= 1 * BattleGroupManager.Instance.timeScale;
        }
        else
        {
            aiTriumphTimer = 0;
            allowedToStartCombat = true;
            CancelInvoke("UpdateTriumphTimer");
        }
    }
    public bool reachedDestination = false;
    private void Update()
    {
        if (controlledBy == controlStatus.PlayerControlled)
        { 
            if (Vector3.Distance(transform.position, aiTarget.position) <= 0.01f && !reachedDestination)
            {
                reachedDestination = true;
                BattleGroupManager.Instance.ForcePause();
            } 
        }
        UpdateLineRenderer();
        if (controlledBy == controlStatus.AIControlled)
        {
            if (aiCurrentBehavior == aiBehavior.Ignorant)
            {
                aiSightDistance = 0;
            }
            else
            {
                aiSightDistance = 21;
            }
            switch (aiCurrentPriority)
            {
                case aiPriority.Hunt: 
                    if (supplies < aiLowSuppliesThreshold || !allowedToStartCombat)
                    {
                        aiCurrentPriority = aiPriority.FindSupplies;
                    }
                    else
                    {
                        if (aiCanSeePlayer) //if we can see the player go there
                        {
                            aiTarget.position = OverworldManager.Instance.playerBattleGroup.transform.position;
                        }
                        else //if we can't go to last known position
                        {
                            aiTarget.position = aiLastKnownPlayerPosition;
                        }
                    }
                    break;
                case aiPriority.Patrol:
                    if (aiCanSeePlayer)
                    {
                        if (aiCurrentBehavior == aiBehavior.Aggressive)
                        { 
                            aiCurrentPriority = aiPriority.Hunt;
                        }
                        else if (aiCurrentBehavior == aiBehavior.Opportunistic)
                        {
                            if (IsPlayerWeakerOrStronger())
                            {
                                aiCurrentPriority = aiPriority.Hunt;
                            }
                            else if (Vector3.Distance(transform.position, OverworldManager.Instance.playerBattleGroup.transform.position) > aiPlayerTooCloseDistance)
                            {
                                if (supplies < aiLowSuppliesThreshold)
                                {
                                    aiCurrentPriority = aiPriority.FindSupplies;
                                }
                                else
                                {
                                    Patrol();
                                }
                            }
                            else
                            {
                                aiCurrentPriority = aiPriority.Flee;
                            }
                        }
                        else if (aiCurrentBehavior == aiBehavior.Fearful)
                        { 
                            aiCurrentPriority = aiPriority.Flee;
                        }
                    }
                    else
                    {
                        if (supplies < aiLowSuppliesThreshold)
                        {
                            aiCurrentPriority = aiPriority.FindSupplies;
                        }
                        else
                        { 
                            Patrol();
                        }
                    }
                    break;
                case aiPriority.Wander:
                    break;
                case aiPriority.FindSupplies:
                    if (aiNowResupplying == false)
                    { 
                        aiNowResupplying = true; 
                    }
                    if (Vector3.Distance(transform.position, OverworldManager.Instance.playerBattleGroup.transform.position) > aiPlayerTooCloseDistance || IsPlayerWeakerOrStronger())
                    {
                        if (aiNearestSupplyPoint == null)
                        {
                            SupplyPoint[] array = BattleGroupManager.Instance.allSupplyPointsArray;
                            float initDistance = 9999;
                            SupplyPoint closest = null;
                            for (int i = 0; i < array.Length; i++)
                            {
                                float distance = Helper.Instance.GetSquaredMagnitude(array[i].transform.position, transform.position);
                                if (distance < initDistance)
                                {
                                    initDistance = distance;
                                    closest = array[i];
                                }
                            }
                            aiNearestSupplyPoint = closest;
                        }
                        else
                        {
                            if (supplies < aiLowSuppliesThreshold)
                            {
                                aiTarget.position = aiNearestSupplyPoint.transform.position;
                            }
                            else
                            {
                                aiCurrentPriority = aiPriority.Patrol;
                                aiNowResupplying = false;
                            }
                        }
                    }
                    else
                    { 
                        aiCurrentPriority = aiPriority.Flee;
                        aiNowResupplying = false;
                    }
                    
                    break;
                case aiPriority.Flee:
                    if (aiCanSeePlayer)
                    {
                        Vector3 heading = transform.position - OverworldManager.Instance.playerBattleGroup.transform.position;
                        heading = heading.normalized;
                        float distance = 10;
                        aiTarget.transform.position = transform.position + (heading * distance);
                    }
                    else
                    {
                        aiCurrentPriority = aiPriority.FindSupplies;
                    }
                    break;
                case aiPriority.Idle:
                    //do nothing
                    break;
                default:
                    break;
            }
        }
    }
    public float CalculateThreatValueOfArmy()
    {
        float total = 0;
        for (int i = 0; i < listOfUnitsInThisArmy.Count; i++)
        {
            UnitInfoClass unit = listOfUnitsInThisArmy[i];
            if (unit != null)
            {
                ArmyCardScriptableObj cardInfo = ConvertUnitToCard(unit);
                total += unit.troops * cardInfo.threatValuePerIndividual;
            }
        }
        return total;
    }
    private void OnDrawGizmos()
    { 
        if (controlledBy == controlStatus.AIControlled)
        { 
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aiSightDistance);
        }
    }
    public void UpdateSpeedBasedOnNumberOfUnits(float timeScale) //bigger armies move slower
    {
        pathfindingAI.maxSpeed = (maxSpeed + horses * horseSpeedBuff- listOfUnitsInThisArmy.Count * unitSpeedDebuff) * timeScale;
    }
    private void UpdateLineRenderer()
    {
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, transform.position.y + .2f, transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(aiTarget.position.x, aiTarget.position.y + .2f, aiTarget.position.z));
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
        threatValue = CalculateThreatValueOfArmy();
    }
    public float threatValue = 0;
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
        SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>(); //see if we are close to a supply giver
        LocaleInvestigatable collidedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
        BattleGroup collidedBattleGroup = other.gameObject.GetComponent<BattleGroup>();
        #endregion
        #region OnEnterForPlayerOnly
        if (controlledBy == controlStatus.PlayerControlled)
        { 
            if (collidedSupplyPoint != null)
            {
                UpdateSupplyStatus(collidedSupplyPoint, true);
            }
            if (collidedLocale != null)
            {
                UpdateLocaleStatus(collidedLocale, true);//update screen to show locale options
            }
            if (collidedBattleGroup != null && collidedBattleGroup.controlledBy == controlStatus.AIControlled && collidedBattleGroup.allowedToStartCombat)
            {
                collidedBattleGroup.allowedToStartCombat = false;
                OverworldToFieldBattleManager.Instance.StartFieldBattleWithEnemyBattleGroup(collidedBattleGroup);
                //if you lose the AI should stop being able to see you for a while 
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
        #endregion
        #region OnEnterForAIOnly
        if (controlledBy == controlStatus.AIControlled)
        {
            if (collidedSupplyPoint != null)
            {
                AIResupply(collidedSupplyPoint);
            }
        }
        #endregion 
        /*
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = true;
        }*/
    }
    private void AIResupply(SupplyPoint supplyPoint)
    {
        //give army as many supplies as they can carry and that the town is willing to spare
        while (supplies < maxSupplies && supplyPoint.storedSupplies > 0 && supplyPoint.storedSupplies > supplyPoint.amountOfProvisionsToReserve)
        {
            supplies++;
            supplyPoint.storedSupplies--;
        }
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
