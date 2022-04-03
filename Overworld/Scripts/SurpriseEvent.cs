using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurpriseEvent : MonoBehaviour
{
    [SerializeField] private Collider surpriseTrigger;
    public bool eventTriggered = false;
    public DialogueScriptableObject eventDialogue;
}
