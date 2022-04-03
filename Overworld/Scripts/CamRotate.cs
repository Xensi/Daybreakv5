using UnityEngine;

public class CamRotate : MonoBehaviour
{
    [SerializeField] private Vector2 turn;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float originalRotation = 60;
/*    public void ToggleTilesOnOff()
    {

        cam.cullingMask ^= 1 << LayerMask.NameToLayer("Tiles");
    }
*/    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (Input.GetMouseButton(2))
        {

            turn.y += Input.GetAxis("Mouse Y") * sensitivity;
            transform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);


            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {

            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
