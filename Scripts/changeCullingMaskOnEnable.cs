using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeCullingMaskOnEnable : MonoBehaviour
{
    public Camera mainCam;
    private void OnEnable()
    {
        mainCam.cullingMask |= 1 << LayerMask.NameToLayer("Altgard");
    }
}
