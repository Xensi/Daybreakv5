using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float panSpeed = 20;
    public float pan = 20;
    public Vector2 panLimit;
    public Vector2 turn;
    public float sensitivity = 1;
    public float scrollSpeed = 20f;
    public float minY = 20f;
    public float maxY = 120f;
    public Rigidbody body;
    public Collider collider;
    public float distToGround;
    // Start is called before the first frame update
    void Start()
    {
        distToGround = collider.bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {

            turn.x += Input.GetAxis("Mouse X") * sensitivity;
            transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        }
        Strafe();
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Unit") //units layer
        {

            //Debug.Log("Other" + other);
            var piece = other.GetComponent<Piece>();
            foreach (var item in piece.soldierObjects)
            {
                if (item != null)
                {
                    var updater = item.GetComponent<UpdateAgentDestination>();
                    if (updater != null)
                    {
                        updater.animationsEnabled = true;
                    }
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Unit") //units layer
        {

            //Debug.Log("Other" + other);
            var piece = other.GetComponent<Piece>();
            foreach (var item in piece.soldierObjects)
            {
                if (item != null)
                {
                    var updater = item.GetComponent<UpdateAgentDestination>();
                    if (updater != null)
                    {
                        updater.animationsEnabled = true;
                    }
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Unit") //units layer
        {

            //Debug.Log("Other" + other);
            var piece = other.GetComponent<Piece>();
            foreach (var item in piece.soldierObjects)
            {
                if (item != null)
                {
                    var updater = item.GetComponent<UpdateAgentDestination>();
                    if (updater != null)
                    {
                        updater.animationsEnabled = false;
                    }
                }
            }
        }
    }

    void Strafe()
    {


        Vector3 pos = transform.position;

        pan = panSpeed * Mathf.Sqrt(pos.y) * 0.1f;

        if (Input.GetKey("w"))
        {
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
        var speed = scroll * scrollSpeed * 1f * Time.deltaTime * Mathf.Sqrt(pos.y)/2;
        pos.y -= speed;


        pos.x = Mathf.Clamp(pos.x, -panLimit.x / 2, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y / 2, panLimit.y);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        
        //if close to ground lower scroll speed?

        transform.position = pos; //sets position equal to updated position


        if (scroll == 0 && !IsGrounded())
        {
            body.velocity = new Vector3(0, 0, 0);
        }
    }

}
