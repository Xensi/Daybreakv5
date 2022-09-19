using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoldierBlock : MonoBehaviour
{

    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private GameObject magePrefab; 
    public string mageType = "";
    [SerializeField] private Transform modelParent;
    public List<Position> formationPositions;
    public List<Position> magePositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    public Transform target;
    [SerializeField] private string team = "Altgard";
    [SerializeField] private FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public List<SoldierModel> listMageModels;

    public SoldierModel[] modelsArray; //all, max 80
    public Position[] reinforcePositionsArray; //all, max 80


    public SoldierModel arbiter;

    public List<Position> reinforceablePositions;

    public float modelAttackRange = 4.5f;

    [SerializeField] private float desiredWalkingSpeed = 3;
    public bool melee = true;

    public bool canBeRanged = false;
    public bool arcingProjectiles = true; //will our projectiles arc or be direct fired?

    public bool hasSpecialVeterans = false;

    public List<ProjectileFromSoldier> listProjectiles;

    public bool useActualMaxSpeed = true;
    public float forcedMaxSpeed = 0;


    void Start()
    {
        formPos = GetComponentInChildren<FormationPosition>();
        formPos.team = team;
        int num = 0;
        int increment = 0;
        int row = 1;
        //formPos.tag = team + "Formation";
        formPos.walkingSpeed = desiredWalkingSpeed / 2;
        formPos.sprintSpeed = desiredWalkingSpeed; 
        int arrayInc = 0;
        modelsArray = new SoldierModel[82];
        reinforcePositionsArray = new Position[70];

        foreach (Position position in formationPositions)
        {
            increment++;
            num++;
            GameObject soldier = Instantiate(soldierPrefab, position.transform.position, angleToFace, modelParent);
            AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
            aiDesSet.target = position.transform;  

            SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
            modelsArray[arrayInc] = model;


            model.walkSpeed = desiredWalkingSpeed;
            model.runSpeed = desiredWalkingSpeed * 2;
            model.richAI.maxSpeed = desiredWalkingSpeed;
            model.target = target;
            model.team = team;
            model.formPos = formPos;
            model.self.tag = team + "Model";
            //model.attackRange = modelAttackRange;
            
            listSoldierModels.Add(model);


            if (num == 40)
            {
                arbiter = model;
            }

            if (num <= 10) //row 1
            {
                formPos.frontlinePositions.Add(position);
                formPos.firstLineModels.Add(model);
            }
            if (num <= 70)
            {
                //reinforceablePositions.Add(position);
                reinforcePositionsArray[arrayInc] = position;
            }
            if (num >= 71 && hasSpecialVeterans)
            {
                model.isVeteran = true;
                foreach (SkinnedMeshRenderer mesh in model.normalMeshes)
                {
                    mesh.enabled = false;
                }
                foreach (SkinnedMeshRenderer mesh in model.veteranMeshes)
                {
                    mesh.enabled = true;
                }
            }
            position.formPos = formPos;
            position.assignedSoldierModel = model;
            model.position = position;
            position.row = row;
            position.team = team;
            if (increment >= 10)
            {
                increment = 0;
                row++;
            } 
            arrayInc++;
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
                model.target = target;
                model.team = team;
                model.formPos = formPos;
                model.self.tag = team + "Model";

                listSoldierModels.Add(model);
                listMageModels.Add(model);
                position.formPos = formPos;
                position.assignedSoldierModel = model;
                model.position = position;
                position.row = row;
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