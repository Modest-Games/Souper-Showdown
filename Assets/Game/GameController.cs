using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
    // singleton class object
    public static GameController Instance { get; private set; }

    public enum GameState
    {
        Stopped,
        Running,
        Paused
    }

    public delegate void DebugDelegate();
    public static event DebugDelegate DebugEnabled;
    public static event DebugDelegate DebugDisabled;

    public delegate void GameStateDelegate();
    public static event GameStateDelegate GameStarted;
    public static event GameStateDelegate GameStopped;
    public static event GameStateDelegate GamePaused;
    public static event GameStateDelegate GameResumed;

    [Header("Debug")]
    public bool debugEnabledByDefault;
    [ReadOnly] public bool isDebugEnabled;

    [Header("Game State")]
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.Stopped);
    private GameState lastGameState;

    [Header("Game Config")]
    public int numStartingPollutants;
    public bool autoStart;
    [Tooltip("The duration of the game in seconds")] public float gameDuration;

    [SerializeField] [ReadOnly] private float gameTimeElapsed;
    private SoupPot_Behaviour soupPot;
    private ObjectSpawner spawner;
    private PlayerControlsMapping controls;

    private void Awake()
    {
        controls = new PlayerControlsMapping();

        // map relevant control inputs
        controls.Debug.ToggleDebug.performed += ctx => ToggleDebug();

        // singleton stuff
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // setup variables
        lastGameState = GameState.Stopped;
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

        // listen for the server to start
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            // setup the map
            SetupMap();

            // start the game
            if (autoStart)
                StartGame();
        };
    }

    void Update()
    {
        // check if the gamestate has changed
        if (lastGameState != gameState.Value)
        {
            // call the corresponding event
            switch (gameState.Value)
            {
                case GameState.Paused:
                    PauseGame();
                    break;

                case GameState.Running:
                    switch (lastGameState)
                    {
                        case GameState.Paused:
                            ResumeGame();
                            break;

                        case GameState.Stopped:
                            StartGame();
                            break;
                    }
                    break;

                case GameState.Stopped:
                    StopGame();
                    break;
            }

            // update lastGameState
            lastGameState = gameState.Value;
        }
        
        switch (gameState.Value)
        {
            case GameState.Running:
                // check if the game should be over
                if (gameTimeElapsed >= gameDuration)
                {
                    // stop the game
                    UpdateGameStateServerRpc(GameState.Stopped);
                } else
                {
                    // add delta time to the time elapsed
                    gameTimeElapsed += Time.deltaTime;
                }

                break;
        }
    }

    public void SetupMap()
    {
        // spawn the starting pollutants
        spawner.SpawnManyPollutants(numStartingPollutants);
    }

    [Button("Start Game")]
    public void StartGame()
    {
        gameState.Value = GameState.Running;

        if (GameStarted != null)
            GameStarted();
    }

    private void PauseGame()
    {
        gameState.Value = GameState.Paused;

        if (GamePaused != null)
            GamePaused();
    }

    private void ResumeGame()
    {
        gameState.Value = GameState.Running;

        if (GameResumed != null)
            GameResumed();
    }

    [Button("Stop Game")]
    private void StopGame()
    {
        gameState.Value = GameState.Stopped;

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

    private void OnSoupReceivedTrash()
    {
        if (gameState.Value != GameState.Stopped)
            spawner.SpawnPollutant();
    }

    private void OnEnable()
    {
        // setup event listeners
        SoupPot_Behaviour.SoupReceivedTrash += OnSoupReceivedTrash;

        // enable controls
        controls.Debug.Enable();
    }

    private void OnDisable()
    {
        // clear event listeners
        SoupPot_Behaviour.SoupReceivedTrash -= OnSoupReceivedTrash;

        // disable controls
        controls.Debug.Disable();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateGameStateServerRpc(GameState newState)
    {
        gameState.Value = newState;
    }
}
