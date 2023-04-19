using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScreen : MonoBehaviour
{
    public string sceneToLoad = "Overworld";
    AsyncOperation loadingOperation;
    AsyncOperation loadOp2;
    public Slider progressBar;

    public CanvasGroup canvasGroup;

    public MainMenu menu;
    public int levelLoad = 1;
    private bool finishedLoad = false;
     
    // Start is called before the first frame update
    void Start()
    {
        menu = FindObjectOfType<MainMenu>();
        if (menu != null)
        {
            levelLoad = menu.levelLoad;
            loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            //loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad); //load overworld, of course  
            loadOp2 = SceneManager.LoadSceneAsync("Level" + levelLoad, LoadSceneMode.Additive); //load in units in preset spots
            menu.ui.SetActive(false);



            //must be last
            SceneManager.UnloadSceneAsync("MainMenu");
        }
    }
    void Update()
    {
        progressBar.value = Mathf.Clamp01((loadingOperation.progress + loadOp2.progress) / 0.9f); //
        if (loadingOperation.progress >= 1 && !finishedLoad) //&& loadOp2.progress >= 1 
        {
            finishedLoad = true;

            LevelManager.Instance.NoteLevel(levelLoad);
            //FightManager fightmanager = FindObjectOfType<FightManager>();
            //FightManager.Instance.UpdateAllFormArrayAndStartAIToBeginBattle();



            SceneManager.UnloadSceneAsync("LoadGame"); //remove loading screen

            if (menu.loadSavedGame)
            {
                GameDataManager.Instance.readFile();
            }
        }
    }
    IEnumerator FadeLoadingScreen(float duration, float startValue = 0, float endValue = 1)
    { 
        float time = 0;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endValue;
    }
}
