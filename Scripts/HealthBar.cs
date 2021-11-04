using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Piece piece;
    public string hpType = "model";
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image teamColor;
    private GameInitializer gameInit;
    public bool isFloat = false;
    public void Awake()
    {
        if (gameInit == null)
        {
            var game = GameObject.Find("GameInitializer");
            gameInit = game.GetComponent(typeof(GameInitializer)) as GameInitializer;
            //Debug.Log("game init set");
        }
        if (hpType == "model")
        {
            SetMaxHealth(piece.models);
        }
        else if (hpType == "morale")
        {
            SetMaxHealth(piece.morale);

        }
        else if (hpType == "energy")
        {
            SetMaxHealth(piece.energy);

        }
    }

    public void Start()
    {

        if (hpType == "model")
        {
            
            int num = 0;
            foreach (var color in gameInit.teamColorDefinitions)
            {
                if (piece.team == color)
                {
                    teamColor.material = gameInit.unlitTeamUI[num];
                    break;
                }
                num++;
            }
        }
    }
    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void Update()
    {
        if (!isFloat)
        {
            hpText.text = Mathf.RoundToInt(slider.value).ToString();
        }
        else
        {

            var f = Mathf.Round(slider.value * 100.0f) * 0.01f;
            hpText.text = f.ToString();
        }
    }

    public void SetHealth(float health)
    {
        //slider.value = health;
        if (!piece.isCampaignToken && !piece.isCampaignObjective) //if it's a normal piece
        {
            StartCoroutine(TweenHP(health));
        }

    }
    private IEnumerator TweenHP(float health)
    {
        var tween = DOTween.To(() => slider.value, x => slider.value = x, health, 6);
        yield return new WaitForSeconds(tween.Duration());

        if(health <= 0 && hpType == "model")
        {
            piece.allowedToDie = true;
            piece.StartCoroutine(piece.SelfDestruct());
        }
        if (health <= 0 && hpType == "morale") //self destruct when morale hit 0 for now
        {
            piece.allowedToDie = true;
            piece.StartCoroutine(piece.SelfDestruct());
        }

    }
}
