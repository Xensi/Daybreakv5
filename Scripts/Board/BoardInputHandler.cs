using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInputHandler : MonoBehaviour, IInputHandler
{

    private Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }
    public void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action callback, int mouse)
    {
        //Debug.Log("Selected");
        board.OnSquareSelected(inputPosition, mouse);
    }
}
