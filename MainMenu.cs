using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public int team = 0; //altgard, zhanguo
    public string sceneToLoad = "LoadGame";
    public int levelLoad = 1;
    public GameObject ui;
    public bool loadSavedGame = false;
    // Start is called before the first frame update
    void Start()
    { 
        //DontDestroyOnLoad(gameObject);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }
    public void LoadGame()
    {
        loadSavedGame = true;
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }
    public void ChooseLevel(int level)
    { 
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
        levelLoad = level; 
    }
    public void ExitGame()
    { 
        Application.Quit();
    }
}
