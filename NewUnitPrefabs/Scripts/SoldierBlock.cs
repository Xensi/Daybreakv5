using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using System.Threading;
using System.Threading.Tasks;
public class SoldierBlock : MonoBehaviour
{
    public List<Row> rows; 
    public Row frontRow; 
    [SerializeField] private GameObject soldierPrefab; 
    [SerializeField] private GameObject magePrefab; 
    public enum MageTypes {
        None, Pyromancer, Gallowglass, Eldritch, Flammen, Seele, Torches
    }
    public MageTypes mageType;
    public float ability1Range = 25;
    public float ability2Range = 25;

    public Transform modelParent; 
    public List<Position> magePositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    public GlobalDefines.Team teamType = GlobalDefines.Team.Altgard; 
    [SerializeField] public FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public List<SoldierModel> listMageModels;



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
    public SoldierModel[] modelsArray; //all, max 80
    public Transform[] modelTransformArray;
    public Position[] formationPositions; //all, max 80

    private void OnEnable()
    {
        //SetUpSoldiers();
    }

    public bool AddModelToSoldierBlock(SoldierModel model)
    { 
        if (formPos.numberOfAliveSoldiers < formPos.maxSoldiers)
        { 
            bool cont = false;
            for (int i = 0; i < modelsArray.Length; i++) //assign to array
            {
                SoldierModel checkedModel = modelsArray[i];
                if (checkedModel == null) //if empty, set
                {
                    modelsArray[i] = model;
                    model.formPos = formPos;
                    cont = true;
                    break;
                }
                else if (!checkedModel.alive) //if dead, set
                {
                    modelsArray[i] = model;
                    model.formPos = formPos;
                    cont = true;
                    break;
                }
            }
            if (cont)
            { 
                foreach (Row rowItem in rows)
                {
                    foreach (Position position in rowItem.positionsInRow)
                    {
                        if (position.assignedSoldierModel == null)
                        {
                            rowItem.modelsInRow.Add(model);
                            position.assignedSoldierModel = model;
                            model.target = position.transform;
                            model.modelPosition = position;
                            formPos.numberOfAliveSoldiers++;
                            return true;
                        }
                        else if (!position.assignedSoldierModel.alive)
                        {
                            rowItem.modelsInRow.Add(model);
                            position.assignedSoldierModel = model;
                            model.target = position.transform;
                            model.modelPosition = position;
                            formPos.numberOfAliveSoldiers++;
                            return true;
                        }
                    } 
                }
            }
            return false;
        }
        return false;
    }
    public Vector3 GenerateDispersalVector(float dispersal)
    {
        return new Vector3(UnityEngine.Random.Range(-dispersal, dispersal), 0, UnityEngine.Random.Range(-dispersal, dispersal));
    }
    private float dispersalLevel = .25f;
    public async void SetUpSoldiers()
    { 
        if (initialized)
        {
            return;
        }
        initialized = true;
        modelsArray = new SoldierModel[80];
        modelTransformArray = new Transform[80];
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

            formPos.walkingSpeed = desiredWalkingSpeed * .5f;
        }
        formPos.sprintSpeed = desiredWalkingSpeed;
        int arrayInc = 0;

        //
        formPos.numberOfAliveSoldiers = 0;
        formPos.infantrySpeed = desiredWalkingSpeed/2;

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
                    if (formPos.numberOfAliveSoldiers < soldiersToCreate)
                    {
                        Vector3 dispersal = GenerateDispersalVector(dispersalLevel); 
                        //spawn soldier
                        GameObject soldier = Instantiate(soldierPrefab, position.transform.position + dispersal, angleToFace, modelParent);//position.transform
                        SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();

                           
                        formPos.numberOfAliveSoldiers++;
                        //getmodel 
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
                        model.startingMaxSpeed = desiredWalkingSpeed;
                        model.walkSpeed = desiredWalkingSpeed;
                        model.runSpeed = desiredWalkingSpeed * 2; 
                        model.team = teamType;
                        model.formPos = formPos;
                        model.melee = melee;
                        listSoldierModels.Add(model);
                        position.assignedSoldierModel = model;
                        model.falter = 1 + (rowItem.rowNum) * 0.1f;
                        modelsArray[arrayInc] = model;
                        modelTransformArray[arrayInc] = model.transform;
                    }
                    position.formPos = formPos;
                    position.row = rowItem;
                    position.team = teamType;
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
                SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
                model.target = position.transform;
                modelsArray[arrayInc] = model;
                model.walkSpeed = desiredWalkingSpeed;
                model.runSpeed = desiredWalkingSpeed * 2; 
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
        await Task.Yield();
    }
    public void SetUpPositionsOnly()
    {
        modelsArray = new SoldierModel[82];
        formationPositions = new Position[80];

        formPos = GetComponentInChildren<FormationPosition>();
        formPos.team = teamType; 
        if (formPos.formationType == FormationPosition.FormationType.Cavalry)
        {
            formPos.walkingSpeed = desiredWalkingSpeed * .75f;
        }
        else
        {

            formPos.walkingSpeed = desiredWalkingSpeed * .5f;
        }
        formPos.sprintSpeed = desiredWalkingSpeed;  
        formPos.numberOfAliveSoldiers = 0;
        int arrayInc = 0;
        foreach (Row rowItem in rows)
        {
            rowItem.soldierBlock = this;
            rowItem.rowPositionInList = rows.IndexOf(rowItem);
            foreach (Position position in rowItem.positionsInRow)
            { 
                position.formPos = formPos;
                position.row = rowItem;
                position.team = teamType;
                formationPositions[arrayInc] = position;
                arrayInc++;
            }
        }
    }

    public void SelfDestruct()
    {
        Destroy(this);
    }

}