using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [HideInInspector] public float multiplierDamage = 1;
    public enum BodyType
    {
        Head,
        Body
    }
    public BodyType type = BodyType.Head;

    private void Start()
    {
        switch (type)
        {
            case BodyType.Head:
                multiplierDamage = 2;
                break;
            case BodyType.Body:
                multiplierDamage = 1;
                break;
            default:
                break;
        }
    }
}
