using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMatSettingsOnEnable : MonoBehaviour
{
    public Material dissolve;
    private float value = 0;
    public float increment = 0.005f;
    public float end;
    private void Start()
    {
        dissolve.SetFloat("ManualTime", 0f);

    }
    private void OnEnable()
    {
        StartCoroutine(FloatSetter());   
    }

    private void OnDisable()
    {

        value = 0;
        dissolve.SetFloat("ManualTime", 0f);
    }

    public IEnumerator FloatSetter()
    {
        value += increment;
        dissolve.SetFloat("ManualTime", value);
        yield return new WaitForSeconds(.01f);
        StartCoroutine(FloatSetter());
    }
}
