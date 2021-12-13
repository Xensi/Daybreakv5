using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionChooserController : MonoBehaviour
{
    [TextArea(3, 100)]
    public string[] descriptionTexts;
    [TextArea(3, 100)]
    public string[] gameplayTexts;

    public GameObject descriptionBox;

    public GameObject gameplayBox;

    public TMP_Text descriptionText;
    public TMP_Text gameplayText;

    public MenuController menuController;

    public int faction = 999;

    // Start is called before the first frame update
    void Start()
    {
        descriptionBox.SetActive(false);
        gameplayBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void enableBoxes()
    {
        descriptionBox.SetActive(true);
        gameplayBox.SetActive(true);
    }

    public void SelectFaction()
    {
        if (faction == 999)
        {
            Debug.Log("No faction selected");
        }
        else if (faction == 0)
        {

            menuController.LoadSinglePlayer();
        }
        else
        {
            Debug.Log("ERROR FACTION NOT IMPLEMENTED LMAO");
        }
    }


    public void ClickOnAltgard()
    {
        enableBoxes();
        descriptionText.text = descriptionTexts[0];
        gameplayText.text = gameplayTexts[0];
        faction = 0;

    }
    public void ClickOnZhanguo()
    {
        enableBoxes();
        descriptionText.text = descriptionTexts[1];
        gameplayText.text = gameplayTexts[1];
        faction = 1;

    }
    public void ClickOnWarborn()
    {
        enableBoxes();
        descriptionText.text = descriptionTexts[2];
        gameplayText.text = gameplayTexts[2];
        faction = 2;

    }
    public void ClickOnUruum()
    {
        enableBoxes();
        descriptionText.text = descriptionTexts[3];
        gameplayText.text = gameplayTexts[3];
        faction = 3;

    }

}
