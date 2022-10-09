using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneToLoad = "LoadGame";
    public int levelLoad = 1;
    public GameObject ui;
    // Start is called before the first frame update
    void Start()
    { 
        DontDestroyOnLoad(gameObject);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }
    public void ChooseLevel(int level)
    { 
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
        levelLoad = level; 
    }
}
