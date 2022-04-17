using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        LeftClickCheck();
        RightClickCheck();
    }
    private void LeftClickCheck()
    {


        if (Input.GetMouseButtonDown(0))
        {

            startPos = Input.mousePosition;
            AttemptToSelectUnit();
        }
        if (Input.GetMouseButton(0)) //held
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseSelectionBox();
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
                selectedFormations.Add(form);
                form.SetSelected(true);
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
        LayerMask layerMask = LayerMask.GetMask("Formation");

        if (Physics.Raycast(ray, out RaycastHit hit, layerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Formation"))
            {
                if (!Input.GetKey(KeyCode.LeftShift)) //if not holding shift, deselect units
                {
                    DeselectUnits();
                }
                FormationPosition form = hit.transform.gameObject.GetComponent<FormationPosition>();

                form.SetSelected(!form.selected);
                if (form.selected)
                {
                    selectedFormations.Add(form);
                }
                else
                {
                    selectedFormations.Remove(form);
                }
            }
            else
            {
                DeselectUnits();
            }
        }
        else
        {
            DeselectUnits();
        }
    }
    private void DeselectUnits()
    {
        selectedFormations.Clear();
        foreach (FormationPosition form in yourFormations)
        {
            form.SetSelected(false);
        }
    }

    private void RightClickCheck()
    {
        if (Input.GetMouseButtonDown(1)) //set movepos
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) //ignores formation colliders
            {
                clickPosition = hit.point;
                clickPosition.y = 0;
                heldPosition = clickPosition;

                foreach (FormationPosition item in selectedFormations)
                {
                    Vector3 pos = item.transform.position;
                    item.lineRenderer2.enabled = true;
                    item.lineRenderer2.SetPosition(0, new Vector3(pos.x, 0, pos.z));
                    item.lineRenderer2.SetPosition(1, heldPosition);
                    item.lineRenderer2.SetPosition(2, heldPosition);
                    item.pathSet = false;
                }
            }
        }
        if (Input.GetMouseButton(1)) //update lines
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                clickPosition.y = 0;
                foreach (FormationPosition item in selectedFormations)
                { 
                    item.lineRenderer2.SetPosition(2, clickPosition);
                    item.rotTarget.position = clickPosition;
                }
            }
        }

        if (Input.GetMouseButtonUp(1)) //set rotation and confirm movement
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                foreach (FormationPosition item in selectedFormations)
                { 
                    item.aiTarget.transform.position = heldPosition;
                    //item.CheckDirectionOfMovement();

                    item.lineRenderer2.enabled = false;
                    item.pathSet = true;
                }
                clickPosition = hit.point;
                clickPosition.y = 0;
            }
        }
    }
}
