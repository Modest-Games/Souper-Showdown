using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
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
    [ReadOnly] public GameState gameState;

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
    }

    void Start()
    {
        // setup variables
        gameState = GameState.Stopped;
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
            // only run setup code on the server
            //if (!IsServer)
            //    return;

            // setup the map
            SetupMap();

            // start the game
            if (autoStart)
                StartGame();
        };
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.Running:
                // check if the game should be over
                if (gameTimeElapsed >= gameDuration)
                {
                    // stop the game
                    StopGame();
                } else
                {
                    // add delta time to the time elapsed
                    gameTimeElapsed += Time.deltaTime;
                }

                break;
        }
    }

    private void SetupMap()
    {
        // spawn the starting pollutants
        spawner.SpawnManyPollutants(numStartingPollutants);
    }

    [Button("Start Game")]
    public void StartGame()
    {
        gameState = GameState.Running;

        if (GameStarted != null)
            GameStarted();
    }

    private void PauseGame()
    {
        gameState = GameState.Paused;

        if (GamePaused != null)
            GamePaused();
    }

    private void ResumeGame()
    {
        gameState = GameState.Running;

        if (GameResumed != null)
            GameResumed();
    }

    [Button("Stop Game")]
    private void StopGame()
    {
        gameState = GameState.Stopped;

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
        if (gameState != GameState.Stopped)
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
}
