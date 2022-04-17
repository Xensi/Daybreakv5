using UnityEngine;

public class FieldBattleCam : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20;
    [SerializeField] private float pan = 20; 
    [SerializeField] private Vector2 turn;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float scrollSpeed = 20f; 
    [SerializeField] private int defaultSpeed = 5;  
    [SerializeField] private float minY = 20f;
    [SerializeField] private Transform panCorner1;
    [SerializeField] private Transform panCorner2;
    [SerializeField] private Transform maxYPos;
    [SerializeField] private float radiusToEnableAnimations = 20;
    [SerializeField] private FightManager fightManager;

    private void Start()
    {
        InvokeRepeating("FindFormationsNearMe", .5f, .5f);
    }
    private void Update()
    {
        if (Input.GetMouseButton(2))
        {

            turn.x += Input.GetAxis("Mouse X") * sensitivity;
            transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        }
        Strafe();
    }
    private void Strafe()
    {

        Vector3 pos = transform.position;

        pan = panSpeed * Mathf.Sqrt(pos.y + defaultSpeed) * 0.1f;

        if (Input.GetKey("w"))
        {
            //Debug.Log("TEST");
            pos += transform.forward * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("s"))
        {
            pos -= transform.forward * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("d"))
        {
            pos += transform.right * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("a"))
        {
            pos -= transform.right * pan * Time.deltaTime; //multipying by delta time keeps movement consistent
        }


        float scroll = Input.GetAxis("Mouse ScrollWheel");
        var speed = scroll * scrollSpeed * 1f * Time.deltaTime * Mathf.Sqrt(pos.y + defaultSpeed) / 2;
        pos.y -= speed;

        pos.x = Mathf.Clamp(pos.x, panCorner1.position.x, panCorner2.position.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxYPos.position.y);
        pos.z = Mathf.Clamp(pos.z, panCorner1.position.z, panCorner2.position.z);

        //if close to ground lower scroll speed?

        transform.position = pos; //sets position equal to updated position
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radiusToEnableAnimations);
    }
    private void FindFormationsNearMe()
    {
        foreach (FormationPosition item in fightManager.allFormations)
        {
            item.enableAnimations = false;
        }
        int layerMask = 1 << 23; //layer 23 formations
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radiusToEnableAnimations, layerMask, QueryTriggerInteraction.Ignore);
        foreach (Collider hitCollider in hitColliders)
        { 
            FormationPosition form = hitCollider.gameObject.GetComponent<FormationPosition>();
            form.enableAnimations = true;
        }
    }
}
