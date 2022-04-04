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
    void Start()
    {
        FormationPosition formation = GetComponentInChildren<FormationPosition>();
        formation.team = team; 

        foreach (Transform elTransform in formationPositions)
        {
            GameObject soldier = Instantiate(soldierPrefab, elTransform.position, angleToFace, modelParent);
            AIDestinationSetter aiDesSet = soldier.GetComponentInChildren<AIDestinationSetter>();
            aiDesSet.target = elTransform; 

            SoldierModel model = soldier.GetComponentInChildren<SoldierModel>();
            model.target = target;
            model.team = team;
        }
    }
}