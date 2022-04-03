using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DaylightManager : MonoBehaviour
{
    [SerializeField, Range(0, 24)] private int timeHour = 0;
    [SerializeField] private Light sun;

    private void Update()
    {
        ChangeTimeOfDay(timeHour / 24f);
    }

    public void ChangeTimeOfDay(float timePercent) //in terms of hours
    {
        sun.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170, 0));
    }
}

