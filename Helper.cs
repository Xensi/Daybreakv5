using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public static Helper Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public float GetSquaredMagnitude(Vector3 a, Vector3 b)
    {
        Vector3 diff = a - b;
        return diff.sqrMagnitude;
    }
    public Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
