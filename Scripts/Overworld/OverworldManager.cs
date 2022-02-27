using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using TMPro;

public class OverworldManager : MonoBehaviour
{
    public List<Army> armies;
    public List<Army> armiesGoingToSplit;
    public List<GameObject> splitIndicatorList;

    public Vector3 clickPosition;
    public Army selectedArmy;

    public Transform clickPosIndicator;
    public Transform maximumRange;

    public GameObject armyPrefab;

    public bool readyToSpawnArmy = false;

    public bool readyToCombineArmy = false;
    public bool readyToSplitArmy = false;

    public Army firstArmy;
    public Army secondArmy;

    public Button executeButton;
    public Button combineArmyButton;
    public Button splitArmyButton;

    public Button increaseStrengthButton;
    public Button decreaseStrengthButton;
    public Button confirmStrengthButton;

    public UIHover ui;

    public TMP_Text combineArmyReminder;
    public TMP_Text splitArmyReminder;

    public int strengthToTransfer = 0;

    public GameObject selectionIndicator;

    public GameObject splittingIndicator;
    public Button splitOffButtonTemplate;
    public int tempStrengthHolder = 0;

    public GameObject uiSplitOffParent;

    // Start is called before the first frame update
    void Start()
    {
        //executeButton.interactable = false;
        combineArmyButton.interactable = false;
        splitArmyButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ui.hovering == false)
        {
            LeftClickCheck();
            RightClickCheck();
        }
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
        foreach (GameObject i in splitIndicatorList)
        {
            Destroy(i);
        }
        splitIndicatorList.Clear();

        selectionIndicator.SetActive(false);
        foreach (Army elArmy in armiesGoingToSplit)
        {
            foreach (Button b in elArmy.listOfSplitOffs)
            {
                b.gameObject.SetActive(false);
                Destroy(b.gameObject);
            }
            elArmy.listOfSplitOffs.Clear();
            elArmy.SpawnSplitArmy();
            elArmy.strengthToSplitOff.Clear();
            elArmy.destinationForSplitOff.Clear();
            elArmy.actuallySplitOffOrNot.Clear();
        }

        armiesGoingToSplit.Clear();

        foreach (Army elArmy in armies)
        {
            elArmy.StartMoving();
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
                    SpawnArmy(clickPosition);
                }
                else if (readyToSplitArmy) //select location to send split army to
                {
                    //set it so that on execution, army will be spawned
                    selectedArmy.strengthToSplitOff.Add(strengthToTransfer);
                    selectedArmy.destinationForSplitOff.Add(clickPosition);
                    selectedArmy.actuallySplitOffOrNot.Add(true);

                    readyToSplitArmy = false;
                    splitArmyButton.interactable = false;
                    splitArmyReminder.gameObject.SetActive(false);
                    armiesGoingToSplit.Add(selectedArmy);
                    GameObject indicator = Instantiate(splittingIndicator, clickPosition, Quaternion.identity);
                    splitIndicatorList.Add(indicator);
                    selectedArmy.indicators.Add(indicator);


                    //create a button used to cancel the split later
                    int num = selectedArmy.strengthToSplitOff.Count - 1;
                    Button newButton = Instantiate(splitOffButtonTemplate);
                    var text = newButton.GetComponentInChildren<TMP_Text>(); 
                    text.text = "Units: " + selectedArmy.strengthToSplitOff[num] + "/ Destination" + selectedArmy.destinationForSplitOff[num].x + " " + selectedArmy.destinationForSplitOff[num].z;

                    newButton.transform.SetParent(uiSplitOffParent.transform);

                    RectTransform rect = newButton.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(700, 300);
                    selectedArmy.listOfSplitOffs.Add(newButton);
                    newButton.onClick.AddListener(delegate { CancelSplitOff(selectedArmy, num); });
                    newButton.gameObject.SetActive(false);
                    ShowSplitOffs();
                }
                else if (readyToCombineArmy)
                {
                    secondArmy = ChooseArmy();
                    if (secondArmy != null)
                    {
                        //Debug.LogError("Clicked on second army");
                        if (firstArmy.target != null)
                        {
                            firstArmy.target.position = clickPosition;
                            ABPath path = firstArmy.DrawPath();
                            int selectedArmySpeedMax = firstArmy.speedMax;
                            if (selectedArmySpeedMax > path.vectorPath.Count - 1)
                            {
                                selectedArmySpeedMax = path.vectorPath.Count - 1;
                            }
                            maximumRange.position = path.vectorPath[selectedArmySpeedMax];

                            firstArmy.awaitingCollisionWith = secondArmy;
                        }
                    }
                    else
                    {
                        firstArmy = null;
                        secondArmy = null;
                    }
                    readyToCombineArmy = false;
                    combineArmyButton.interactable = false;
                    combineArmyReminder.gameObject.SetActive(false);
                }
                else
                {
                    splitArmyButton.interactable = false;
                    combineArmyButton.interactable = false;
                    confirmStrengthButton.gameObject.SetActive(false);
                    increaseStrengthButton.gameObject.SetActive(false);
                    decreaseStrengthButton.gameObject.SetActive(false);
                    splitArmyReminder.gameObject.SetActive(false);
                    combineArmyReminder.gameObject.SetActive(false);
                    SelectArmy();
                }
            }
        }
    }
    private void ShowSplitOffs() //display and update splitoffs
    {
        int tally = 0;
        for (int i = 0; i < selectedArmy.listOfSplitOffs.Count; i++)
        {
            Button button = selectedArmy.listOfSplitOffs[i];
            if (selectedArmy.actuallySplitOffOrNot[i] == true)
            {
                button.gameObject.SetActive(true);
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(700, 300 - tally * 100);
                tally++;
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }
    private void CancelSplitOff(Army army, int num)
    {
        army.actuallySplitOffOrNot[num] = false;
        army.availableUnitsInArmy += army.strengthToSplitOff[num];
        Destroy(army.indicators[num]);
        ShowSplitOffs();
    }
    public void CombineArmy()
    {
        readyToCombineArmy = true;
        firstArmy = selectedArmy;
        secondArmy = null;
        combineArmyButton.interactable = false;
        splitArmyButton.interactable = false;
        combineArmyReminder.gameObject.SetActive(true);
    }
    public void SplitArmy()
    {
        if (selectedArmy.availableUnitsInArmy >= 2)
        {
            splitArmyButton.interactable = false;
            combineArmyButton.interactable = false;
            splitArmyReminder.gameObject.SetActive(true);

            increaseStrengthButton.gameObject.SetActive(true);
            decreaseStrengthButton.gameObject.SetActive(true);
            confirmStrengthButton.gameObject.SetActive(true);

            strengthToTransfer = Mathf.RoundToInt(selectedArmy.availableUnitsInArmy / 2);

            splitArmyReminder.text = "How many troops should be split off? " + strengthToTransfer;
        }
        else
        {
            Debug.LogError("Not enough units available");
        }
    }

    public void ConfirmStrengthAmount()
    {
        confirmStrengthButton.gameObject.SetActive(false);
        increaseStrengthButton.gameObject.SetActive(false);
        decreaseStrengthButton.gameObject.SetActive(false);

        int speed = 6;
        if (strengthToTransfer >= 24) //large
        {
            speed = 2;
        }
        else if (strengthToTransfer >= 12) //med
        {
            speed = 4;
        }
        else if (strengthToTransfer >= 6) //small
        {
            speed = 5;
        }
        else //scout
        {
            speed = 6;
        }
        selectedArmy.availableUnitsInArmy -= strengthToTransfer;
        //splitArmyReminder.gameObject.SetActive(false);
        splitArmyReminder.text = "Choose a location to send the new army to. Speed of " + speed;
        readyToSplitArmy = true;
    }

    public void IncreaseSplitStrength() //come back later and add checks for multiple splits?
    {
        if (strengthToTransfer < selectedArmy.availableUnitsInArmy - 1)
        {
            strengthToTransfer++;
        }
        splitArmyReminder.text = "How many troops should be split off? " + strengthToTransfer;
    }
    public void DecreaseSplitStrength()
    {
        if (strengthToTransfer > 1)
        {
            strengthToTransfer--;
        }
        splitArmyReminder.text = "How many troops should be split off? " + strengthToTransfer;
    }
    public Army SpawnArmy(Vector3 spawnPos)
    {
        GameObject newArmy = Instantiate(armyPrefab, Vector3.zero, Quaternion.identity); //instantiate army prefab
        Transform transform = newArmy.transform.GetChild(0); //get transform of figurine
        transform.position = spawnPos; //move figurine to click pos
        Transform targetTransform = newArmy.transform.GetChild(1);
        targetTransform.position = spawnPos;
        Transform aiTargetTransform = newArmy.transform.GetChild(2);
        aiTargetTransform.position = spawnPos;


        Army newArmyComp = newArmy.GetComponentInChildren<Army>(); //get army
        SingleNodeBlocker newArmyBlocker = newArmy.GetComponentInChildren<SingleNodeBlocker>();

        foreach (Army elArmy in armies) //other armies need to treat this as a blocker
        {
            newArmyComp.obstacles.Add(elArmy.GetComponentInChildren<SingleNodeBlocker>());
            elArmy.obstacles.Add(newArmyBlocker);
        }
        armies.Add(newArmyComp); //put army in list so that it can be selected


        readyToSpawnArmy = false;

        return newArmyComp;
    }
    private Army ChooseArmy() //don't select
    {
        bool foundOne = false;
        foreach (Army checkedArmy in armies)
        {
            Vector3 diff = checkedArmy.transform.position - clickPosition;
            diff.x = Mathf.Abs(diff.x);
            diff.z = Mathf.Abs(diff.z);

            if (diff.x < 0.5f && diff.z < 0.5f) //safe check to see if we clicked on army
            {
                foundOne = true;
                return checkedArmy;
            }
        }
        if (foundOne == false)
        {
        }
        return null;
    }
    private Army SelectArmy()
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
                combineArmyButton.interactable = true;
                if (selectedArmy.availableUnitsInArmy >= 2)
                {
                    splitArmyButton.interactable = true;
                }
                selectedArmy.CheckSizeAndChangeSpeed();

                selectionIndicator.transform.position = new Vector3(selectedArmy.transform.position.x, 0.01f, selectedArmy.transform.position.z);
                selectionIndicator.SetActive(true);

                ShowSplitOffs();
                return checkedArmy;
            }
        }
        if (foundOne == false)
        {
            selectedArmy = null;
            combineArmyButton.interactable = false;
            splitArmyButton.interactable = false;
            selectionIndicator.SetActive(false);
        }
        return null;
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
                    if (selectedArmySpeedMax > path.vectorPath.Count - 1)
                    {
                        selectedArmySpeedMax = path.vectorPath.Count - 1;
                    }
                    maximumRange.position = path.vectorPath[selectedArmySpeedMax];

                    if (selectedArmySpeedMax <= 0)
                    {
                        maximumRange.gameObject.SetActive(false);
                        selectedArmy.target.gameObject.SetActive(false);
                    }
                    else
                    {
                        maximumRange.gameObject.SetActive(true);
                        selectedArmy.target.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

}
