using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGameOnStart : MonoBehaviour
{

    public GameInitializer gameInit;
    public ChessUIManager chessUI;
    

    
    // Start is called before the first frame update
    void Start()
    {
        gameInit.CreateSinglePlayerBoard();
        chessUI.OnSingleplayerModeSelected();
        gameInit.InitializeSinglePlayerController();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
