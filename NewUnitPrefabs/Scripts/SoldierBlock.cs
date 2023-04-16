using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoldierBlock : MonoBehaviour
{
    public List<Row> rows; 
    public Row frontRow; 
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private GameObject magePrefab; 
    public enum MageTypes {
        None, Pyromancer, Gallowglass, Eldritch, Flammen, Seele
    }
    public MageTypes mageType;
    public float ability1Range = 25;
    public float ability2Range = 25;

    public Transform modelParent; 
    public Position[] formationPositions; //all, max 80
    public List<Position> magePositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    public GlobalDefines.Team teamType = GlobalDefines.Team.Altgard; 
    [SerializeField] private FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public List<SoldierModel> listMageModels;

    public SoldierModel[] modelsArray; //all, max 80 


    public SoldierModel arbiter;

    public List<Position> reinforceablePositions;

    public List<Position> retreatPositions;

    [HideInInspector] public float modelAttackRange = 4.5f;

    public float desiredWalkingSpeed = 3;
    public bool melee = true;

    public bool canBeRanged = false; 

    public bool hasSpecialVeterans = false;

    public List<ProjectileFromSoldier> listProjectiles; 
    public float forcedMaxSpeed = 0;

    [SerializeField] private bool manuallyAssignRows = false;

    private bool initialized = false;

    public FightManager manager;

    public int soldiersToCreate = 80;

    private void OnEnable()
    {
        //SetUpSoldiers();
    }
    
    public void SetUpSoldiers()
    { 
        if (initialized)
        {
            return;
        }
        initialized = true;
        modelsArray = new SoldierModel[82];
        formationPositions = new Position[80];

        formPos = GetComponentInChildren<FormationPosition>(); 
        formPos.team = teamType;
        int num = 0;
        int increment = 0;
        if (formPos.formationType == FormationPosition.FormationType.Cavalry)
        {
            formPos.walkingSpeed = desiredWalkingSpeed * .75f;
        }
        else
        {

            formPos.walkingSpeed = desiredWalkingSpeed * .5f; /// 2;
        }
        formPos.sprintSpeed = desiredWalkingSpeed;
        int arrayInc = 0;

        //
        formPos.numberOfAliveSoldiers = 0;
        if (rows.Count > 0)
        {
            foreach (Row rowItem in rows)
            {
                rowItem.soldierBlock = this;
                rowItem.rowPositionInList = rows.IndexOf(rowItem);
                foreach (Position position in rowItem.positionsInRow)
                {
                    //raise
                    increment++;
                    num++;
                    //spawn soldier
                    GameObject soldier = Instantiate(soldierPrefab, position.transform.position, angleToFace, modelParent);
                    formPos.numberOfAliveSoldiers++;
                    //getmodel
                    SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
                    if (model.attackType == SoldierModel.AttackType.Melee || model.attackType == SoldierModel.AttackType.CavalryCharge)
                    { 
                        modelAttackRange = model.meleeAttackRange;
                    }
                    else
                    { 
                        modelAttackRange = model.rangedAttackRange;
                    }
                    model.target = position.transform;
                    model.modelPosition = position;
                    rowItem.modelsInRow.Add(model);
                    modelsArray[arrayInc] = model;
                    model.walkSpeed = desiredWalkingSpeed;
                    model.runSpeed = desiredWalkingSpeed * 2;
                    model.pathfindingAI.maxSpeed = desiredWalkingSpeed; 
                    model.team = teamType;
                    model.formPos = formPos;
                    model.melee = melee;
                    listSoldierModels.Add(model);
                    position.formPos = formPos;
                    position.assignedSoldierModel = model;
                    position.row = rowItem; 
                    position.team = teamType;
                    formationPositions[arrayInc] = position;
                    arrayInc++;

                    if (formPos.numberOfAliveSoldiers >= soldiersToCreate)
                    {
                        break;
                    }
                }
                if (formPos.numberOfAliveSoldiers >= soldiersToCreate)
                {
                    break;
                }
            }
        }

        if (magePrefab != null)
        {
            foreach (Position position in magePositions)
            {
                increment++;
                GameObject soldier = Instantiate(magePrefab, position.transform.position, angleToFace, modelParent); 
                SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
                model.target = position.transform;
                modelsArray[arrayInc] = model;
                model.walkSpeed = desiredWalkingSpeed;
                model.runSpeed = desiredWalkingSpeed * 2;
                model.pathfindingAI.maxSpeed = desiredWalkingSpeed; 
                model.team = teamType;
                model.formPos = formPos;
                model.melee = melee;
                listSoldierModels.Add(model);
                listMageModels.Add(model);
                position.formPos = formPos;
                position.assignedSoldierModel = model;
                model.modelPosition = position; 
                position.team = teamType;
                arrayInc++;
            }
        }
    }

    public void SelfDestruct()
    {
        Destroy(this);
    }

}