using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SoldierBlock : MonoBehaviour
{

    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private Transform modelParent;
    [SerializeField] private List<Transform> formationPositions;
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
        int num = 1;
        foreach (Transform elTransform in formationPositions)
        {
            GameObject soldier = Instantiate(soldierPrefab, elTransform.position, angleToFace, modelParent);
            AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
            aiDesSet.target = elTransform; 


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
            num++;

        }
    }
}