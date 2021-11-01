using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    public GameObject cam;
    public Transform camForward;
    public Vector3 offset = Vector3.zero;

    private void Awake()
    {
        cam = GameObject.Find("Main Camera");
        camForward = cam.transform;
    }

    void FixedUpdate()
    {
        transform.LookAt(transform.position + camForward.forward + offset); //billboard script
        //transform.Rotate(transform.rotation.x, transform.rotation.y, -90);
    }
}
