using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portalCamera : MonoBehaviour
{
    public Transform playerCam;
    public Transform portal;
    public Transform otherportal;


    // Update is called once per frame
    void Update()
    {
        Vector3 playerOffsetFromPortal = playerCam.position - otherportal.position;
        transform.position = portal.position + playerOffsetFromPortal;

        float angularDifferenceBetweeenPortalRotations = Quaternion.Angle(portal.rotation, otherportal.rotation); //get angular difference

        Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweeenPortalRotations, Vector3.up); //set axis 
        Vector3 newCameraDirection = portalRotationalDifference * playerCam.forward; //set new direction

        transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up); //actually apply direction
    }
}
