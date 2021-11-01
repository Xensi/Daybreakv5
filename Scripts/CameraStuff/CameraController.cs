using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float panBorderThickness = 10f;


    public Vector2 turn;
    public float sensitivity = 1;
    public float originalRotation = 60;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void Update()
    {


        if (Input.GetMouseButton(1)){

            turn.y += Input.GetAxis("Mouse Y") * sensitivity;
            transform.localRotation = Quaternion.Euler(-turn.y + originalRotation, 0, 0);


            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {

            Cursor.lockState = CursorLockMode.Confined;
        }


        /*if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += panSpeed * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime; //multipying by delta time keeps movement consistent
        }
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime; //multipying by delta time keeps movement consistent
        }*/

    }
}
