using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneOnEnable : MonoBehaviour
{
    public SceneLoader sceneLoader;

    private void OnEnable()
    {
        sceneLoader.EnableScene();
    }
}
