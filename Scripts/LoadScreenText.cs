using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadScreenText : MonoBehaviour
{
    public TMP_Text quoteText;
    [TextArea(3, 100)]
    public string[] quotes;


    void Start()
    {
        //Text sets your text to say this message
        //m_MyText.text = "This is my text";

        ChangeText();


    }

    void Update()
    {
        //Press the space key to change the Text message
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeText();
        }
    }

    void ChangeText()
    {
        int random = Random.Range(1, quotes.Length);
        quoteText.text = quotes[random];
    }
}