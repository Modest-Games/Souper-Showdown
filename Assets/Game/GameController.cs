using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    // singleton class object
    public static GameController Instance { get; private set; }

    public enum GameState
    {
        Unstarted,
        Stopped,
        Running,
        Paused
    }

    public delegate void DebugDelegate();
    public static event DebugDelegate DebugEnabled;
    public static event DebugDelegate DebugDisabled;

    public delegate void GameStateDelegate();
    public static event GameStateDelegate GameCreated;
    public static event GameStateDelegate GameStarted;
    public static event GameStateDelegate GameStopped;
    public static event GameStateDelegate GamePaused;
    public static event GameStateDelegate GameResumed;

    [Header("Debug")]
    public bool debugEnabledByDefault;
    [ReadOnly] public bool isDebugEnabled;

    [Header("Game State")]
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.Unstarted);

    [Header("Game Config")]
    public int numStartingPollutants;
    public bool autoStart;
    [Tooltip("The duration of the game in seconds")] public float gameDuration;

    [SerializeField] [ReadOnly] private float gameTimeElapsed;
    private SoupPot_Behaviour soupPot;
    private ObjectSpawner spawner;
    private PlayerControlsMapping controls;

    public GameObject controllerMappingSprite;
    public int countdownTime;
    public Text countdownDisplay;

    private void Awake()
    {
        // singleton stuff
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        controls = new PlayerControlsMapping();

        // map relevant control inputs
        controls.Debug.ToggleDebug.performed += ctx => ToggleDebug();
    }

    void Start()
    {
        if (GameCreated != null)
            GameCreated();


        Debug.Log(gameState.Value);
        //gameState.Value = GameState.Stopped;
        spawner = FindObjectOfType<ObjectSpawner>();
        soupPot = FindObjectOfType<SoupPot_Behaviour>();
        isDebugEnabled = debugEnabledByDefault;

        // fire the enable or disable debug event
        if (isDebugEnabled)
        {
            if (DebugEnabled != null)
                DebugEnabled();
        }
        else
        {
            if (DebugDisabled != null)
                DebugDisabled();
        }

        // start the game after 3 seconds
        StartCoroutine(GameStartDelay());
    }

    public void InitializeGame()
    {
        if (IsServer)
        {
            // setup the map
            SetupMap();

            // start the game (server only)
            if (autoStart)
                gameState.Value = GameState.Running;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            switch (gameState.Value)
            {
                case GameState.Running:
                    // check if the game should be over
                    if (gameTimeElapsed >= gameDuration)
                    {
                        // stop the game
                        gameState.Value = GameState.Stopped;
                    } else
                    {
                        // add delta time to the time elapsed
                        gameTimeElapsed += Time.deltaTime;
                    }

                    break;
            }
        }
    }

    public void SetupMap()
    {
        TerrainManager.Instance.GenerateAllTerrain();

        // Spawn the starting pollutants:
        spawner.SpawnManyPollutants(numStartingPollutants);
    }

    public void StartGame()
    {
        Debug.Log("STARTING GAME");

        if (GameStarted != null)
            GameStarted();
    }

    private void PauseGame()
    {
        if (GamePaused != null)
            GamePaused();
    }

    private void ResumeGame()
    {
        if (GameResumed != null)
            GameResumed();
    }

    private void StopGame()
    {
        if (GameStopped != null)
            GameStopped();
    }

    void ToggleDebug()
    {
        // fire the enable or disable debug event
        if (isDebugEnabled)
        {
            if (DebugDisabled != null)
                DebugDisabled();

            isDebugEnabled = false;
        } else
        {
            if (DebugEnabled != null)
                DebugEnabled();

            isDebugEnabled = true;
        }

        Debug.Log("Debug toggled. Now set to: " + isDebugEnabled);
    }

    public float TimeRemaining
    {
        get
        {
            return Mathf.Max(gameDuration - gameTimeElapsed, 0f);
        }
    }

    private void OnSoupReceivedTrash(float amount)
    {
        if (gameState.Value == GameState.Stopped) return;

        spawner.SpawnPollutant();
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.LogFormat("old: {0}, new: {1}", oldState, newState);

        switch (newState)
        {
            case GameState.Stopped:
                StopGame();
                break;

            case GameState.Running:
                switch (oldState)
                {
                    case GameState.Paused:
                        ResumeGame();
                        break;

                    default:
                        StartGame();
                        break;
                }
                break;

            case GameState.Paused:
                PauseGame();
                break;
        }
    }

    private void OnSoupSpoiled()
    {
        StopGame();
    }

    private void OnEnable()
    {
        // setup event listeners
        SoupPot_Behaviour.SoupReceivedTrash += OnSoupReceivedTrash;
        SpoilMeter.SoupSpoiled += OnSoupSpoiled;

        // setup network variable listeners
        gameState.OnValueChanged += OnGameStateChanged;

        // enable controls
        controls.Debug.Enable();
    }

    private void OnDisable()
    {
        // clear event listeners
        SoupPot_Behaviour.SoupReceivedTrash -= OnSoupReceivedTrash;
        SpoilMeter.SoupSpoiled -= OnSoupSpoiled;

        // clear network variable listeners
        gameState.OnValueChanged += OnGameStateChanged;

        // disable controls
        controls.Debug.Disable();
    }

    public IEnumerator GameStartDelay()
    {
        // set it active
        controllerMappingSprite.SetActive(true);

        // wait 6 seconds
        yield return new WaitForSeconds(6f);

        // deactivate the UI
        controllerMappingSprite.SetActive(false);

        // handles countdown timer
        countdownDisplay.gameObject.SetActive(true);
        while (countdownTime > 0)
        {
            countdownDisplay.text = countdownTime.ToString();

            yield return new WaitForSeconds(1f);

            countdownTime--;
        }

        countdownDisplay.text = "GO";

        // start game
        InitializeGame();

        yield return new WaitForSeconds(1f);

        countdownDisplay.gameObject.SetActive(false);
    }
}
