using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablesManager : MonoBehaviour
{
    public AttackableObject[] interactables; //put armories here

    public static InteractablesManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        //interactables = FindObjectsOfType<AttackableObject>();
    }
}
