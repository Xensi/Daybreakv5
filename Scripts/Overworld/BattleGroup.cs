using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int provisions = 50;
    public int maxProvisions = 100;
    public int spoils = 0;
    public int maxSpoils = 100;

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
    private void OnTriggerEnter(Collider other)
    {
        #region OnEnterForAll
        SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>(); //see if we are close to a supply giver
        if (collidedSupplyPoint != null)
        {
            UpdateSupplyStatus(collidedSupplyPoint, true);
        }
        #endregion
        #region OnEnterForAI
        #endregion
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
        //Debug.LogError("collision?");
        /*if (awaitingCollisionWith != null)
        {
            if (collidedArmy == awaitingCollisionWith)
            {
                //Debug.LogError("First army collided with second army");'
                collidedArmy.numberOfUnitsInArmy += numberOfUnitsInArmy;
                awaitingCollisionWith = null;
                overworldManager.armies.Remove(this);
                Destroy(parent);
            }
        }
        
        if (!aiControlled && collidedArmy != null && collidedArmy.faction != faction)
        {
            Debug.Log("WAR");
            OverworldToFieldBattleManager.Instance.StartFieldBattleWithEnemyArmy(collidedArmy);
            //numberOfMovementAttempts = 100;
            //collidedArmy.numberOfMovementAttempts = 100;
        }
        LocaleInvestigatable collidedLocale = other.gameObject.GetComponent<LocaleInvestigatable>();
        if (collidedLocale != null)
        {
            currentLocale = collidedLocale;
        }
        if (other == watchdogBounds)
        {
            withinWatchdogBounds = true;
        }*/
    }
    private void OnTriggerExit(Collider other)
    {
        SupplyPoint collidedSupplyPoint = other.gameObject.GetComponent<SupplyPoint>();
        if (collidedSupplyPoint != null)
        {
            UpdateSupplyStatus(collidedSupplyPoint, false);
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
