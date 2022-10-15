using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public static Helper Instance { get; private set; }

    public float GetSquaredMagnitude(Vector3 a, Vector3 b)
    {
        Vector3 diff = a - b;
        return diff.sqrMagnitude;
    }
    private void Awake()
    {
        Instance = this;
    }
}
