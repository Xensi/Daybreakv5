using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoldierBlock : MonoBehaviour
{

    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform modelParent;
    public List<Position> formationPositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    [SerializeField] private Transform target;
    [SerializeField] private string team = "Altgard";
    [SerializeField] private FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public SoldierModel arbiter;

    public List<Position> reinforceablePositions;

    public float modelAttackRange = 4.5f;

    void Start()
    {
        formPos = GetComponentInChildren<FormationPosition>();
        formPos.team = team;
        int num = 0;
        int increment = 0;
        int row = 1;
        //formPos.tag = team + "Formation";
        foreach (Position position in formationPositions)
        {
            increment++;
            num++;
            GameObject soldier = Instantiate(soldierPrefab, position.transform.position, angleToFace, modelParent);
            AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
            aiDesSet.target = position.transform;  

            SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
            model.target = target;
            model.team = team;
            model.formPos = formPos;
            model.self.tag = team + "Model";
            model.attackRange = modelAttackRange;
            
            listSoldierModels.Add(model);
            
            if (num == 40)
            {
                arbiter = model;
            }

            if (num <= 10) //row 1
            {
                formPos.frontlinePositions.Add(position);
            }
            if (num <= 70)
            {
                reinforceablePositions.Add(position);
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
        }
    }
}