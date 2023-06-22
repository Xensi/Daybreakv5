using UnityEngine;

public class CamRotate : MonoBehaviour
{ 
    [SerializeField] private float originalRotation = 60;
    private float modifier = 100;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform parentTransform;

    [SerializeField] private float panSpeed = 20;
    [SerializeField] private float pan = 20;
    [SerializeField] private float scrollSpeed = 20f;
    [SerializeField] private int defaultSpeed = 5;
    [SerializeField] private float minY = 20f;
    [SerializeField] private Transform panCorner1;
    [SerializeField] private Transform panCorner2;
    [SerializeField] private Transform maxYPos;

    private float terrainHeightBelowUs = 0;

    [SerializeField] private Vector2 turn;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float radiusToEnableAnimations = 20;
    [SerializeField] private FightManager fightManager;


    Plane[] planes;
    [SerializeField] private Camera cam;
    private void Start()
    { 
        fightManager = FindObjectOfType<FightManager>().GetComponent<FightManager>();
        InvokeRepeating("FindFormationsNearMe", 0f, .5f);
        //InvokeRepeating("UpdateFarAwayIcons", 0.1f, 0.1f);

        Cursor.lockState = CursorLockMode.Confined;

        cameraTransform.localRotation = Quaternion.Euler(originalRotation, 0, 0); 
    }
    private void FindFormationsNearMe()
    {
        if (fightManager.allFormationsList.Count > 0)
        {
            for (int i = 0; i < fightManager.allArray.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, fightManager.allArray[i].formationPositionBasedOnSoldierModels);
                fightManager.allArray[i].distanceToCamera = distance;
            }
        } 
    }

    private void CheckVisibilityOfModelsInVisibleForms()
    {
        //Debug.Log("checking");
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        FormationPosition[] allForms = fightManager.allArray;
        for (int i = 0; i < allForms.Length; i++) //determine if formation is in our view frustum
        {
            FormationPosition form = allForms[i];
            if (form != null && form.cameraCollider != null)
            {
                if (GeometryUtility.TestPlanesAABB(planes, form.cameraCollider.bounds)) //in bounds, check distance
                {
                    float distance = Vector3.Distance(transform.position, form.transform.position);
                    float reqDistance = QualitySettings.lodBias * 100;
                    Color color = form.farAwayIcon.color;
                    float gradual = 1000;
                    //form.showSoldierModels = true;
                    if (distance > reqDistance)
                    {
                        float math = Mathf.Clamp((Mathf.Exp(distance - reqDistance) - 1) / gradual, 0, 1);
                        color.a = math;
                        if (math >= 1)
                        {
                            form.SetVisibleInFrustum(false);
                        }
                        else if (math > 0)
                        {
                            form.SetVisibleInFrustum(true);
                        }
                    }
                    else
                    {
                        color.a = 0;
                        form.SetVisibleInFrustum(true);
                    }
                    form.farAwayIcon.color = color;
                    form.frontIcon.color = color;
                    if (form.selectedSprite != null)
                    {
                        form.selectedSprite.color = color;
                    }
                }
                else //out of bounds, show form icon and hide soldiers
                {
                    Color color = form.farAwayIcon.color;
                    color.a = 1;
                    form.SetVisibleInFrustum(false);
                    form.farAwayIcon.color = color;
                    form.frontIcon.color = color;
                    if (form.selectedSprite != null)
                    {
                        form.selectedSprite.color = color;
                    }
                }
            }
            
        } 
    } 
    private void Update()
    {
        //CheckVisibilityOfModelsInVisibleForms(); 

        UpdateTerrainHeightValue();
        if (Input.GetKey("left"))
        {
            turn.x -= modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            cameraTransform.localRotation = Quaternion.Euler(0, turn.x, 0);
        }
        if (Input.GetKey("right"))
        {
            turn.x += modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            cameraTransform.localRotation = Quaternion.Euler(0, turn.x, 0);
        } 
        //UpdateFarAwayIcons();
        Strafe();
        if (Input.GetKey("up"))
        {
            turn.y += modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            cameraTransform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);
        }
        if (Input.GetKey("down"))
        {
            turn.y -= modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            cameraTransform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);
        }
        if (Input.GetMouseButton(2))
        {
            turn.y += Input.GetAxis("Mouse Y") * sensitivity;
            turn.x += Input.GetAxis("Mouse X") * sensitivity;

            cameraTransform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);
            parentTransform.localRotation = Quaternion.Euler(0, turn.x + originalRotation, 0);

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        { 
            Cursor.lockState = CursorLockMode.Confined;
        }
        UpdateBanners();
    }
    private void UpdateTerrainHeightValue()
    {
        LayerMask layerMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        Vector3 vec = new Vector3(transform.position.x, 100, transform.position.z);
        if (Physics.Raycast(vec, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            terrainHeightBelowUs = hit.point.y;
        }
    }
    private void UpdateBanners()
    {
        if (fightManager.allFormationsList.Count > 0)
        {
            foreach (FormationPosition form in fightManager.allArray)
            {
                if (form.formationIconsParent != null && form.formationIconsParent.activeInHierarchy)
                {  
                    form.formationIconsParent.transform.forward = -transform.forward;
                }

                float distance = Vector3.Distance(transform.position, form.transform.position);
                float reqDistance = 40;
                Color color = Color.white;
                float gradual = 1000; 
                if (distance > reqDistance)
                {
                    float math = Mathf.Clamp((Mathf.Exp(distance - reqDistance) - 1) / gradual, 0, 1);
                    color.a = math;

                }
                else
                {
                    color.a = 0;
                }
                form.farAwayIcon.color = color; 
            }
        }
    }
    private void UpdateFarAwayIcons()
    {
        if (fightManager.allFormationsList.Count > 0)
        {
            foreach (FormationPosition form in fightManager.allArray)
            {
                float distance = Vector3.Distance(transform.position, form.transform.position);
                float reqDistance = 75;
                Color color = form.farAwayIcon.color;
                float gradual = 1000;
                //form.showSoldierModels = true;
                if (distance > reqDistance)
                {
                    float math = Mathf.Clamp((Mathf.Exp(distance - reqDistance) - 1) / gradual, 0, 1);
                    color.a = math; 

                }
                else
                {
                    color.a = 0; 
                }
                form.farAwayIcon.color = color;
                if (form.selectedSprite != null)
                {
                    form.selectedSprite.color = color;
                }
            }
        }
    } 
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radiusToEnableAnimations);
    }
    private void Strafe()
    {
        Vector3 pos = parentTransform.position;

        float mod = 1;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            mod = 2;
        }
        pan = panSpeed * Mathf.Sqrt(pos.y + defaultSpeed) * 0.1f * mod;

        if (Input.GetKey("w"))
        {
            //Debug.Log("TEST");
            pos += parentTransform.forward * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("s"))
        {
            pos -= parentTransform.forward * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("d"))
        {
            pos += parentTransform.right * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("a"))
        {
            pos -= parentTransform.right * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        } 
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        var speed = scroll * scrollSpeed * Time.deltaTime * Mathf.Sqrt(pos.y + defaultSpeed) / 2;

        float modifier = .5f;
        if (Input.GetKey(KeyCode.Space))
        {
            speed -= Time.deltaTime * scrollSpeed * modifier;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed += Time.deltaTime * scrollSpeed * modifier;
        }
        pos.y -= speed;

        pos.x = Mathf.Clamp(pos.x, panCorner1.position.x, panCorner2.position.x);
        pos.y = Mathf.Clamp(pos.y, terrainHeightBelowUs + 1, maxYPos.position.y);
        pos.z = Mathf.Clamp(pos.z, panCorner1.position.z, panCorner2.position.z);

        //if close to ground lower scroll speed?

        parentTransform.position = pos; //sets position equal to updated position  
    }
}
