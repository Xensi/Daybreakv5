using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputReceiver : MonoBehaviour
{
    protected IInputHandler[] inputHandlers;

    public abstract void OnInputReceived(int mouse);

    private void Awake()
    {
        inputHandlers = GetComponents<IInputHandler>();
    }
}
