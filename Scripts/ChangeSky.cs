using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSky : MonoBehaviour
{
    public Material skybox;  // assign via inspector

    public void OnEnable()
    {
        RenderSettings.skybox = skybox;
    }
}
