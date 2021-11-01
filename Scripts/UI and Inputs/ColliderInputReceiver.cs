using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInputReceiver : InputReceiver
{
    private Vector3 clickPosition;
    private ChessUIManager UIManager;
    private void Start()
    {
        var UI = GameObject.Find("UI"); //fetch ui manager so we can access uihover variable
        UIManager = UI.GetComponent(typeof(ChessUIManager)) as ChessUIManager;
    }

    private void Update()
    {
        if (!UIManager.UIHover) //not hovering over ui?
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                
                if (Physics.Raycast(ray, out hit))
                {
                    clickPosition = hit.point;
                    OnInputReceived(0);
                }
            }
            if (Input.GetMouseButtonDown(1)) //right click functionality
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    clickPosition = hit.point;
                    OnInputReceived(1);
                }
            }
        }

        //Debug.DrawLine(Camera.main.transform.position, clickPosition);

    }


    public override void OnInputReceived(int mouse)
    {
        foreach(var handler in inputHandlers)
        {
            handler.ProcessInput(clickPosition, null, null, mouse);
        }
    }


}
