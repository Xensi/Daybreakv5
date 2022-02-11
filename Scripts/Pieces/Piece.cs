using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using TMPro;

//[RequireComponent(typeof(IObjectTweener))]
//[RequireComponent(typeof(MaterialSetter))]

public abstract class Piece : MonoBehaviour
{
    public Board board { get; set; }

    [Header("Campaign stuff")]
    public bool isCampaignToken = false;
    public bool isCampaignObjective = false;
    public string missionToLoad; //mission that should be loaded when this is attacked, if it's campaign objective
    public MenuController menuController;
    [Header("Unit attributes (modifiable)")]
    public string unitName;
    public string displayName;
    public int originalSpeed = 1;
    public float models = 100; //the number of dudes in a squad/unit/battalion whatever
    public float startingModels;
    [HideInInspector] public float oldModels = 0;
    public float health = 1; //the health of each model 
    public float damage = 1f;
    public float morale = 10;
    public float maxMorale;
    public float energy = 15; //overall energy
    public float startingEnergy; //starting energy, which is set by energy
    public float startingMorale;

    public int damageLevel = 0;
    public float armorLevel = 0;

    public string attackType = "melee"; //options: melee, ranged, mixed
    public string unitType = "infantry"; //options: infantry, cavalry, artillery, spellcaster
    //public bool attackIgnoresArmor = false;
    public int effectiveRange = 1; //melee = 1, for ranged this shows the furthest an enemy can be to deal full damage
    public float soldierScale = 1f;
    public int rowSize = 7;
    public float hexOffset = .6f;
    public float hexOffsetY = 0f;
    public float hexOffsetZ = .25f;
    public int numberOfDudesInCircle = 20;
    public int numberOfDudesInCircle2 = 15;
    public int numberOfDudesInCircle3 = 10;
    public float circleRadius = .65f;
    public float circleRadius2 = .5f;
    public float circleRadius3 = .35f;
    [SerializeField] private GameObject markerVisualPrefab;
    public GameObject arrowMarkerVisualPrefab;
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private GameObject navPrefab;
    [SerializeField] private GameObject navPrefab2;
    [SerializeField] private GameObject navPrefab3;
    [SerializeField] private LineRenderer linePrefab;
    public HealthBar modelBar;
    public HealthBar moraleBar;
    public HealthBar energyBar;
    public GameObject rectangleNavParent;
    public GameObject circleNavParent;
    public GameObject staggeredNavParent;
    public Sprite unitPortrait;
    public bool armorPiercing = false;
    public bool arcingAttack = false;
    private AudioSource _audioSource;
    public AudioClip[] attackOrderSoundEffects;
    public AudioClip[] moveOrderSoundEffects;
    public AudioClip[] selectOrderSoundEffects;

    private int soldierNum = 0;
    [Header("Information that should not be modified")]
    public int terrainSpeedModifier;
    public float flankingDamage = 1; //multiplier now
    public int originalEffectiveRange = 1; //stores value of effectiveRange
    public int originalMidRange = 1; //stores value of effectiveRange
    public int originalLongRange = 1; //stores value of effectiveRange
    public bool soldierAttacked = false;
    public Piece attackerPiece;

    public float rangeAttackDistance = 0;

    public Vector2Int targetedSquare;

    public int midRange;
    public int longRange;
    public int beyondTargetableRange;
    public int rangedDamageReduction = 0;
    public int fireDamageBonus = 0;
    public int unitID = 0;
    public int queueTime = 0;
    public int safeQueueTime = 0;
    public int speed = 0;
    public int sprintSpeed;
    public float queuedDamage = 0;
    public int turnTime = 0;
    private int numberOfRows;
    private int smallModelCount;
    private int downscale = 1;
    private int soldierID = 0;
    public int holdTime = 0;
    public float remainingMovement;
    public float defenseModifier = 0;
    public float tempDamage = 0f;
    //public float meleeDefenseMultiplier = 0f; //percentage to multiply meleeDamage by. for example -25% damage = 0.75 multiplier
    public float tweenSpeed = 1;
    public Vector2Int occupiedSquare;
    private Vector2Int moveDirection;
    public bool enemyAdjacent = false;
    public bool hasMoved { get; private set; }


    public bool targetAdjacent = false;
    public bool conflict = false;
    public bool markForDeselect;
    public bool allowedToDie = false;
    public bool startOfTurn = true;
    public bool FinishedMoving = false;
    public bool oneStepFinished = false;
    private bool cancelQueueMovement = false;
    public bool wonTieBreak = false;
    public bool armyLossesApplied = false;
    public bool holdingPosition = false;

    public bool moveAndAttackEnabled = false; //this only matters if you are a ranged unit
    public TeamColor team { get; set; }

    public List<Vector2Int> availableMoves;
    public List<Vector2Int> queuedMoves; //this is where will store future movement information 
    public List<Vector2Int> stashedMoves; //this is where will store future movement information 
    public List<Marker> instantiatedMarkers = new List<Marker>(); //cache marker info
    public List<GameObject> markerVisuals = new List<GameObject>(); //[SerializeField] private ChessGameController chessController;
    public List<int> flankDirections = new List<int>();
    public List<int> frontDirections = new List<int>();
    public List<GameObject> soldierObjects;
    public List<GameObject> markedSoldiers;
    public List<GameObject> deadSoldiers;
    private List<GameObject> navObjects;
    private List<GameObject> navObjectsCircle;
    private List<GameObject> navObjectsStaggered;
    public List<LineRenderer> instantiatedLines = new List<LineRenderer>(); //cache marker info
    public List<GameObject> instantiatedCylinders = new List<GameObject>(); //cache marker info (for ranged attacks)
    public List<GameObject> aestheticCylinders = new List<GameObject>(); //cache marker info (for line aesthetic)

    public Vector2Int attackTile;

    private IObjectTweener tweener;

    public Marker markerPrefab;

    public Piece targetToAttackPiece;
    public Piece tieBreakPiece;

    public int facingDirection = 0; // 0 = north 2 east 4 south 6 west
    public int flankedByHowMany = 0;

    public Marker[,] thisMarkerGrid; //the marker grid we're gonna use

    [HideInInspector] public bool moving = true;
    public bool sprinting = false;
    public bool disengaging = false;
    public bool attacking = false;
    [HideInInspector] public bool turning = false;
    [HideInInspector] public bool routing = false;

    private GameInitializer gameInit;
    [HideInInspector]
    public Vector2Int[] adjacentTiles = new Vector2Int[]
    {
        new Vector2Int(0 , 1), //up
        new Vector2Int(1, 1), //north east
        new Vector2Int(1 , 0), //east
        new Vector2Int(1, -1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1 , 0),
        new Vector2Int(-1, 1),
    };
    [HideInInspector]
    public Vector2Int[] speed2 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),
    };
    [HideInInspector] public Vector2Int[] speed3;
    [HideInInspector] public Vector2Int[] speed4;
    [HideInInspector] public Vector2Int[] speed5;
    [HideInInspector] public Vector2Int[] speed6;
    [HideInInspector] public Vector2Int[] speed7;
    [HideInInspector] public Vector2Int[] speed8;
    [HideInInspector] public Vector2Int[] speed9;
    [HideInInspector] public Vector2Int[] speed10;
    [HideInInspector] public bool arbitratedConflict = false;
    public int conflictTime = 0;
    public int bonusMoraleDamage;

    private float actualOffset;
    public float armyLossesThreshold;
    public bool markedDeaths = false;
    public int inflictedDeaths = 0;
    private Vector2Int forwardVector;
    Vector2Int[] cardinalTiles = new Vector2Int[]
    {
        new Vector2Int(0 , 1), //north
        new Vector2Int(1 , 0), //east
        new Vector2Int(0 , -1), //south
        new Vector2Int(-1 , 0), //west
     };
    Vector2Int[] diagonalTiles = new Vector2Int[]
     {
        new Vector2Int(1, 1), //northeast
        new Vector2Int(1, -1), //southeast
        new Vector2Int(-1, -1),//southwest
        new Vector2Int(-1, 1),//northwest
     };
    public string OnTerrainType = null;
    public string currentFormation = "rectangle";
    public string queuedFormation = "nothing";
    private bool waitingForFirstAttack = false;
    private int markedSoldiersCount = 0;
    public float accuracy;
    private Vector3 oldRotation;
    private Vector3 rotationGoal;
    public bool ordersGiven = false; //usually true
    public bool defensiveAttacking = false;
    public bool attackedThisTurn = false;
    public bool animationsOver = false; //set by soldiers
    public bool alreadyCalculatedDamage = false;
    public bool alreadyAppliedDamage = false;
    public bool movementStopped = true;
    public bool markForRemovalFromSecondWave = false;
    private bool waveringEventTriggered;

    public List<string> strList = new List<string>(); // 
    public List<Color> colorList = new List<Color>(); // 
    public List<float> floatList = new List<float>(); // 

    public Vector3 lastEventEndPos = Vector3.zero;
    public Piece lastTarget = null;

    public bool aggressiveAttitude = true;

    public int placementID = 0;

    public int holdTimeDirection = 0;
    //public int finalDirectionToTurn = 8; //8 means maintain default turn
    public List<GameObject> childProjectiles;

    public bool placedByBoard = false;

    public abstract List<Vector2Int> SelectAvailableSquares(Vector2Int startingSquare);

    public void CheckIfNoOrdersGiven() //just check
    {
        if (queuedMoves.Count <= 0) //if no queued moves
        {
            ordersGiven = false;
        }
        else
        {
            ordersGiven = true;
        }
    }

    private void OnEnable()
    {
        if (attackType == "ranged")
        {
            aggressiveAttitude = false;
        }
        startingModels = models; 
        startingMorale = morale;
        startingEnergy = energy;

        flankingDamage = 1; //set this to 1 bc its all zero right now >>

        _audioSource = GetComponent<AudioSource>();

        midRange = Mathf.RoundToInt(effectiveRange * 1.5f);

        longRange = Mathf.RoundToInt(effectiveRange * 1.75f);

        beyondTargetableRange = 0;

        if (midRange == effectiveRange)
        {
            midRange++;
        }
        if (longRange < midRange)
        {
            longRange = midRange + 1;
        }
        else if (longRange == midRange)
        {
            longRange++;
        }
        beyondTargetableRange = longRange + 1;


        originalEffectiveRange = effectiveRange;
        originalMidRange = midRange;
        originalLongRange = longRange;



        startOfTurn = true;
        if (gameInit == null)
        {
            var game = GameObject.Find("GameInitializer");
            gameInit = game.GetComponent(typeof(GameInitializer)) as GameInitializer;
            //Debug.Log("game init set");
        }

        FinishedMoving = false;
        availableMoves = new List<Vector2Int>();
        queuedMoves = new List<Vector2Int>();
        soldierObjects = new List<GameObject>();
        navObjects = new List<GameObject>();
        navObjectsCircle = new List<GameObject>();
        navObjectsStaggered = new List<GameObject>();
        markedSoldiers = new List<GameObject>();
        tweener = GetComponent<IObjectTweener>();
        //materialSetter = GetComponent<MaterialSetter>();

        remainingMovement = originalSpeed; //set remaining movement to speed.
        sprintSpeed = originalSpeed * 2;
        speed = originalSpeed;
        //smallModelCount = Mathf.RoundToInt(models / downscale); // downsize by factor of 10, so 450 is 45
        numberOfRows = Mathf.RoundToInt(models / rowSize); //for example: 45/7 = 6.4 round down to 6

        armyLossesThreshold = models * 0.5f; //prefers even numbers

        speed3 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),
        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),
    };
        speed4 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),

    };
        speed5 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
    };
        speed6 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
        //6
        //top line
        new Vector2Int(-6 , 6),
        new Vector2Int(-5 , 6),
        new Vector2Int(-4 , 6),
        new Vector2Int(-3 , 6),
        new Vector2Int(-2 , 6),
        new Vector2Int(-1 , 6),
        new Vector2Int(0 , 6),
        new Vector2Int(1 , 6),
        new Vector2Int(2 , 6),
        new Vector2Int(3 , 6),
        new Vector2Int(4 , 6),
        new Vector2Int(5 , 6),
        new Vector2Int(6 , 6),
        //bottom line
        new Vector2Int(-6 , -6),
        new Vector2Int(-5 , -6),
        new Vector2Int(-4 , -6),
        new Vector2Int(-3 , -6),
        new Vector2Int(-2 , -6),
        new Vector2Int(-1 , -6),
        new Vector2Int(0 , -6),
        new Vector2Int(1 , -6),
        new Vector2Int(2 , -6),
        new Vector2Int(3 , -6),
        new Vector2Int(4 , -6),
        new Vector2Int(5 , -6),
        new Vector2Int(6 , -6),
        //left side
        new Vector2Int(-6 , 5),
        new Vector2Int(-6 , 4),
        new Vector2Int(-6 , 3),
        new Vector2Int(-6 , 2),
        new Vector2Int(-6 , 1),
        new Vector2Int(-6 , 0),
        new Vector2Int(-6 , -1),
        new Vector2Int(-6 , -2),
        new Vector2Int(-6 , -3),
        new Vector2Int(-6 , -4),
        new Vector2Int(-6 , -5),
        //right side
        new Vector2Int(6 , 5),
        new Vector2Int(6 , 4),
        new Vector2Int(6 , 3),
        new Vector2Int(6 , 2),
        new Vector2Int(6 , 1),
        new Vector2Int(6 , 0),
        new Vector2Int(6 , -1),
        new Vector2Int(6 , -2),
        new Vector2Int(6 , -3),
        new Vector2Int(6 , -4),
        new Vector2Int(6 , -5),
    };
        speed7 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
        //6
        //top line
        new Vector2Int(-6 , 6),
        new Vector2Int(-5 , 6),
        new Vector2Int(-4 , 6),
        new Vector2Int(-3 , 6),
        new Vector2Int(-2 , 6),
        new Vector2Int(-1 , 6),
        new Vector2Int(0 , 6),
        new Vector2Int(1 , 6),
        new Vector2Int(2 , 6),
        new Vector2Int(3 , 6),
        new Vector2Int(4 , 6),
        new Vector2Int(5 , 6),
        new Vector2Int(6 , 6),
        //bottom line
        new Vector2Int(-6 , -6),
        new Vector2Int(-5 , -6),
        new Vector2Int(-4 , -6),
        new Vector2Int(-3 , -6),
        new Vector2Int(-2 , -6),
        new Vector2Int(-1 , -6),
        new Vector2Int(0 , -6),
        new Vector2Int(1 , -6),
        new Vector2Int(2 , -6),
        new Vector2Int(3 , -6),
        new Vector2Int(4 , -6),
        new Vector2Int(5 , -6),
        new Vector2Int(6 , -6),
        //left side
        new Vector2Int(-6 , 5),
        new Vector2Int(-6 , 4),
        new Vector2Int(-6 , 3),
        new Vector2Int(-6 , 2),
        new Vector2Int(-6 , 1),
        new Vector2Int(-6 , 0),
        new Vector2Int(-6 , -1),
        new Vector2Int(-6 , -2),
        new Vector2Int(-6 , -3),
        new Vector2Int(-6 , -4),
        new Vector2Int(-6 , -5),
        //right side
        new Vector2Int(6 , 5),
        new Vector2Int(6 , 4),
        new Vector2Int(6 , 3),
        new Vector2Int(6 , 2),
        new Vector2Int(6 , 1),
        new Vector2Int(6 , 0),
        new Vector2Int(6 , -1),
        new Vector2Int(6 , -2),
        new Vector2Int(6 , -3),
        new Vector2Int(6 , -4),
        new Vector2Int(6 , -5),
        //7
        //top line
        new Vector2Int(-7 , 7),
        new Vector2Int(-6 , 7),
        new Vector2Int(-5 , 7),
        new Vector2Int(-4 , 7),
        new Vector2Int(-3 , 7),
        new Vector2Int(-2 , 7),
        new Vector2Int(-1 , 7),
        new Vector2Int(0 , 7),
        new Vector2Int(1 , 7),
        new Vector2Int(2 , 7),
        new Vector2Int(3 , 7),
        new Vector2Int(4 , 7),
        new Vector2Int(5 , 7),
        new Vector2Int(6 , 7),
        new Vector2Int(7 , 7),
        //bottom line
        new Vector2Int(-7 , -7),
        new Vector2Int(-6 , -7),
        new Vector2Int(-5 , -7),
        new Vector2Int(-4 , -7),
        new Vector2Int(-3 , -7),
        new Vector2Int(-2 , -7),
        new Vector2Int(-1 , -7),
        new Vector2Int(0 , -7),
        new Vector2Int(1 , -7),
        new Vector2Int(2 , -7),
        new Vector2Int(3 , -7),
        new Vector2Int(4 , -7),
        new Vector2Int(5 , -7),
        new Vector2Int(6 , -7),
        new Vector2Int(7 , -7),
        //left side
        new Vector2Int(-7 , 6),
        new Vector2Int(-7 , 5),
        new Vector2Int(-7 , 4),
        new Vector2Int(-7 , 3),
        new Vector2Int(-7 , 2),
        new Vector2Int(-7 , 1),
        new Vector2Int(-7 , 0),
        new Vector2Int(-7 , -1),
        new Vector2Int(-7 , -2),
        new Vector2Int(-7 , -3),
        new Vector2Int(-7 , -4),
        new Vector2Int(-7 , -5),
        new Vector2Int(-7 , -6),
        //right side
        new Vector2Int(7 , 6),
        new Vector2Int(7 , 5),
        new Vector2Int(7 , 4),
        new Vector2Int(7 , 3),
        new Vector2Int(7 , 2),
        new Vector2Int(7 , 1),
        new Vector2Int(7 , 0),
        new Vector2Int(7 , -1),
        new Vector2Int(7 , -2),
        new Vector2Int(7 , -3),
        new Vector2Int(7 , -4),
        new Vector2Int(7 , -5),
        new Vector2Int(7 , -6),
    };
        speed8 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
        //6
        //top line
        new Vector2Int(-6 , 6),
        new Vector2Int(-5 , 6),
        new Vector2Int(-4 , 6),
        new Vector2Int(-3 , 6),
        new Vector2Int(-2 , 6),
        new Vector2Int(-1 , 6),
        new Vector2Int(0 , 6),
        new Vector2Int(1 , 6),
        new Vector2Int(2 , 6),
        new Vector2Int(3 , 6),
        new Vector2Int(4 , 6),
        new Vector2Int(5 , 6),
        new Vector2Int(6 , 6),
        //bottom line
        new Vector2Int(-6 , -6),
        new Vector2Int(-5 , -6),
        new Vector2Int(-4 , -6),
        new Vector2Int(-3 , -6),
        new Vector2Int(-2 , -6),
        new Vector2Int(-1 , -6),
        new Vector2Int(0 , -6),
        new Vector2Int(1 , -6),
        new Vector2Int(2 , -6),
        new Vector2Int(3 , -6),
        new Vector2Int(4 , -6),
        new Vector2Int(5 , -6),
        new Vector2Int(6 , -6),
        //left side
        new Vector2Int(-6 , 5),
        new Vector2Int(-6 , 4),
        new Vector2Int(-6 , 3),
        new Vector2Int(-6 , 2),
        new Vector2Int(-6 , 1),
        new Vector2Int(-6 , 0),
        new Vector2Int(-6 , -1),
        new Vector2Int(-6 , -2),
        new Vector2Int(-6 , -3),
        new Vector2Int(-6 , -4),
        new Vector2Int(-6 , -5),
        //right side
        new Vector2Int(6 , 5),
        new Vector2Int(6 , 4),
        new Vector2Int(6 , 3),
        new Vector2Int(6 , 2),
        new Vector2Int(6 , 1),
        new Vector2Int(6 , 0),
        new Vector2Int(6 , -1),
        new Vector2Int(6 , -2),
        new Vector2Int(6 , -3),
        new Vector2Int(6 , -4),
        new Vector2Int(6 , -5),
        //7
        //top line
        new Vector2Int(-7 , 7),
        new Vector2Int(-6 , 7),
        new Vector2Int(-5 , 7),
        new Vector2Int(-4 , 7),
        new Vector2Int(-3 , 7),
        new Vector2Int(-2 , 7),
        new Vector2Int(-1 , 7),
        new Vector2Int(0 , 7),
        new Vector2Int(1 , 7),
        new Vector2Int(2 , 7),
        new Vector2Int(3 , 7),
        new Vector2Int(4 , 7),
        new Vector2Int(5 , 7),
        new Vector2Int(6 , 7),
        new Vector2Int(7 , 7),
        //bottom line
        new Vector2Int(-7 , -7),
        new Vector2Int(-6 , -7),
        new Vector2Int(-5 , -7),
        new Vector2Int(-4 , -7),
        new Vector2Int(-3 , -7),
        new Vector2Int(-2 , -7),
        new Vector2Int(-1 , -7),
        new Vector2Int(0 , -7),
        new Vector2Int(1 , -7),
        new Vector2Int(2 , -7),
        new Vector2Int(3 , -7),
        new Vector2Int(4 , -7),
        new Vector2Int(5 , -7),
        new Vector2Int(6 , -7),
        new Vector2Int(7 , -7),
        //left side
        new Vector2Int(-7 , 6),
        new Vector2Int(-7 , 5),
        new Vector2Int(-7 , 4),
        new Vector2Int(-7 , 3),
        new Vector2Int(-7 , 2),
        new Vector2Int(-7 , 1),
        new Vector2Int(-7 , 0),
        new Vector2Int(-7 , -1),
        new Vector2Int(-7 , -2),
        new Vector2Int(-7 , -3),
        new Vector2Int(-7 , -4),
        new Vector2Int(-7 , -5),
        new Vector2Int(-7 , -6),
        //right side
        new Vector2Int(7 , 6),
        new Vector2Int(7 , 5),
        new Vector2Int(7 , 4),
        new Vector2Int(7 , 3),
        new Vector2Int(7 , 2),
        new Vector2Int(7 , 1),
        new Vector2Int(7 , 0),
        new Vector2Int(7 , -1),
        new Vector2Int(7 , -2),
        new Vector2Int(7 , -3),
        new Vector2Int(7 , -4),
        new Vector2Int(7 , -5),
        new Vector2Int(7 , -6),
        //8
        //top line
        new Vector2Int(-8 , 8),
        new Vector2Int(-7 , 8),
        new Vector2Int(-6 , 8),
        new Vector2Int(-5 , 8),
        new Vector2Int(-4 , 8),
        new Vector2Int(-3 , 8),
        new Vector2Int(-2 , 8),
        new Vector2Int(-1 , 8),
        new Vector2Int(0 , 8),
        new Vector2Int(1, 8),
        new Vector2Int(2, 8),
        new Vector2Int(3, 8),
        new Vector2Int(4, 8),
        new Vector2Int(5, 8),
        new Vector2Int(6, 8),
        new Vector2Int(7, 8),
        new Vector2Int(8, 8),
        //bottom line
        new Vector2Int(-8 , -8),
        new Vector2Int(-7 , -8),
        new Vector2Int(-6 , -8),
        new Vector2Int(-5 , -8),
        new Vector2Int(-4 , -8),
        new Vector2Int(-3 , -8),
        new Vector2Int(-2 , -8),
        new Vector2Int(-1 , -8),
        new Vector2Int(0 , -8),
        new Vector2Int(1, -8),
        new Vector2Int(2, -8),
        new Vector2Int(3, -8),
        new Vector2Int(4, -8),
        new Vector2Int(5, -8),
        new Vector2Int(6, -8),
        new Vector2Int(7, -8),
        new Vector2Int(8, -8),
        //left side
        new Vector2Int(-8 , 7),
        new Vector2Int(-8 , 6),
        new Vector2Int(-8 , 5),
        new Vector2Int(-8 , 4),
        new Vector2Int(-8 , 3),
        new Vector2Int(-8 , 2),
        new Vector2Int(-8 , 1),
        new Vector2Int(-8 , 0),
        new Vector2Int(-8 , -1),
        new Vector2Int(-8 , -2),
        new Vector2Int(-8 , -3),
        new Vector2Int(-8 , -4),
        new Vector2Int(-8 , -5),
        new Vector2Int(-8 , -6),
        new Vector2Int(-8 , -7),
        //right side
        new Vector2Int(8 , 7),
        new Vector2Int(8 , 6),
        new Vector2Int(8 , 5),
        new Vector2Int(8 , 4),
        new Vector2Int(8 , 3),
        new Vector2Int(8 , 2),
        new Vector2Int(8 , 1),
        new Vector2Int(8 , 0),
        new Vector2Int(8 , -1),
        new Vector2Int(8 , -2),
        new Vector2Int(8 , -3),
        new Vector2Int(8 , -4),
        new Vector2Int(8 , -5),
        new Vector2Int(8 , -6),
        new Vector2Int(8 , -7),

    };
        speed9 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
        //6
        //top line
        new Vector2Int(-6 , 6),
        new Vector2Int(-5 , 6),
        new Vector2Int(-4 , 6),
        new Vector2Int(-3 , 6),
        new Vector2Int(-2 , 6),
        new Vector2Int(-1 , 6),
        new Vector2Int(0 , 6),
        new Vector2Int(1 , 6),
        new Vector2Int(2 , 6),
        new Vector2Int(3 , 6),
        new Vector2Int(4 , 6),
        new Vector2Int(5 , 6),
        new Vector2Int(6 , 6),
        //bottom line
        new Vector2Int(-6 , -6),
        new Vector2Int(-5 , -6),
        new Vector2Int(-4 , -6),
        new Vector2Int(-3 , -6),
        new Vector2Int(-2 , -6),
        new Vector2Int(-1 , -6),
        new Vector2Int(0 , -6),
        new Vector2Int(1 , -6),
        new Vector2Int(2 , -6),
        new Vector2Int(3 , -6),
        new Vector2Int(4 , -6),
        new Vector2Int(5 , -6),
        new Vector2Int(6 , -6),
        //left side
        new Vector2Int(-6 , 5),
        new Vector2Int(-6 , 4),
        new Vector2Int(-6 , 3),
        new Vector2Int(-6 , 2),
        new Vector2Int(-6 , 1),
        new Vector2Int(-6 , 0),
        new Vector2Int(-6 , -1),
        new Vector2Int(-6 , -2),
        new Vector2Int(-6 , -3),
        new Vector2Int(-6 , -4),
        new Vector2Int(-6 , -5),
        //right side
        new Vector2Int(6 , 5),
        new Vector2Int(6 , 4),
        new Vector2Int(6 , 3),
        new Vector2Int(6 , 2),
        new Vector2Int(6 , 1),
        new Vector2Int(6 , 0),
        new Vector2Int(6 , -1),
        new Vector2Int(6 , -2),
        new Vector2Int(6 , -3),
        new Vector2Int(6 , -4),
        new Vector2Int(6 , -5),
        //7
        //top line
        new Vector2Int(-7 , 7),
        new Vector2Int(-6 , 7),
        new Vector2Int(-5 , 7),
        new Vector2Int(-4 , 7),
        new Vector2Int(-3 , 7),
        new Vector2Int(-2 , 7),
        new Vector2Int(-1 , 7),
        new Vector2Int(0 , 7),
        new Vector2Int(1 , 7),
        new Vector2Int(2 , 7),
        new Vector2Int(3 , 7),
        new Vector2Int(4 , 7),
        new Vector2Int(5 , 7),
        new Vector2Int(6 , 7),
        new Vector2Int(7 , 7),
        //bottom line
        new Vector2Int(-7 , -7),
        new Vector2Int(-6 , -7),
        new Vector2Int(-5 , -7),
        new Vector2Int(-4 , -7),
        new Vector2Int(-3 , -7),
        new Vector2Int(-2 , -7),
        new Vector2Int(-1 , -7),
        new Vector2Int(0 , -7),
        new Vector2Int(1 , -7),
        new Vector2Int(2 , -7),
        new Vector2Int(3 , -7),
        new Vector2Int(4 , -7),
        new Vector2Int(5 , -7),
        new Vector2Int(6 , -7),
        new Vector2Int(7 , -7),
        //left side
        new Vector2Int(-7 , 6),
        new Vector2Int(-7 , 5),
        new Vector2Int(-7 , 4),
        new Vector2Int(-7 , 3),
        new Vector2Int(-7 , 2),
        new Vector2Int(-7 , 1),
        new Vector2Int(-7 , 0),
        new Vector2Int(-7 , -1),
        new Vector2Int(-7 , -2),
        new Vector2Int(-7 , -3),
        new Vector2Int(-7 , -4),
        new Vector2Int(-7 , -5),
        new Vector2Int(-7 , -6),
        //right side
        new Vector2Int(7 , 6),
        new Vector2Int(7 , 5),
        new Vector2Int(7 , 4),
        new Vector2Int(7 , 3),
        new Vector2Int(7 , 2),
        new Vector2Int(7 , 1),
        new Vector2Int(7 , 0),
        new Vector2Int(7 , -1),
        new Vector2Int(7 , -2),
        new Vector2Int(7 , -3),
        new Vector2Int(7 , -4),
        new Vector2Int(7 , -5),
        new Vector2Int(7 , -6),
        //8
        //top line
        new Vector2Int(-8 , 8),
        new Vector2Int(-7 , 8),
        new Vector2Int(-6 , 8),
        new Vector2Int(-5 , 8),
        new Vector2Int(-4 , 8),
        new Vector2Int(-3 , 8),
        new Vector2Int(-2 , 8),
        new Vector2Int(-1 , 8),
        new Vector2Int(0 , 8),
        new Vector2Int(1, 8),
        new Vector2Int(2, 8),
        new Vector2Int(3, 8),
        new Vector2Int(4, 8),
        new Vector2Int(5, 8),
        new Vector2Int(6, 8),
        new Vector2Int(7, 8),
        new Vector2Int(8, 8),
        //bottom line
        new Vector2Int(-8 , -8),
        new Vector2Int(-7 , -8),
        new Vector2Int(-6 , -8),
        new Vector2Int(-5 , -8),
        new Vector2Int(-4 , -8),
        new Vector2Int(-3 , -8),
        new Vector2Int(-2 , -8),
        new Vector2Int(-1 , -8),
        new Vector2Int(0 , -8),
        new Vector2Int(1, -8),
        new Vector2Int(2, -8),
        new Vector2Int(3, -8),
        new Vector2Int(4, -8),
        new Vector2Int(5, -8),
        new Vector2Int(6, -8),
        new Vector2Int(7, -8),
        new Vector2Int(8, -8),
        //left side
        new Vector2Int(-8 , 7),
        new Vector2Int(-8 , 6),
        new Vector2Int(-8 , 5),
        new Vector2Int(-8 , 4),
        new Vector2Int(-8 , 3),
        new Vector2Int(-8 , 2),
        new Vector2Int(-8 , 1),
        new Vector2Int(-8 , 0),
        new Vector2Int(-8 , -1),
        new Vector2Int(-8 , -2),
        new Vector2Int(-8 , -3),
        new Vector2Int(-8 , -4),
        new Vector2Int(-8 , -5),
        new Vector2Int(-8 , -6),
        new Vector2Int(-8 , -7),
        //right side
        new Vector2Int(8 , 7),
        new Vector2Int(8 , 6),
        new Vector2Int(8 , 5),
        new Vector2Int(8 , 4),
        new Vector2Int(8 , 3),
        new Vector2Int(8 , 2),
        new Vector2Int(8 , 1),
        new Vector2Int(8 , 0),
        new Vector2Int(8 , -1),
        new Vector2Int(8 , -2),
        new Vector2Int(8 , -3),
        new Vector2Int(8 , -4),
        new Vector2Int(8 , -5),
        new Vector2Int(8 , -6),
        new Vector2Int(8 , -7),
        //9
        //top line
        new Vector2Int(-9 , 9),
        new Vector2Int(-8 , 9),
        new Vector2Int(-7 , 9),
        new Vector2Int(-6 , 9),
        new Vector2Int(-5 , 9),
        new Vector2Int(-4 , 9),
        new Vector2Int(-3 , 9),
        new Vector2Int(-2 , 9),
        new Vector2Int(-1 , 9),
        new Vector2Int(0 , 9),
        new Vector2Int(1, 9),
        new Vector2Int(2, 9),
        new Vector2Int(3, 9),
        new Vector2Int(4, 9),
        new Vector2Int(5, 9),
        new Vector2Int(6, 9),
        new Vector2Int(7, 9),
        new Vector2Int(8, 9),
        new Vector2Int(9, 9),
        //top line
        new Vector2Int(-9 , -9),
        new Vector2Int(-8 , -9),
        new Vector2Int(-7 , -9),
        new Vector2Int(-6 , -9),
        new Vector2Int(-5 , -9),
        new Vector2Int(-4 , -9),
        new Vector2Int(-3 , -9),
        new Vector2Int(-2 , -9),
        new Vector2Int(-1 , -9),
        new Vector2Int(0 , -9),
        new Vector2Int(1, -9),
        new Vector2Int(2, -9),
        new Vector2Int(3, -9),
        new Vector2Int(4, -9),
        new Vector2Int(5, -9),
        new Vector2Int(6, -9),
        new Vector2Int(7, -9),
        new Vector2Int(8, -9),
        new Vector2Int(9, -9),
        //left side
        new Vector2Int(-9 , 8),
        new Vector2Int(-9 , 7),
        new Vector2Int(-9, 6),
        new Vector2Int(-9 , 5),
        new Vector2Int(-9 , 4),
        new Vector2Int(-9 , 3),
        new Vector2Int(-9 , 2),
        new Vector2Int(-9 , 1),
        new Vector2Int(-9 , 0),
        new Vector2Int(-9 , -1),
        new Vector2Int(-9 , -2),
        new Vector2Int(-9 , -3),
        new Vector2Int(-9 , -4),
        new Vector2Int(-9 , -5),
        new Vector2Int(-9 , -6),
        new Vector2Int(-9 , -7),
        new Vector2Int(-9 , -8),
        //right side
        new Vector2Int(9 , 8),
        new Vector2Int(9 , 7),
        new Vector2Int(9, 6),
        new Vector2Int(9 , 5),
        new Vector2Int(9 , 4),
        new Vector2Int(9 , 3),
        new Vector2Int(9 , 2),
        new Vector2Int(9 , 1),
        new Vector2Int(9 , 0),
        new Vector2Int(9 , -1),
        new Vector2Int(9 , -2),
        new Vector2Int(9 , -3),
        new Vector2Int(9 , -4),
        new Vector2Int(9 , -5),
        new Vector2Int(9 , -6),
        new Vector2Int(9 , -7),
        new Vector2Int(9 , -8),
    };
        speed10 = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1 , 0),
        new Vector2Int(0 , 1),
        new Vector2Int(0 , -1),
        new Vector2Int(-1 , 0),

        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(0 , 2),
        new Vector2Int(0 , -2),
        new Vector2Int(-2 , 0),
        new Vector2Int(2 , 0),
        new Vector2Int(2 , 2),
        new Vector2Int(-2 , -2),
        new Vector2Int(-2 , 2),
        new Vector2Int(2 , -2),

        //top line
        new Vector2Int(-3 , 3),
        new Vector2Int(-2 , 3),
        new Vector2Int(-1 , 3),
        new Vector2Int(0 , 3),
        new Vector2Int(1 , 3),
        new Vector2Int(2 , 3),
        new Vector2Int(3 , 3),
        //bottom line
        new Vector2Int(-3 , -3),
        new Vector2Int(-2 , -3),
        new Vector2Int(-1 , -3),
        new Vector2Int(0 , -3),
        new Vector2Int(1 , -3),
        new Vector2Int(2 , -3),
        new Vector2Int(3 , -3),
        //left side
        new Vector2Int(-3 , 2),
        new Vector2Int(-3 , 1),
        new Vector2Int(-3 , 0),
        new Vector2Int(-3 , -1),
        new Vector2Int(-3 , -2),
        //right side
        new Vector2Int(3 , 2),
        new Vector2Int(3 , 1),
        new Vector2Int(3 , 0),
        new Vector2Int(3 , -1),
        new Vector2Int(3 , -2),

        //4
        //top line
        new Vector2Int(-4 , 4),
        new Vector2Int(-3 , 4),
        new Vector2Int(-2 , 4),
        new Vector2Int(-1 , 4),
        new Vector2Int(0 , 4),
        new Vector2Int(1 , 4),
        new Vector2Int(2 , 4),
        new Vector2Int(3 , 4),
        new Vector2Int(4 , 4),
        //bottom line
        new Vector2Int(-4 , -4),
        new Vector2Int(-3 , -4),
        new Vector2Int(-2 , -4),
        new Vector2Int(-1 , -4),
        new Vector2Int(0 , -4),
        new Vector2Int(1 , -4),
        new Vector2Int(2 , -4),
        new Vector2Int(3 , -4),
        new Vector2Int(4 , -4),
        //left side
        new Vector2Int(-4 , 3),
        new Vector2Int(-4 , 2),
        new Vector2Int(-4 , 1),
        new Vector2Int(-4 , 0),
        new Vector2Int(-4 , -1),
        new Vector2Int(-4 , -2),
        new Vector2Int(-4 , -3),
        //right side
        new Vector2Int(4 , 3),
        new Vector2Int(4 , 2),
        new Vector2Int(4 , 1),
        new Vector2Int(4 , 0),
        new Vector2Int(4 , -1),
        new Vector2Int(4 , -2),
        new Vector2Int(4 , -3),
        //5
        //top line
        new Vector2Int(-5 , 5),
        new Vector2Int(-4 , 5),
        new Vector2Int(-3 , 5),
        new Vector2Int(-2 , 5),
        new Vector2Int(-1 , 5),
        new Vector2Int(0 , 5),
        new Vector2Int(1 , 5),
        new Vector2Int(2 , 5),
        new Vector2Int(3 , 5),
        new Vector2Int(4 , 5),
        new Vector2Int(5 , 5),
        //bottom line
        new Vector2Int(-5 , -5),
        new Vector2Int(-4 , -5),
        new Vector2Int(-3 , -5),
        new Vector2Int(-2 , -5),
        new Vector2Int(-1 , -5),
        new Vector2Int(0 , -5),
        new Vector2Int(1 , -5),
        new Vector2Int(2 , -5),
        new Vector2Int(3 , -5),
        new Vector2Int(4 , -5),
        new Vector2Int(5 , -5),
        //left side
        new Vector2Int(-5 , 4),
        new Vector2Int(-5 , 3),
        new Vector2Int(-5 , 2),
        new Vector2Int(-5 , 1),
        new Vector2Int(-5 , 0),
        new Vector2Int(-5 , -1),
        new Vector2Int(-5 , -2),
        new Vector2Int(-5 , -3),
        new Vector2Int(-5 , -4),
        //right side
        new Vector2Int(5 , 4),
        new Vector2Int(5 , 3),
        new Vector2Int(5 , 2),
        new Vector2Int(5 , 1),
        new Vector2Int(5 , 0),
        new Vector2Int(5 , -1),
        new Vector2Int(5 , -2),
        new Vector2Int(5 , -3),
        new Vector2Int(5 , -4),
        //6
        //top line
        new Vector2Int(-6 , 6),
        new Vector2Int(-5 , 6),
        new Vector2Int(-4 , 6),
        new Vector2Int(-3 , 6),
        new Vector2Int(-2 , 6),
        new Vector2Int(-1 , 6),
        new Vector2Int(0 , 6),
        new Vector2Int(1 , 6),
        new Vector2Int(2 , 6),
        new Vector2Int(3 , 6),
        new Vector2Int(4 , 6),
        new Vector2Int(5 , 6),
        new Vector2Int(6 , 6),
        //bottom line
        new Vector2Int(-6 , -6),
        new Vector2Int(-5 , -6),
        new Vector2Int(-4 , -6),
        new Vector2Int(-3 , -6),
        new Vector2Int(-2 , -6),
        new Vector2Int(-1 , -6),
        new Vector2Int(0 , -6),
        new Vector2Int(1 , -6),
        new Vector2Int(2 , -6),
        new Vector2Int(3 , -6),
        new Vector2Int(4 , -6),
        new Vector2Int(5 , -6),
        new Vector2Int(6 , -6),
        //left side
        new Vector2Int(-6 , 5),
        new Vector2Int(-6 , 4),
        new Vector2Int(-6 , 3),
        new Vector2Int(-6 , 2),
        new Vector2Int(-6 , 1),
        new Vector2Int(-6 , 0),
        new Vector2Int(-6 , -1),
        new Vector2Int(-6 , -2),
        new Vector2Int(-6 , -3),
        new Vector2Int(-6 , -4),
        new Vector2Int(-6 , -5),
        //right side
        new Vector2Int(6 , 5),
        new Vector2Int(6 , 4),
        new Vector2Int(6 , 3),
        new Vector2Int(6 , 2),
        new Vector2Int(6 , 1),
        new Vector2Int(6 , 0),
        new Vector2Int(6 , -1),
        new Vector2Int(6 , -2),
        new Vector2Int(6 , -3),
        new Vector2Int(6 , -4),
        new Vector2Int(6 , -5),
        //7
        //top line
        new Vector2Int(-7 , 7),
        new Vector2Int(-6 , 7),
        new Vector2Int(-5 , 7),
        new Vector2Int(-4 , 7),
        new Vector2Int(-3 , 7),
        new Vector2Int(-2 , 7),
        new Vector2Int(-1 , 7),
        new Vector2Int(0 , 7),
        new Vector2Int(1 , 7),
        new Vector2Int(2 , 7),
        new Vector2Int(3 , 7),
        new Vector2Int(4 , 7),
        new Vector2Int(5 , 7),
        new Vector2Int(6 , 7),
        new Vector2Int(7 , 7),
        //bottom line
        new Vector2Int(-7 , -7),
        new Vector2Int(-6 , -7),
        new Vector2Int(-5 , -7),
        new Vector2Int(-4 , -7),
        new Vector2Int(-3 , -7),
        new Vector2Int(-2 , -7),
        new Vector2Int(-1 , -7),
        new Vector2Int(0 , -7),
        new Vector2Int(1 , -7),
        new Vector2Int(2 , -7),
        new Vector2Int(3 , -7),
        new Vector2Int(4 , -7),
        new Vector2Int(5 , -7),
        new Vector2Int(6 , -7),
        new Vector2Int(7 , -7),
        //left side
        new Vector2Int(-7 , 6),
        new Vector2Int(-7 , 5),
        new Vector2Int(-7 , 4),
        new Vector2Int(-7 , 3),
        new Vector2Int(-7 , 2),
        new Vector2Int(-7 , 1),
        new Vector2Int(-7 , 0),
        new Vector2Int(-7 , -1),
        new Vector2Int(-7 , -2),
        new Vector2Int(-7 , -3),
        new Vector2Int(-7 , -4),
        new Vector2Int(-7 , -5),
        new Vector2Int(-7 , -6),
        //right side
        new Vector2Int(7 , 6),
        new Vector2Int(7 , 5),
        new Vector2Int(7 , 4),
        new Vector2Int(7 , 3),
        new Vector2Int(7 , 2),
        new Vector2Int(7 , 1),
        new Vector2Int(7 , 0),
        new Vector2Int(7 , -1),
        new Vector2Int(7 , -2),
        new Vector2Int(7 , -3),
        new Vector2Int(7 , -4),
        new Vector2Int(7 , -5),
        new Vector2Int(7 , -6),
        //8
        //top line
        new Vector2Int(-8 , 8),
        new Vector2Int(-7 , 8),
        new Vector2Int(-6 , 8),
        new Vector2Int(-5 , 8),
        new Vector2Int(-4 , 8),
        new Vector2Int(-3 , 8),
        new Vector2Int(-2 , 8),
        new Vector2Int(-1 , 8),
        new Vector2Int(0 , 8),
        new Vector2Int(1, 8),
        new Vector2Int(2, 8),
        new Vector2Int(3, 8),
        new Vector2Int(4, 8),
        new Vector2Int(5, 8),
        new Vector2Int(6, 8),
        new Vector2Int(7, 8),
        new Vector2Int(8, 8),
        //bottom line
        new Vector2Int(-8 , -8),
        new Vector2Int(-7 , -8),
        new Vector2Int(-6 , -8),
        new Vector2Int(-5 , -8),
        new Vector2Int(-4 , -8),
        new Vector2Int(-3 , -8),
        new Vector2Int(-2 , -8),
        new Vector2Int(-1 , -8),
        new Vector2Int(0 , -8),
        new Vector2Int(1, -8),
        new Vector2Int(2, -8),
        new Vector2Int(3, -8),
        new Vector2Int(4, -8),
        new Vector2Int(5, -8),
        new Vector2Int(6, -8),
        new Vector2Int(7, -8),
        new Vector2Int(8, -8),
        //left side
        new Vector2Int(-8 , 7),
        new Vector2Int(-8 , 6),
        new Vector2Int(-8 , 5),
        new Vector2Int(-8 , 4),
        new Vector2Int(-8 , 3),
        new Vector2Int(-8 , 2),
        new Vector2Int(-8 , 1),
        new Vector2Int(-8 , 0),
        new Vector2Int(-8 , -1),
        new Vector2Int(-8 , -2),
        new Vector2Int(-8 , -3),
        new Vector2Int(-8 , -4),
        new Vector2Int(-8 , -5),
        new Vector2Int(-8 , -6),
        new Vector2Int(-8 , -7),
        //right side
        new Vector2Int(8 , 7),
        new Vector2Int(8 , 6),
        new Vector2Int(8 , 5),
        new Vector2Int(8 , 4),
        new Vector2Int(8 , 3),
        new Vector2Int(8 , 2),
        new Vector2Int(8 , 1),
        new Vector2Int(8 , 0),
        new Vector2Int(8 , -1),
        new Vector2Int(8 , -2),
        new Vector2Int(8 , -3),
        new Vector2Int(8 , -4),
        new Vector2Int(8 , -5),
        new Vector2Int(8 , -6),
        new Vector2Int(8 , -7),
        //9
        //top line
        new Vector2Int(-9 , 9),
        new Vector2Int(-8 , 9),
        new Vector2Int(-7 , 9),
        new Vector2Int(-6 , 9),
        new Vector2Int(-5 , 9),
        new Vector2Int(-4 , 9),
        new Vector2Int(-3 , 9),
        new Vector2Int(-2 , 9),
        new Vector2Int(-1 , 9),
        new Vector2Int(0 , 9),
        new Vector2Int(1, 9),
        new Vector2Int(2, 9),
        new Vector2Int(3, 9),
        new Vector2Int(4, 9),
        new Vector2Int(5, 9),
        new Vector2Int(6, 9),
        new Vector2Int(7, 9),
        new Vector2Int(8, 9),
        new Vector2Int(9, 9),
        //bottom line
        new Vector2Int(-9 , -9),
        new Vector2Int(-8 , -9),
        new Vector2Int(-7 , -9),
        new Vector2Int(-6 , -9),
        new Vector2Int(-5 , -9),
        new Vector2Int(-4 , -9),
        new Vector2Int(-3 , -9),
        new Vector2Int(-2 , -9),
        new Vector2Int(-1 , -9),
        new Vector2Int(0 , -9),
        new Vector2Int(1, -9),
        new Vector2Int(2, -9),
        new Vector2Int(3, -9),
        new Vector2Int(4, -9),
        new Vector2Int(5, -9),
        new Vector2Int(6, -9),
        new Vector2Int(7, -9),
        new Vector2Int(8, -9),
        new Vector2Int(9, -9),
        //left side
        new Vector2Int(-9 , 8),
        new Vector2Int(-9 , 7),
        new Vector2Int(-9, 6),
        new Vector2Int(-9 , 5),
        new Vector2Int(-9 , 4),
        new Vector2Int(-9 , 3),
        new Vector2Int(-9 , 2),
        new Vector2Int(-9 , 1),
        new Vector2Int(-9 , 0),
        new Vector2Int(-9 , -1),
        new Vector2Int(-9 , -2),
        new Vector2Int(-9 , -3),
        new Vector2Int(-9 , -4),
        new Vector2Int(-9 , -5),
        new Vector2Int(-9 , -6),
        new Vector2Int(-9 , -7),
        new Vector2Int(-9 , -8),
        //right side
        new Vector2Int(9 , 8),
        new Vector2Int(9 , 7),
        new Vector2Int(9, 6),
        new Vector2Int(9 , 5),
        new Vector2Int(9 , 4),
        new Vector2Int(9 , 3),
        new Vector2Int(9 , 2),
        new Vector2Int(9 , 1),
        new Vector2Int(9 , 0),
        new Vector2Int(9 , -1),
        new Vector2Int(9 , -2),
        new Vector2Int(9 , -3),
        new Vector2Int(9 , -4),
        new Vector2Int(9 , -5),
        new Vector2Int(9 , -6),
        new Vector2Int(9 , -7),
        new Vector2Int(9 , -8),
        //10
        //top line
        new Vector2Int(-10 , 10),
        new Vector2Int(-9 , 10),
        new Vector2Int(-8 , 10),
        new Vector2Int(-7 , 10),
        new Vector2Int(-6 , 10),
        new Vector2Int(-5 , 10),
        new Vector2Int(-4 , 10),
        new Vector2Int(-3 , 10),
        new Vector2Int(-2 , 10),
        new Vector2Int(-1 , 10),
        new Vector2Int(0 , 10),
        new Vector2Int(1, 10),
        new Vector2Int(2, 10),
        new Vector2Int(3, 10),
        new Vector2Int(4, 10),
        new Vector2Int(5, 10),
        new Vector2Int(6, 10),
        new Vector2Int(7, 10),
        new Vector2Int(8, 10),
        new Vector2Int(9, 10),
        new Vector2Int(10 , 10),
        //bottom line
        new Vector2Int(-10 , -10),
        new Vector2Int(-9 , -10),
        new Vector2Int(-8 , -10),
        new Vector2Int(-7 , -10),
        new Vector2Int(-6 , -10),
        new Vector2Int(-5 , -10),
        new Vector2Int(-4 , -10),
        new Vector2Int(-3 , -10),
        new Vector2Int(-2 , -10),
        new Vector2Int(-1 , -10),
        new Vector2Int(0 , -10),
        new Vector2Int(1, -10),
        new Vector2Int(2, -10),
        new Vector2Int(3, -10),
        new Vector2Int(4, -10),
        new Vector2Int(5, -10),
        new Vector2Int(6, -10),
        new Vector2Int(7, -10),
        new Vector2Int(8, -10),
        new Vector2Int(9, -10),
        new Vector2Int(10 , -10),
        
        //left side
        new Vector2Int(-10 , 9),
        new Vector2Int(-10 , 8),
        new Vector2Int(-10 , 7),
        new Vector2Int(-10, 6),
        new Vector2Int(-10 , 5),
        new Vector2Int(-10 , 4),
        new Vector2Int(-10 , 3),
        new Vector2Int(-10 , 2),
        new Vector2Int(-10 , 1),
        new Vector2Int(-10 , 0),
        new Vector2Int(-10 , -1),
        new Vector2Int(-10 , -2),
        new Vector2Int(-10 , -3),
        new Vector2Int(-10 , -4),
        new Vector2Int(-10 , -5),
        new Vector2Int(-10 , -6),
        new Vector2Int(-10 , -7),
        new Vector2Int(-10 , -8),
        new Vector2Int(-10 , -9),
        
        //right side
        new Vector2Int(10 , 9),
        new Vector2Int(10 , 8),
        new Vector2Int(10 , 7),
        new Vector2Int(10, 6),
        new Vector2Int(10 , 5),
        new Vector2Int(10 , 4),
        new Vector2Int(10 , 3),
        new Vector2Int(10 , 2),
        new Vector2Int(10 , 1),
        new Vector2Int(10 , 0),
        new Vector2Int(10 , -1),
        new Vector2Int(10 , -2),
        new Vector2Int(10 , -3),
        new Vector2Int(10 , -4),
        new Vector2Int(10 , -5),
        new Vector2Int(10 , -6),
        new Vector2Int(10 , -7),
        new Vector2Int(10 , -8),
        new Vector2Int(10 , -9),
    };

        DefineFront(); //tell us what direction we facing 
    }

    public void PlayClip(AudioClip clip)
    {
        if (!_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
    private void CreateSoldier(Vector3 position)
    {
        var navPoint = Instantiate(navPrefab, position, Quaternion.Euler(0, 45 * facingDirection, 0));
        navObjects.Add(navPoint);

        navPoint.transform.parent = rectangleNavParent.transform;
        navPoint.transform.localScale = new Vector3(.1f, .1f, .1f);

        //soldier generation
        var newSoldier = Instantiate(soldierPrefab, position, Quaternion.Euler(0, 45 * facingDirection, 0));
        newSoldier.GetComponent<NavMeshAgent>().enabled = false;
        //setting position?
        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(position, out closestHit, 500, 1))
        {
            navPoint.transform.position = closestHit.position;
            newSoldier.transform.position = closestHit.position;
            newSoldier.GetComponent<NavMeshAgent>().enabled = true;
        }
        soldierObjects.Add(newSoldier);
        var teamSetter = newSoldier.GetComponent<SetTeamColor>();

        var agent = newSoldier.GetComponent<NavMeshAgent>();
        var updater = newSoldier.GetComponent<UpdateAgentDestination>();
        updater.positionInUnitList = soldierNum;
        updater.thisNavPoint = navPoint;
        updater.navAgent = agent;
        updater.parentPiece = this;
        var animator = newSoldier.GetComponent<Animator>();
        updater.animator = animator;
        var teamColor = gameInit.teamColors[0];

        int num = 0;
        foreach (TeamColor color in gameInit.teamColorDefinitions)
        {

            if (team == color)
            {
                teamColor = gameInit.teamColors[num];
                break;
            }
            num++;
        }
        teamSetter.SetTeamMaterial(teamColor); //set team color material
        teamSetter.SetTeamMaterial(teamColor); //set team color material
        newSoldier.transform.parent = gameInit.modelsParent.transform;
    }

    private void Start()
    {

        SetRotation(facingDirection);
        if (isCampaignToken) //if commander find menu controller so you can load scenes
        {
            var menuObj = GameObject.Find("Menu Controller");
            menuController = menuObj.GetComponent(typeof(MenuController)) as MenuController;
        }



        board.UnitList.Add(this); //this will hopefully give us a way to find this unit in mp
        unitID = board.unitNumber;
        board.unitNumber++;
        //set marker grid based on team color but not dependent on specific colors
        int num = 0;
        foreach (var color in gameInit.teamColorDefinitions)
        {
            num++;
            if (team == color)
            {
                if (num == 1) //blue
                {
                    Debug.Log("set markergrid");
                    thisMarkerGrid = board.markerGrid;
                }
                else if (num == 2) //yellow
                {
                    Debug.Log("set markergrid2");
                    thisMarkerGrid = board.markerGrid2;
                }
                else if (num == 3) //white
                {
                    Debug.Log("set markergrid1");
                    thisMarkerGrid = board.markerGrid;
                }
                else if (num == 4) //black
                {
                    Debug.Log("set markergrid2");
                    thisMarkerGrid = board.markerGrid2;
                }
            }
        }

        //make soldiers
        //soldier generation in formation
        if (models == 1)
        {
            Vector3 position = new Vector3(transform.position.x, transform.position.y - hexOffsetY, transform.position.z);
            CreateSoldier(position);
        }
        else
        {//example: 45 units, 7 per row, num rows = 6
            var tempModelCount = models;
            for (int i = 0; i < numberOfRows; i++) //repeat for number of rows
            {
                if (i % 2 == 0)
                {
                    actualOffset = hexOffset - .1f;
                }
                else
                {
                    actualOffset = hexOffset;
                }

                for (int j = 0; j < rowSize; j++) //make a soldier up to row size
                {
                    tempModelCount--; //subtract from tempModelCount
                    Vector3 position = new Vector3(transform.position.x - actualOffset + j * .2f, transform.position.y - hexOffsetY, transform.position.z - hexOffsetZ + i * .1f);
                    CreateSoldier(position);

                }
            }
            if (tempModelCount > 0) //if there are soldiers still remaining that haven't been created
            {
                for (int j = 0; j < tempModelCount; j++) //make a soldier up to row size
                {
                    Vector3 position = new Vector3(transform.position.x - actualOffset + j * .2f, transform.position.y - hexOffsetY, transform.position.z - hexOffsetZ + (numberOfRows + 1) * .1f);
                    CreateSoldier(position);
                }
            }
        }





        for (int i = 0; i < numberOfDudesInCircle; i++)
        {
            var angle = 360 / numberOfDudesInCircle;
            var navPoint = Instantiate(navPrefab2, new Vector3(transform.position.x + circleRadius, transform.position.y, transform.position.z), Quaternion.Euler(0, 45 * facingDirection, 0));
            navPoint.transform.parent = circleNavParent.transform;
            navPoint.transform.localScale = new Vector3(.1f, .1f, .1f);
            navPoint.transform.RotateAround(transform.position, Vector3.up, angle * i);
            navObjectsCircle.Add(navPoint);
        }
        for (int i = 0; i < numberOfDudesInCircle2; i++)
        {
            var angle = 360 / numberOfDudesInCircle2;
            var navPoint = Instantiate(navPrefab2, new Vector3(transform.position.x + circleRadius2, transform.position.y, transform.position.z), Quaternion.Euler(0, 45 * facingDirection, 0));
            navPoint.transform.parent = circleNavParent.transform;
            navPoint.transform.localScale = new Vector3(.1f, .1f, .1f);
            navPoint.transform.RotateAround(transform.position, Vector3.up, angle * i);
            navObjectsCircle.Add(navPoint);
        }
        for (int i = 0; i < numberOfDudesInCircle3; i++)
        {
            var angle = 360 / numberOfDudesInCircle3;
            var navPoint = Instantiate(navPrefab2, new Vector3(transform.position.x + circleRadius3, transform.position.y, transform.position.z), Quaternion.Euler(0, 45 * facingDirection, 0));
            navPoint.transform.parent = circleNavParent.transform;
            navPoint.transform.localScale = new Vector3(.1f, .1f, .1f);
            navPoint.transform.RotateAround(transform.position, Vector3.up, angle * i);
            navObjectsCircle.Add(navPoint);
        }

        for (int i = 0; i < numberOfRows; i++) //repeat for number of rows
        {
            if (i % 2 == 0)
            {
                actualOffset = hexOffset - .1f + .2f;
            }
            else
            {
                actualOffset = hexOffset + .2f;
            }
            for (int j = 0; j < models / numberOfRows; j++)
            {
                var navPoint = Instantiate(navPrefab, new Vector3(transform.position.x - actualOffset + j * .225f, transform.position.y - hexOffsetY, transform.position.z - hexOffsetZ - .2f + i * .2f), Quaternion.Euler(0, 45 * facingDirection, 0));
                navObjectsStaggered.Add(navPoint);

                navPoint.transform.parent = staggeredNavParent.transform;
                navPoint.transform.localScale = new Vector3(.1f, .1f, .1f);
            }
        }
        rectangleNavParent.transform.Rotate(new Vector3(0, 45 * facingDirection, 0));
        circleNavParent.transform.Rotate(new Vector3(0, 45 * facingDirection, 0));
        staggeredNavParent.transform.Rotate(new Vector3(0, 45 * facingDirection, 0));

        foreach (var soldier in soldierObjects) //teleports soldiers to where they need to be
        {
            var updater = soldier.GetComponent<UpdateAgentDestination>();
            soldier.transform.position = updater.thisNavPoint.transform.position;
        }

        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        //CheckIfEnemyInFront();
        //ChangeFormation("circle");
        modelBar.ImmediateSetHealth(models);
        moraleBar.ImmediateSetHealth(morale);
        energyBar.ImmediateSetHealth(energy);
    }

    public void DisplayFormation(string formationType)
    {
        if (formationType == "nothing")
        {
            rectangleNavParent.SetActive(false);
            circleNavParent.SetActive(false);
            staggeredNavParent.SetActive(false);
        }

        else if (formationType == "circle")
        {
            rectangleNavParent.SetActive(false);
            circleNavParent.SetActive(true);
            staggeredNavParent.SetActive(false);
        }
        else if (formationType == "rectangle" || formationType == "braced")
        {

            rectangleNavParent.SetActive(true);
            circleNavParent.SetActive(false);
            staggeredNavParent.SetActive(false);

        }
        else if (formationType == "staggered")
        {

            rectangleNavParent.SetActive(false);
            circleNavParent.SetActive(false);
            staggeredNavParent.SetActive(true);

        }
    }
    public void ChangeFormation(string formationType) //make this work in mp please
    {
        if (formationType == "circle")
        {
            Debug.Log("Circle");
            for (int i = 0; i < soldierObjects.Count; i++)
            {
                if (soldierObjects[i] == null || navObjectsCircle[i] == null)
                {
                    continue;
                }
                var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                updater.thisNavPoint = navObjectsCircle[i];
            }
        }
        else if (formationType == "rectangle" || formationType == "braced")
        {

            Debug.Log("Rectangle/Braced");
            for (int i = 0; i < soldierObjects.Count; i++)
            {
                if (soldierObjects[i] == null || navObjects[i] == null)
                {
                    continue;
                }
                var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                updater.thisNavPoint = navObjects[i];
            }

            //extra effects based on which formation chosen
        }
        else if (formationType == "staggered")
        {

            Debug.Log("Staggered");
            for (int i = 0; i < soldierObjects.Count; i++)
            {
                if (soldierObjects[i] == null || navObjects[i] == null)
                {
                    continue;
                }
                var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                updater.thisNavPoint = navObjectsStaggered[i];
            }
        }
    }

    public void UpdateTerrainType(int x, int y)
    {
        board.PieceUpdateTerrainType(unitID, x, y);
    }
    public void OnUpdateTerrainType(int x, int y)
    {
        terrainSpeedModifier = 0;
        var previousTerrainType = OnTerrainType;
        OnTerrainType = board.terrainGrid[x, y]; //check to see if mp has access to this
        //OnTerrainType = board.terrainGrid[occupiedSquare.x, occupiedSquare.y];
        //Debug.Log("update" + x + " " + y);
        defenseModifier = 0;
        if (previousTerrainType == "road" && OnTerrainType != "road") //if we go off road
        {
            if (sprinting)
            {
                speed = sprintSpeed;
            }
            else
            {
                speed = originalSpeed;
            }
        }
        if (OnTerrainType == "mud")
        {

            /*if (unitType == "cavalry") //cavalry should go to 1 in mud
            {
                speed = 1;
            }
            else
            {
                speed--; //subtract one speed
                remainingMovement--;
                if (speed < 0)
                {
                    speed = 0;
                }
                if (remainingMovement < 0)
                {
                    remainingMovement = 0;
                }
            }*/
            speed = 1;
            remainingMovement = 1;
            defenseModifier = -1;
        }
        else if (OnTerrainType == "hill")
        {
            if (previousTerrainType != "hill") //if we are moving uphill
            {
                speed = 0;
                remainingMovement = 0;
            }
        }
        else if (OnTerrainType == "foliage")
        {
            if (unitType == "infantry")
            {
                rangedDamageReduction = 1; //+1 armor against ranged attacks
            }
            speed--; //subtract one speed
            remainingMovement--;
            if (speed < 0)
            {
                speed = 0;
            }
            if (remainingMovement < 0)
            {
                remainingMovement = 0;
            }
        }
        else if (OnTerrainType == "trench")
        {
            defenseModifier = 2;
            fireDamageBonus = 2; //but fire deals extra damage
        }
        else if (OnTerrainType == "wall")
        {
            defenseModifier = 100;
        }
        else if (OnTerrainType == "road")
        {
            if (startOfTurn) //only do once, at start of turn . . .?
            {
                //speed++;
                //remainingMovement++;
                terrainSpeedModifier = 1;
                ////Debug.LogError("Updated speed to" + speed);
            }
        }
        else if (OnTerrainType == "shallow river")
        {
            if (unitType == "cavalry") //cavalry should go to 1 in mud
            {
                speed = 1;
            }
            else
            {
                speed--; //subtract one speed
                remainingMovement--;
                if (speed < 0)
                {
                    speed = 0;
                }
                if (remainingMovement < 0)
                {
                    remainingMovement = 0;
                }
            }
            defenseModifier = -2;
        }


        //Debug.LogError("updated terrain type of unit " + unitID + " to " + OnTerrainType);

    }

    public void StartMoveCoroutines(int waveNum)
    {
        startOfTurn = false;

        StartCoroutine(MovePiece(waveNum)); //coroutine so that we can pause it for a bit and then come back
    }
    public void SetRotation(int direction)
    {
        //transform.Rotate(0f, 45 * direction, 0.0f, Space.Self);
        DefineFlanks();
        DefineFront();
    }

    public bool IsFromSameTeam(Piece piece)
    {
        return team == piece.team;
    }

    public bool CanMoveTo(Vector2Int coords)
    {
        return availableMoves.Contains(coords);
    }
    public void ClearQueuedMoves()
    {
        Debug.Log("Clear moves and lines");
        board.ClearMoves(unitID);
    }
    public void IsQueuedMoveDiagonal(Vector2Int coords, Vector2Int queuedPosition) //use to find if our attempted queue move will be diagonal or not
    {
        for (int i = 0; i < diagonalTiles.Length; i++) //check each diagonal
        {
            Vector2Int nextCoords = queuedPosition + diagonalTiles[i];
            if (coords == nextCoords)
            {
                moveDirection = diagonalTiles[i];
                //Debug.Log("diagonal");
                return;
            }
        }
        moveDirection = Vector2Int.zero; //no result
    }
    public bool diagonalMove(Vector2Int coords, Vector2Int queuedPosition) //use to find if our attempted queue move will be diagonal or not
    {
        for (int i = 0; i < diagonalTiles.Length; i++) //check each diagonal
        {
            Vector2Int nextCoords = queuedPosition + diagonalTiles[i];
            if (coords == nextCoords)
            {
                return true;
            }
        }
        return false;
    }
    public void CheckIfPiece(int num, Vector2Int queuedPosition)
    {
        Vector2Int nextCoords = queuedPosition + cardinalTiles[num];
        //Debug.Log(nextCoords + "nextcoords");
        Piece piece = board.GetPieceOnSquare(nextCoords); //fetch piece on this tile, if there is one
        //Debug.Log(thisMarkerGrid[nextCoords.x, nextCoords.y]);
        var gridMarker = thisMarkerGrid[nextCoords.x, nextCoords.y];
        if (piece != null) //if we find a piece
        {
            //Debug.Log("Found piece in first tile");
            if (num == 3)
                num = 0;
            else
                num++;
            CheckIfMarker(num, piece, queuedPosition); //check if marker on next tile
        }
        else if (gridMarker != null && turnTime > 1) // if we find a marker
        {
            //Debug.Log("Found marker in first tile");
            if (num == 3)
                num = 0;
            else
                num++;
            piece = gridMarker.parentPiece;
            CheckIfMarker(num, piece, queuedPosition); //check if marker on next tile
        }
        else//if we don't find a piece or a marker
        {//check the next cardinal position
            if (num == 3)
                num = 0;
            else
                num++;
            //Debug.Log(num);
            nextCoords = queuedPosition + cardinalTiles[num]; //fetch new coords
            piece = board.GetPieceOnSquare(nextCoords); //fetch piece on this tile, if there is one
            gridMarker = thisMarkerGrid[nextCoords.x, nextCoords.y];
            if (piece != null) //if we find a piece
            {
                //Debug.Log("Found piece in second tile");
                if (num == 0)
                    num = 3;
                else
                    num--;
                CheckIfMarker(num, piece, queuedPosition); //check if marker on next tile
            }
            else if (gridMarker != null && turnTime > 1) // if we find a marker
            {
                //Debug.Log("Found piece in first tile");
                if (num == 0)
                    num = 3;
                else
                    num--;
                piece = gridMarker.parentPiece;
                CheckIfMarker(num, piece, queuedPosition); //check if marker on next tile
            }
            //if we don't find a piece, just don't do anything
        }
    }

    private void CheckIfMarker(int num, Piece piece, Vector2Int queuedPosition)
    {
        Vector2Int nextCoords = queuedPosition + cardinalTiles[num];
        var gridMarker = thisMarkerGrid[nextCoords.x, nextCoords.y];
        if (gridMarker != null && gridMarker.parentPiece == piece) //if we find a gridmarker and it's parent is the piece we found earlier
        {
            //Debug.Log("Found marker belonging to first piece");
            cancelQueueMovement = true; //cancel queue movement
            return;
        }
        //Debug.Log("No marker found");
    }

    public void QueueMove(Vector2Int coords)
    {
        Debug.Log("Queuing move");

        turnTime++; //turn time needs to be 1 for reasons   (this means first queued move has turn time of 1, so trying to get last queued move should be queuedMove[turnTime-1]
        holdingPosition = true;
        holdTime = turnTime - 1; //for example, second move holdtime 1



        
        Vector2Int queuedPosition = occupiedSquare; //so this will let us determine the position after moves are applied
        for (int i = 0; i < queuedMoves.Count; i++)
        {
            Vector2Int distance2 = queuedPosition - queuedMoves[i]; //first find distance between current position and new position
            queuedPosition -= distance2; //then subtract this distance to get the new position again
        }
        //Debug.Log(queuedPosition + "queued position");

        cancelQueueMovement = false;
        IsQueuedMoveDiagonal(coords, queuedPosition); //i think this needs to use queuedCoords to determine diagonal
        if (moveDirection != Vector2Int.zero) //if we grab a direction, then check corresponding cardinal tiles based on that direction
        {
            //Debug.Log("Since diagonal, checking for pieces");
            //Debug.Log(moveDirection);
            if (moveDirection == diagonalTiles[0]) //northeast
            {
                //Debug.Log("initiating check on north tile");
                CheckIfPiece(0, queuedPosition); //check north east
            }
            else if (moveDirection == diagonalTiles[1]) //southeast
            {
                //Debug.Log("initiating check on east tile");
                CheckIfPiece(1, queuedPosition); //check east south
            }
            else if (moveDirection == diagonalTiles[2]) //southwest
            {
                //Debug.Log("initiating check on south tile");
                CheckIfPiece(2, queuedPosition); //check north east
            }
            else if (moveDirection == diagonalTiles[3]) //northwest
            {
                //Debug.Log("initiating check on west tile");
                CheckIfPiece(3, queuedPosition); //check north east
            }
        }
        if (cancelQueueMovement) //if ordered to cancel queue movement
        {
            turnTime--; //but if we cancel the move then reset it.
            return;
        }
        //Debug.Log(board.markerGrid[coords.x, coords.y]);

        if (attacking) //attack tile gets overwritten anyways so this should work with new system
        {
            //attackTile = coords;
            //Debug.Log(attackTile);
            board.PieceCommunicateAttackTile(unitID, coords.x, coords.y);
        }
        //start of conflict resolution
        var gridMarker = thisMarkerGrid[coords.x, coords.y]; //if we find a marker on the coords we're moving to and it conflicts with us

        if (attacking && gridMarker != null && gridMarker.team == team && gridMarker.turnTime == turnTime) //if we're attacking,
        {
            if (holdingPosition || gridMarker.parentPiece.holdingPosition) //if either of us are holding position
            {
                if (holdTime < gridMarker.parentPiece.holdTime) //if our hold time is less, then we can queue a move but we need to deselect our piece so that we can't do any shenanigans
                {
                    markForDeselect = true;
                    Debug.Log("clicked on a marker that would conflict but we are holding position so it's cool, but we need to deselect");
                }
                else if (holdTime == gridMarker.parentPiece.holdTime) // if our hold positions are the same 
                {
                    Debug.Log("clicked on a marker that would conflict but we are holding position so it's cool");
                }
                else // and we're not holding pos
                {
                    Debug.Log("clicked on a marker that would result in conflict1");
                    turnTime--; //but if we cancel the move then reset it.
                    return;
                }
            }
            else// and we're not holding pos
            {
                Debug.Log("clicked on a marker that would result in conflict2");
                turnTime--; //but if we cancel the move then reset it.
                return;
            }
        }
        else if (gridMarker != null && gridMarker.team == team && gridMarker.turnTime == turnTime) // and we're not holding pos
        {
            Debug.Log("clicked on a marker that would result in conflict3");
            turnTime--; //but if we cancel the move then reset it.
            return;
        }
        /*else if (attacking)
        {//Debug.Log("turn time met speed");
            var facingDirection = 0;
            Vector2Int directionVector = coords - queuedPosition;
            for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
            {
                if (adjacentTiles[t] == directionVector)
                {
                    facingDirection = t; //direction of the queued move (the attack direction)
                }
            }
            bool inFrontArea = false;
            foreach (var dir in frontDirections)
            {
                if (facingDirection == dir) //this is the desired case
                {
                    inFrontArea = true;
                    break;
                }
                else
                {
                    continue;
                }
            }
            if (inFrontArea == false)
            {
                Debug.Log("clicked on area not in front when attacking");
                turnTime--;
                return;
            }

        }*/
        //end of checking to see if we should cancel move or not

        //Debug.Log(turnTime);
        FinishedMoving = false;
        if (remainingMovement <= 0 && !moveAndAttackEnabled)
        {
            ClearQueuedMoves();
        }

        //start of remaining movement logic
        Vector2 distance = queuedPosition - coords;
        float absDistance;
        if (Mathf.Abs(distance.x) > 1 || Mathf.Abs(distance.y) > 1) //if distance is greater than 1
        {
            if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y)) //use whichever value is higher
            {
                absDistance = Mathf.Abs(distance.x);
            }
            else
            {
                absDistance = Mathf.Abs(distance.y);
            }
            //Debug.Log(absDistance);
            if (absDistance > 1 || absDistance > remainingMovement) //if exceeds movement just don't do it fam, also if it doesn't make a complete path (1 after another)
            {
                if (!moveAndAttackEnabled && attackType == "ranged" && attacking) //if i'm a ranged unit and i can't move and attack and i'm attacking
                {
                    if (absDistance > remainingMovement) //basically, we don't care if how much distance there is but we still need to respect range
                    {
                        ////Debug.LogError("Cancelling movement 1");
                        turnTime--;
                        return;
                    }
                }
                else if (moveAndAttackEnabled && attackType == "ranged" && attacking && turnTime <= 1)
                {
                    ////Debug.LogError("Cancelling movement 2");
                    turnTime--; //every time we cancel a move, we need to reset the turn time
                    return;
                }
                else if (moveAndAttackEnabled && attackType == "ranged" && attacking && turnTime >= 2)
                {
                    if (absDistance > remainingMovement) //basically, we don't care if how much distance there is but we still need to respect range
                    {
                        ////Debug.LogError("Cancelling movement 3");
                        turnTime--;
                        return;
                    }
                }
                else if (attackType == "ranged" && !attacking)
                {
                    ////Debug.LogError("Cancelling movement 4");
                    turnTime--; //every time we cancel a move, we need to reset the turn time
                    return;
                }
                else if (attackType == "melee")
                {
                    ////Debug.LogError("Cancelling movement 5");
                    turnTime--; //every time we cancel a move, we need to reset the turn time
                    return;
                }
            }

            else
            {
                remainingMovement -= absDistance; //but if it is within, then we can subtract
            }
        }
        else
        {
            remainingMovement--; //subtract 1 movement
        }
        //end of checking if we have enough movement for this

        //if attack moving: reset to original speed.
        /*if (moveAndAttackEnabled && attackType == "ranged" && attacking && turnTime >= 1)
        {
            speed = originalSpeed;
        }*/
        ////Debug.LogError("Placing marker");
        PlaceMarker(coords, queuedPosition); //just place a marker


        //beginning of attack logic
        targetToAttackPiece = null; //reset and then set if attacking
        if (attacking)
        {
            CommunicateTargetToAttackPiece(coords); //tell mp 
            //sound effect logic
            int random = Random.Range(0, attackOrderSoundEffects.Length);
            PlayClip(attackOrderSoundEffects[random]);
        }

        stashedMoves.Clear();
        for (int i = 0; i < queuedMoves.Count; i++)
        {
            stashedMoves.Add(queuedMoves[i]);
        }

        //CheckIfTerrainShouldStopUsFromQueuingMoreMoves();


        if (moving || sprinting)
        {
            if (remainingMovement <= 0)
            {
                //sound effect logic
                int random = Random.Range(0, moveOrderSoundEffects.Length);
                PlayClip(moveOrderSoundEffects[random]);
            }
        }
        //trying to update available movement

        //board.squareSelector.ClearSelection();
        board.ShowSelectionSquares(SelectAvailableSquares(queuedMoves[queuedMoves.Count - 1]), this); //select based on last queued move position

    }

    private void CheckIfTerrainShouldStopUsFromQueuingMoreMoves()
    {
        var lastQueuedMoveNum = queuedMoves.Count - 1;
        //if queued move is on hill
        var terrainTypeAtQueuedPos = board.terrainGrid[queuedMoves[lastQueuedMoveNum].x, queuedMoves[lastQueuedMoveNum].y];

        var penultimateQueuedMoveNum = queuedMoves.Count - 2;

        var terrainTypeAtPenultimateQueuedPos = "grass";
        if (penultimateQueuedMoveNum < 0)
        {
            terrainTypeAtPenultimateQueuedPos = board.terrainGrid[occupiedSquare.x, occupiedSquare.y];
            ////Debug.LogError("terrain last" + terrainTypeAtQueuedPos + "terrain penult" + terrainTypeAtPenultimateQueuedPos);
            ////Debug.LogError("terrain last" + queuedMoves[lastQueuedMoveNum] + "terrain penult" + occupiedSquare);
        }
        else
        {
            terrainTypeAtPenultimateQueuedPos = board.terrainGrid[queuedMoves[penultimateQueuedMoveNum].x, queuedMoves[penultimateQueuedMoveNum].y];
            ////Debug.LogError("terrain last" + terrainTypeAtQueuedPos + "terrain penult" + terrainTypeAtPenultimateQueuedPos);
            ////Debug.LogError("terrain last" + queuedMoves[lastQueuedMoveNum] + "terrain penult" + queuedMoves[penultimateQueuedMoveNum]);
        }

        if (terrainTypeAtQueuedPos == "hill" && terrainTypeAtPenultimateQueuedPos != "hill") //if we queue a move onto a hill from non hill
        {
            ////Debug.LogError("Setting movement to 0");
            remainingMovement = 0;
        }
        else if (terrainTypeAtQueuedPos != "road" && terrainTypeAtPenultimateQueuedPos == "road") //if we queue a move onto a non road from a road
        {
            if (sprinting)
            {
                remainingMovement = sprintSpeed - queuedMoves.Count; //normally remaining movement would be sprint speed + 1 - queued moves
            }
            else if (attacking)
            {
                remainingMovement = originalSpeed + 1 - queuedMoves.Count; //normally remaining movement would be sprint speed + 1 - queued moves
            }
            else
            {//in the case of the knight, our remaining movement on road would be 3, and speed off road would be 2
                //if we queue a move off of road, remaining movement is now 2 because we have spent 1 movement. since we are off road we can only spend one more movement 
                remainingMovement = originalSpeed - queuedMoves.Count;
            }
        }

    }


    public void OnCommunicateAttackTile(int x, int y)
    {
        attackTile = new Vector2Int(x, y);
    }
    public void CommunicateTargetToAttackPiece(Vector2Int coords)
    {
        board.PieceCommunicateTargetToAttackPiece(unitID, coords.x, coords.y);
    }

    public void OnCommunicateTargetToAttackPiece(int x, int y)
    {
        targetToAttackPiece = null;
        Vector2Int coords = new Vector2Int(x, y);
        targetToAttackPiece = board.GetPieceOnSquare(coords);
        targetedSquare = coords;
    }

    private void PlaceMarker(Vector2Int coords, Vector2Int queuedPosition)
    {
        ////Debug.LogError("Placed marker");
        var x = coords.x;
        var y = coords.y;
        board.CommunicateQueuedMoves(unitID, x, y); //tell multiplayer where pieces will move and stuff and  . . .. also turn time and hold time
        board.CommunicateTurnHoldTime(unitID, turnTime, holdTime);
        //queuedMoves.Add(coords); //store these coords in moves list
        Vector3 targetPosition = board.CalculatePositionFromCoords(coords); //calculate worldspace coords 
        //marker code

        var x2 = targetPosition.x;
        var y2 = targetPosition.y;
        var z2 = targetPosition.z;

        int intRemainMovement = (int)remainingMovement;
        board.CommunicateMarkers(unitID, x2, y2, z2, x, y, team.ToString(), intRemainMovement);

        GameObject markerVisual;
        //Debug.LogError("remaining movement" + remainingMovement);
        if (remainingMovement <= 0) //if attacking and last queued move set arrow instead of circle (not working when clicking on enemy unit twice?attacking &&  && attackType == "melee"
        {
            markerVisual = Instantiate(arrowMarkerVisualPrefab, targetPosition, Quaternion.identity);

            var facingDirection = 0;
            Vector2Int directionVector = coords - queuedPosition;
            for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
            {
                if (adjacentTiles[t] == directionVector)
                {
                    facingDirection = t;
                }
            }

            Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0); //set rotation goal

            markerVisual.transform.Rotate(rotationGoal);


        }
        else
        {

            markerVisual = Instantiate(markerVisualPrefab, targetPosition, Quaternion.identity);
        }

        markerVisuals.Add(markerVisual);

        NavMeshHit closestHit;
        if (NavMesh.SamplePosition(targetPosition, out closestHit, 500, 1))
        {
            Vector3 newPos = new Vector3(closestHit.position.x, closestHit.position.y, closestHit.position.z); // - 0.063f
            markerVisual.transform.position = newPos;
            /*
                        Vector3 temp1 = new Vector3(0, 0.01f, 0);
                        markerVisual.transform.position += temp1;*/
        }




        //var matSetter = markerVisual.GetComponentsInChildren<MaterialSetter>();
        foreach (var matSetter in markerVisual.GetComponentsInChildren<MaterialSetter>()) //necessary to change all the pieces
        {
            if (attacking && remainingMovement > 0)
            {
                matSetter.SetSingleMaterial(gameInit.red);

            }
            else if (attacking && attackType == "ranged")
            {
                matSetter.SetSingleMaterial(gameInit.red);

            }
            else if (disengaging)
            {
                matSetter.SetSingleMaterial(gameInit.yellow);

            }
            /*if (disengaging)
            {

                matSetter.SetSingleMaterial(gameInit.disengageMaterial);
            }
            else if (attacking)
            {
                matSetter.SetSingleMaterial(gameInit.attackMaterial);

            }
            else if (turning)
            {
                matSetter.SetSingleMaterial(gameInit.turnMaterial);

            }
            else
            {
                matSetter.SetSingleMaterial(gameInit.defaultMaterial);
            }*/
        }
        //end of marker code

        LineRenderer line = Instantiate(linePrefab, targetPosition, Quaternion.identity); //then place a line there. make sure it uses worldspace!
        instantiatedLines.Add(line); //add it to the list so we can delete if need be
        LineCollision lineCollisionScript = line.GetComponent(typeof(LineCollision)) as LineCollision;
        lineCollisionScript.parentPiece = this;


        if (disengaging)
        {

            line.startColor = Color.yellow;
            line.endColor = Color.yellow;
        }
        else if (attacking)
        {
            line.startColor = Color.red;
            line.endColor = Color.red;

        }
        else if (turning)
        {
            line.startColor = Color.white;
            line.endColor = Color.white;

        }
        else
        {
            line.startColor = Color.green;
            line.endColor = Color.green;
        }
        List<Vector3> pos = new List<Vector3>();
        Vector3 queuedPosConverted = board.CalculatePositionFromCoords(queuedPosition); //calculate worldspace coords
        Vector3 temp2 = new Vector3(0, 0.1f, 0);
        queuedPosConverted += temp2;
        targetPosition += temp2;
        pos.Add(queuedPosConverted);
        pos.Add(targetPosition);
        line.SetPositions(pos.ToArray()); //all this does is set a line from the marker to the last point so that we can visualize the path taken

        //needs to be set after line positions are set
        if (attacking && attackType == "ranged")
        {
            /*   foreach (var item in instantiatedCylinders)
               {
                   Destroy(item);
               }
               instantiatedCylinders.Clear();
               lineCollisionScript.ApproximateCollision();*/
        }
        foreach (var item in instantiatedCylinders)
        {
            Destroy(item);
        }
        instantiatedCylinders.Clear();
        lineCollisionScript.ApproximateCollision(); //places actual cylindersx 
        //place aesthetic cylinders
        lineCollisionScript.PlaceAestheticCylinders();
        if (attackType == "ranged")
        {
            StartCoroutine(WaitForPhysics());
        }


    }

    public IEnumerator WaitForPhysics() //this exists so that we don't have to do all the processing in one frame
    {
        Debug.Log("Checking");
        var numNotFinished = 0;

        foreach (var item in instantiatedCylinders) //go through each of their cylinders
        {
            var script = item.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;
            if (script.finishedProcessing == false) //basically if not finished processing
            {
                numNotFinished++;
            }
        }


        if (numNotFinished == 0) //if all of them are finished
        {
            if (arcingAttack == false) //if musketeer: check if obstructed. if bowman: don't!
            {
                for (int i = 0; i < instantiatedCylinders.Count; i++) //start at 5th cylinder
                {
                    var cylinder = instantiatedCylinders[i];
                    var script = cylinder.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;
                    Piece unit = script.unitOnTile; //fetch the unit associated with this cylinder
                    if (unit == this || unit == null || i < 4)  //skip the first 4
                    {
                        continue; //skip any cylinders that are this unit
                    }
                    else if (OnTerrainType == "hill" && unit.OnTerrainType != "hill") //if we're on a hill, and they're not, we can shoot over them
                    {
                        continue;
                    }
                    else if (unit.IsFromSameTeam(this)) //if that unit is from the same team, cancel the attack
                    {
                        SpawnEvent("Shot blocked by friendly!", Color.red, 2.5f);
                        break;
                    }
                    /*else if (!unit.IsFromSameTeam(this)) //if that unit is from the same team, cancel the attack
                    {
                        SpawnEvent("Shot blocked by enemy", Color.white, 2.5f);
                        break;
                    }*/
                }
            }

            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(.1f);
            StartCoroutine(WaitForPhysics());
        }

    }

    public void SpawnEvent(string str, Color color, float time)
    {
        strList.Add(str);
        colorList.Add(color);
        floatList.Add(time);
    }

    public void PlaySelection()
    {

        //sound effect logic
        int random = Random.Range(0, selectOrderSoundEffects.Length);
        PlayClip(selectOrderSoundEffects[random]);
    }
    public bool EnemyAdjacent() //actually detects adjacent enemies that aren't disengaging
    {
        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                continue;
            }
            if (piece != null && !piece.IsFromSameTeam(this) && !piece.disengaging)// && !piece.disengaging
            {
                return true; //if detect enemy, don't start movement at all;
            }
        }
        return false; //if no enemy, no enemy adjacent
    }
    public void CheckIfEnemiesAdjacent()
    {
        if (EnemyAdjacent()) //simply checks each adjacent tile to see if there is an enemy there
        {
            enemyAdjacent = true;
        }
        else
        {
            enemyAdjacent = false;
        }


        if (targetToAttackPiece != null && TargetAdjacent(targetToAttackPiece)) //if we have a target and they are next to us, then:
        {
            targetAdjacent = true;
        }
        else
        {
            targetAdjacent = false;
        }
        Debug.Log(enemyAdjacent + "enemy adjacent");
    }

    public void CheckIfMarkersOverlap()
    {
        for (int i = 0; i < instantiatedMarkers.Count; i++) // go through each of this piece's markers
        {

            //Debug.Log("checking markers");
            var friendlyMarker = instantiatedMarkers[i];
            Vector2Int funnyCoords = instantiatedMarkers[i].coords;

            if (board.CheckIfCoordinatedAreOnBoard(funnyCoords)) //if these coords are on the board
            {
                var enemyMarker = board.markerGrid2[funnyCoords.x, funnyCoords.y]; //dummy value
                                                                                   //we need to set enemy markers based on new team colors.
                int num = 0;
                foreach (var color in gameInit.teamColorDefinitions)
                {
                    num++;
                    if (team == color)
                    {
                        if (num == 1)
                        {
                            //Debug.Log("set markergrid");
                            enemyMarker = board.markerGrid2[funnyCoords.x, funnyCoords.y];
                        }
                        else if (num == 2)
                        {
                            //Debug.Log("set markergrid2");
                            enemyMarker = board.markerGrid[funnyCoords.x, funnyCoords.y];
                        }
                        else if (num == 3)
                        {
                            //Debug.Log("set markergrid2");
                            enemyMarker = board.markerGrid2[funnyCoords.x, funnyCoords.y];
                        }
                        else if (num == 4)
                        {
                            //Debug.Log("set markergrid2");
                            enemyMarker = board.markerGrid[funnyCoords.x, funnyCoords.y];
                        }
                    }
                }
                if (enemyMarker != null)
                {
                    ////Debug.LogError("found an enemy marker overlapping one of our markers. searching for conflicts");
                    SearchForConflict(friendlyMarker, enemyMarker);
                }
            }


        }
    }

    public void CheckIfDead()
    {
        if (models <= 0) //are we dead?
        {// if so delete unit position from grid

            board.grid[occupiedSquare.x, occupiedSquare.y] = null;
            Debug.Log("dead");
        }
    }

    public void CheckIfRouting()
    {
        if (morale <= 0) //are we routing?
        {// if so delete unit position from grid

            //Debug.Log("routing");
            board.grid[occupiedSquare.x, occupiedSquare.y] = null;
            //board.routingGrid[occupiedSquare.x, occupiedSquare.y] = this; //add unit pos to routing grid so we can still keep track of it
            routing = true;
        }
    }

    public void SearchForConflict(Marker friendlyMarker, Marker enemyMarker)
    {

        tieBreakPiece = enemyMarker.parentPiece;
        var ourPiece = friendlyMarker.parentPiece;
        ////Debug.LogError(enemyMarker.turnTime + "friendly" + friendlyMarker.turnTime);
        if (enemyMarker.turnTime == friendlyMarker.turnTime) //if we find an enemy marker on the same coords and they have the same turn time
        { //then we need to check if one is attacking or not. if so, check the hold time. if hold time 0, and turn time 1, obviously turn time 1 wins and no arbitration needed. 
            //we need to check if we're both attacking or not: if so check if 

            if (tieBreakPiece.attacking && ourPiece.attacking && tieBreakPiece.holdTime == ourPiece.holdTime)
            {
                Debug.Log("attack move conflict");
            }
            else
            {

                if (tieBreakPiece.attacking && tieBreakPiece.holdTime < ourPiece.turnTime)
                {
                    conflict = false;
                    conflictTime = 0;
                    tieBreakPiece.conflict = false;
                    Debug.Log("resetting conflict time to " + conflictTime);
                    return;
                }
                if (ourPiece.attacking && ourPiece.holdTime < tieBreakPiece.turnTime)
                {
                    conflict = false;
                    conflictTime = 0;
                    tieBreakPiece.conflict = false;
                    Debug.Log("resetting conflict time to " + conflictTime);
                    return;
                }
            }

            //end of returns
            Debug.Log("we found a marker overlapping ours"); //send to board for arbitration
            conflict = true;
            tieBreakPiece.conflict = true;
            if (tieBreakPiece != null)
            {
                board.prosecutor = this;
                board.defendant = tieBreakPiece;
                board.ArbitrateConflict();
                //board.ArbitrateConflict(this, tieBreakPiece);
            }
            conflictTime = friendlyMarker.turnTime;
            Debug.Log("Setting conflict time to " + conflictTime);
        }
        else
        {
            conflict = false;
            conflictTime = 0;
            tieBreakPiece.conflict = false;
            Debug.Log("resetting conflict time to " + conflictTime);
        }

    }
    public void CalculateDamage()
    {
        board.PieceCalculateDamage(unitID);
    }
    public void OnCalculateDamage() //calculate damage
    {
        //don't calculate damage if we already have or we have no attack target
        if (alreadyCalculatedDamage)
        {
            return;
        }
        if (targetToAttackPiece == null)
        {
            return;
        }
        //Debug.Log("Calculating damage" + unitID);

        //Debug.Log(models);
        //Debug.Log(meleeDamage);
        //Debug.Log(targetToAttackPiece.armor);
        //Debug.Log(targetToAttackPiece.health);

        //initialize temp variables
        var attackBonus = 0;
        float meleeMultiplier = 1;
        float rangedMultiplier = 1;
        float energyMultiplier = 1f;

        //Attacker damage - defender armor = total damage

        if (targetToAttackPiece.OnTerrainType != "hill" && OnTerrainType == "hill") //bonus from attacking downhill
        {
            attackBonus = 1; //+1 damage attacking down hill
        }
        else if (OnTerrainType != "hill" && targetToAttackPiece.OnTerrainType == "hill") //debuff froom attacking uphill
        {
            attackBonus = -1; //-1 damage attacking up hill
        }
        
        
        if (targetToAttackPiece.currentFormation == "braced") //-50% to melee damage dealt to braced units facing us
        {
            if (targetToAttackPiece.CheckIfFacingEnemy(this)) //if target is facing this attacker
            {
                if (attackType == "melee")
                {
                    //Debug.Log("reducing damage of attacker by 50%");
                    meleeMultiplier = .5f;
                }
            }
        }
        else if (targetToAttackPiece.currentFormation == "staggered") //+1 melee damage to staggered units, -50% ranged damage
        {
            if (attackType == "melee")
            {
                attackBonus += 1;
            }
            else if (attackType == "ranged")
            {
                rangedMultiplier = .5f;
            }

        }
        else if (targetToAttackPiece.currentFormation == "circle") //-25% damage to circle formation units
        {
            if (attackType == "melee")
            {
                meleeMultiplier = .75f;
            }
        }

        if (energy < startingEnergy && energy > startingEnergy * .5f)
        {
            //no penalty
        }
        else if (energy < startingEnergy * .5f && energy > 0) //-25% damage
        {
            energyMultiplier = .75f;
        }
        else if (energy <= 0) //-50% damage
        {
            energyMultiplier = .5f;
        }
        
        var calculatedDamage = damage + attackBonus - armorLevel - targetToAttackPiece.defenseModifier; //base damage
        
        if (calculatedDamage < 0) //make it so it's not negative
        {
            calculatedDamage = 0;
        }
        //Debug.Log("Calculated damage" + calculatedDamage);

        float disengageMultiplier = 1;
        if (targetToAttackPiece.disengaging && attackType == "melee")
        {
            disengageMultiplier = .5f;
        }

        if (attackType == "melee") //ignores accuracy for melee units
        {
            accuracy = 1f;
        }

        tempDamage = calculatedDamage * meleeMultiplier * rangedMultiplier * energyMultiplier * flankingDamage * accuracy * disengageMultiplier; //apply all possible multipliers

        //Debug.Log("Unit ID " + unitID + "temp damage" + tempDamage + "models" + models + "calculated damage" + calculatedDamage + "Damage effect" + damageEffect + "melee multiplier" + meleeMultiplier + "ranged multiplier" + rangedMultiplier + "energy multiplier" + energyMultiplier + "flanking damage" + flankingDamage + "accuracy" + accuracy);

        queuedDamage = tempDamage; //no longer defenders killed, just damage
        if (queuedDamage < 0)
            queuedDamage = 0; //just make sure damage can never be negative

        alreadyCalculatedDamage = true;
    }

    public bool CheckIfStopMove()
    {
        //Debug.Log("trying to move");
        ////Debug.LogError("tie break win " + wonTieBreak);
        //if there's no conflict or we wont the tiebreak
        //Debug.Log("conflict" + conflict + "wonTiebreak?" + wonTieBreak + "conflictTime" + conflictTime + "queueTime" + queueTime);
        if (attacking && queuedMoves.Count == 1) //when implementing things with higher attack, change this to a variable that tells you at what move to stop
        {
            Debug.Log("stopping because hold position attack");
            return true;
        }

        if (conflict && conflictTime == queueTime) //if there is a conflict on the same timeframe
        {
            if (!wonTieBreak)
            {
                Debug.Log("We lost the tie break, so stop");
                return true;
            }

        }
        Debug.Log(attacking + " " + queueTime + " " + speed);
        //turning = false;

        if (attacking && queueTime == speed) //if attacking and we're up to original speed already, stop moving
        { //this doesn't seem to ever be met?
            Debug.Log("Halting because out of speed and attacking" + queueTime + speed);
            return true;
        }
        /*else if (queueTime == holdTime)
        {
            turning = true;
            Debug.Log("Halting because we met holdtime (means we're turning)" + queueTime + holdTime);
            return true;

        }
*/

        if (attacking && targetAdjacent && targetToAttackPiece != null && attackType == "melee" && aggressiveAttitude == true) //if attacking and target is right next to us and we're melee, stop moving
        {
            Debug.Log("Halting because target next to us and we're aggressive");
            return true;
        } //ranged units  will prioritize finishing their move before attacking, whereas melee units will try to attack as soon as possible 
        if (attackerPiece != null) //if we are being attacked . . . by a melee piece
        { 
            if (attackerPiece.attackType == "melee" && !disengaging && TargetAdjacent(attackerPiece))
            { 
                Debug.Log("Halting because pinned");
                SpawnEvent("Pinned!", Color.red, 5f);
                return true;
            }
        }

        var checkCoords = queuedMoves[queueTime - 1];
        if (board.grid[checkCoords.x, checkCoords.y] != null) //if the position we're trying to move into is full
        {
            Debug.Log("Halting because the position is full");
            return true;
        }

        Debug.Log("Found no reason to stop");
        return false;
    }

    /*public void QueueRout() //this will only be called if the thing is actually routing, check board
    {
        var xdifference = 0;
        var ydifference = 0;
        var xdirection = "left";
        var ydirection = "down";
        var directionToGo = "down";
        //check which direction will get us off the map fastest
        //fetch board size: standard 8, so top right is 7,7, bottom left is 0, 0
        // compare against current coords
        //determine the direction we should go
        if (occupiedSquare.x < board.BOARD_SIZE / 2) //8/2 = 4, so less than would be 3
        {
            //rout leftv
            xdifference = occupiedSquare.x; //eg all the way on left side = 0;
            xdirection = "left";
        }
        else //4 or greater 
        {
            //rout right
            xdifference = board.BOARD_SIZE - 1 - occupiedSquare.x; // all the way on right side is 7-7 = 0
            xdirection = "right";
        }
        if (occupiedSquare.y < board.BOARD_SIZE / 2)
        {
            ydifference = occupiedSquare.y;
            ydirection = "down";
        }
        else
        {
            ydifference = board.BOARD_SIZE - 1 - occupiedSquare.y;
            ydirection = "up";
        }

        if (xdifference < ydifference)//now we should see if this distance is less than vertical difference. if it is, then go left.
        {
            if (xdirection == "left") //this means that we're closer to the left than the right
            {
                directionToGo = "left";
            }
            else //xdirection right
            {
                directionToGo = "right";
            }
        }
        else if (xdifference > ydifference)//check which direction is closer vertically
        {
            if (ydirection == "up")
            {
                directionToGo = "up";
            }
            else
            {
                directionToGo = "down";
            }
        }
        else if (xdifference == ydifference)
        {
            if (ydirection == "up")
            {
                directionToGo = "north";
            }
            else
            {
                directionToGo = "south";
            }
            if (xdirection == "left") //this means that we're closer to the left than the right
            {
                directionToGo += "west";
            }
            else //xdirection right
            {
                directionToGo += "east";
            }
        }
        Vector2Int directionVector = ConvertDirectionToCoords(directionToGo);
        Debug.Log(directionVector);
        var coords = occupiedSquare + directionVector;
        if (enemyAdjacent) //if there are enemies next to us, queue a "disengage"
        {
            disengaging = true;
        }
        else //sprint
        {

        }
        PlaceMarker(coords, occupiedSquare); //first destination, second original pos

    }*/

    public Vector2Int ConvertDirectionToCoords(string direction)
    {
        if (direction == "up")
        {
            return adjacentTiles[0];
        }
        else if (direction == "northeast")
        {
            return adjacentTiles[1];
        }
        else if (direction == "right")
        {
            return adjacentTiles[2];
        }
        else if (direction == "southeast")
        {
            return adjacentTiles[3];
        }
        else if (direction == "down")
        {
            return adjacentTiles[4];
        }
        else if (direction == "southwest")
        {
            return adjacentTiles[5];
        }
        else if (direction == "left")
        {
            return adjacentTiles[6];
        }
        else if (direction == "northwest")
        {
            return adjacentTiles[7];
        }
        else
        {
            return adjacentTiles[0];
        }
    }

    public IEnumerator MovePiece(int waveNum) //moves piece once according to queued movement
    {
        /*if (routing)
        {
            //If they run into a friendly unit, stop the frliendly’s movement. If they end on friendly, idk
            //If they run into an enemy unit, they attack them
            //If they move off the edge of the map, disappear and reappear later
            //agent.destination = navPoint.transform.position;
            queueTime++;
            //Debug.Log(queueTime);
            if (queueTime > 10) //overflow check
            {
                Debug.Log("overflow stop");
                oneStepFinished = true;
                FinishedMoving = true;
                yield break;
            }
            if (queuedMoves.Count > 0) //if there are moves
            {
                //Debug.Log("there are queued moves");
                FinishedMoving = false;
                oneStepFinished = false;
                if (CheckIfStopMove()) //if we need to stop movement for whatever reason
                {
                    Debug.Log("Found a reason to stop");
                    oneStepFinished = true;
                    FinishedMoving = true;
                    HandleMovementStoppage(); //then we shall attempt to stop our movement
                    yield break;
                }
                else
                {
                    Debug.Log("we are moving because we could not find a reason to stop" + team);
                    
                    SubtractEnergy();
                    for (int i = 0; i < soldierObjects.Count; i++)
                    {
                        if (soldierObjects[i] != null)
                        {
                            var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                            updater.attacking = false;
                            updater.Unfreeze();
                        }
                    }
                    Vector2Int coords = queuedMoves[queueTime - 1];
                    Vector3 targetPosition = board.CalculatePositionFromCoords(coords); //kiosk
                    if (!turning)
                    {
                        //sets new coords be our piece, and old coords to null
                        board.UpdateBoardOnPieceMove(coords, occupiedSquare, this, null); //must be called before occupied square is actually updated
                                                                                          //determine direction that we're moving
                        Vector2Int directionVector = coords - occupiedSquare;
                        for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
                        {
                            if (adjacentTiles[t] == directionVector)
                            {
                                facingDirection = t;
                            }
                        }

                        occupiedSquare = coords; //now change position
                        float distance = Vector3.Distance(targetPosition, transform.position);
                        Tween tween = transform.DOMove(targetPosition, distance / tweenSpeed).SetEase(Ease.Linear);
                        //Tween tween = transform.DOMove(targetPosition, 0).SetEase(Ease.Linear);

                        Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0);
                        Tween rotateTween = transform.DORotate(rotationGoal, tween.Duration());
                        //yield return tween.WaitForCompletion();
                        if (diagonalMove(occupiedSquare, coords))
                        {
                            yield return new WaitForSeconds(tween.Duration() + 3.5f);
                        }
                        else
                        {
                            yield return new WaitForSeconds(tween.Duration() + 1);

                        }

                    }
                    else
                    {
                        Vector2Int directionVector = coords - occupiedSquare;
                        for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
                        {
                            if (adjacentTiles[t] == directionVector)
                            {
                                facingDirection = t;
                            }
                        }
                        Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0);
                        Tween rotateTween = transform.DORotate(rotationGoal, 3);
                        yield return rotateTween.WaitForCompletion();
                    }
                    //Debug.Log("Moved");
                    HandleMovementStoppage();
                }
            }
            else //if no moves, stop
            {
                //Debug.LogError("Setting queue time to 0");
                queueTime = 0;
                oneStepFinished = true;
                FinishedMoving = true;
                HandleMovementStoppage(); //then we shall attempt to stop our movement
                yield break;
            }
        }*/
        if (queuedFormation != "nothing") //switch formation if queued formation is something other than default
        {
            currentFormation = queuedFormation;
            ChangeFormation(queuedFormation);
            //add something here to delay it a bit
            oneStepFinished = true;
            FinishedMoving = true;
            hasMoved = true;
            remainingMovement = speed;
            ClearQueuedMoves();
            SelectAvailableSquares(occupiedSquare);
            conflict = false;
            wonTieBreak = false;
            queuedFormation = "nothing";
            DisplayFormation(queuedFormation);
        }
        else //normal movement
        {
            //checking to see if we should pause this units' movement  (this doesn't work with sprinting) (also breaks attacking?)
            if (queueTime == 0 && queuedMoves.Count > 0) //if first wave ; actually wave num not important anymore waveNum == 0 && 
            {
                var dist = occupiedSquare - queuedMoves[0];
                ////Debug.LogError("Dist" + dist);
                if (Mathf.Abs(dist.x) <= 1 || Mathf.Abs(dist.y) <= 1) //if an adjacent move
                {

                    ////Debug.LogError(queueTime + "queue Time");
                    var checkCoords = queuedMoves[queueTime];
                    //////Debug.LogError("checkcoords" + checkCoords);
                    if (board.grid[checkCoords.x, checkCoords.y] != null && board.grid[checkCoords.x, checkCoords.y].IsFromSameTeam(this)) //if the position we're trying to move into is full and is occupied by a teammate
                    {
                        Debug.Log("Halting because the position is full");
                        if (!board.secondPassMoveWave.Contains(this))
                        {

                            board.secondPassMoveWave.Add(this);
                        }
                        yield break;
                    }
                    else //if it opens up, remove us from list
                    {
                        markForRemovalFromSecondWave = true;
                    }
                }



            }

            //agent.destination = navPoint.transform.position;
            queueTime++;
            safeQueueTime++;
            //Debug.Log(queueTime);
            if (queueTime > 10) //overflow check
            {
                Debug.Log("overflow stop");
                oneStepFinished = true;
                FinishedMoving = true;
                yield break;
            }
            if (queueTime <= queuedMoves.Count && queuedMoves.Count > 0) //if our queuetime has not exceeded queuedMoves total, and we have moves at all
            {
                //Debug.Log("there are queued moves");
                FinishedMoving = false;
                oneStepFinished = false;

                if (CheckIfStopMove()) //Check to see if we need to stop moving for the whole turn
                {
                    Debug.Log("Found a reason to stop");
                    oneStepFinished = true; //we've finished moving one step
                    FinishedMoving = true; //and we're also done moving totally
                    HandleMovementStoppage(); //then we shall attempt to stop our movement
                    yield break;
                }
                else //if we have no reason to stop moving
                {
                    //Debug.Log("we are moving because we could not find a reason to stop" + team);
                    SubtractEnergy();
                    UnfreezeSoldiers();
                    Debug.Log(queueTime + "queue time");
                    Vector2Int coords = queuedMoves[queueTime - 1]; //get coordinates of the move we are executing
                    Vector3 targetPosition = board.CalculatePositionFromCoords(coords); //find physical space based on coordinates

                    if (queueTime - 1 == holdTime && !attacking) //turning to a direction
                    {
                        Vector2Int directionVector = coords - occupiedSquare;
                        for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
                        {
                            if (adjacentTiles[t] == directionVector)
                            {
                                facingDirection = t;
                            }
                        }
                        Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0);

                        Tween rotateTween = transform.DORotate(rotationGoal, 0);
                        //yield return rotateTween.WaitForCompletion();

                    }
                    else //normal movement
                    {
                        //sets new coords be our piece, and old coords to null
                        board.UpdateBoardOnPieceMove(coords, occupiedSquare, this, null); //must be called before occupied square is actually updated

                        //determine direction that we're moving
                        Vector2Int directionVector = coords - occupiedSquare;
                        for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
                        {
                            if (adjacentTiles[t] == directionVector)
                            {
                                facingDirection = t;
                            }
                        }

                        occupiedSquare = coords; //now change position
                        float distance = Vector3.Distance(targetPosition, transform.position); //find the distance
                        Tween tween = transform.DOMove(targetPosition, distance / tweenSpeed).SetEase(Ease.Linear); //start tweening to position (only moves health bar object)
                        //Tween tween = transform.DOMove(targetPosition, 0).SetEase(Ease.Linear);

                        Vector3 rotationGoal = new Vector3(0, 45 * facingDirection, 0); //set rotation goal
                        Tween rotateTween = transform.DORotate(rotationGoal, tween.Duration()); //rotate towards rotation goal (todo make it so that hp bar doesnt rotate)
                        //yield return tween.WaitForCompletion();
                        if (diagonalMove(occupiedSquare, coords)) //if we're moving diagonally give tween more time bc more distance to cover
                        {
                            yield return new WaitForSeconds(tween.Duration() + 3.5f);
                        }
                        else
                        {
                            yield return new WaitForSeconds(tween.Duration() + 1);

                        }
                    }
                    //Debug.Log("Moved");
                    HandleMovementStoppage(); //if we finish moving or turning for this queued move, stop;
                }
            }
            else //if no queued moves, stop
            {
                safeQueueTime = queueTime;
                queueTime = 0;
                oneStepFinished = true;
                FinishedMoving = true;
                HandleMovementStoppage(); //then we shall attempt to stop our movement
                yield break;
            }
        }


    }

    private void UnfreezeSoldiers()
    {

        for (int i = 0; i < soldierObjects.Count; i++)
        {
            if (soldierObjects[i] != null)
            {
                var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                updater.attacking = false;
                updater.Unfreeze();
            }
        }
    }
    private void SubtractEnergy()
    {
        if (sprinting && OnTerrainType != "road") //if sprinting and not on road (thus if sprinting and on road, normal energy --)
        {
            energy -= 1.25f;
        }
        else
        {
            energy--;
        }
        energyBar.SetHealth(energy);
    }
    public void HandleMovementStoppage()
    {
        //Debug.Log("Handling movement stoppage" + this.occupiedSquare);
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y); //update what terrain we're on
        DefineFlanks();
        DefineFront(); //update direction facing when finishing a movement step
        if (moveAndAttackEnabled && attackType == "ranged" && attacking && queueTime >= queuedMoves.Count - 1) //if move attacking as ranged unit and moved up to penult
        {
            oneStepFinished = true;
            FinishedMoving = true;
            hasMoved = true;
            remainingMovement = speed;
            ClearQueuedMoves();
            SelectAvailableSquares(occupiedSquare);
            conflict = false;
            wonTieBreak = false;
            //board.CheckIfAllMovesFinished();
        }
        else if (attacking && queueTime >= speed - 1) //if attacking, and we've moved once
        {
            oneStepFinished = true;
            FinishedMoving = true;
            hasMoved = true;
            remainingMovement = speed;
            ClearQueuedMoves();
            SelectAvailableSquares(occupiedSquare);
            conflict = false;
            wonTieBreak = false;
            //board.CheckIfAllMovesFinished();
        }
        else if (queueTime >= speed || queueTime == 0) //if queue time has met speed or unit has not chosen to move
        {
            //Debug.Log("Queue time has met speed");
            //Debug.Log("Done with coroutines, now updating available squares");
            oneStepFinished = true;
            FinishedMoving = true;
            hasMoved = true;
            remainingMovement = speed;
            ClearQueuedMoves();
            SelectAvailableSquares(occupiedSquare);
            conflict = false;
            wonTieBreak = false;
            //board.CheckIfAllMovesFinished();
        }
        else //queue time hasn't met speed, which means we still have more moves to process
        {
            Debug.Log("Queue time hasn't met speed");
            //Debug.Log("One step finished");
            oneStepFinished = true;
            //board.OneStepFinished();
        }
    }

    public void CheckIfEnemyInAttackTile()
    {
        /*if (attackTile == null) //if attack tile is not set, make it the tile directly in front of us;
        {
            attackTile = 
        }*/
        Piece potentialEnemy = board.GetPieceOnSquare(attackTile); //attacking commented out
        if (targetToAttackPiece == null && attackTile != null && potentialEnemy != null && !potentialEnemy.IsFromSameTeam(this)) //if we're attacking but we don't have a target, look at tile marked by attacking unit
        {
            Debug.Log("Found enemy in attack tile");
            targetToAttackPiece = potentialEnemy;
        }
    }

    public void CheckIfEnemyInFirstQueuedMove()
    {
        if (stashedMoves.Count <= 0)
        {
            //////Debug.LogError("we have no moves" + this);
            return;
        }

        if (!attacking || !enemyAdjacent)
        {
            return;
        }


        if (enemyAdjacent && attacking)
        {

            Piece piece = board.GetPieceOnSquare(stashedMoves[0]); //if we've moved and then had to stop we adjust this by the number of moves we've done so far
            if (piece != null && !piece.IsFromSameTeam(this))
            {
                targetToAttackPiece = piece; //set new target

                attackTile = stashedMoves[0];


            }
        }
    }

    public void CheckIfEnemyInRelativeStashedMove()
    {
        if (stashedMoves.Count <= 0 || !attacking)
        {
            //////Debug.LogError("we have no moves" + this);
            return;
        }

        ////Debug.LogError("queuetime" + queueTime);
        var normalizedQueueTime = queueTime - 1; //not sure why this is needed, but here we are
        if (normalizedQueueTime < 0)
        {
            normalizedQueueTime = 0;
        }
        if (stashedMoves.Count - 1 < normalizedQueueTime)
        {
            normalizedQueueTime = stashedMoves.Count - 1;
        }

        Piece piece = board.GetPieceOnSquare(stashedMoves[normalizedQueueTime]); //if we've moved and then had to stop we adjust this by the number of moves we've done so far
        if (piece != null && !piece.IsFromSameTeam(this))
        {
            targetToAttackPiece = piece; //set new target 
            attackTile = stashedMoves[normalizedQueueTime];
            Debug.LogError(targetToAttackPiece + "target");
        }
    }

    public void CheckIfEnemyNearUs()
    {
        //let's check to see if out target to attack piece is next to us or not

        for (int i = 0; i < adjacentTiles.Length; i++) //for each adjacent tile
        {
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[i];
            Piece checkTarget = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords)) //if off board skip
            {
                continue;
            }
            if (checkTarget != null && !checkTarget.IsFromSameTeam(this) && checkTarget == targetToAttackPiece) //if target tile has a piece and it's an enemy and it's our original target
            {
                attackTile = nextCoords; //set this just in case?
                return; //we can stop because we know the target is next to us and we can fight it
            }
        }
        //if we can't find the target, find a new one from the ones next to us (starting with the tile in front of us, then right, then left, then clockwise?
        targetToAttackPiece = null;
        CheckFronts(1); //front
        if (targetToAttackPiece != null)
        {
            return;
        }
        CheckFronts(0); //left

        if (targetToAttackPiece != null)
        {
            return;
        }
        CheckFronts(2); //right

        if (targetToAttackPiece != null)
        {
            return;
        }

        for (int i = 0; i < adjacentTiles.Length; i++) //for each adjacent tile 
        {
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[i];
            ////Debug.LogError(this + " checking" + nextCoords);
            Piece checkTarget = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords)) //if off board skip
            {
                continue;
            }
            if (checkTarget != null && !checkTarget.IsFromSameTeam(this)) //if target tile has a piece and it's an enemy 
            {
                targetToAttackPiece = checkTarget;
                attackTile = nextCoords; //set this just in case?
                break; //we can stop because we know the target is next to us and we can fight it
            }
        }


    }

    private void CheckFronts(int front)
    {
        int frontNum = frontDirections[front]; //the very front
        Vector2Int frontCoords = occupiedSquare + adjacentTiles[frontNum];
        ////Debug.LogError(this + " checking" + frontCoords);
        Piece piece = board.GetPieceOnSquare(frontCoords);
        if (board.CheckIfCoordinatedAreOnBoard(frontCoords) && piece != null && !piece.IsFromSameTeam(this)) //if we detect an enenmy on the front
        {//they can be our new target
            targetToAttackPiece = piece;
            attackTile = frontCoords;
        }
    }

    public void CalculateLineOfSight() //for attacking, ranged units
    {//TODO ADD ABILITY TO TARGET EMPTY AND DO OVERWATCH, ESSENTIALLY
        board.PieceCalculateLineOfSight(unitID);
    }
    public void OnCalculateLineOfSight()
    {

        Debug.Log("create new line");
        //create a new line because surely the old one got deleted
        targetedSquare = targetToAttackPiece.occupiedSquare;
        var targetCoords = targetToAttackPiece.occupiedSquare; //target coords is generated from targetAttackPiece's occupied square
        Vector3 targetPosition = board.CalculatePositionFromCoords(targetCoords); //calculate worldspace coords

        LineRenderer line = Instantiate(linePrefab, targetPosition, Quaternion.identity); //then place a line there. make sure it uses worldspace!
        instantiatedLines.Add(line); //add it to the list so we can delete if need be

        LineCollision lineCollisionScript = line.GetComponent(typeof(LineCollision)) as LineCollision;
        lineCollisionScript.parentPiece = this; //set parent piece of script to this

        List<Vector3> pos = new List<Vector3>();
        Vector3 occupiedPosition = board.CalculatePositionFromCoords(occupiedSquare); //calculate position of this
        Vector3 temp2 = new Vector3(0, 0.1f, 0);
        occupiedPosition += temp2; //adjust above the board
        targetPosition += temp2; //adjust
        pos.Add(occupiedPosition); //set position to this position
        pos.Add(targetPosition); //and target position
        line.SetPositions(pos.ToArray()); //all this does is set a line from the marker to the last point so that we can visualize the path taken

        foreach (var item in instantiatedCylinders)
        {
            Destroy(item);
        }
        instantiatedCylinders.Clear();
        lineCollisionScript.ApproximateCollision();
        //create cylinders, which will hit tiles and give us data on the units on those tiles
    }

    public void RunThroughCylinders() //check to see if LOS is blocked
    {
        board.PieceRunThroughCylinders(unitID);
    }
    public void OnRunThroughCylinders()
    {
        var tileNumber = 0;

        var tileDistance = targetedSquare - occupiedSquare;
        Debug.Log(tileDistance + " tile distance"); //we can check to see if it's within our existing arrays to find how far it is away;
        bool foundIt = false;
        foreach (var item in adjacentTiles) //first look through here
        {
            if (tileDistance == item)
            {
                foundIt = true;
                tileNumber = 1;
                break;
            }
        }

        if (!foundIt)
        {
            foreach (var item in speed2) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 2;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed3) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 3;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed4) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 4;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed5) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 5;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed6) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 6;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed7) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 7;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed8) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 8;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed9) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 9;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (var item in speed10) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 10;
                    break;
                }
            }
        }
        /*if (!foundIt)
        {
            foreach (var item in speed11) //
            {
                if (tileDistance == item)
                {
                    foundIt = true;
                    tileNumber = 11;
                    break;
                }
            }
        }*/
        if (foundIt)
        {
            Debug.Log("Found it" + tileNumber);
        }
        if (arcingAttack == false) //if musketeer: check if obstructed. if bowman: don't!
        {
            for (int i = 0; i < instantiatedCylinders.Count; i++) //start at 5th cylinder
            {
                var cylinder = instantiatedCylinders[i];
                var script = cylinder.GetComponent(typeof(LineCollidePrefabScript)) as LineCollidePrefabScript;
                Piece unit = script.unitOnTile; //fetch the unit associated with this cylinder
                if (unit == this || unit == null || i < 4)  //skip the first 4
                {
                    continue; //skip any cylinders that are this unit
                }
                else if (OnTerrainType == "hill" && unit.OnTerrainType != "hill") //if we're on a hill, and they're not, we can shoot over them
                {
                    continue;
                }
                else if (unit.IsFromSameTeam(this)) //if that unit is from the same team, cancel the attack
                {
                    //not sure exactly how to cancel the attack
                    queuedDamage = 0;
                    tempDamage = 0;
                    attacking = false;
                    targetToAttackPiece = null;
                    SpawnEvent("Shot blocked by friendly!", Color.red, 2.5f);
                    break;
                }
                else if (!unit.IsFromSameTeam(this)) //if an enemy unit, set them as the new target
                {
                    targetToAttackPiece = unit; //set them as new target
                    break; //break out of loop
                }
            }
        }

        ApplyAccuracy(tileNumber);
    }
    public void CheckIfAttacked() //
    {
        defensiveAttacking = false;
        //check adjacent enemy units and see if any of them have a target equal to us
        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[i];
            Piece checkTarget = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords)) //if off board skip
            {
                continue;
            }
            //Debug.LogError("checktarget" + checkTarget + );
            if (checkTarget != null && !checkTarget.IsFromSameTeam(this) && checkTarget.targetToAttackPiece == this) //if target tile has a piece and it's an enemy and it's attacking us
            {
                
                //we will attack them back. this action does not turn the unit, so flanking damage is still applied. but unit can turn on a normal attack
                ////Debug.LogError("im under attack and i've got no orders");
                targetToAttackPiece = checkTarget; //set our target to be our attacker
                attacking = true;
                targetAdjacent = true; //we just proved they're next to us;
                defensiveAttacking = true; //prevent turning on the attack
                break;
            }
        }
    }

    public void ApplyAccuracy(int tileNumber)
    {
        accuracy = 1f;
        if (tileNumber == 1) //point blank
        {
            accuracy = .5f;
        }
        else if (tileNumber <= effectiveRange) // effective range
        {
            accuracy = 1f;
        }
        else if (tileNumber <= midRange) //midrange
        {
            accuracy = .5f;
        }
        else if (tileNumber <= longRange) // longrange
        {
            accuracy = .25f;
        }
        else if (tileNumber >= beyondTargetableRange) //beyond targetable, so no damage
        {
            accuracy = 0f;
            //accuracy = .25f;
        }
        /*else
        {
            accuracy = .25f;
        }*/
        //
        Debug.LogError("Tile number" + tileNumber + "beyond range" + beyondTargetableRange);
        Debug.Log("accuracy" + accuracy + "temp damage" + tempDamage);
    }
    public void ApplyDamage() //physical and morale damage
    {
        board.PieceApplyDamage(unitID); //tell mp
    }

    public void OnApplyDamage() //after communicating with mp
    {
        CheckIfEnemiesAdjacent();

        if (alreadyAppliedDamage)
        {
            Debug.LogError("already applied damage so returning" + "unit id" + unitID);
            return;
        }

        if (targetToAttackPiece == null || !attacking || queuedDamage < 0) //if target nonexistent, or not attacking, or queued damage is less than 0. 0 damage is still valid
        {
            Debug.LogError("Returning1" + "unit id" + unitID);
            return;
        }

        if (!targetToAttackPiece.disengaging && !targetAdjacent && attackType == "melee") //if target is not disengaging and target is not adjacent and attack type is melee
        {
            Debug.LogError("Returning2" + "unit id" + unitID + "target adjacent" + targetAdjacent + "enemy adjacent" + enemyAdjacent);
            return;
        }

        //Debug.Log(targetAdjacent + "" + targetToAttackPiece + "" + attacking + queuedDamage);
        targetToAttackPiece.attackerPiece = this;

        Debug.Log("attempting to apply damage" + queuedDamage + "target" + targetToAttackPiece + "attacking" + attacking + "unit id" + unitID);

        targetToAttackPiece.models -= queuedDamage;
        targetToAttackPiece.modelBar.SetHealth(targetToAttackPiece.models);
        Debug.Log("target models " + targetToAttackPiece.models);
        if (targetToAttackPiece.models < 0) //make sure models can't go below zero
            targetToAttackPiece.models = 0;

        /*if (targetToAttackPiece != null && queuedDamage > 0 && targetToAttackPiece.isCampaignObjective && menuController != null) //start mission if campaign
        {
            menuController.LoadSpecificScene(targetToAttackPiece.missionToLoad);

        }*/
        if (defensiveAttacking == false) //if we are defensive attacking, we do not turn
        {
            //this changes the direction of unit based on what direction we are attacking
            Vector2Int directionVector = attackTile - occupiedSquare; //start investigation here: attack tile is not being updated for some reason. occupied square is
            if (attackTile.x < occupiedSquare.x) //if target is on left of us
            {
                directionVector.x = -1;
            }
            else if (attackTile.x > occupiedSquare.x) //if target is on right of us
            {

                directionVector.x = 1;
            }

            if (attackTile.y < occupiedSquare.y) //if target is beneath us
            {

                directionVector.y = -1;
            }
            else if (attackTile.y > occupiedSquare.y) //if target is above us
            {

                directionVector.y = 1;
            }
            Debug.Log(directionVector + "dir vector" + occupiedSquare + "occ square");
            for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up// this doesnt work because the attack tile is not adjacent
            {
                if (adjacentTiles[t] == directionVector)
                {
                    facingDirection = t;
                    Debug.Log("updated facing direction");
                }
            }
            oldRotation = transform.localEulerAngles;
            rotationGoal = new Vector3(0, 45 * facingDirection, 0);
            Tween rotateTween = transform.DORotate(rotationGoal, 1);
        }

        SubtractEnergy();
        //board.PieceTriggerAttacksForSoldiers(unitID); //this works in mp! so why not define flanks?
        OnTriggerAttacksForSoldiers();
        ClearQueuedMoves();
        alreadyAppliedDamage = true;

    }

    public void OnTriggerAttacksForSoldiers() //finished communicating with mp
    {
        for (int i = 0; i < soldierObjects.Count; i++) //trigger attacks for soldier objects
        {
            if (soldierObjects[i] != null)
            {
                var updater = soldierObjects[i].GetComponent<UpdateAgentDestination>();
                updater.enemy = targetToAttackPiece;
                updater.Unfreeze();
                updater.queuedDamage = queuedDamage;
                updater.attacking = true;
                updater.id = i;
                updater.targetPiece = targetToAttackPiece;
                inflictedDeaths = 0;
                updater.navOffsetAdd = 0;
                updater.numberOfAttacks = 0;
                updater.idleSet = false;
                updater.moveSet = false;
                if (oldRotation != rotationGoal && attackType == "ranged")
                {
                    updater.rangedAndNeedsToTurnToFaceEnemy = true;
                    updater.rotationGoal = rotationGoal;
                    //Debug.LogError("old rotation does not equal rotation goal" + oldRotation + rotationGoal);
                }
                else if (attackType == "ranged")
                {
                    updater.ableToAttack = true;
                }

                updater.StartCoroutine(updater.AttackInterval());
                updater.StartCoroutine(updater.Freeze());
            }
        }
    }

    public IEnumerator SelfDestruct()
    {
        yield return new WaitUntil(() => allowedToDie);
        for (int i = 0; i < soldierObjects.Count; i++) //destroy all soldiers
        {
            Destroy(soldierObjects[i]);
        }

        Destroy(gameObject);
    }

    public void ImmediateRemoval()
    {
        foreach (var item in soldierObjects)
        {
            Destroy(item);
        }
        Destroy(gameObject);
    }

    public void MarkForDeath(float damage)
    {
        board.PieceMarkForDeath(unitID, damage);
    }

    public void OnMarkForDeath(float damage) //after communicating with mp
    {
        if (damage <= 0)
        {
            return;
        }
        //something is wrong in mp regarding model count and actual soldier count
        //modelBar.SetHealth(models);

        Debug.Log("Marking for death " + damage);
        //int scaledDamage = Mathf.RoundToInt(damage / downscale);
        int scaledDamage = Mathf.RoundToInt(damage);

        if (attackerPiece != null) //still need to make this based on direction
        {
            if (attackerPiece.attackType == "melee")
            {
                for (int i = 0; i < scaledDamage; i++)//linear marking
                {
                    var rowRandom = Random.Range(0, rowSize - 1); //random from front row
                    if (rowSize > soldierObjects.Count)
                    {
                        rowRandom = Random.Range(0, soldierObjects.Count - 1);
                    }
                    markedSoldiers.Add(soldierObjects[rowRandom]);
                    deadSoldiers.Add(soldierObjects[rowRandom]);
                    soldierObjects.RemoveAt(rowRandom); //remove it right away so it can't be marked twice
                    Debug.Log("marked " + i);
                }
            }
            else
            {

                for (int i = 0; i < scaledDamage; i++)//linear marking
                {
                    var random = Random.Range(0, soldierObjects.Count - 1);
                    markedSoldiers.Add(soldierObjects[random]);
                    deadSoldiers.Add(soldierObjects[random]);
                    soldierObjects.RemoveAt(random); //remove it right away so it can't be marked twice
                    Debug.Log("marked " + i);
                }
            }
        }

        //TODO need to make this start depending on direction . . .

        waitingForFirstAttack = true;
        markedSoldiersCount = markedSoldiers.Count;
        Debug.LogError("marked Soldiers" + markedSoldiersCount);
        StartCoroutine(KillOff());
    }


    public IEnumerator KillOff()
    {
        if (markedSoldiers.Count > 0) //if there are still soldiers to kill
        {
            if (waitingForFirstAttack && attackerPiece != null)
            {
                yield return new WaitUntil(() => attackerPiece.soldierAttacked == true); //wait until the attacker has attacked with at least one unit
                waitingForFirstAttack = false;
                //attackerPiece.soldierAttacked = false; //set it to false to be ready for the next one
            }
            yield return new WaitForSeconds(Random.Range(0.1f, 5 / markedSoldiersCount)); //this should make soldiers die in a more timely fashion. soldiers die faster the more there are to kill

            if (markedSoldiers[0] != null)
            {
                //initiate kill function
                var soldierScript = markedSoldiers[0].GetComponent<UpdateAgentDestination>();
                soldierScript.KillThis(markedSoldiers[0]);

                markedSoldiers.RemoveAt(0); //remove listing


                //yield return new WaitForSeconds(Random.Range(.1f, .5f)); //wait for some interval

                /*if (markedSoldiers.Count <= 0) //if no more soldiers
                {
                    allowedToDie = true;
                }*/
            }
            StartCoroutine(KillOff()); //prepare for next one

        }
        else //if there are no more soldiers to kill
        {
            yield return null;
        }
    }
    public void CheckFlankingDamage() //before attacks happen
    {
        board.PieceCheckFlankingDamage(unitID);
    }

    public void OnCheckFlankingDamage()
    {
        if (attacking) //this will turn us if we are attacking
        {
            Vector2Int directionVector = attackTile - occupiedSquare; //turn while attacking
            for (int t = 0; t < adjacentTiles.Length; t++) //check cardinal directions to see if they match up
            {
                if (adjacentTiles[t] == directionVector)
                {
                    facingDirection = t;
                }
            }
        }
        DefineFlanks();
        DefineFront();

        Debug.Log("checking flanking damage");
        if (currentFormation != "circle")
        {
            for (int i = 0; i < flankDirections.Count; i++)
            {
                int flank = flankDirections[i];
                Vector2Int nextCoords = occupiedSquare + adjacentTiles[flank];
                Piece piece = board.GetPieceOnSquare(nextCoords);
                if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
                {
                    continue;
                }
                else if (piece != null && !piece.IsFromSameTeam(this) && piece.attacking && piece.targetToAttackPiece == this) //if we detect an enenmy on the flank and it's attacking us
                {
                    Debug.Log("there is an attacking enemy on the flank");
                    if (i >= 1 && i <= 3) //rear tiles
                    {
                        //piece.flankingDamage = 3;
                        piece.flankingDamage = 2;
                    }

                    if (i == 0 || i == 4) //side tiles
                    {
                        piece.flankingDamage = 1.5f;
                    }
                }

            }
        }
    }

    public void ApplyMorale() //after attacks happen
    {
        if (models <= armyLossesThreshold && !armyLossesApplied)
        {
            morale -= 5;
            armyLossesApplied = true;

            SpawnEvent("Heavy losses!", Color.black, 5f);
        }

        if (morale <= startingMorale / 2 && !waveringEventTriggered)
        {
            waveringEventTriggered = true;

            SpawnEvent("Wavering!", Color.blue, 5f);
        }

        if (currentFormation != "circle")
        {

            CheckIfFlanked(); //check if flanked
            if (flankedByHowMany > 0)
            {
                //Debug.Log("morale drops by" + flankedByHowMany);
                //morale -= flankedByHowMany + bonusMoraleDamage;
                morale -= bonusMoraleDamage;
            }
        }
        moraleBar.SetHealth(morale); //tween hp bar
    }
    public void DefineFlanks()
    {
        flankDirections.Clear();
        for (int i = facingDirection; i < 5 + facingDirection; i++)
        {
            var b = 2 + i;
            if (b >= 8)
            {
                b -= 8;
            }
            flankDirections.Add(b);
        }
    }
    public void CheckIfFlanked()
    {
        DefineFlanks();
        flankedByHowMany = 0;
        bonusMoraleDamage = 0;
        for (int i = 0; i < flankDirections.Count; i++)
        {
            int flank = flankDirections[i];
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[flank];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                continue;
            }
            else if (piece != null && !piece.IsFromSameTeam(this)) //if we detect an enenmy on the flank
            {
                flankedByHowMany++;
                //Debug.Log("flanked by" + flankedByHowMany);
                if (i >= 1 && i <= 3) //rear tiles
                {
                    if (piece.attacking && piece.targetToAttackPiece == this) //bonus morale damage applied if attacking this in the rear
                    {
                        bonusMoraleDamage += 2;
                    }
                }

                if (i == 0 || i == 4) //side tiles
                {
                    if (piece.attacking && piece.targetToAttackPiece == this) //bonus morale damage applied if attacking this in the rear
                    {
                        bonusMoraleDamage += 1;
                    }
                }
            }
        }
    }
    public bool CheckIfFacingEnemy(Piece attackerPiece)
    {
        DefineFront(); //we need to update our front just in case
        for (int i = 0; i < frontDirections.Count; i++)
        {
            int front = frontDirections[i];
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[front];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                continue;
            }
            else if (piece != null && !piece.IsFromSameTeam(this) && piece == attackerPiece) //if we detect an enenmy on the front and it's the enemy we're loking for as well
            {
                return true;
            }
        }
        return false;
    }
    private void DefineFront()
    {
        frontDirections.Clear();
        for (int i = facingDirection; i < 3 + facingDirection; i++)
        {
            var b = -1 + i;

            if (b <= 0)
            {
                b += 8;
            }
            if (b >= 8)
            {
                b -= 8;
            }
            frontDirections.Add(b);
        }
    }
    public bool TargetAdjacent(Piece target)
    {
        if (target == null)
            return false;
        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + adjacentTiles[i];
            Piece checkTarget = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatedAreOnBoard(nextCoords))
            {
                continue;
            }
            if (checkTarget != null && !checkTarget.IsFromSameTeam(this) && checkTarget == target) //if check target exists, is enemy, and equals target 
            {
                Debug.LogError("found target" + checkTarget.unitID);
                return true; //if detect enemy, don't start movement at all;
            }
        }
        return false; //if no enemy, no enemy adjacent
    }

    protected void TryToAddMove(Vector2Int coords)
    {
        availableMoves.Add(coords);
    }

    public void SetData(Vector2Int coords, TeamColor team, Board board, int direction)
    {
        this.team = team;

        facingDirection = direction;
        //Debug.Log(facingDirection);
        occupiedSquare = coords;
        this.board = board;
        transform.position = board.CalculatePositionFromCoords(coords);
        
    }

    public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach (var square in availableMoves)
        {
            if (board.GetPieceOnSquare(square) is T)
                return true;
        }
        return false;
    }
    private void CheckFormationForSpeed()
    {
        if (currentFormation == "braced" || currentFormation == "circle")
        {
            speed = 0;
            remainingMovement = 0;
        }
    }
    public void SetStanceSprint()
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        disengaging = false;
        moving = false;
        attacking = false;
        turning = false;
        sprinting = true;
        moveAndAttackEnabled = false;
        //speed = sprintSpeed;

        ClearQueuedMoves(); //resets speed and such
        speed = originalSpeed; //reset speed, again i guess
        remainingMovement = speed; //reset speed, again i guess
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        speed = originalSpeed + terrainSpeedModifier;
        speed *= 2; //double speed
        speed++;
        remainingMovement = speed;

        if (OnTerrainType == "mud")
        {
            speed = 2;
            remainingMovement = 2;
        }
        CheckFormationForSpeed();
        if (energy <= 0) //if no energy, do normal movement
        {
            speed = originalSpeed + 1;
            remainingMovement = speed;
        }
        board.UpdateUIManager();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);

    }
    public void ResetRanges()
    {
        effectiveRange = originalEffectiveRange;
        midRange = originalMidRange;
        longRange = originalLongRange;
    }

    public void ResetStance()
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        markedDeaths = false;
        arbitratedConflict = false;
        ////Debug.LogError("Setting queue time to 0");
        queueTime = 0;
        safeQueueTime = 0;
        queuedDamage = 0;
        flankingDamage = 1;
        conflict = false;
        moving = true;
        wonTieBreak = false;
        disengaging = false;
        attacking = false;
        turning = false;
        sprinting = false;
        moveAndAttackEnabled = false;
        conflictTime = 0;
        speed = originalSpeed;
        ClearQueuedMoves();
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        speed = originalSpeed + terrainSpeedModifier;
        remainingMovement = speed;
        CheckFormationForSpeed();
    }
    public void SetStanceMove()
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        queuedDamage = 0;
        disengaging = false;
        moving = true;
        attacking = false;
        turning = false;
        sprinting = false;
        moveAndAttackEnabled = false;
        //bool wasSprinting = false;
        //if(speed > originalSpeed)
        //{
        //    wasSprinting = true;
        //}
        ClearQueuedMoves();
        speed = originalSpeed + 1; //extra speed for turning
        remainingMovement = speed;
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        speed = originalSpeed + terrainSpeedModifier + 1;
        remainingMovement = speed;
        //if (wasSprinting)
        //{
        CheckFormationForSpeed();

        board.UpdateUIManager();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);
        //}
        ////Debug.LogError("Setting stance to move, resetting speed to original speed");
    }
    public void SetStanceDisengage()
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        disengaging = true;
        moving = false;
        attacking = false;
        turning = false;
        sprinting = false;
        moveAndAttackEnabled = false;
        speed = originalSpeed;
        board.UpdateUIManager();
        ClearQueuedMoves();
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        speed = originalSpeed + terrainSpeedModifier;
        remainingMovement = speed;
        CheckFormationForSpeed();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);
    }
    public void SetStanceTurn()
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        disengaging = false;
        moving = false;
        attacking = false;
        turning = true;
        sprinting = false;
        moveAndAttackEnabled = false;
        speed = 1;
        board.UpdateUIManager();
        ClearQueuedMoves();
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);
        CheckFormationForSpeed();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);
    }
    public void SetStanceAttack() //steady attack
    {
        ResetRanges();
        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        disengaging = false;
        moving = false;
        attacking = true;
        turning = false;
        sprinting = false;
        moveAndAttackEnabled = false;

        ClearQueuedMoves();
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);

        if (attackType == "ranged")
        {

            effectiveRange = originalEffectiveRange + 1;
            midRange = originalMidRange + 1;
            longRange = originalLongRange + 1;

            if (OnTerrainType == "hill")
            {
                effectiveRange++;
                midRange++;
                longRange++;
            }


            speed = longRange;
            remainingMovement = speed;
        }
        else
        {
            speed = originalSpeed + terrainSpeedModifier + 1; //last bit of speed represents attack target, not movement
            remainingMovement = speed;
        }
        board.UpdateUIManager();

        CheckFormationForSpeed();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);
    }

    public void RecalculateAttackRanges()
    {
        midRange = Mathf.RoundToInt(effectiveRange * 1.5f);

        longRange = Mathf.RoundToInt(effectiveRange * 1.75f);

        beyondTargetableRange = 0;

        if (midRange == effectiveRange)
        {
            midRange++;
        }
        if (longRange < midRange)
        {
            longRange = midRange + 1;
        }
        else if (longRange == midRange)
        {
            longRange++;
        }
        beyondTargetableRange = longRange + 1;
    }
    public void SetStanceMoveAttack() //ranged move and attack
    {
        ResetRanges();


        markForDeselect = false;
        holdTime = 0;
        holdingPosition = false;
        disengaging = false;
        moving = false;
        attacking = true;
        turning = false;
        sprinting = false;
        Debug.Log("Move and attack enabled");
        moveAndAttackEnabled = true;

        ClearQueuedMoves();
        UpdateTerrainType(occupiedSquare.x, occupiedSquare.y);

        if (OnTerrainType == "hill" && attackType == "ranged")
        {
            effectiveRange++;
            midRange++;
            longRange++;
        }
        speed = originalSpeed + terrainSpeedModifier;
        remainingMovement = speed;

        board.UpdateUIManager();
        CheckFormationForSpeed();
        board.ShowSelectionSquares(SelectAvailableSquares(occupiedSquare), this);
    }

}
