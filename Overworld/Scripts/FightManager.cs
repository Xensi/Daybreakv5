using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class FightManager : MonoBehaviour
{
    private Vector3 clickPosition;

    public List<FormationPosition> allFormations;

    public List<FormationPosition> yourFormations;

    public List<FormationPosition> selectedFormations;
    [SerializeField] private string team = "Altgard";


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
    [SerializeField] private GameObject pyromancerUI;
    [SerializeField] private Button setHoldButton;
    [SerializeField] private Button setFireButton;
    [SerializeField] private Button setFreeFireButton;
    [SerializeField] private Button setMarchButton;
    [SerializeField] private Button setHaltButton;

    [SerializeField] private Button setCeaseButton;
    [SerializeField] private Button setAllowFightButton;

    [SerializeField] private Button setHoldPositionButton;
    [SerializeField] private Button setPursueButton; 

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
    void OnEnable()
    {
        allFormations.Clear();
        FormationPosition[] array = FindObjectsOfType<FormationPosition>();
        foreach (FormationPosition item in array)
        {
            allFormations.Add(item);
            if (item.team == "Altgard")
            {
                yourFormations.Add(item);
            }
        }

    }

    private void Start()
    {
        battleUI.SetActive(false);
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
            item.ResumeCommand();
        }
        UpdateGUI();
    }

    private void UpdateGUI()
    {
        bool isSelectedRanged = false;
        bool isSelectedMelee = false;
        int numberOfRanged = 0;
        int numberHoldingFire = 0;
        int numberFreeFiring = 0;
        int numStopped = 0;
        int numChasing = 0;
        foreach (GameObject item in targetList)
        {
            Destroy(item.gameObject);
        }  
        targetList.Clear();
        pyromancerUI.SetActive(false);
        foreach (FormationPosition formation in selectedFormations)
        { 
            if (formation.soldierBlock.mageType == "Pyromancer")
            {
                pyromancerUI.SetActive(true);
            } 
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

    public void SetAutoFire( bool holdFire) //for bowmen
    {
        foreach (FormationPosition formation in selectedFormations)
        {
            formation.holdFire = holdFire;
        }
        UpdateGUI();
    }

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
    }
     

    private void TargetMagicLeftClickCheck()
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
    private void TargetMagicRightClickCheck()
    {
        if (Input.GetMouseButtonDown(1))
        {
            magicTargeting = false;
            forceFireTarget.SetActive(false); 
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
        float distanceBetween = Vector3.Distance(forceFireTarget.transform.position, selectedFormations[0].transform.position);
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
            Vector3 screenPos = cam.WorldToScreenPoint(form.transform.position);

            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                if (form.alive)
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
            float radius = .5f;
            int numColliders = Physics.OverlapSphereNonAlloc(candidateHit.point, radius, hitColliders, layerMask, QueryTriggerInteraction.Ignore); //nonalloc generates no garbage 

            if (numColliders > 0)
            {
                SoldierModel model = hitColliders[0].gameObject.GetComponentInParent<SoldierModel>();

                FormationPosition form = model.formPos;
                if (!Input.GetKey(KeyCode.LeftShift)) 
                {
                    DeselectOtherUnits(form);
                }
                 
                if (form.alive && model.alive)
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
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                { 
                    DeselectUnits();
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
                            lineFormationPosList.Add(candidateHit.point); 
                            GameObject target = Instantiate(forceFireTargetPrefab, candidateHit.point, Quaternion.Euler(90, 0, 0));
                            placementMarkers.Add(target);
                        }
                        else if (Vector3.Distance(lineFormationPosList[lineFormationPosList.Count - 1], candidateHit.point) >= lineOffset) //if distance between last and new is high enough
                        {
                            lineFormationPosList.Add(candidateHit.point);
                            GameObject target = Instantiate(forceFireTargetPrefab, candidateHit.point, Quaternion.Euler(90, 0, 0));
                            placementMarkers.Add(target);
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
                        item.aiTarget.transform.position = heldPosition;
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
                /*foreach (FormationPosition item in formList) //for each point 
                {
                    Vector3 tempPos = Vector3.zero;
                    float currentDistance = 99999;
                    foreach (Vector3 pos in lineFormationPosList) //get closest formation
                    {
                        float newDistance = Vector3.Distance(item.transform.position, pos);
                        if (newDistance < currentDistance)
                        {
                            currentDistance = newDistance;
                            tempPos = pos;
                        }
                    }
                    item.aiTarget.transform.position = tempPos; //tell closest formation to go there 
                    item.destinationsList.Clear();
                    item.destinationsList.Add(tempPos);
                    item.pathSet = true;
                    item.obeyingMovementOrder = true;
                    item.shouldRotateToward = false;
                    lineFormationPosList.Remove(tempPos); //so it can't be chosen again //if this becomes a problem then make another list
                }*/
            }
        }
    }
}
