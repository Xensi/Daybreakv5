using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OverworldCam : MonoBehaviour
{
    public float panSpeed = 20;
    public float pan = 20;
    public Vector2 panLimit;
    public Vector2 turn;
    public float sensitivity = 1;
    public float scrollSpeed = 20f;
    public float minY = 20f;
    public float maxY = 120f;
    public float distToGround;
    public int defaultSpeed = 5;
    public Camera cam;
    public float minZoom = 0.01f;
    public float maxZoom = 100;
    // Start is called before the first frame update 
    public CinemachineVirtualCamera cinemachine;
    public virtual void Start()
    {
    }

    // Update is called once per frame
    public virtual void Update()
    {
        Strafe();
    }
    public virtual void Strafe()
    {

        Vector3 pos = transform.position;

        pan = panSpeed * Mathf.Sqrt(pos.y+ defaultSpeed) * 0.1f;

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
        var speed = scroll * scrollSpeed * 1f * Time.deltaTime * Mathf.Sqrt(pos.y+defaultSpeed) / 2;
        //pos.y -= speed;
        cinemachine.m_Lens.OrthographicSize -= speed;
        if (cinemachine.m_Lens.OrthographicSize < minZoom)
        {
            cinemachine.m_Lens.OrthographicSize = minZoom;
        }
        else if (cinemachine.m_Lens.OrthographicSize > maxZoom)
        {
            cinemachine.m_Lens.OrthographicSize = maxZoom;
        }



        pos.x = Mathf.Clamp(pos.x, -panLimit.x / 2, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y / 2, panLimit.y);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        //if close to ground lower scroll speed?

        transform.position = pos; //sets position equal to updated position

    }

}
