using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationSpawner : MonoBehaviour
{
    public SoldierBlock prefab;
    public enum SpawnCondition
    {
            Sleeping, Periodic, OneTime, Disabled
    }
    public SpawnCondition condition;

    public Collider conditionCollider; //trigger
    public List<FormationPosition> nearbyFormations;
    public int timer = 30;
    public bool timerCounting = false;
    public GlobalDefines.Team team = GlobalDefines.Team.Zhanguo;
    public int numberOfSoldiersToSpawn = 20;
    public int charges = 4;
    public int timerMax = 30;
    private void Start()
    {
        timerMax = timer;
    }
    private void OnTriggerEnter(Collider other)
    {
        FormationPosition formPos = other.GetComponent<FormationPosition>();
        if (formPos != null)
        {
            Debug.Log("Counting down");
            //nearbyFormations.Add(formPos);
            switch (condition)
            {
                case SpawnCondition.Sleeping: 
                    if (!timerCounting)
                    {
                        timerCounting = true;
                        Invoke("TimerCountDown", 1);
                    }
                    break;
                case SpawnCondition.Periodic:
                    break;
                case SpawnCondition.OneTime:
                    break;
                default:
                    break;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    { 
        FormationPosition formPos = other.GetComponent<FormationPosition>();
        if (formPos != null)
        {
            //Debug.Log("TEST2");
            //nearbyFormations.Remove(formPos);
        }
    }
    private void TimerCountDown()
    { 
        if (timerCounting && timer > 0 && condition != SpawnCondition.Disabled)
        { 
            timer--;
            if (timer <= 0)
            {
                timerCounting = false;
                TimerFinished();
            }
            else
            {
                Invoke("TimerCountDown", 1);
            }
        }
    }
    private void TimerFinished()
    {
        charges--;

        switch (condition)
        {
            case SpawnCondition.Sleeping:
                SpawnFormation(transform.position, numberOfSoldiersToSpawn, team);
                break;
            case SpawnCondition.Periodic:
                break;
            case SpawnCondition.OneTime:
                break;
            default:
                break;
        }
        condition = SpawnCondition.Disabled;
        /*if (charges <= 0)
        {
        }
        else
        {
            timer = timerMax;
            timerCounting = true;
        }*/
    }
    public void SpawnFormation(Vector3 pos, int numberOfSoldiers, GlobalDefines.Team team)
    {
        SoldierBlock block = Instantiate(prefab, pos, Quaternion.identity);
        block.soldiersToCreate = numberOfSoldiers; //set how many soldiers to create
        block.teamType = team;

        FormationPosition form = block.GetComponentInChildren<FormationPosition>();
        if (form != null)
        {
            FightManager.Instance.allFormationsList.Add(form);
            FightManager.Instance.allArray = FightManager.Instance.allFormationsList.ToArray();
            if (block.teamType == FightManager.Instance.team)
            {
                FightManager.Instance.playerControlledFormations.Add(form);
            }
            else
            {
                FightManager.Instance.enemyControlledFormations.Add(form);
                form.AIControlled = true;
            }
            form.FixPositions();
            block.SetUpSoldiers(); // create soldiers
            form.Invoke("BeginUpdates", 1);
        }
    }
}
