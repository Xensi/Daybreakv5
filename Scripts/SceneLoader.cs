using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;
    public AsyncOperation loadOperation;
    private bool loadScene = false;
    private void Start()
    {
        // Load the next scene.
        //var currentScene = SceneManager.GetActiveScene();
        //loadOperation = SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);
        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        // Don't active the scene when it's fully loaded, let the progress bar finish the animation.
        // With this flag set, progress will stop at 0.9f.
        loadOperation.allowSceneActivation = false;
    }
    public void EnableScene()
    {
        if (loadOperation != null)
        {

            loadOperation.allowSceneActivation = true;
        }
    }
}
