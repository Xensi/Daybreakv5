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
    public string mageType = ""; //Pyromancer, Gallowglass
    public Transform modelParent; 
    public Position[] formationPositions; //all, max 80
    public List<Position> magePositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    public GlobalDefines.Team teamType = GlobalDefines.Team.Altgard;
    [SerializeField] private string team = "Altgard";
    [SerializeField] private FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public List<SoldierModel> listMageModels;

    public SoldierModel[] modelsArray; //all, max 80 


    public SoldierModel arbiter;

    public List<Position> reinforceablePositions;

    public List<Position> retreatPositions;

    public float modelAttackRange = 4.5f;

    public float desiredWalkingSpeed = 3;
    public bool melee = true;

    public bool canBeRanged = false; 

    public bool hasSpecialVeterans = false;

    public List<ProjectileFromSoldier> listProjectiles;

    public bool useActualMaxSpeed = true;
    public float forcedMaxSpeed = 0;

    [SerializeField] private bool manuallyAssignRows = false;

    private bool initialized = false;

    public FightManager manager;
    private void OnEnable()
    {
        if (initialized)
        {    
            return;
        }
        initialized = true;
        modelsArray = new SoldierModel[82];
        formationPositions = new Position[80];

        formPos = GetComponentInChildren<FormationPosition>();
        formPos.team = team;
        int num = 0;
        int increment = 0; 
        if (formPos.isCavalry)
        { 
            formPos.walkingSpeed = desiredWalkingSpeed *.75f;
        }
        else
        {

            formPos.walkingSpeed = desiredWalkingSpeed / 2;
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

                    AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
                    aiDesSet.target = position.transform;
                    //getmodel
                    SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
                    model.modelPosition = position;
                    rowItem.modelsInRow.Add(model);
                    modelsArray[arrayInc] = model;
                    model.walkSpeed = desiredWalkingSpeed;
                    model.runSpeed = desiredWalkingSpeed * 2;
                    model.richAI.maxSpeed = desiredWalkingSpeed; 
                    model.team = team;
                    model.formPos = formPos;
                    model.melee = melee; 
                    listSoldierModels.Add(model);  
                    position.formPos = formPos;
                    position.assignedSoldierModel = model;
                    position.row = rowItem;
                    position.team = team; 
                    formationPositions[arrayInc] = position; 
                    arrayInc++;

                }
            }
        }
        
        if (magePrefab != null)
        { 
            foreach (Position position in magePositions)
            {
                increment++;
                GameObject soldier = Instantiate(magePrefab, position.transform.position, angleToFace, modelParent);
                AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
                aiDesSet.target = position.transform;
                SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
                modelsArray[arrayInc] = model;
                model.walkSpeed = desiredWalkingSpeed;
                model.runSpeed = desiredWalkingSpeed * 2;
                model.richAI.maxSpeed = desiredWalkingSpeed; 
                model.team = team;
                model.formPos = formPos;
                model.melee = melee; 
                listSoldierModels.Add(model);
                listMageModels.Add(model);
                position.formPos = formPos;
                position.assignedSoldierModel = model;
                model.modelPosition = position; 
                position.team = team; 
                arrayInc++; 
            }
        } 
    }

    public void SelfDestruct()
    {
        Destroy(this);
    }

}