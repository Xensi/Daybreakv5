using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using System.Threading.Tasks;
using System.Threading;
public class ArmingStation : MonoBehaviour
{
    public AttackableObject attackable;
    public GlobalDefines.Team team = GlobalDefines.Team.Zhanguo;

    public GameObject prefabUpgrade;
    public SoldierBlock formPrefab;
    public FormationPosition formPos; 

    private void OnTriggerEnter(Collider other)
    {
        ArmSoldier(other);
    }
    private async void ArmSoldier(Collider other)
    {
        if (attackable.alive)
        {
            SoldierModel model = other.GetComponentInParent<SoldierModel>();
            if (model != null && model.gameObject.tag == "Unarmed" && model.team == team) //
            {
                Debug.Log("arming unarmed soldier model");
                if (formPos == null)
                {
                    LayerMask mask = LayerMask.GetMask("Formation");
                    int maxColliders = 32; //lower numbers stop working
                    Collider[] colliders = new Collider[maxColliders];
                    int numColliders = Physics.OverlapSphereNonAlloc(transform.position, 30, colliders, mask, QueryTriggerInteraction.Ignore);
                    for (int i = 0; i < numColliders; i++) //go for hurtboxes
                    {
                        FormationPosition form = colliders[i].GetComponentInParent<FormationPosition>();
                        if (form != null && form.team == team)
                        {
                            formPos = form;
                            break;
                        }
                    }
                    if (formPos == null)
                    {
                        formPos = SpawnFormation(transform.position, 1, team); 
                    }
                }
                if (formPos != null)
                { 
                    GameObject obj = Instantiate(prefabUpgrade, model.transform.position, Quaternion.identity);
                    SoldierModel newModel = obj.GetComponentInChildren<SoldierModel>();
                    if (newModel != null)
                    {
                        newModel.team = team;
                        bool worked = formPos.soldierBlock.AddModelToSoldierBlock(newModel);
                        if (worked)
                        {
                            Debug.Log("Added success");
                            if (!formPos.updatesBegun)
                            {
                                formPos.BeginUpdates();
                            }
                            formPos.StopFleeing();
                        }
                    }
                    Destroy(model.transform.parent.gameObject);
                }
            }
        }
        await Task.Yield();
    }
    private FormationPosition SpawnFormation(Vector3 pos, int numberOfSoldiers, GlobalDefines.Team team)
    {
        SoldierBlock block = Instantiate(formPrefab, pos, Quaternion.identity);
        block.soldiersToCreate = numberOfSoldiers; //set how many soldiers to create
        block.teamType = team;

        FormationPosition form = block.formPos;
        if (form != null)
        {
            form.canRout = false;
            form.fightManager = FightManager.Instance; 
            form.aiPath = form.GetComponent<RichAI>();

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
            block.SetUpPositionsOnly();
            //block.SetUpSoldiers(); // create soldiers
        }
        return form;
    }
}
