using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.EventSystems;// Required when using Event data.

public class ChessUIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scene Dependencies")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button whiteTeamButton;
    [SerializeField] private Button blackTeamButton;


    [Header("Texts")]
    [SerializeField] private TextMeshPro resultText;
    [SerializeField] private TMP_Text connectionStatusText;


    [Header("ScreenGameObjects")]
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private GameObject teamSelectionScreen;
    [SerializeField] private GameObject gameModeSelectionScreen;
    [SerializeField] private GameObject unitInfoScreen;

    [Header("Other UI")]
    [SerializeField] private TMP_Dropdown gameLevelSelection;

    public Texture2D errorTexture;
    public Texture2D cursorTexture;
    public Texture2D actionSelector;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;


    public bool UIHover = false;

    public bool matchStarted = false;


    public void OnPointerEnter(PointerEventData eventData)
    {
        UIHover = true;
        //Debug.Log("UIhover true");


        var name = eventData.pointerEnter.transform.parent.name;

        if (name == "UnitInfoScreen")// || name == "GamePlayScreen" || name == "Execute" || name == "ActionDropDown" ||
        {
            Cursor.SetCursor(errorTexture, hotSpot, cursorMode);
        }
        else
        {
            Cursor.SetCursor(actionSelector, hotSpot, cursorMode);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        UIHover = false;
        //Debug.Log("UIhover false");
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    private void Awake()
    {
        gameLevelSelection.AddOptions(Enum.GetNames(typeof(ChessLevel)).ToList());
        OnGameLaunched();
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

    }
    private void Start()
    {
        
    }

    private void OnGameLaunched()
    {
        DisableAllScreens();
        gameModeSelectionScreen.SetActive(true);
    }

    public void OnSingleplayerModeSelected()
    {
        DisableAllScreens();
    }

    public void OnMultiplayerModeSelected()
    {
        connectionStatusText.gameObject.SetActive(true);
        DisableAllScreens();
        connectScreen.SetActive(true);
    }
    private void DisableAllScreens()
    {

        gameoverScreen.SetActive(false);
        connectScreen.SetActive(false);
        teamSelectionScreen.SetActive(false);
        gameModeSelectionScreen.SetActive(false);
        unitInfoScreen.SetActive(false);
    }
    public void ShowUnitInfoScreen(Piece piece)
    {
        unitInfoScreen.SetActive(true);
        var name = GameObject.Find("NameText");
        var nameText = name.GetComponent<TMP_Text>();
        nameText.text = piece.unitName;
        var attack = GameObject.Find("AttackText");
        var attackText = attack.GetComponent<TMP_Text>();
        attackText.text = "Attack: " + piece.damage;

        var damageLevel = GameObject.Find("DamageLevelText");
        var damageLevelText = damageLevel.GetComponent<TMP_Text>();
        string dmglvl = "Unarmored";
        if(piece.damageLevel == 1)
        {
            dmglvl = "Light";
        }
        else if (piece.damageLevel == 2)
        {
            dmglvl = "Medium";
        }
        else if (piece.damageLevel == 3)
        {
            dmglvl = "Heavy";
        }
        else if (piece.damageLevel == 4)
        {
            dmglvl = "Superheavy";
        }
        damageLevelText.text = "Good against: " + dmglvl;

        var armor = GameObject.Find("ArmorText");
        var armorText = armor.GetComponent<TMP_Text>();

        string armrlvl = "Unarmored";
        if (piece.armorLevel == 1)
        {
            armrlvl = "Light";
        }
        else if (piece.armorLevel == 2)
        {
            armrlvl = "Medium";
        }
        else if (piece.armorLevel == 3)
        {
            armrlvl = "Heavy";
        }
        else if (piece.armorLevel == 4)
        {
            armrlvl = "Superheavy";
        }


        armorText.text = "Armor Level: " + armrlvl;
        var speed = GameObject.Find("SpeedText");
        var speedText = speed.GetComponent<TMP_Text>();
        speedText.text = "Speed: " + piece.originalSpeed;
        var hp = GameObject.Find("HPText");
        var hpText = hp.GetComponent<TMP_Text>();
        hpText.text = "HP/model: " + piece.health;
        var range = GameObject.Find("RangeText");
        var rangeText = range.GetComponent<TMP_Text>();
        rangeText.text = "Effective Range: " + piece.effectiveRange;

        var maxRange = GameObject.Find("MaxRangeText");
        var maxRangeText = maxRange.GetComponent<TMP_Text>();
        maxRangeText.text = "Max Range: " + piece.longRange;

        var effects = GameObject.Find("EffectsText");
        var effectsText = effects.GetComponent<TMP_Text>();
        effectsText.text = "Effects: ";

        if (!piece.moveAndAttackEnabled && piece.attacking && piece.attackType == "ranged") //if steady attacking
        {
            effectsText.text += "\nSteady attacking: + 1 range";
        }
        if (piece.OnTerrainType != "hill") //if not on hill
        {
            effectsText.text += "\nMoving uphill causes this to stop.";

            if (piece.unitType == "infantry")
            {
                effectsText.text += "\n-1 damage attacking enemies on hills.";
            }
            else if (piece.unitType == "cavalry")
            {
                effectsText.text += "\n-2 damage attacking enemies on hills.";
            }
        }
        else if (piece.OnTerrainType == "hill") //if on hill
        {
            effectsText.text += "\nHill: + 1 range";

            if (piece.attackType == "melee" && piece.unitType == "infantry")
            {
                effectsText.text += "\nHigh ground: +1 damage attacking enemies on non-hills.";
            }
            else if (piece.attackType == "melee" && piece.unitType == "cavalry")
            {
                effectsText.text += "\nHigh ground: +2 damage attacking enemies on non-hills.";
            }
            if (!piece.arcingAttack && piece.attackType == "ranged")
            {
                effectsText.text += "\nHigh ground: +1 damage attacking enemies on non-hills. Able to fire over units that aren't on hills";
            }
        }
        
        if (piece.OnTerrainType == "road") //if on road
        {
            effectsText.text += "\nRoad: +1 speed while on roads (returns to default speed if moving onto non-road)";
        }
        else if (piece.OnTerrainType == "mud")
        {
            if (piece.unitType == "infantry")
            {
                effectsText.text += "\nMud: -1 speed, -1 defense)";
            }
            if (piece.unitType == "cavalry")
            {
                effectsText.text += "\nMud: speed reduced to 1, can't sprint)";
            }
        }

        var portrait = GameObject.Find("UnitImage");
        var portraitSprite = portrait.GetComponent<Image>();
        portraitSprite.sprite = piece.unitPortrait;

    }
    public void HideUnitInfoScreen()
    {
        unitInfoScreen.SetActive(false);
    }

    internal void OnGameStarted()
    {
        DisableAllScreens();
        connectionStatusText.gameObject.SetActive(false);
    }

    public void OnConnect()
    {
        networkManager.SetPlayerLevel((ChessLevel)gameLevelSelection.value);
        networkManager.Connect();
    }

    public void SetConnectionStatus(string status)
    {
        connectionStatusText.text = status;
    }

    public void ShowTeamSelectionScreen()
    {
        DisableAllScreens();
        teamSelectionScreen.SetActive(true);
    }

    public void SelectTeam(int team)
    {
        networkManager.SelectTeam(team);
    }
    internal void RestrictTeamChoice(TeamColor occupiedTeam)
    {
        var buttonToDeactivate = occupiedTeam == TeamColor.White ? whiteTeamButton : blackTeamButton;
        buttonToDeactivate.interactable = false;
    }

    internal void OnGameFinished(string winner)
    {
        gameoverScreen.SetActive(true);
        resultText.text = string.Format("{0} won", winner);
    }
}

