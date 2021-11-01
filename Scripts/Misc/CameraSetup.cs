using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    public CameraMover cameraMover;

    public void SetupCamera(TeamColor team)
    {
        if(team == TeamColor.Black)
        {
            FlipCamera();
        }
    }

    private void FlipCamera()
    {
        /*    mainCamera.transform.position = new Vector3
            (
                mainCamera.transform.position.x,
                mainCamera.transform.position.y,
                6.43f);
            mainCamera.transform.Rotate(Vector3.up, 180f, Space.World);*/
        cameraMover.turn.x = 180; 
        cameraMover.transform.localRotation = Quaternion.Euler(0, cameraMover.turn.x, 0);
    }
}
