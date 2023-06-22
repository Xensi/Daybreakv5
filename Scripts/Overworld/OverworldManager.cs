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
    public GlobalDefines.Team currentTeam = GlobalDefines.Team.Altgard;
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
    [SerializeField] private GameObject taxParent;
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

    public Button fleeBattleButton;

    #region Refactored
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //Application.targetFrameRate = 60;
        /*
        combineArmyButton.interactable = false;
        splitArmyButton.interactable = false;*/
        GameObject[] supplyPoints;
        supplyPoints = GameObject.FindGameObjectsWithTag("SupplyGiver"); //find all supply points
        foreach (GameObject item in supplyPoints)
        {
            SupplyPoint comp = item.GetComponent<SupplyPoint>();
            supplyGivers.Add(comp);
        }
        ShowArmyInfoAndUpdateArmyBars();
    }
    public void PlayerBattleGroupEnteredSupplyPoint()
    {
        UpdateTownInfo();
        requestSuppliesButton.interactable = true;  //enable req supplies
        townOptionsParent.SetActive(true);
        if (playerBattleGroup.currentSupplyPoint != null) //update values
        {
            var point = playerBattleGroup.currentSupplyPoint;
            float availableProvisos = point.storedSupplies - point.amountOfProvisionsToReserve;
            if (availableProvisos < 0)
            {
                availableProvisos = 0;
                requestSuppliesButton.interactable = false;
            }
        }
        if (playerBattleGroup.currentSupplyPoint != null && playerBattleGroup.currentSupplyPoint.team != currentTeam) //if enemy-aligned point
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
                        playerBattleGroup.aiTarget.position = hit.point; //set movement destination
                        playerBattleGroup.reachedDestination = false;
                        BattleGroupManager.Instance.ForceUnpause();
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
        if (ui.hovering == false && allowInput && !readingDialogue)
        {
            LeftClickCheck();
            RightClickCheck();
        } 
    }
    public void DialogueOver()
    {
        readingDialogue = false;
        ShowArmyInfoAndUpdateArmyBars();
    }
    private void LeftClickCheck()
    {
        /*if (armiesMoving)
        {
            return;
        }*/
        if (Input.GetMouseButtonDown(0))
        {
            //RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        } 
    }
    public void HideArmyInfo()
    {
        armyOptionsParent.SetActive(false);
        townOptionsParent.SetActive(false);
        localeParent.SetActive(false);
    }
    public void HideAllArmyInfo()
    { 
        armyOptionsParent.SetActive(false);
        townOptionsParent.SetActive(false);
        localeParent.SetActive(false);
        mapParent.SetActive(false);
    }
    public GameObject mapParent;
    public void ShowArmyInfoAndUpdateArmyBars()
    {
        mapParent.gameObject.SetActive(true);
        armyOptionsParent.SetActive(true);
        moraleBarSlider.maxValue = playerBattleGroup.maxMorale;
        moraleBarSlider.value = playerBattleGroup.morale;
        supplyBarSlider.maxValue = playerBattleGroup.maxSupplies;
        supplyBarSlider.value = playerBattleGroup.supplies;
        spoilsBarSlider.value = playerBattleGroup.spoils;
        spoilsBarSlider.maxValue = playerBattleGroup.maxSpoils;

        moraleNumber.text = playerBattleGroup.morale + "/" + playerBattleGroup.maxMorale;
        supplyNumber.text = Mathf.RoundToInt(playerBattleGroup.supplies) + "/" + playerBattleGroup.maxSupplies;
        spoilsNumber.text = playerBattleGroup.spoils + "/" + playerBattleGroup.maxSpoils;
        if (playerBattleGroup.currentSupplyPoint != null)
        {
            UpdateTownInfo();
        }
        else if (playerBattleGroup.currentLocale != null)
        {
            UpdateLocaleInfo();
        }
        else
        {
            townOptionsParent.SetActive(false);
            localeParent.SetActive(false);
        }
    }
    private void UpdateTownInfo()
    {
        var point = playerBattleGroup.currentSupplyPoint;
        townOptionsParent.SetActive(true);
        supplyName.text = point.supplyName;
        float availableProvisos = point.storedSupplies - point.amountOfProvisionsToReserve;
        if (availableProvisos < 0)
        {
            availableProvisos = 0;
        }
        float actualReservedProv = 0;
        if (point.storedSupplies >= point.amountOfProvisionsToReserve)
        {
            actualReservedProv = point.amountOfProvisionsToReserve;
        }
        else
        {
            actualReservedProv = point.storedSupplies;
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

        supplyFaction.text = "Faction: " + point.team;
        supplyPopulation.text = "Population: " + point.population;
        supplyRelations.text = "Relations: " + point.relations;

        if (point.isFort && point.team == playerBattleGroup.team)
        {
            taxText.text = "Resupply";
        }
        else
        {
            taxText.text = "Tax";
        }
        if (point.team == playerBattleGroup.team) //no extorting or pillaging things that belong to you
        {
            extortParent.SetActive(false);
            pillageParent.SetActive(false);
            talkParent.SetActive(true);
            taxParent.SetActive(true);
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
        else if (point.team == GlobalDefines.Team.Maukland) //neutrals, can be extorted
        {
            taxParent.SetActive(true);
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
        else if (point.team == GlobalDefines.Team.Zhanguo)  //an enemy! harshest option available only. no mercy :)
        {
            reinforcementParent.SetActive(false);
            talkParent.SetActive(false);
            taxParent.SetActive(false);
            extortParent.SetActive(false);
            pillageParent.SetActive(true); 
        }
    }
    #endregion

    #region Unrefactored 
    public void ToggleArmyCards() //triggered by armycomp button
    {
        if (armyCardsParent.activeSelf)
        {
            armyCardsParent.SetActive(false);
            foreach (ArmyCard card in playerBattleGroup.armyDisplayCards)
            {
                card.gameObject.SetActive(false);
            }
        }
        else
        {
            playerBattleGroup.GenerateArmy();
            armyCardsParent.SetActive(true);
            foreach (ArmyCard card in playerBattleGroup.armyDisplayCards)
            {
                card.gameObject.SetActive(true);
            }
        }
        /*codeNameText.text = "Force: " + playerBattleGroup.befestigungName;
        headedByText.text = "Headed by: " + playerBattleGroup.oberkommandantName;
        sizeText.text = "Size: " + playerBattleGroup.size;
        visionRangeText.text = "Vision range: " + playerBattleGroup.sightRadius;
        upkeepText.text = "Upkeep: " + playerBattleGroup.supplyUpkeep;
        speedText.text = "Speed: " + playerBattleGroup.speedMax;*/
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
    public void RequestSupplies()
    {
        SupplyPoint giver = playerBattleGroup.currentSupplyPoint;
        if (giver != null)
        {//give army as many supplies as they can carry and that the town is willing to spare
            while (playerBattleGroup.supplies < playerBattleGroup.maxSupplies && giver.storedSupplies > 0 && giver.storedSupplies > giver.amountOfProvisionsToReserve)
            {
                playerBattleGroup.supplies++;
                giver.storedSupplies--;
            }
            while (playerBattleGroup.spoils < playerBattleGroup.maxSpoils && giver.storedSpoils > 0 && giver.storedSpoils > giver.reservedSpoils)
            {
                playerBattleGroup.spoils++;
                giver.storedSpoils--;
            }
            //UpdateTownInfo();
            ShowArmyInfoAndUpdateArmyBars();
        }
    } 
    public void ExtortSupplies()
    {
        SupplyPoint giver = playerBattleGroup.currentSupplyPoint;
        if (giver != null)
        {//request
            while (playerBattleGroup.supplies < playerBattleGroup.maxSupplies && giver.storedSupplies > 0 && giver.storedSupplies > giver.amountOfProvisionsToReserve)
            {
                playerBattleGroup.supplies += 0.0001f;
                giver.storedSupplies -= 0.0001f;
            }
            //give army as many supplies as they can carry and that the town is willing to spare (extortion). for each supply given from reserves, generate 1 anger
            while (playerBattleGroup.supplies < playerBattleGroup.maxSupplies && giver.storedSupplies > 0 && giver.storedSupplies > giver.extortionReservedProvisions)
            {
                playerBattleGroup.supplies += 0.0001f;
                giver.storedSupplies -= 0.0001f;
                giver.mood -= 0.0001f; //anger is negative mood
            }
            giver.UpdateRelations();

            UpdateTownInfo();
            ShowArmyInfoAndUpdateArmyBars();
        }
    } 
    public void PillageSupplies()
    {
        SupplyPoint giver = playerBattleGroup.currentSupplyPoint;
        if (giver != null)
        {//request
            while (playerBattleGroup.supplies < playerBattleGroup.maxSupplies && giver.storedSupplies > 0 && giver.storedSupplies > giver.amountOfProvisionsToReserve)
            {
                playerBattleGroup.supplies++;
                giver.storedSupplies--;
            }
            //give army as many supplies as they can carry
            while (playerBattleGroup.supplies < playerBattleGroup.maxSupplies && giver.storedSupplies > 0)
            {
                playerBattleGroup.supplies++;
                giver.storedSupplies--;
                giver.mood -= 2; //generate double the anger for each supply pillaged
                giver.population -= UnityEngine.Random.Range(1, 40);
                if (giver.population < 0)
                {
                    giver.population = 0;
                }
            }
            giver.UpdateRelations();
            /*if (giver.team == GlobalDefines.Team.Zhanguo)
            {
                giver.team = GlobalDefines.Team.Altgard;
            }*/
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

    public void PlayerBattleGroupEnteredLocale()
    {
        UpdateLocaleInfo();
    }
    public void PlayerBattleGroupExitedLocale()
    {
        localeParent.SetActive(false);
    }
    private void UpdateLocaleInfo()
    {
        var locale = playerBattleGroup.currentLocale;
        if (locale.investigatable)
        {
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
        else
        {
            localeParent.SetActive(false);
        }
        
    } 
    public void ExploreLocale()
    {
        dialogueEvent = false; 
        DialogueScriptableObject dialogue = playerBattleGroup.currentLocale.localeInvestigationDialogue;
        playerBattleGroup.currentLocale.investigated = true;
        DialogueManager.Instance.loadedDialogue = dialogue;
        DialogueManager.Instance.StartDialogue();
        HideAllArmyInfo();
    }
    public bool readingDialogue = false;
    public void TalkToSupplyGiver()
    {
        readingDialogue = true;
        BattleGroupManager.Instance.ForcePause();
        dialogueEvent = false;

        SupplyPoint supply = playerBattleGroup.currentSupplyPoint;
        if (supply == null)
        {
            Debug.LogError("Supply is null");
            return;
        }
        DialogueScriptableObject dialogue = supply.talkToVillageDialogue;
        if (dialogue == null)
        {
            Debug.LogError("Dialogue is null");
            return;
        }
        if (supply.talkDescriptionRead && playerBattleGroup.currentSupplyPoint.afterReadTalkToDialogue != null)
        {
            dialogue = playerBattleGroup.currentSupplyPoint.afterReadTalkToDialogue;
        }
        supply.talkDescriptionRead = true;
        DialogueManager.Instance.loadedDialogue = dialogue;
        DialogueManager.Instance.StartDialogue();
        HideAllArmyInfo(); 
    } 
    #endregion 
}
