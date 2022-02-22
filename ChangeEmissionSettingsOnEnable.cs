using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeEmissionSettingsOnEnable : MonoBehaviour
{
    public Material materialWithEmission;
    //private float value = 0;
    private float increment = .5f;
    //public float end;

    public Color startEmissionColor = new Color(3, 191, 0, 0);

    private float value = 7f;
    private float defaultValue = 7f;
    private float maxValue = 25f;
    private void Start()
    {
        materialWithEmission.SetColor("_EmissionColor", startEmissionColor * value);

    }
    private void OnEnable()
    {
        //materialWithEmission.SetColor("_EmissionColor", startEmissionColor * (value+9));
        StartCoroutine(FloatSetter());
    }

    private void OnDisable()
    {
        materialWithEmission.SetColor("_EmissionColor", startEmissionColor * value);
    }

    public IEnumerator FloatSetter()
    {

        //float emission = Mathf.PingPong(Time.time, 6.3f);
        //Color finalColor = startEmissionColor * Mathf.LinearToGammaSpace(emission);
        //materialWithEmission.SetColor("_EmissionColor", finalColor);
        value += increment;
        materialWithEmission.SetColor("_EmissionColor", startEmissionColor * value);
        if (value > maxValue)
        {
            yield return new WaitForSeconds(.01f);
            StartCoroutine(FloatDesetter());
        }
        else
        {
            yield return new WaitForSeconds(.001f);
            StartCoroutine(FloatSetter());
        }
        //yield return new WaitForSeconds(.01f);
        //StartCoroutine(FloatSetter());
    }

    public IEnumerator FloatDesetter()
    {
        value -= increment;

        materialWithEmission.SetColor("_EmissionColor", startEmissionColor * value);
        if (value <= defaultValue)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(.01f);
            StartCoroutine(FloatDesetter());
        }
    }
}
