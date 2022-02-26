using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

public class OverworldManager : MonoBehaviour
{
    public List<Army> armies;
    public Vector3 clickPosition;
    public Army selectedArmy;

    public Transform clickPosIndicator;
    public Transform maximumRange;

    public GameObject armyPrefab;

    public bool readyToSpawnArmy = false;
        
    public Button executeButton;
    public Button combineArmy;


    // Start is called before the first frame update
    void Start()
    {
        //executeButton.interactable = false;
        combineArmy.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        LeftClickCheck();
        RightClickCheck();
    }
    public void ReadyToSpawnArmy()
    {
        if (readyToSpawnArmy)
        {
            readyToSpawnArmy = false;
        }
        else
        {
            readyToSpawnArmy = true;
        }
    }

    public void ExecuteMovesForAllArmies()
    {
        foreach (Army elementArmy in armies)
        {
            elementArmy.StartMoving();
        }
    }

    private void LeftClickCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                clickPosition.x = RoundToZeroOrHalf(clickPosition.x); 
                clickPosition.y = 0;
                clickPosition.z = RoundToZeroOrHalf(clickPosition.z);

                clickPosIndicator.position = clickPosition;

                if (readyToSpawnArmy)
                {
                    SpawnArmy();
                }
                else
                {
                    SelectArmy();
                }
            }
        }
    }

    public void CombineArmy()
    {
        //wait for you to left click on another army you own
        //when you do the ai target becomes the other army
        //when the first army collides with it, give second army all of first army's units. destroy first army
    }
    public void SplitArmy()
    {
        //choose how many troops to split
        //left click on position to place new army
        //new army gets the troops that split, og army loses those troops
    }

    private void SpawnArmy()
    {
        GameObject newArmy = Instantiate(armyPrefab, Vector3.zero, Quaternion.identity); //instantiate army prefab
        Transform transform = newArmy.transform.GetChild(0); //get transform of figurine
        transform.position = clickPosition; //move figurine to click pos
        Transform targetTransform = newArmy.transform.GetChild(1);
        targetTransform.position = clickPosition;
        Transform aiTargetTransform = newArmy.transform.GetChild(2);
        aiTargetTransform.position = clickPosition;


        Army newArmyComp = newArmy.GetComponentInChildren<Army>(); //get army
        SingleNodeBlocker newArmyBlocker = newArmy.GetComponentInChildren<SingleNodeBlocker>();

        foreach (Army elArmy in armies) //other armies need to treat this as a blocker
        {
            newArmyComp.obstacles.Add(elArmy.GetComponentInChildren<SingleNodeBlocker>());
            elArmy.obstacles.Add(newArmyBlocker);
        }
        armies.Add(newArmyComp); //put army in list so that it can be selected
        

        readyToSpawnArmy = false;
    }
    private void SelectArmy()
    {
        bool foundOne = false;
        foreach (Army checkedArmy in armies)
        {
            Vector3 diff = checkedArmy.transform.position - clickPosition;
            diff.x = Mathf.Abs(diff.x);
            diff.z = Mathf.Abs(diff.z);

            if (diff.x < 0.5f && diff.z < 0.5f) //safe check to see if we clicked on army
            {
                selectedArmy = checkedArmy;
                foundOne = true;
                combineArmy.interactable = true;
                break;
            }
        }
        if (foundOne == false)
        {
            selectedArmy = null;
            combineArmy.interactable = false;
        }
    }
    private float RoundToZeroOrHalf(float a) //1.52 will be 1.5, 1.1232 will be 1
    {
        int b = Mathf.RoundToInt(a);
        if (a > b)
        {
            return b + .5f;
        }
        else
        {
            return b - .5f;
        }
    }
    private void RightClickCheck()
    {
        if (Input.GetMouseButtonDown(1) && selectedArmy != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                clickPosition.x = RoundToZeroOrHalf(clickPosition.x);
                clickPosition.y = 0;
                clickPosition.z = RoundToZeroOrHalf(clickPosition.z);
                if (selectedArmy.target != null)
                {
                    selectedArmy.target.position = clickPosition;
                    ABPath path = selectedArmy.DrawPath();
                    int selectedArmySpeedMax = selectedArmy.speedMax;
                    if (selectedArmySpeedMax > path.vectorPath.Count-1)
                    {
                        selectedArmySpeedMax = path.vectorPath.Count - 1;
                    }
                    maximumRange.position = path.vectorPath[selectedArmySpeedMax];
                }
            }
        }
    }

}
