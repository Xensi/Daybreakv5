using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Pathfinding;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance { get; private set; }

    public Transform virtualCamTransform;

    [SerializeField] private GameObject destPrefab;
    private Vector3 clickPosition;

    public List<FormationPosition> allFormationsList;
    public FormationPosition[] allArray;

    public List<FormationPosition> playerControlledFormations; 
    public List<FormationPosition> enemyControlledFormations;

    public List<FormationPosition> selectedFormations;
    public GlobalDefines.Team team = GlobalDefines.Team.Altgard; 


    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private bool started = false;

    public Camera cam;

    [SerializeField] private Transform rotationTarget;
    [SerializeField] private Vector3 heldPosition;
    //[SerializeField] private LineRenderer

    [SerializeField] private GameObject battleUI;

    [SerializeField] private GameObject rangedUI;
    [SerializeField] private GameObject meleeUI;
    [SerializeField] private GameObject magicUI;
    [SerializeField] private GameObject braceUI;
    [SerializeField] private GameObject chaffBombUI;
    [SerializeField] private Button setChargeButton;
    [SerializeField] private Button setBraceButton;
    [SerializeField] private Button setUnbraceButton;
    [SerializeField] private Button setHoldButton;
    [SerializeField] private Button setFireButton;
    [SerializeField] private Button setFreeFireButton;
    [SerializeField] private Button setMarchButton;
    [SerializeField] private Button setHaltButton;

    [SerializeField] private Button setCeaseButton;
    [SerializeField] private Button setAllowFightButton;

    [SerializeField] private Button setHoldPositionButton;
    [SerializeField] private Button setPursueButton;
    [SerializeField] private Button mageAbility1;
    [SerializeField] private Button mageAbility2; 

    [SerializeField] private TMP_Text mageHeader;

    public bool hoveringUI = false;

    [SerializeField] private GameObject forceFireTarget;
    [SerializeField] private GameObject forceFireTargetPrefab;
    [SerializeField] private bool forceFiring = false;
    [SerializeField] private bool wasFocusFiring = false;
    [SerializeField] private List<GameObject> targetList = new List<GameObject>();
    [SerializeField] private FormationPosition formationToFocusFire;
    [SerializeField] private List<Vector3> lineFormationPosList = new List<Vector3>();
    [SerializeField] private float lineOffset = 15;
    [SerializeField] private List<GameObject> placementMarkers = new List<GameObject>();
    [SerializeField] private bool magicTargeting = false;
    [SerializeField] private bool wasMagicTargeting = false;
    [SerializeField] private int abilityNumber = 0;
    [SerializeField] private bool drawingLine = false;

    [SerializeField] private Slider lodSlider;

    [SerializeField] private List<Vector3> destinations;

    [SerializeField] private bool testing = false;


    public bool placingSoldiers = true;

    [SerializeField] private Collider friendlyPlacementZone;
    [SerializeField] private Collider enemyPlacementZone;

    private void Awake()
    {
        Instance = this;
    }

    private enum combatStrategy
    {
        Attack,
        Defend
    } 
    private combatStrategy aiState = combatStrategy.Attack;
      
    public void ModifyLOD()
    {
        QualitySettings.lodBias = lodSlider.value;
    }
    private void Start()
    {

        Instance = this;
        if (testing)
        {
            UpdateAllFormArrayAndStartAIToBeginBattle();
        }
        finishedPlacingButton.interactable = false;
        
    }
    #region SoldierPlacements 
    public void StartPlacingSoldiers()
    {
        GenerateSoldierButtons();
        RandomPlacePlayerFormations();

        PlaceEnemySoldiers();
        placerUI.SetActive(true);
        if (friendlyPlacementZone != null)
        {
            friendlyPlacementZone.gameObject.SetActive(true);
        }
        placingSoldiers = true;
    }
    private void RandomPlacePlayerFormations()
    {
        foreach (UnitInfoClass unit in OverworldManager.Instance.playerBattleGroup.listOfUnitsInThisArmy)
        {
            Vector3 randomPoint = Helper.Instance.RandomPointInBounds(friendlyPlacementZone.bounds);
            Vector3 vec = new Vector3(randomPoint.x, 100, randomPoint.z);
            LayerMask layerMask = LayerMask.GetMask("Terrain");
            RaycastHit hit;
            if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                randomPoint.y = hit.point.y;
            }
            PlaceFormationAtPositionOfType(unit.type, unit.troops, randomPoint, GlobalDefines.Team.Altgard);
        }
    }
    private void GenerateSoldierButtons()
    {
        PurgeSoldierButtons();
        int i = 0;
        foreach (UnitInfoClass unit in OverworldManager.Instance.playerBattleGroup.listOfUnitsInThisArmy)
        {
            Button soldierButton = Instantiate(soldierButtonPrefab, Vector3.zero, Quaternion.identity, placerUI.transform);
            TMP_Text text = soldierButton.GetComponentInChildren<TMP_Text>();
            text.text = unit.type.ToString() + ": " + unit.troops.ToString();
            soldierButton.transform.localPosition = new Vector3(-890, 460 - 45 * i, 0);
            soldierButton.onClick.AddListener(() => ChooseSoldierToPlace(unit.type.ToString()));
            int tempID = i;
            soldierButton.onClick.AddListener(() => UpdateButtonID(tempID));
            soldierButton.onClick.AddListener(() => UpdateSoldierCount(unit.troops));
            soldierButtonsList.Add(soldierButton);
            soldierButton.interactable = false;
            i++;
        }
    }
    public void FindPlacementAreasAndUpdate()
    { 
        friendlyPlacementZone = GameObject.FindWithTag("FriendlyPlacementZone").GetComponent<Collider>();
        enemyPlacementZone = GameObject.FindWithTag("EnemyPlacementZone").GetComponent<Collider>();
    }
    public void LoadScenario()
    {
        FindPlacementAreasAndUpdate();
        UpdateGUI();
    }
    private void UpdateSoldierCount(int num)
    {
        soldiersToCreateNum = num;
    }
    private void PurgeSoldierButtons()
    {
        foreach (Button button in soldierButtonsList)
        {
            Destroy(button.gameObject);
        }
        soldierButtonsList.Clear();
    }
    [SerializeField] private Button soldierButtonPrefab;

    [SerializeField] private List<Button> soldierButtonsList;
    [SerializeField] private GameObject placerUI;
    private void StopPlacingSoldiers()
    {
        placerUI.SetActive(false);

        if (friendlyPlacementZone != null)
        {
            friendlyPlacementZone.gameObject.SetActive(false);
        }
        placingSoldiers = false;
        formationPlacerIndicator.SetActive(false);
    }
    private GlobalDefines.SoldierTypes soldierToPlace = GlobalDefines.SoldierTypes.none;
    public void ChooseSoldierToPlace(string soldierType)
    {
        System.Enum.TryParse(soldierType, true, out GlobalDefines.SoldierTypes convertedToEnum); //convert to enum
        soldierToPlace = convertedToEnum;
        /*if (formationPlacerIndicator != null)
        {
            formationPlacerIndicator.SetActive(true);
        }*/
    }
    [SerializeField] private int soldierButtonID = 0;
    public void UpdateButtonID(int id)
    {
        //Debug.Log(id);
        soldierButtonID = id;
    }
    [SerializeField] private GameObject formationPlacerIndicator;
    [SerializeField] private FormationPosition formPosToReposition;
    public void ConfirmFinishedPlacingSoldiers()
    {
        StopPlacingSoldiers();
        UpdateGUI();
        HidePlacementZones();
        UpdateAllFormArrayAndStartAIToBeginBattle();
    }
    private void HidePlacementZones()
    {
        enemyPlacementZone.gameObject.SetActive(false); 
        friendlyPlacementZone.gameObject.SetActive(false);
    }
    private void PlaceEnemySoldiers() //places soldiers randomly within the placement zone and gives them to zhanguo
    {
        foreach (UnitInfoClass unit in UnitManager.Instance.unitsInEnemyArmyList)
        {
            Vector3 randomPoint = Helper.Instance.RandomPointInBounds(enemyPlacementZone.bounds);
            Vector3 vec = new Vector3(randomPoint.x, 100, randomPoint.z);
            LayerMask layerMask = LayerMask.GetMask("Terrain");
            RaycastHit hit;
            if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                randomPoint.y = hit.point.y;
            }
            PlaceFormationAtPositionOfType(unit.type, unit.troops, randomPoint, GlobalDefines.Team.Zhanguo);
        }
    } 
    private bool CheckIfAllAvailableSoldiersPlaced()
    {
        foreach (Button item in soldierButtonsList)
        {
            if (item.interactable)
            {
                return false;
            }
        }
        return true;
    } 
    [SerializeField] private Button finishedPlacingButton;
    #endregion 
    public void UpdateAllFormArrayAndStartAIToBeginBattle()
    { 
        allArray = new FormationPosition[30];

        battleUI.SetActive(false);
        InvokeRepeating("AIBrain", 0f, 1f);
        InvokeRepeating("AIBrainMage", 5f, 5f); //don't do immediately, not urgent

        allFormationsList.Clear();
        FormationPosition[] array = FindObjectsOfType<FormationPosition>();
        allArray = array;
        int id = 0;
        playerControlledFormations.Clear();
        enemyControlledFormations.Clear();
        foreach (FormationPosition item in array)
        {
            allFormationsList.Add(item);
            if (item.team == team)
            {
                playerControlledFormations.Add(item);
            }
            else
            {
                enemyControlledFormations.Add(item);
                item.AIControlled = true;
            }
            item.FixPositions();
            item.BeginUpdates();
            if (item.shaker != null)
            {
                item.shaker.id = id;
            }
            //item.ClearOrders(); //resets targets
            id++;
        }

        MusicManager.Instance.PlayCombatMusicBasedOnBattleSize();
        InvokeRepeating("GameOverCheck", 2, 2);
    } 
    private void AIBrain()
    {
        switch (aiState)
        {
            case combatStrategy.Attack:
                AIRaisePursueRadius();
                AITryToCharge();
                AICheckIfBraceNeeded();
                break;
            case combatStrategy.Defend:
                AISetDefaultPursueRadius();
                break;
            default:
                break;
        }
    }
    private void AIBrainMage()
    {
        switch (aiState)
        {
            case combatStrategy.Attack: 
                AIAllMagesPickTargetsAndFire();
                break;
            case combatStrategy.Defend:
                AIAllMagesPickTargetsAndFire();
                break;
            default:
                break;
        }
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void AICheckIfBraceNeeded()
    {
        foreach (FormationPosition formPos in enemyControlledFormations)
        {
            if (formPos.soldierBlock.melee && formPos.usesSpears)
            {
                formPos.AICheckIfNeedToBrace();
            } 
        }
    }
    private void AITryToCharge()
    {
        foreach (FormationPosition formPos in enemyControlledFormations)
        {
            if (formPos.soldierBlock.melee && formPos.chargeRecharged)
            {
                if (formPos.enemyFormationToTarget != null && formPos.enemyFormationToTarget.alive && !formPos.enemyFormationToTarget.routing)
                { 
                    formPos.formationToFocusFire = formPos.enemyFormationToTarget; 
                    formPos.StartCharging();
                }
                else
                {
                    formPos.formationToFocusFire = null;
                }
            } 
        }
    }
    private void AIRaisePursueRadius()
    {
        float newRadius = 999;
        foreach (FormationPosition formPos in enemyControlledFormations)
        {
            if (formPos.soldierBlock.melee)
            { 
                formPos.engageEnemyRadius = newRadius;
            }
            else
            {
                formPos.engageEnemyRadius = newRadius;
                //formPos.engageEnemyRadius = formPos.soldierBlock.modelAttackRange;
            }
        }
    }
    private void AISetDefaultPursueRadius()
    {
        foreach (FormationPosition formPos in enemyControlledFormations)
        {
            if (formPos.soldierBlock.melee)
            {
                formPos.engageEnemyRadius = formPos.startingPursueRadius;
            }
            else
            {
                formPos.engageEnemyRadius = formPos.soldierBlock.modelAttackRange;
            }
        }
    }
    private void AIAllMagesPickTargetsAndFire()
    {
        List<FormationPosition> aiFormList = new List<FormationPosition>();
        foreach (FormationPosition aiForm in enemyControlledFormations)
        {
            aiFormList.Add(aiForm);
        } 
        List<FormationPosition> curatedPlayerForms = new List<FormationPosition>();
        foreach (FormationPosition form in playerControlledFormations)
        {
            if (form.alive && !form.routing)
            { 
                curatedPlayerForms.Add(form);
            }
        }
        foreach (FormationPosition playerForms in curatedPlayerForms) //for each enemy formation
        {
            FormationPosition tempFormPos = null;
            float currentDistance = 99999;
            foreach (FormationPosition item in aiFormList) //get closest formation
            {
                float newDistance = Vector3.Distance(item.transform.position, playerForms.transform.position);

                float tooClose = 20;

                if (newDistance < tooClose) //if too close
                { 
                    continue; //skip this one to avoid friendly fire
                }

                if (newDistance < currentDistance)
                {
                    currentDistance = newDistance;
                    tempFormPos = item;
                }
            }
            if (tempFormPos != null)
            {
                float offset = playerForms.movingSpeed; 
                tempFormPos.CastMagic(playerForms.transform.position + (playerForms.transform.forward * offset), 0); //0 is temp   
                aiFormList.Remove(tempFormPos); //so it can't be chosen again //if this becomes a problem then make another list
            }
        } 
    }
    public void FleeFromFieldBattle()
    {
        /*if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.FieldBattle)
        { 
            DisplayDefeat();
            OverworldToFieldBattleManager.Instance.UpdateUnitManagerArmies();
            Invoke("BattleOver", 5);
        }*/
        CancelInvoke("Game");
        CheckIfFieldBattleOver(true);
    }
    private void GameOverCheck()
    {
        CheckIfFieldBattleOver();
    }
    private void CheckIfFieldBattleOver(bool invokeFailure = false)
    {
        if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.FieldBattle)
        {
            bool gameIsOver = false;
            int victoryStatus = 0; //0 is undecided, 1 is player, 2 is ai 
            int numberOfLostFormations = 0;
            int numberOfAILostFormations = 0;
            foreach (FormationPosition form in playerControlledFormations)
            {
                if (form.numberOfAliveSoldiers <= 0 || form.routing || form == null) //if all dead, or fleeing
                {
                    numberOfLostFormations++;
                }
            }
            foreach (FormationPosition form in enemyControlledFormations)
            {
                if (form.numberOfAliveSoldiers <= 0 || form.routing || form == null) //if all dead, or fleeing
                {
                    numberOfAILostFormations++;
                }
            }
            if (numberOfAILostFormations >= enemyControlledFormations.Count)
            {
                gameIsOver = true;
                victoryStatus = 1;
            }
            if (numberOfLostFormations >= playerControlledFormations.Count)
            {
                gameIsOver = true;
                victoryStatus = 2;
            }
            if (invokeFailure)
            {
                gameIsOver = true;
                victoryStatus = 2;
            }
            if (gameIsOver)
            {
                CancelAllTasks();
                if (victoryStatus == 1)
                {
                    DisplayVictory();
                    victorBattleGroup = OverworldManager.Instance.playerBattleGroup;
                    loserBattleGroup = OverworldManager.Instance.enemyBattleGroup;
                }
                else if (victoryStatus == 2)
                {
                    DisplayDefeat();
                    victorBattleGroup = OverworldManager.Instance.enemyBattleGroup;
                    loserBattleGroup = OverworldManager.Instance.playerBattleGroup;
                }
                OverworldToFieldBattleManager.Instance.UpdateUnitManagerArmies();
                Invoke("BattleOver", 5);
            }
        }
        
    }
    private void CancelAllTasks()
    {
        foreach (FormationPosition form in playerControlledFormations)
        {
            form.CancelTasks();
        }
        foreach (FormationPosition form in enemyControlledFormations)
        {
            form.CancelTasks();
        }
    }


    public BattleGroup victorBattleGroup;

    public BattleGroup loserBattleGroup;
    [SerializeField] private GameObject victoryDisplay;
    [SerializeField] private GameObject defeatDisplay;

    private void DisplayVictory()
    {
        victoryDisplay.SetActive(true);
        //OverworldManager.Instance.armyThatWeAreFighting.parent.SetActive(false);
        //need to remove the army from the world and the overworld update list
        //need to stop all movement, or only start a battle once movement is settled?
        //make armies not collide with each other, can take up same tile
    }
    private void DisplayDefeat()
    { 
        defeatDisplay.SetActive(true);
    }
    private void HideOutcomeDisplays()
    { 
        victoryDisplay.SetActive(false);
        defeatDisplay.SetActive(false);
    }
    private void BattleOver()
    { 
        AfterBattleCleanup();
        CancelInvoke("GameOverCheck");
        HideOutcomeDisplays();
        OverworldToFieldBattleManager.Instance.EndFieldBattle();
        MusicManager.Instance.PlayOverworldMusic();
    }
    private void AfterBattleCleanup()
    { 
        enemyPlacementZone.gameObject.SetActive(true);
        friendlyPlacementZone.gameObject.SetActive(true);
    }
    public void HoldPositionCommand()
    { 
        foreach (FormationPosition item in selectedFormations)
        {
            item.StopChaseCommand();
        }
        UpdateGUI();
    }
    public void PursueCommand()
    { 
        foreach (FormationPosition item in selectedFormations)
        {
            item.PursueCommand();
        }
        UpdateGUI();
    }

    public void BeginTargetFire()
    {
        forceFiring = true;
        wasFocusFiring = true;
        forceFireTarget.SetActive(true);
    }

    public void BeginMagicTargeting(int abilityNum)
    {
        magicTargeting = true;
        wasMagicTargeting = true;
        forceFireTarget.SetActive(true);
        abilityNumber = abilityNum;
    }

    private void CheckIfShowCombatGUI()
    {
        if (selectedFormations.Count > 0 )
        {
            battleUI.SetActive(true);
            UpdateGUI();
        }
        else
        { 
            battleUI.SetActive(false);
        }
    }

    public void HaltCommand()
    { 
        foreach (FormationPosition item in selectedFormations)
        {
            item.StopCommand();
        }
        UpdateGUI();
    }
    public void RoutCommand()
    {
        foreach (FormationPosition item in selectedFormations)
        {
            item.RoutCommand();
        }
        UpdateGUI();
    }

    public void MarchCommand()
    {
        foreach (FormationPosition item in selectedFormations)
        {
            if (!item.braced)
            { 
                item.ResumeCommand();
                //item.ForceUpdateSoldiersDestinations();
                item.RapidUpdateDestinations();
            }
        }
        UpdateGUI();
    }

    public void SetBrace(bool val)
    {
        foreach (FormationPosition formation in selectedFormations)
        {
            if (formation.canBrace)
            {
                formation.SetBrace(val);
            }
        }
        UpdateGUI();
    }
    public void UpdateGUI()
    { 
        bool isSelectedRanged = false;
        bool isSelectedMelee = false;
        int numberOfRanged = 0;
        int numberHoldingFire = 0;
        int numberFreeFiring = 0;
        int numStopped = 0;
        int numChasing = 0;
        int numBraced = 0;
        foreach (GameObject item in targetList)
        {
            Destroy(item.gameObject);
        }  
        targetList.Clear();
        magicUI.SetActive(false);

        mageAbility1.gameObject.SetActive(true);
        mageAbility2.gameObject.SetActive(true); 
        mageAbility1.interactable = false;
        mageAbility2.interactable = false;
        setChargeButton.interactable = false;

        int numActuallySelected = 0;

        foreach (FormationPosition formation in selectedFormations)
        { 
            if (formation.chargeRecharged)
            {
                setChargeButton.interactable = true;
            }
            if (formation.selected)
            {
                numActuallySelected++;
            }
            if (formation.canBrace)
            {
                braceUI.SetActive(true);
            }
            else
            {
                braceUI.SetActive(false);
            }
            if (formation.braced)
            {
                numBraced++;
            }
            //show mage interface
            if (formation.soldierBlock.listMageModels.Count > 0) //formation.soldierBlock.mageType == "Pyromancer"
            {
                foreach(SoldierModel model in formation.soldierBlock.listMageModels)
                {
                    if (model.alive)
                    { 
                        magicUI.SetActive(true);
                        if (model.magicCharged && formation.allowedToCastMagic)
                        { 
                            mageAbility1.interactable = true;
                            mageAbility2.interactable = true;
                        }
                    } 
                }
            }

            if (formation.soldierBlock.mageType == "Gallowglass")
            { 
                magicUI.SetActive(true);
                if (formation.abilityCharged)
                {
                    mageAbility1.interactable = true;
                    mageAbility2.interactable = true;
                }
            }

            // change abilities 
            mageHeader.text = formation.soldierBlock.mageType;
            TMP_Text text = mageAbility1.GetComponentInChildren<TMP_Text>();
            TMP_Text text2 = mageAbility2.GetComponentInChildren<TMP_Text>();
            mageAbility1.enabled = true;
            mageAbility2.enabled = true;
            if (formation.soldierBlock.mageType == "Pyromancer")
            { 
                text.text = "Fireball";
                text2.text = "Smokescreen";
            }
            if (formation.soldierBlock.mageType == "Gallowglass")
            {
                text.text = "Chaff Bombs";
                mageAbility2.gameObject.SetActive(false);
            }
            if (formation.soldierBlock.mageType == "Eldritch")
            {
                text.text = "Eldritch Morass";
                text2.text = "Auroral Barrier";
            }
            if (formation.soldierBlock.mageType == "Seele")
            {
                text.text = "Raise Dead";
                text2.text = "Curse Foe";
            }
            if (formation.soldierBlock.mageType == "Flammen")
            {
                text.text = "Disgorge Flame";
                mageAbility2.gameObject.SetActive(false);
            }
            //
            if (formation.holdFire)
            {
                numberHoldingFire++;
            }
            if (formation.chaseDetectedEnemies)
            {
                numChasing++;
            }
            if (formation.soldierBlock.canBeRanged)
            {
                isSelectedRanged = true;
                numberOfRanged++;
                if (!formation.focusFire)
                {
                    numberFreeFiring++;
                }
                else //for each that IS focus firing
                {
                    Vector3 spawnPos = new Vector3(0, 0, 0);
                    if (formation.formationToFocusFire != null)
                    {
                        spawnPos = formation.formationToFocusFire.transform.position;
                    }
                    else
                    {
                        spawnPos = formation.focusFirePos;
                    }
                    GameObject target = Instantiate(forceFireTargetPrefab, spawnPos, Quaternion.Euler(90,0,0));
                    if (formation.formationToFocusFire != null) // if targeting a formation, then parent it so it follows it
                    {
                        target.transform.parent = formation.formationToFocusFire.transform;
                    }
                    targetList.Add(target);
                    Vector3 pos = formation.focusFirePos;
                    float distanceBetween = Vector3.Distance(pos, formation.transform.position);
                    distanceBetween *= 0.25f;
                    distanceBetween = Mathf.Clamp(distanceBetween, 5, 999);
                    target.transform.localScale = new Vector3(distanceBetween, distanceBetween, 100);
                }
            }
            if (!formation.soldierBlock.canBeRanged)
            { 
                isSelectedMelee = true;
            }
            if (formation.movementManuallyStopped)
            {
                numStopped++;
            }
        }
        if (numActuallySelected <= 0)
        {
            /*rangedUI.SetActive(false); //if at least one is ranged or melee, then we activate
            meleeUI.SetActive(false);
            braceUI.SetActive(false);
            magicUI.SetActive(false);*/
            battleUI.SetActive(false);
        }
        else
        {
            if (numBraced == selectedFormations.Count)
            {
                setBraceButton.interactable = false;
                setUnbraceButton.interactable = true;
            }
            else
            {
                setBraceButton.interactable = true;
                setUnbraceButton.interactable = false;
            }
            if (numberHoldingFire == selectedFormations.Count)
            {
                setHoldButton.interactable = false;
                setCeaseButton.interactable = false;
            }
            else
            {
                setHoldButton.interactable = true;
                setCeaseButton.interactable = true;
            }
            if (numberHoldingFire == 0)
            {
                setFireButton.interactable = false;
                setAllowFightButton.interactable = false;
            }
            else
            {
                setFireButton.interactable = true;
                setAllowFightButton.interactable = true;
            }

            if (numberFreeFiring == 0)
            {
                setFreeFireButton.interactable = true;
            }
            else
            {
                setFreeFireButton.interactable = false;
            }

            setMarchButton.interactable = true;
            setHaltButton.interactable = true;
            if (numStopped == 0) //all moving
            {
                setMarchButton.interactable = false;
                setHaltButton.interactable = true;
            }
            if (numStopped == selectedFormations.Count) //all stopped
            {
                setMarchButton.interactable = true;
                setHaltButton.interactable = false;
            }

            setHoldPositionButton.interactable = true;
            setPursueButton.interactable = true;
            if (numChasing == 0) //all moving
            {
                setHoldPositionButton.interactable = false;
                setPursueButton.interactable = true;
            }
            if (numChasing == selectedFormations.Count) //all stopped
            {
                setHoldPositionButton.interactable = true;
                setPursueButton.interactable = false;
            }
            rangedUI.SetActive(isSelectedRanged); //if at least one is ranged or melee, then we activate
            meleeUI.SetActive(isSelectedMelee); 
        }
        
    }

    public void ClearOrders()
    {
        foreach (FormationPosition formation in selectedFormations)
        {
            formation.ClearOrders(); 
        }

        //Clear pos list based on selected
        UpdateGUI();
    }

    public void OrderRetreat()
    {
        List<FormationPosition> list = new List<FormationPosition>(selectedFormations);
        foreach (FormationPosition formation in list)
        {
            formation.RoutCommand();
        }
    }

    public void SetAutoFire( bool holdFire) //for bowmen
    {
        foreach (FormationPosition formation in selectedFormations)
        {
            formation.holdFire = holdFire;
        }
        UpdateGUI();
    }

    private float doubleClickTimeOut = .25f;
    private float doubleClickTime = 0;
    private bool checkingDoubleClick = false;
    // Update is called once per frame
    void Update()
    {
        if (OverworldToFieldBattleManager.Instance.state == OverworldToFieldBattleManager.possibleGameStates.FieldBattle)
        {
            if (placingSoldiers)
            {
                if (!hoveringUI)
                {
                    CheckIfCursorInPlacementBounds();
                    if (soldierToPlace != GlobalDefines.SoldierTypes.none && formPosToReposition == null)
                    {
                        PlacerLeftClickCheck();
                    }
                    else
                    {
                        GrabberLeftClickCheck();
                    }
                }
                else
                {
                    formationPlacerIndicator.SetActive(false);
                }
            }
            else
            {
                if (forceFiring)
                {
                    UpdateTargeter(); //update forcefire targeter
                    ForceFireLeftClickCheck();
                    ForceFireRightClickCheck();
                }
                else if (magicTargeting)
                {
                    UpdateTargeter();
                    TargetMagicLeftClickCheck();
                    TargetMagicRightClickCheck();
                }
                else
                {
                    LeftClickCheck();
                    RightClickCheck();
                }

                if (checkingDoubleClick)
                {
                    doubleClickTime += Time.deltaTime;
                    if (doubleClickTime >= doubleClickTimeOut)
                    {
                        checkingDoubleClick = false;
                        doubleClickTime = 0;
                    }
                }
            }
        } 
    }
    #region PlacingSoldiers
    private void CheckIfCursorInPlacementBounds()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {
            cursorWithinTroopPlacementBounds = friendlyPlacementZone.bounds.Contains(hit.point);
        }
        if (cursorWithinTroopPlacementBounds)
        {
            if (soldierToPlace != GlobalDefines.SoldierTypes.none && formPosToReposition == null)
            { 
                formationPlacerIndicator.transform.position = hit.point;
                formationPlacerIndicator.SetActive(true);
            } 
            else if (formPosToReposition != null)
            { 
                UpdateGrabbedFormPosPosition();
                formationPlacerIndicator.SetActive(false);
            }
        }
        else
        { 
            formationPlacerIndicator.SetActive(false);
        }
    }
    private bool cursorWithinTroopPlacementBounds = false;
    private int soldiersToCreateNum = 80;
    private void GrabberLeftClickCheck()
    { 
        if (!hoveringUI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (formPosToReposition == null)
                {
                    AttemptToGrabUnit();
                }
                else if (cursorWithinTroopPlacementBounds)
                { 
                    PlaceUnitDownAtMousePoint(soldierToPlace, soldiersToCreateNum, GlobalDefines.Team.Altgard);
                }
            }
        }
    }
    private void UpdateGrabbedFormPosPosition()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {  
            formPosToReposition.soldierBlock.gameObject.transform.position = hit.point;
        }
    }
    private void AttemptToGrabUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {
            clickPosition = hit.point; 
            LayerMask formMask = LayerMask.GetMask("Formation");
            RaycastHit formHit;
            Vector3 vec = new Vector3(hit.point.x, 100, hit.point.z);
            if (Physics.Raycast(vec, Vector3.down, out formHit, Mathf.Infinity, formMask))
            {
                formPosToReposition = formHit.transform.gameObject.GetComponentInParent<FormationPosition>();

                soldierToPlace = formPosToReposition.soldierType;
                soldiersToCreateNum = formPosToReposition.numberOfAliveSoldiers;
                Destroy(formPosToReposition.soldierBlock.gameObject);
                formPosToReposition = null;
                /*formPosToReposition.aiPath.enabled = false;
                formPosToReposition.ToggleFormationSoldiersPathfinding(false); */
                finishedPlacingButton.interactable = false;
            } 
        }
    }
    private GlobalDefines.Team soldierTeam = GlobalDefines.Team.Altgard;
    private void PlaceUnitDownAtMousePoint(GlobalDefines.SoldierTypes soldierType, int numberOfSoldiers, GlobalDefines.Team team = GlobalDefines.Team.Altgard)
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        { 
            PlaceFormationAtPositionOfType(soldierToPlace, soldiersToCreateNum, hit.point, team); 
        }
    }
    private void PlacerLeftClickCheck()
    {
        if (!hoveringUI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (cursorWithinTroopPlacementBounds)
                { 
                    PlaceUnitDownAtMousePoint(soldierToPlace, soldiersToCreateNum);
                }
            }
        } 
    }
    private void PlaceFormationAtPositionOfType(GlobalDefines.SoldierTypes soldierType, int numberOfSoldiers, Vector3 pos, GlobalDefines.Team placementTeam = GlobalDefines.Team.Altgard)
    {
        SoldierBlock toPlace = GoThroughUnitManagerFindUnitOfType(soldierType);
        if (toPlace != null)
        {  
            SoldierBlock createdBlock = Instantiate(toPlace, pos, Quaternion.identity);
            createdBlock.soldiersToCreate = numberOfSoldiers; //set how many soldiers to create
            createdBlock.teamType = placementTeam;
            createdBlock.SetUpSoldiers(); // create soldiers  
            if (placementTeam == team) //if we're placing stuff, do some clean up
            { 
                soldierToPlace = GlobalDefines.SoldierTypes.none;
                soldierButtonsList[soldierButtonID].interactable = false;
                formationPlacerIndicator.SetActive(false);
                finishedPlacingButton.interactable = CheckIfAllAvailableSoldiersPlaced();
            }               
        }
        else
        {
            Debug.LogError("No formation found");
        }
    } 
    private SoldierBlock GoThroughUnitManagerFindUnitOfType(GlobalDefines.SoldierTypes soldierType)
    {
        int i = 0;
        foreach (GlobalDefines.SoldierTypes item in UnitManager.Instance.unitTypes)
        {
            if (item == soldierType)
            {
                return UnitManager.Instance.formationsToInstantiateBasedOnUnitType[i];
            }
            i++;
        }
        return null;
    }

    #endregion
    private void UpdateTargeter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {
            forceFireTarget.transform.position = hit.point;
            //hit terrain, now check if formation here
            LayerMask formMask = LayerMask.GetMask("Formation");
            RaycastHit formHit;
            Vector3 vec = new Vector3(hit.point.x, 100, hit.point.z);
            if (Physics.Raycast(vec, Vector3.down, out formHit, Mathf.Infinity, formMask))
            {
                formationToFocusFire = formHit.transform.gameObject.GetComponentInParent<FormationPosition>();
            }
            else
            {
                formationToFocusFire = null;
            }
        }
        float distanceBetween = Vector3.Distance(forceFireTarget.transform.position, GetClosestSelectedFormationToPoint(forceFireTarget.transform.position).transform.position);
        distanceBetween *= 0.25f;
        distanceBetween = Mathf.Clamp(distanceBetween, 5, 999);
        forceFireTarget.transform.localScale = new Vector3(distanceBetween, distanceBetween, .1f * distanceBetween);
    }
    private void TargetMagicLeftClickCheck()
    {
        if (selectedFormations.Count == 1)
        { 
            if (Input.GetMouseButtonDown(0))
            {
                foreach (FormationPosition item in selectedFormations)
                {
                    item.CastMagic(forceFireTarget.transform.position, abilityNumber);
                }
                UpdateGUI();
                magicTargeting = false;
                forceFireTarget.SetActive(false);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !hoveringUI) //start drawing line
            {
                lineFormationPosList.Clear();
                ClearPlacementMarkers();
                drawingLine = true;
            }
            if (Input.GetMouseButton(0) && !hoveringUI && drawingLine) //held and moving and such
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float distanceSoFar = 9999;
                var hits = Physics.RaycastAll(ray, distanceSoFar);
                bool haveHit = false;
                RaycastHit candidateHit = new RaycastHit();
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag == "Terrain")
                    {
                        if (hit.distance < distanceSoFar)
                        {
                            candidateHit = hit;
                            distanceSoFar = hit.distance;
                            haveHit = true;
                        }
                    }
                }
                if (haveHit)
                {
                    if (lineFormationPosList.Count < selectedFormations.Count) //only create formpos positions up to number of selected formpos
                    {
                        if (lineFormationPosList.Count == 0)
                        {
                            PlaceTargeter(candidateHit.point);
                        }
                        else if (Vector3.Distance(lineFormationPosList[lineFormationPosList.Count - 1], candidateHit.point) >= lineOffset) //if distance between last and new is high enough
                        {
                            PlaceTargeter(candidateHit.point);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && !hoveringUI)
            { 
                List<FormationPosition> formList = new List<FormationPosition>();
                foreach (FormationPosition selForm in selectedFormations)
                {
                    formList.Add(selForm);
                }
                foreach (Vector3 pos in lineFormationPosList) //for each point
                {
                    FormationPosition tempFormPos = null;
                    float currentDistance = 99999;
                    foreach (FormationPosition item in formList) //get closest formation
                    {
                        float newDistance = Vector3.Distance(item.transform.position, pos);
                        if (newDistance < currentDistance)
                        {
                            currentDistance = newDistance;
                            tempFormPos = item;
                        }
                    }
                    tempFormPos.CastMagic(pos, abilityNumber);
                    formList.Remove(tempFormPos); //so it can't be chosen again //if this becomes a problem then make another list
                }
                UpdateGUI();
                magicTargeting = false;
                forceFireTarget.SetActive(false);
                drawingLine = false;
            }
        }
    }
    private void PlaceTargeter(Vector3 pos)
    { 
        lineFormationPosList.Add(pos);
        GameObject target = Instantiate(forceFireTargetPrefab, pos, Quaternion.Euler(90, 0, 0));
        placementMarkers.Add(target);

        float distanceBetween = Vector3.Distance(target.transform.position, GetClosestSelectedFormationToPoint(target.transform.position).transform.position); //not perfect
        distanceBetween *= 0.25f;
        distanceBetween = Mathf.Clamp(distanceBetween, 5, 999);
        target.transform.localScale = new Vector3(distanceBetween, distanceBetween, 100); //adjust scaling
    }

    private FormationPosition GetClosestSelectedFormationToPoint(Vector3 point)
    {
        List<FormationPosition> formList = new List<FormationPosition>();
        foreach (FormationPosition selForm in selectedFormations)
        {
            formList.Add(selForm);
        } 
        FormationPosition tempFormPos = null;
        float currentDistance = 99999;

        foreach (FormationPosition item in formList) //Selects closest formation
        {
            float newDistance = Helper.Instance.GetSquaredMagnitude(item.transform.position, point);
            if (newDistance < currentDistance)
            {
                currentDistance = newDistance;
                tempFormPos = item;
            }
        } 
        return tempFormPos;
    }

    private void TargetMagicRightClickCheck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            magicTargeting = false;
            forceFireTarget.SetActive(false);

            lineFormationPosList.Clear();
            ClearPlacementMarkers();
            drawingLine = false;
        }
    }
    public void StopFocusFire()
    {
        foreach (FormationPosition item in selectedFormations)
        { 
            item.focusFire = false;
            item.formationToFocusFire = null;
        }
        formationToFocusFire = null;
    }
    private void ForceFireLeftClickCheck()
    {
        //
        if (selectedFormations.Count == 1)
        {  
            if (Input.GetMouseButtonDown(0))
            { 
                foreach (FormationPosition item in selectedFormations)
                {
                    item.formationToFocusFire = null;//need to clear this first
                    item.focusFirePos = forceFireTarget.transform.position;
                    item.focusFire = true;
                    if (formationToFocusFire != null)
                    {
                        item.formationToFocusFire = formationToFocusFire;
                    }
                    if (item.soldierBlock.melee)
                    {
                        item.StartCharging();
                    }
                }
                UpdateGUI();
                forceFiring = false;
                forceFireTarget.SetActive(false);
                formationToFocusFire = null;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !hoveringUI) //start drawing line
            {
                lineFormationPosList.Clear();
                ClearPlacementMarkers();
                drawingLine = true;
            }
            if (Input.GetMouseButton(0) && !hoveringUI && drawingLine) //held and moving and such
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float distanceSoFar = 9999;
                var hits = Physics.RaycastAll(ray, distanceSoFar);
                bool haveHit = false;
                RaycastHit candidateHit = new RaycastHit();
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag == "Terrain")
                    {
                        if (hit.distance < distanceSoFar)
                        {
                            candidateHit = hit;
                            distanceSoFar = hit.distance;
                            haveHit = true;
                        }
                    }
                }
                if (haveHit)
                {
                    if (lineFormationPosList.Count < selectedFormations.Count) //only create formpos positions up to number of selected formpos
                    {
                        if (lineFormationPosList.Count == 0)
                        {
                            PlaceTargeter(candidateHit.point);
                        }
                        else if (Vector3.Distance(lineFormationPosList[lineFormationPosList.Count - 1], candidateHit.point) >= lineOffset) //if distance between last and new is high enough
                        {
                            PlaceTargeter(candidateHit.point);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && !hoveringUI)
            {

                if (lineFormationPosList.Count == 1)
                { 
                    foreach (FormationPosition item in selectedFormations)
                    {
                        item.formationToFocusFire = null;//need to clear this first
                        item.focusFire = true;
                        Vector3 pos = lineFormationPosList[0];
                        LayerMask layerMask = LayerMask.GetMask("Formation");
                        int maxColliders = 1;
                        float radius = 5;
                        Collider[] hitColliders = new Collider[maxColliders];
                        int numColliders = Physics.OverlapSphereNonAlloc(pos, radius, hitColliders, layerMask, QueryTriggerInteraction.Ignore);

                        if (hitColliders[0] != null) //is formation
                        {
                            FormationPosition form = hitColliders[0].gameObject.GetComponent<FormationPosition>();
                            item.formationToFocusFire = form;
                        }
                        else
                        {
                            item.focusFirePos = pos;
                        }
                        if (item.soldierBlock.melee)
                        {
                            item.StartCharging();
                        }
                    }
                    UpdateGUI();
                    forceFiring = false;
                    forceFireTarget.SetActive(false);
                    formationToFocusFire = null;
                    drawingLine = false;
                }
                else
                {
                    List<FormationPosition> formList = new List<FormationPosition>();
                    foreach (FormationPosition selForm in selectedFormations)
                    {
                        formList.Add(selForm);
                    }
                    foreach (Vector3 pos in lineFormationPosList) //for each point
                    {
                        FormationPosition tempFormPos = null;
                        float currentDistance = 99999;
                        foreach (FormationPosition item in formList) //get closest formation
                        {
                            //float newDistance = Vector3.Distance(item.transform.position, pos);
                            float newDistance = Helper.Instance.GetSquaredMagnitude(item.transform.position, pos);
                            if (newDistance < currentDistance)
                            {
                                currentDistance = newDistance;
                                tempFormPos = item;
                            }
                        }

                        tempFormPos.formationToFocusFire = null;
                        tempFormPos.focusFire = true;
                        //check to see if pos is a formation or not

                        LayerMask layerMask = LayerMask.GetMask("Formation");
                        int maxColliders = 1;
                        float radius = 5;
                        Collider[] hitColliders = new Collider[maxColliders];
                        int numColliders = Physics.OverlapSphereNonAlloc(pos, radius, hitColliders, layerMask, QueryTriggerInteraction.Ignore);

                        if (hitColliders[0] != null) //is formation
                        {
                            FormationPosition form = hitColliders[0].gameObject.GetComponent<FormationPosition>();
                            tempFormPos.formationToFocusFire = form;
                        }
                        else
                        {
                            tempFormPos.focusFirePos = pos;
                        }
                        if (tempFormPos.soldierBlock.melee)
                        {
                            tempFormPos.StartCharging();
                        }
                        formList.Remove(tempFormPos); //so it can't be chosen again //if this becomes a problem then make another list
                    }

                    UpdateGUI();
                    forceFiring = false;
                    forceFireTarget.SetActive(false);
                    formationToFocusFire = null;
                    drawingLine = false;
                } 
            }
        }

    }
    private void ForceFireRightClickCheck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (FormationPosition item in selectedFormations)
            { 
                item.focusFire = false;
                item.formationToFocusFire = null;
            }
            forceFiring = false;
            forceFireTarget.SetActive(false);
            formationToFocusFire = null;
        } 
    }
    private void LeftClickCheck()
    {  
        if (!hoveringUI)
        {
            if (Input.GetMouseButtonDown(0))
            {
                wasFocusFiring = false;
                wasMagicTargeting = false;
                startPos = Input.mousePosition;
                AttemptToSelectUnit();
                CheckIfShowCombatGUI();
                ClearPlacementMarkers();
            }
            if (Input.GetMouseButton(0) && !wasFocusFiring && !wasMagicTargeting) //held and moving and such
            {
                UpdateSelectionBox(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0) && !wasFocusFiring && !wasMagicTargeting)
            {
                ReleaseSelectionBox();
                CheckIfShowCombatGUI();
            }
        }
        
    }
    private void ReleaseSelectionBox()
    {
        selectionBox.gameObject.SetActive(false);
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
        foreach (FormationPosition form in playerControlledFormations) //select units if in box
        {
            if (form.routing)
            {
                continue;
            }
            Vector3 screenPos = cam.WorldToScreenPoint(form.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                if (form.alive && form.team == team && form.selectable)
                {   

                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        form.SetSelected(true);
                        selectedFormations.Add(form);  
                        form.TriggerSelectionCircles(true);
                    }
                    else
                    {
                        form.SetSelected(!form.selected);
                        if (form.selected)
                        {
                            selectedFormations.Add(form);
                        }
                        else
                        {
                            selectedFormations.Remove(form);
                        }
                        form.TriggerSelectionCircles(form.selected);
                    } 
                }
            }
        }
    }

    private void UpdateSelectionBox(Vector2 mousePos)
    {
        if (!selectionBox.gameObject.activeInHierarchy)
            selectionBox.gameObject.SetActive(true);
        float width = mousePos.x - startPos.x;
        float height = mousePos.y - startPos.y;
        //magic
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = startPos + new Vector2(width / 2, height / 2);

    }

    private void AttemptToSelectUnit()
    {
        //Debug.Log("HEY");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit; 

        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
        {
            clickPosition = hit.point;
            //hit terrain, now check if formation here
            LayerMask formMask = LayerMask.GetMask("Formation");
            RaycastHit formHit;
            Vector3 vec = new Vector3(hit.point.x, 100, hit.point.z);
            if (Physics.Raycast(vec, Vector3.down, out formHit, Mathf.Infinity, formMask))
            {
                FormationPosition form = formHit.transform.gameObject.GetComponentInParent<FormationPosition>();  

                if (form.routing)
                {
                    return;
                }
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectOtherUnits(form);
                }

                if (form.alive && form.team == team)
                {
                    form.SetSelected(!form.selected);
                    if (form.selected)
                    {
                        selectedFormations.Add(form);
                    }
                    else
                    {
                        selectedFormations.Remove(form);
                    }
                    form.TriggerSelectionCircles(form.selected);

                    if (checkingDoubleClick)
                    {
                        SelectSimilar(form);
                    }
                    checkingDoubleClick = true;
                } 
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectUnits();
                }
            }
        } 
    }
    private void SelectSimilar(FormationPosition ogForm)
    {
        foreach (FormationPosition form in playerControlledFormations)
        {
            if (form.formationType == ogForm.formationType && form.alive && !form.routing)
            { 
                form.SetSelected(true);
                form.TriggerSelectionCircles(true);
                if (!selectedFormations.Contains(form))
                {
                    selectedFormations.Add(form);
                }
            }
        }
    }
    private void DeselectOtherUnits(FormationPosition exclude)
    {
        selectedFormations.Clear();
        foreach (FormationPosition form in playerControlledFormations)
        {
            if (form != exclude)
            { 
                form.SetSelected(false);
                form.TriggerSelectionCircles(false);
            }
        }
        UpdateGUI();
    }
    private void DeselectUnits()
    {
        selectedFormations.Clear();
        foreach (FormationPosition form in playerControlledFormations)
        {
            form.SetSelected(false); 
            form.TriggerSelectionCircles(false);
        }
        UpdateGUI();
    }
    public void DeselectFormation(FormationPosition formPos)
    {
        if (selectedFormations.Contains(formPos))
        {
            selectedFormations.Remove(formPos);
        }
        formPos.SetSelected(false);
        formPos.TriggerSelectionCircles(false);
        UpdateGUI();
    }
    void OnDrawGizmosSelected()
    {
        foreach (Vector3 item in lineFormationPosList)
        { 
            Gizmos.DrawWireSphere(item, 10);
            Gizmos.color = Color.red;
        }
    }
    private void ClearPlacementMarkers()
    { 
        foreach (GameObject item in placementMarkers)
        {
            Destroy(item);
        }
        placementMarkers.Clear();
    }
    private void SetTransformOnGround(Transform transform, string layer)
    {
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        LayerMask layerMask = LayerMask.GetMask(layer);
        RaycastHit hit;
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
    }
    private void RightClickCheck()
    {
        if (Input.GetMouseButtonDown(1)) //set movepos
        {
            MarchCommand();
            lineFormationPosList.Clear();
            wasFocusFiring = false;
            wasMagicTargeting = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
             
            LayerMask layerMask = LayerMask.GetMask("Terrain");
            RaycastHit hit;

            FormationPosition formToFollow = null;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
            {
                clickPosition = hit.point;
                //hit terrain, now check if formation here
                LayerMask formMask = LayerMask.GetMask("Formation");
                RaycastHit formHit;
                Vector3 vec = new Vector3(hit.point.x, 100, hit.point.z);
                if (Physics.Raycast(vec, Vector3.down, out formHit, Mathf.Infinity, formMask))
                {
                    formToFollow = formHit.transform.gameObject.GetComponentInParent<FormationPosition>();
                } 
            } 

            foreach (FormationPosition item in selectedFormations) //selected formations, go there plox
            {
                Vector3 pos = item.transform.position;
                item.lineRenderer2.enabled = true;
                item.lineRenderer2.SetPosition(0, clickPosition);
                item.lineRenderer2.SetPosition(1, clickPosition); 
                item.pathSet = false;
                 
                if (!Input.GetKey(KeyCode.LeftShift)) //if not holding shift, clear destinations
                {
                    item.destinationsList.Clear();
                } 
                item.destinationsList.Add(clickPosition);
                item.formationToFollow = formToFollow;
            }
             
            UpdateGUI();
            forceFiring = false;
            forceFireTarget.SetActive(false);
            formationToFocusFire = null;
            ClearPlacementMarkers();

        }
        if (Input.GetMouseButton(1) && !wasFocusFiring && !wasMagicTargeting) //update lines while held
        { 
            if (selectedFormations.Count == 1)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
                float distanceSoFar = 9999;
                var hits = Physics.RaycastAll(ray, distanceSoFar);
                bool haveHit = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag == "Terrain")
                    {
                        if (hit.distance < distanceSoFar)
                        {
                            clickPosition = hit.point;
                            distanceSoFar = hit.distance;
                            haveHit = true;
                        }
                    }
                }
                if (haveHit)
                {
                    foreach (FormationPosition item in selectedFormations)
                    {
                        item.lineRenderer2.SetPosition(1, clickPosition);
                        item.rotTarget.position = clickPosition;
                    }
                }
            }
            else if (selectedFormations.Count > 1)
            { 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float distanceSoFar = 9999;
                var hits = Physics.RaycastAll(ray, distanceSoFar);
                bool haveHit = false;
                RaycastHit candidateHit = new RaycastHit();
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag == "Terrain")
                    {
                        if (hit.distance < distanceSoFar)
                        {
                            candidateHit = hit;
                            distanceSoFar = hit.distance;
                            haveHit = true;
                        }
                    }
                }
                if (haveHit)
                {
                    if (lineFormationPosList.Count < selectedFormations.Count) //only create formpos positions up to number of selected formpos
                    {
                        if (lineFormationPosList.Count == 0)
                        {
                            PlaceTargeter(candidateHit.point);
                        }
                        else if (Vector3.Distance(lineFormationPosList[lineFormationPosList.Count - 1], candidateHit.point) >= lineOffset) //if distance between last and new is high enough
                        {
                            PlaceTargeter(candidateHit.point);
                        }

                    } 
                }
            } 
        }

        if (Input.GetMouseButtonUp(1) && !wasFocusFiring && !wasMagicTargeting) //set rotation and confirm movement
        { 
            if (selectedFormations.Count == 1)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float distanceSoFar = 9999;
                var hits = Physics.RaycastAll(ray, distanceSoFar);
                bool haveHit = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.tag == "Terrain")
                    {
                        if (hit.distance < distanceSoFar)
                        {
                            clickPosition = hit.point;
                            heldPosition = clickPosition;
                            distanceSoFar = hit.distance;
                            haveHit = true;
                        }
                    }
                }
                if (haveHit)
                {
                    foreach (FormationPosition item in selectedFormations)
                    {
                        item.rotTarget.transform.position = heldPosition;
                        item.pathSet = true;
                        item.obeyingMovementOrder = true;
                        /*foreach (SoldierModel model in item.soldierBlock.listSoldierModels)
                        {
                            model.GenerateDispersalVector();
                        }*/

                        float distanceReq = 1;
                        if (Vector3.Distance(heldPosition, item.destinationsList[0]) >= distanceReq)
                        {
                            item.shouldRotateToward = true;
                        }
                        else
                        {
                            item.shouldRotateToward = false;
                        } 
                        item.aiTarget.transform.position = item.destinationsList[0];
                        item.SetAndSearchPath();
                        SetTransformOnGround(item.aiTarget.transform, "GroundForm");

                        item.CheckIfRotateOrNot();

                    }
                }
                
            }
            else if (selectedFormations.Count > 1)
            {
                List<FormationPosition> formList = new List<FormationPosition>();
                foreach (FormationPosition selForm in selectedFormations)
                {
                    formList.Add(selForm);
                }
                if (lineFormationPosList.Count > 1)
                {
                    foreach (Vector3 pos in lineFormationPosList) //for each point
                    {
                        FormationPosition tempFormPos = null;
                        float currentDistance = 99999;
                        foreach (FormationPosition item in formList) //get closest formation
                        {
                            float newDistance = Helper.Instance.GetSquaredMagnitude(item.transform.position, pos);
                            if (newDistance < currentDistance)
                            {
                                currentDistance = newDistance;
                                tempFormPos = item;
                            }
                        }
                        tempFormPos.aiTarget.transform.position = pos; //tell closest formation to go there  
                        tempFormPos.SetAndSearchPath();
                        SetTransformOnGround(tempFormPos.aiTarget.transform, "GroundForm");
                        tempFormPos.destinationsList.Clear();
                        tempFormPos.destinationsList.Add(pos);
                        tempFormPos.pathSet = true;
                        tempFormPos.obeyingMovementOrder = true;
                        tempFormPos.shouldRotateToward = false;
                        formList.Remove(tempFormPos); //so it can't be chosen again //if this becomes a problem then make another list
                    }
                }
                else if (lineFormationPosList.Count == 1)
                { 
                    //get avg pos
                    float x = 0;
                    float y = 0;
                    float z = 0;
                    for (int i = 0; i < selectedFormations.Count; i++)
                    {
                        x += selectedFormations[i].transform.position.x;
                        y += selectedFormations[i].transform.position.y;
                        z += selectedFormations[i].transform.position.z;
                    }
                    x /= selectedFormations.Count;
                    y /= selectedFormations.Count;
                    z /= selectedFormations.Count;
                    Vector3 avg = new Vector3(x, y, z);

                    GameObject parent = Instantiate(destPrefab, avg, Quaternion.identity);
                    //make gameobjects to represent our destination, starting at our position
                    List<GameObject> dests = new List<GameObject>();
                    foreach (FormationPosition form in selectedFormations)
                    {
                        GameObject child = Instantiate(forceFireTargetPrefab, form.transform.position, Quaternion.identity, parent.transform);
                        dests.Add(child);
                    }
                    //parent gameobjects to big one

                    Vector3 heading = lineFormationPosList[0] - avg;
                    //rotate parent to face destination 
                    parent.transform.rotation = Quaternion.LookRotation(heading, Vector3.up);

                    //move parent to destination
                    parent.transform.position = lineFormationPosList[0];

                    for (int i = 0; i < selectedFormations.Count; i++)
                    {
                        FormationPosition form = selectedFormations[i];
                        Vector3 pos = dests[i].transform.position;

                        form.aiTarget.transform.position = pos; //tell closest formation to go there 
                        form.SetAndSearchPath();
                        SetTransformOnGround(form.aiTarget.transform, "GroundForm");
                        form.destinationsList.Clear();
                        form.destinationsList.Add(pos);
                        form.pathSet = true;
                        form.obeyingMovementOrder = true;
                        form.shouldRotateToward = false;
                    } 
                }

            }
        }
    }
}
