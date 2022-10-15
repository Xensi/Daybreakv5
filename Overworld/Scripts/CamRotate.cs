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


    private void Start()
    {
        fightManager = FindObjectOfType<FightManager>().GetComponent<FightManager>();
        InvokeRepeating("FindFormationsNearMe", 1f, 1f);
        //InvokeRepeating("UpdateFarAwayIcons", 0.1f, 0.1f);

        Cursor.lockState = CursorLockMode.Confined;

        cameraTransform.localRotation = Quaternion.Euler(originalRotation, 0, 0);
    }
    private void Update()
    {
        #region ExperimentalCamera
        /*if (Input.GetKey(KeyCode.Equals))
        {
            CinemachineShake.Instance.cineVirCam.LookAt = null;
            CinemachineShake.Instance.cineVirCam.transform.rotation = Quaternion.identity;

            if (CinemachineShake.Instance.cineVirCam.Follow == null)
            {
                if (FightManager.Instance.selectedFormations.Count == 1)
                {
                    CinemachineShake.Instance.cineVirCam.Follow = FightManager.Instance.selectedFormations[0].transform;
                }
            }
            else
            {
                CinemachineShake.Instance.cineVirCam.Follow = null;
                CinemachineShake.Instance.cineVirCam.transform.position = Vector3.zero;
            }
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            CinemachineShake.Instance.cineVirCam.Follow = null;
            CinemachineShake.Instance.cineVirCam.transform.position = Vector3.zero;

            if (CinemachineShake.Instance.cineVirCam.LookAt == null)
            {
                if (FightManager.Instance.selectedFormations.Count == 1)
                {
                    CinemachineShake.Instance.cineVirCam.LookAt = FightManager.Instance.selectedFormations[0].transform;
                }
            }
            else
            {
                CinemachineShake.Instance.cineVirCam.LookAt = null;
                CinemachineShake.Instance.cineVirCam.transform.rotation = Quaternion.identity;
            }
        }*/
        #endregion

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
        UpdateFarAwayIcons();
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
    private void UpdateFarAwayIcons()
    {
        if (fightManager.allFormations.Count > 0)
        {
            foreach (FormationPosition form in fightManager.allArray)
            {
                float distance = Vector3.Distance(transform.position, form.transform.position);
                float reqDistance = 75;
                Color color = form.farAwayIcon.color;
                float gradual = 1000;
                form.showSoldierModels = true;
                if (distance > reqDistance)
                {
                    float math = Mathf.Clamp((Mathf.Exp(distance - reqDistance) - 1) / gradual, 0, 1);
                    color.a = math;
                    if (math >= 1)
                    {
                        form.showSoldierModels = false;
                    }

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
    private void FindFormationsNearMe()
    {
        if (fightManager.allFormations.Count > 0)
        {
            foreach (FormationPosition item in fightManager.allArray)
            {
                item.enableAnimations = false;
            }
        }
        LayerMask layerMask = LayerMask.GetMask("Formation");
        //int layerMask = 1 << 23; //layer 23 formations
        int maxColliders = 10;

        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, radiusToEnableAnimations, hitColliders, layerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < numColliders; i++)
        {
            FormationPosition form = hitColliders[i].gameObject.GetComponentInParent<FormationPosition>();
            if (form != null)
            {
                form.enableAnimations = true;
            }
        }
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
