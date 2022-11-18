using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using TMPro;

public class OverworldManager : MonoBehaviour
{
    public BattleGroup playerBattleGroup;
    public BattleGroup enemyBattleGroup; //that is fighting is currently
    public static OverworldManager Instance { get; private set; }
    [Header("Add armies/roaming events to these lists to enable their movement.")]
    public List<Army> enemyArmies;
    public List<Army> armies;

    [SerializeField] private List<RoamingEvent> roamingEvents;

    public List<Caravan> caravans;
    public List<SupplyPoint> supplyGivers;
    public Army soleArmy; //temporary way to get the army
    public List<Army> armiesGoingToSplit;
    public List<GameObject> splitIndicatorList;
    public Vector3 clickPosition;
    public Army selectedArmy;
    public Transform clickPosIndicator;
    public Transform maximumRange;
    public GameObject maximumRangeVisual;
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

    public Army localeArmy;
    public string currentFaction = "Altgard";
    public Transform armyCompBoxParent;
    public Transform leftAnchor;

    public bool armiesMoving = false;
    public bool dialogueEvent = false; //if false, then talking or exploring

    private bool allowInput = true;

    [SerializeField] private SupplyPoint selectedSupplyGiver;
    [SerializeField] private Button requestSuppliesButton;
    [SerializeField] private Button extortSuppliesButton;
    [SerializeField] private Button pillageSuppliesButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button reinforcementButton; 
    [SerializeField] private GameObject extortParent;
    [SerializeField] private GameObject pillageParent;
    [SerializeField] private GameObject talkParent;
    [SerializeField] private GameObject reinforcementParent; 
    [SerializeField] private Slider moraleBarSlider; 
    [SerializeField] private TMP_Text moraleNumber; 
    [SerializeField] private Slider supplyBarSlider; 
    [SerializeField] private TMP_Text supplyNumber; 
    [SerializeField] private Slider spoilsBarSlider; 
    [SerializeField] private TMP_Text spoilsNumber; 
    [SerializeField] private GameObject armyCardsParent; 
    [SerializeField] private GameObject armyOptionsParent;
    [SerializeField] private GameObject townOptionsParent; 
    [SerializeField] private TMP_Text supplyName; 
    [SerializeField] private TMP_Text supplySupplyText;
    [SerializeField] private TMP_Text supplySpoilsText;
    [SerializeField] private TMP_Text supplyFaction;
    [SerializeField] private TMP_Text taxText; 
    [SerializeField] private LineRenderer navIndicator;
    [SerializeField] private TMP_Text consumptionText;
    [SerializeField] private TMP_Text consumptionReasonText; 
    [SerializeField] private TMP_Text supplyRelations;
    [SerializeField] private TMP_Text supplyPopulation;
    //army card view
    [SerializeField] private TMP_Text codeNameText;
    [SerializeField] private TMP_Text headedByText;
    [SerializeField] private TMP_Text sizeText;
    [SerializeField] private TMP_Text visionRangeText;
    [SerializeField] private TMP_Text upkeepText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private GameObject localeParent;
    [SerializeField] private GameObject closedEyeParent;
    [SerializeField] private Button localeExploreButton;

    public Army armyThatWeAreFighting;
    public GameObject sutlerParent;
    #region Refactored
    private void Awake()
    {
        Instance = this;
    }
    public void PlayerBattleGroupEnteredSupplyPoint()
    {
        requestSuppliesButton.interactable = true;  //enable req supplies
        townOptionsParent.SetActive(true);
        if (playerBattleGroup.currentSupplyPoint != null) //update values
        {
            var point = playerBattleGroup.currentSupplyPoint;
            int availableProvisos = point.storedProvisions - point.reservedProvisions;
            if (availableProvisos < 0)
            {
                availableProvisos = 0;
                requestSuppliesButton.interactable = false;
            }
        }
        if (playerBattleGroup.currentSupplyPoint != null && playerBattleGroup.currentSupplyPoint.faction != currentFaction) //if enemy-aligned point
        {
            extortSuppliesButton.interactable = true;
            pillageSuppliesButton.interactable = true;
        }
        else //allied point
        {
            extortSuppliesButton.interactable = false;
            pillageSuppliesButton.interactable = false;
        }
    }

    public void PlayerBattleGroupExitedSupplyPoint()
    {
        townOptionsParent.SetActive(false);
        requestSuppliesButton.interactable = false;
        extortSuppliesButton.interactable = false;
        pillageSuppliesButton.interactable = false;
    }
    private void RightClickCheck()
    {
        /*if (armiesMoving)
        {
            return;
        }*/
        /*if (DialogueManager.Instance.readingDialogue)
        {
            return;
        }*/
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            LayerMask layerMask = LayerMask.GetMask("OverworldTerrain");
            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
            {
                if (playerBattleGroup != null)
                {
                    if (playerBattleGroup.aiTarget != null)
                    {
                        playerBattleGroup.aiTarget.position = hit.point;
                    }
                }
                /*RaycastHit hit;
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
                        //selectedArmy.aiTarget.transform.position = clickPosition;
                        ABPath path = selectedArmy.DrawPath();
                        UpdateNavigationIndicator(path);
                        //Debug.Log("Path length" + path.vectorPath.Count-1);
                        UpdateConsumptionText(path);
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
                }*/
            }
        }
    }

    private void Update()
    {
        if (ui.hovering == false && allowInput)
        {
            LeftClickCheck();
            RightClickCheck();
        }
        if (armiesMoving) //detect if armies are still moving. relic of the turn based system
        {
            int numArmiesMoving = 0;
            foreach (Army army in armies)
            {
                if (army.moving)
                {
                    numArmiesMoving++;
                }
            }
            foreach (Army army in enemyArmies)
            {
                if (army.moving)
                {
                    numArmiesMoving++;
                }
            }
            if (numArmiesMoving == 0)
            {
                armiesMoving = false;
                foreach (Army army in armies)
                {
                    army.TriggerEventsAtLocales();
                }
            }
        }
    }
    private void LeftClickCheck()
    {
        /*if (armiesMoving)
        {
            return;
        }*/
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            /*if (Physics.Raycast(ray, out hit))
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
            }*/
        }
    }

    #endregion

    #region Unrefactored
    private void Start()
    {
        //executeButton.interactable = false;
        combineArmyButton.interactable = false;
        splitArmyButton.interactable = false;


        GameObject[] array;
        array = GameObject.FindGameObjectsWithTag("SupplyGiver");
        foreach (GameObject item in array)
        {
            SupplyPoint comp = item.GetComponent<SupplyPoint>();
            supplyGivers.Add(comp);
        }
    }

    public void ToggleArmyCards() //triggered by armycomp button
    {
        if (armyCardsParent.activeSelf)
        {
            armyCardsParent.SetActive(false);
            foreach (ArmyCard card in selectedArmy.cards)
            {
                card.gameObject.SetActive(false);
            }
        }
        else
        {
            //localeParent.SetActive(false);
            armyCardsParent.SetActive(true);
            foreach (ArmyCard card in selectedArmy.cards)
            {
                card.gameObject.SetActive(true);
            }
            codeNameText.text = "Force: " + selectedArmy.befestigungName;
            headedByText.text = "Headed by: " + selectedArmy.oberkommandantName;
            sizeText.text = "Size: " + selectedArmy.size;
            visionRangeText.text = "Vision range: " + selectedArmy.sightRadius;
            upkeepText.text = "Upkeep: " + selectedArmy.supplyUpkeep;
            speedText.text = "Speed: " + selectedArmy.speedMax;
        }
    }
    public void ReadyToSpawnArmy() //triggered by spawn army button
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
    public void ExecuteMovesForAll()
    {
        //SPLIT code
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
        //end split code

        armiesMoving = true;
        foreach (Army elArmy in armies)
        {
            elArmy.StartMoving();
        }
        foreach (Army enemyArmy in enemyArmies)
        {
            enemyArmy.StartMoving();
        }
        foreach (RoamingEvent roamer in roamingEvents)
        {
            roamer.StartMoving();
        }
        townOptionsParent.SetActive(false);
        armyOptionsParent.SetActive(false);
        localeParent.SetActive(false);
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

    public void RequestSupplies()
    {
        SupplyPoint giver = playerBattleGroup.currentSupplyPoint;
        if (giver != null)
        {//give army as many supplies as they can carry and that the town is willing to spare
            while (playerBattleGroup.provisions < playerBattleGroup.maxProvisions && giver.storedProvisions > 0 && giver.storedProvisions > giver.reservedProvisions)
            {
                playerBattleGroup.provisions++;
                giver.storedProvisions--;
            }
            while (playerBattleGroup.spoils < playerBattleGroup.maxSpoils && giver.storedSpoils > 0 && giver.storedSpoils > giver.reservedSpoils)
            {
                playerBattleGroup.spoils++;
                giver.storedSpoils--;
            }
            UpdateTownInfo();
            ShowArmyInfoAndUpdateArmyBars();

        }

    }
    public void ExtortSupplies()
    {
        SupplyPoint giver = playerBattleGroup.currentSupplyPoint;
        if (giver != null)
        {//request
            while (playerBattleGroup.provisions < playerBattleGroup.maxProvisions && giver.storedProvisions > 0 && giver.storedProvisions > giver.reservedProvisions)
            {
                playerBattleGroup.provisions++;
                giver.storedProvisions--;
            }
            //give army as many supplies as they can carry and that the town is willing to spare (extortion). for each supply given from reserves, generate 1 anger
            while (playerBattleGroup.provisions < playerBattleGroup.maxProvisions && giver.storedProvisions > 0 && giver.storedProvisions > giver.extortionReservedProvisions)
            {
                playerBattleGroup.provisions++;
                giver.storedProvisions--;
                giver.mood--; //anger is negative mood
            }
            giver.UpdateRelations();

            UpdateTownInfo();
            ShowArmyInfoAndUpdateArmyBars();
        }
    }

    public void PillageSupplies()
    {
        SupplyPoint giver = selectedArmy.currentSupplyPoint;
        if (giver != null)
        {//request
            while (selectedArmy.provisions < selectedArmy.maxProvisions && giver.storedProvisions > 0 && giver.storedProvisions > giver.reservedProvisions)
            {
                selectedArmy.provisions++;
                giver.storedProvisions--;
            }
            //give army as many supplies as they can carry
            while (selectedArmy.provisions < selectedArmy.maxProvisions && giver.storedProvisions > 0)
            {
                selectedArmy.provisions++;
                giver.storedProvisions--;
                giver.mood -= 2; //generate double the anger for each supply pillaged
                giver.population -= UnityEngine.Random.Range(1, 40);
                if (giver.population < 0)
                {
                    giver.population = 0;
                }
            }
            giver.UpdateRelations();
            UpdateTownInfo();
            ShowArmyInfoAndUpdateArmyBars();
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
    private SupplyPoint SelectSupplyGiver()
    {

        return null;
    }

    private Army SelectArmy()
    {
        if (DialogueManager.Instance.readingDialogue)
        {
            return null;
        }

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

                selectedArmy.targetVisual.SetActive(true);
                navIndicator.gameObject.SetActive(true);

                maximumRangeVisual.SetActive(true);

                selectionIndicator.transform.position = new Vector3(selectedArmy.transform.position.x, 0.01f, selectedArmy.transform.position.z);
                selectionIndicator.SetActive(true);

                ShowSplitOffs();
                ShowArmyInfoAndUpdateArmyBars();

                ABPath path = selectedArmy.DrawPath();
                UpdateNavigationIndicator(path);
                UpdateConsumptionText(path);

                return checkedArmy;
            }
        }
        if (foundOne == false)
        {
            DeselectArmy();
        }
        return null;
    }

    private void UpdateNavigationIndicator(ABPath path)
    {
        navIndicator.positionCount = path.vectorPath.Count;
        for (int i = 0; i < path.vectorPath.Count; i++)
        {

            navIndicator.SetPosition(i, path.vectorPath[i]);
        }
    }

    public void HideNavIndicator()
    {
        navIndicator.gameObject.SetActive(false);
    }

    private void ShowArmyInfoAndUpdateArmyBars()
    {
        armyOptionsParent.SetActive(true);
        moraleBarSlider.maxValue = selectedArmy.maxMorale;
        moraleBarSlider.value = selectedArmy.overallMorale;
        supplyBarSlider.maxValue = selectedArmy.maxProvisions;
        supplyBarSlider.value = selectedArmy.provisions;
        spoilsBarSlider.value = selectedArmy.spoils;
        spoilsBarSlider.maxValue = selectedArmy.maxSpoils;


        moraleNumber.text = selectedArmy.overallMorale + "/" + selectedArmy.maxMorale;
        supplyNumber.text = selectedArmy.provisions + "/" + selectedArmy.maxProvisions;
        spoilsNumber.text = selectedArmy.spoils + "/" + selectedArmy.maxSpoils;
        if (selectedArmy.currentSupplyPoint != null)
        {
            UpdateTownInfo();
        }
        else if (selectedArmy.currentLocale != null)
        {
            UpdateLocaleInfo();
        }
        else
        {
            townOptionsParent.SetActive(false);
        }


    }
    private void UpdateTownInfo()
    {
        var point = selectedArmy.currentSupplyPoint;
        townOptionsParent.SetActive(true);
        supplyName.text = point.supplyName;
        int availableProvisos = point.storedProvisions - point.reservedProvisions;
        if (availableProvisos < 0)
        {
            availableProvisos = 0;
        }
        int actualReservedProv = 0;
        if (point.storedProvisions >= point.reservedProvisions)
        {
            actualReservedProv = point.reservedProvisions;
        }
        else
        {
            actualReservedProv = point.storedProvisions;
        }
        int availableSpoils = point.storedSpoils - point.reservedSpoils;
        if (availableSpoils < 0)
        {
            availableSpoils = 0;
        }
        int actualReservedSpoils = 0;
        if (point.storedSpoils >= point.reservedSpoils)
        {
            actualReservedSpoils = point.reservedSpoils;
        }
        else
        {
            actualReservedSpoils = point.storedSpoils;
        }
        supplySupplyText.text = "Available: " + availableProvisos + "\nReserved: " + actualReservedProv;
        supplySpoilsText.text = "Available: " + availableSpoils + "\nReserved: " + actualReservedSpoils;

        supplyFaction.text = "Faction: " + point.faction;
        supplyPopulation.text = "Population: " + point.population;
        supplyRelations.text = "Relations: " + point.relations;

        if (point.isFort && point.faction == selectedArmy.faction)
        {
            taxText.text = "Resupply";
        }
        else
        {
            taxText.text = "Tax";
        }
        if (point.faction == selectedArmy.faction) //no extorting or pillaging things that belong to you
        {
            extortParent.SetActive(false);
            pillageParent.SetActive(false);
            talkParent.SetActive(true);
            reinforcementParent.SetActive(true);

            if (point.routeClear)
            {
                reinforcementButton.interactable = true;
            }
            else
            {
                reinforcementButton.interactable = false;
            }
        }
        else
        {
            reinforcementParent.SetActive(false);
            if (point.extortable)
            {
                extortParent.SetActive(true);
            }
            else
            {
                extortParent.SetActive(false);
            }
            if (point.pillageable)
            {
                pillageParent.SetActive(true);
            }
            else
            {
                pillageParent.SetActive(false);
            }
            if (!point.pillageable && !point.extortable)
            {
                talkParent.SetActive(true);
            }
            else
            {
                talkParent.SetActive(false);

            }
        }
    }
    private void UpdateLocaleInfo()
    {
        var locale = selectedArmy.currentLocale;
        if (locale.investigated)
        {
            localeExploreButton.interactable = false;
            closedEyeParent.SetActive(true);
        }
        else
        {
            localeExploreButton.interactable = true;
            closedEyeParent.SetActive(false);
        }
        localeParent.SetActive(true);
    }

    public void ExploreLocale()
    {
        dialogueEvent = false;
        localeArmy = selectedArmy;
        DialogueScriptableObject dialogue = selectedArmy.currentLocale.localeInvestigationDialogue;
        selectedArmy.currentLocale.investigated = true;
        DialogueManager.Instance.loadedDialogue = dialogue;
        DialogueManager.Instance.StartDialogue();
        localeParent.SetActive(false);
        armyOptionsParent.SetActive(false);
    }
    public void TalkToSupplyGiver()
    {
        dialogueEvent = false;

        SupplyPoint supply = playerBattleGroup.currentSupplyPoint;
        if (supply == null)
        {
            Debug.Log("2");
            return;
        }
        DialogueScriptableObject dialogue = supply.talkToDialogue;
        if (dialogue == null)
        {
            Debug.Log("3");
            return;
        }
        if (supply.talkDescriptionRead && playerBattleGroup.currentSupplyPoint.afterReadTalkToDialogue != null)
        {
            dialogue = playerBattleGroup.currentSupplyPoint.afterReadTalkToDialogue;
        }
        supply.talkDescriptionRead = true;
        DialogueManager.Instance.loadedDialogue = dialogue;
        DialogueManager.Instance.StartDialogue();
        townOptionsParent.SetActive(false);
        armyOptionsParent.SetActive(false);
    }
    public void DeselectArmy()
    {
        if (selectedArmy != null)
        {
            selectedArmy.targetVisual.SetActive(false);
        }
        selectedArmy = null;
        navIndicator.gameObject.SetActive(false);
        maximumRangeVisual.SetActive(false);

        combineArmyButton.interactable = false;
        splitArmyButton.interactable = false;
        selectionIndicator.SetActive(false);
        armyOptionsParent.SetActive(false);
        townOptionsParent.SetActive(false);
        localeParent.SetActive(false);

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
    
    private void UpdateConsumptionText(ABPath path)
    {
        selectedArmy.predictedMovementSpaces = path.vectorPath.Count - 1;

        int provisionsConsumedThisMovement = 0;
        if (selectedArmy.predictedMovementSpaces >= 4) //forced march threshold
        {
            provisionsConsumedThisMovement += selectedArmy.supplyUpkeep;
        }
        if (selectedArmy.turnCounter == 1)
        {
            provisionsConsumedThisMovement += selectedArmy.supplyUpkeep;
        }
        consumptionText.text = "Consumed this turn: " + provisionsConsumedThisMovement;
        consumptionReasonText.text = "";
        if (selectedArmy.turnCounter == 1)
        {
            consumptionReasonText.text += "Upkeep\n";
        }
        if (selectedArmy.predictedMovementSpaces >= 4)
        {
            consumptionReasonText.text += "Forced march!";
        }
        selectedArmy.predictedSupplyConsumption = provisionsConsumedThisMovement;
    }
    #endregion



}
