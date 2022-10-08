using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBattleManager : MonoBehaviour
{
    [SerializeField] private GameObject FieldBattleParent;
    [SerializeField] private GameObject OverworldParent;
    [SerializeField] private GameObject MenuParent;
    [SerializeField] private GameObject PauseParent;


    private bool paused = false;

    private enum gameState
    {
        Menu,
        Game
    }
    private gameState state = gameState.Menu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        switch (state)
        {
            case gameState.Menu:
                break;
            case gameState.Game:
                if (Input.GetKeyDown("escape"))
                {
                    if (paused)
                    {
                        UnpauseGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
                break;
            default:
                break;
        }
        
    }
    void PauseGame()
    {
        Time.timeScale = 0;
        paused = true;
        PauseParent.SetActive(true);
    }
    void UnpauseGame()
    {
        Time.timeScale = 1;
        paused = false;
        PauseParent.SetActive(false);
    }
    public void StartFieldBattle()
    {
        state = gameState.Game;
        FieldBattleParent.SetActive(true);
        OverworldParent.SetActive(false);
        MenuParent.SetActive(false);
    }
}
