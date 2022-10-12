using UnityEngine;

public class CamRotate : MonoBehaviour
{
    [SerializeField] private Vector2 turn;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float originalRotation = 60;
    private float modifier = 100;
/*    public void ToggleTilesOnOff()
    {

        cam.cullingMask ^= 1 << LayerMask.NameToLayer("Tiles");
    }
*/    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        transform.localRotation = Quaternion.Euler(originalRotation, 0, 0);
    }

    private void Update()
    {
        if (Input.GetKey("up"))
        {
            turn.y += modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            transform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);
        }
        if (Input.GetKey("down"))
        {
            turn.y -= modifier * sensitivity * Time.deltaTime; //multipying by delta time keeps movement consistent
            transform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);
        }
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
