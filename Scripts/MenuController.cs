using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    AsyncOperation loadingOperation;

    public Slider progressBar;

    public Canvas loadingScreen;

    public Canvas menuOptions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (loadingOperation != null)
        {
            progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            
            if (Input.GetMouseButtonDown(0))
            {
                loadingOperation.allowSceneActivation = true;
                Debug.Log("true");
            }

        }

    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void SwitchToFactionSelect()
    {
        SceneManager.LoadScene("FactionSelect");
    }
    public void LoadSinglePlayer()
    {

        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
        }
        if (menuOptions != null)
        {
            menuOptions.gameObject.SetActive(false);
        }
        loadingOperation = SceneManager.LoadSceneAsync("CampaignMap");
        loadingOperation.allowSceneActivation = false;
    }

    public void LoadSpecificScene(string name)
    {
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
        }
        loadingOperation = SceneManager.LoadSceneAsync(name);
        loadingOperation.allowSceneActivation = false;
    }

}
