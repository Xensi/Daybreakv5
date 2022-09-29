using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FightManager : MonoBehaviour
{
    private Vector3 clickPosition;

    public List<FormationPosition> allFormations;
    public FormationPosition[] allArray;

    public List<FormationPosition> yourFormations;
    public List<FormationPosition> aiFormations;

    public List<FormationPosition> selectedFormations;
    [SerializeField] private string team = "Altgard"; //teams are Altgard, Zhanguo


    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private bool started = false;

    [SerializeField] private Camera cam;

    [SerializeField] private Transform rotationTarget;
    [SerializeField] private Vector3 heldPosition;
    //[SerializeField] private LineRenderer

    [SerializeField] private GameObject battleUI;

    [SerializeField] private GameObject rangedUI;
    [SerializeField] private GameObject meleeUI;
    [SerializeField] private GameObject magicUI;
    [SerializeField] private GameObject braceUI;
    [SerializeField] private GameObject chaffBombUI;
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

     
    private enum combatStrategy
    {
        Attack,
        Defend
    } 
    private combatStrategy aiState = combatStrategy.Attack;

    void OnEnable()
    {
    } 

    private void Start()
    {
        allArray = new FormationPosition[30];

        battleUI.SetActive(false); 
        InvokeRepeating("AIBrain", 0f, 1f);
        InvokeRepeating("AIBrainMage", 5f, 5f); //don't do immediately, not urgent

        allFormations.Clear();
        FormationPosition[] array = FindObjectsOfType<FormationPosition>();
        allArray = array;
        foreach (FormationPosition item in array)
        {
            allFormations.Add(item);
            if (item.team == team)
            {
                yourFormations.Add(item);
            }
            else
            {
                aiFormations.Add(item);
            }
        }
    }

    private void AIBrain()
    {
        switch (aiState)
        {
            case combatStrategy.Attack:
                AIRaisePursueRadius();
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
    private void AIRaisePursueRadius()
    {
        float newRadius = 200;
        foreach (FormationPosition formPos in aiFormations)
        {
            formPos.engageEnemyRadius = newRadius;
        }
    }
    private void AISetDefaultPursueRadius()
    {
        foreach (FormationPosition formPos in aiFormations)
        {
            formPos.engageEnemyRadius = formPos.startingPursueRadius;
        }
    }
    private void AIAllMagesPickTargetsAndFire()
    {
        List<FormationPosition> aiFormList = new List<FormationPosition>();
        foreach (FormationPosition aiForm in aiFormations)
        {
            aiFormList.Add(aiForm);
        } 
        List<FormationPosition> curatedPlayerForms = new List<FormationPosition>();
        foreach (FormationPosition form in yourFormations)
        {
            if (form.alive && !form.fleeing)
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

    public void MarchCommand()
    {
        foreach (FormationPosition item in selectedFormations)
        {
            if (!item.braced)
            { 
                item.ResumeCommand();
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
        foreach (FormationPosition formation in selectedFormations)
        { 
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
        if (numChasing== 0) //all moving
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

    public void ClearOrders()
    {
        foreach (FormationPosition formation in selectedFormations)
        {
            formation.ClearOrders(); 
        }

        //Clear pos list based on selected
        UpdateGUI();
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
            float newDistance = Vector3.Distance(item.transform.position, point);
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
                            float newDistance = Vector3.Distance(item.transform.position, pos);
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
    private void UpdateTargeter()
    { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceSoFar = 9999;
        var hits = Physics.RaycastAll(ray, distanceSoFar);
        Vector3 pos = new Vector3(0, 0, 0);

        //check to see if we are clicking on a formation or just on terrain

        RaycastHit candidateHit = new RaycastHit();
        bool formationHitFound = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.tag == "Terrain") 
            {
                if (hit.distance < distanceSoFar) //used to get closest result
                {
                    pos = hit.point;
                    candidateHit = hit;
                    distanceSoFar = hit.distance;
                }
            }
            else if (hit.collider.tag == "SelectableObject")
            { 
                if (hit.distance <= distanceSoFar) //used to get closest result
                {
                    pos = hit.point;
                    candidateHit = hit;
                    distanceSoFar = hit.distance;
                    formationHitFound = true;
                }
            }
        }
        if (formationHitFound && candidateHit.collider.tag == "SelectableObject")
        {
            formationToFocusFire = candidateHit.transform.gameObject.GetComponentInParent<FormationPosition>();
        }
        else
        {
            formationToFocusFire = null;
        }
        forceFireTarget.transform.position = pos;
        float distanceBetween = Vector3.Distance(forceFireTarget.transform.position, GetClosestSelectedFormationToPoint(forceFireTarget.transform.position).transform.position);
        distanceBetween *= 0.25f;
        distanceBetween = Mathf.Clamp(distanceBetween, 5, 999);
        forceFireTarget.transform.localScale = new Vector3(distanceBetween, distanceBetween, 100);
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
        foreach (FormationPosition form in yourFormations) //select units if in box
        {
            if (form.fleeing)
            {
                continue;
            }
            Vector3 screenPos = cam.WorldToScreenPoint(form.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                if (form.alive && form.team == team)
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        float distanceSoFar = 9999;
        var hits = Physics.RaycastAll(ray, distanceSoFar);

        
        RaycastHit candidateHit = new RaycastHit();
        bool hitFound = false;
        foreach (RaycastHit hit in hits)
        { 
            if (hit.collider.tag == "Terrain") //we only want to hit selectable
            {
                if (hit.distance < distanceSoFar) //get closest only
                {
                    distanceSoFar = hit.distance;

                    candidateHit = hit;
                    hitFound = true;
                }
            }
        } 
        if (hitFound)
        {
            LayerMask layerMask = LayerMask.GetMask("Model");
            int maxColliders = 1;
            Collider[] hitColliders = new Collider[maxColliders];
            float radius = 2f;
            int numColliders = Physics.OverlapSphereNonAlloc(candidateHit.point, radius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage 

            if (numColliders > 0) //at least 1
            {
                SoldierModel model = hitColliders[0].gameObject.GetComponentInParent<SoldierModel>();

                FormationPosition form = model.formPos;
                if (form.fleeing)
                {
                    return;
                }
                if (!Input.GetKey(KeyCode.LeftShift)) 
                {
                    DeselectOtherUnits(form);
                }
                 
                if (form.alive && model.alive && form.team == team)
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
        foreach (FormationPosition form in yourFormations)
        {
            if (form.type == ogForm.type && form.alive && !form.fleeing)
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
        foreach (FormationPosition form in yourFormations)
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
        foreach (FormationPosition form in yourFormations)
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

    private void RightClickCheck()
    {
        if (Input.GetMouseButtonDown(1)) //set movepos
        {
            lineFormationPosList.Clear();
            wasFocusFiring = false;
            wasMagicTargeting = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distanceSoFar = 9999;
            var hits = Physics.RaycastAll(ray, distanceSoFar);
            //bool formationHitFound = false;
            //RaycastHit candidateHit = new RaycastHit();
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Terrain")  
                {
                    if (hit.distance < distanceSoFar)
                    { 
                        clickPosition = hit.point;  
                        distanceSoFar = hit.distance;
                    }
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
            if (selectedFormations.Count == 1 || lineFormationPosList.Count == 1)
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
                    tempFormPos.aiTarget.transform.position = pos; //tell closest formation to go there 
                    tempFormPos.destinationsList.Clear();
                    tempFormPos.destinationsList.Add(pos);
                    tempFormPos.pathSet = true;
                    tempFormPos.obeyingMovementOrder = true;
                    tempFormPos.shouldRotateToward = false;
                    formList.Remove(tempFormPos); //so it can't be chosen again //if this becomes a problem then make another list
                } 
            }
        }
    }
}
