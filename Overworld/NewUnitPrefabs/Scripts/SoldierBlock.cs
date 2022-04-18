using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoldierBlock : MonoBehaviour
{

    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform modelParent;
    [SerializeField] private List<Position> formationPositions;
    [SerializeField] private Transform FormationTransform;
    [SerializeField] private Quaternion angleToFace;
    [SerializeField] private Transform target;
    [SerializeField] private string team = "Altgard";
    [SerializeField] private FormationPosition formPos;
    public List<SoldierModel> listSoldierModels;
    public SoldierModel arbiter;
    void Start()
    {
        formPos = GetComponentInChildren<FormationPosition>();
        formPos.team = team;
        int num = 0;
        int increment = 0;
        int row = 1;
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

            listSoldierModels.Add(model);
            
            if (num == 40)
            {
                arbiter = model;
            }

            if (num <= 10)
            {
                formPos.firstRowPositions.Add(position);
            }

            position.assignedSoldierModel = model;
            position.row = row;
            if (increment >= 10)
            {
                increment = 0;
                row++;
            }
        }
    }
}