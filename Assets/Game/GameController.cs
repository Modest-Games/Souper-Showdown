using System.Collections;
using System;
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
        Unstarted,
        Stopped,
        Running,
        Paused
    }

    public delegate void DebugDelegate();
    public static event DebugDelegate DebugEnabled;
    public static event DebugDelegate DebugDisabled;

    public static event GameStateDelegate ChefTeamWin;

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

    [Header("UI Components")]
    private float nextTimestamp;
    private float songInterval = 5.0f;

    [Header("Music Components")]
    //unchangeable attributes
    private float beatInterval = 3.25f;
    private float endTrackTime = 55.30561f;
    public GameObject MusicManager;
    public GameObject[] tracks;
    private int trackNum = 0;
    private float timeToChange = 0f;
    private float runningTime = 0f;
    private bool canSwitchTrack = false;
    private float[] timestamps;
    private int currentTrack = 0;


    [SerializeField] [ReadOnly] private float gameTimeElapsed;

    public NetworkVariable<float> networkTimeStarted = new NetworkVariable<float>();

    private SoupPot_Behaviour soupPot;
    private ObjectSpawner spawner;
    private PlayerControlsMapping controls;

    private void setTrackTimestamps()
    {
        float quarterMatch = gameDuration / 4f;
        timestamps = new float[4];
        timestamps[0] = gameDuration - quarterMatch;
        timestamps[1] = endTrackTime + quarterMatch;
        timestamps[2] = endTrackTime;
        timestamps[3] = 0;
    }

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

        //set music timestamps
        setTrackTimestamps();

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
        //Start the music
        nextTimestamp = gameDuration - songInterval;
        //Play first track
        tracks[trackNum].GetComponent<TransitionMusic>().ChangeSong();
        runningTime = 0f;

        if (IsServer)
        {
            // setup the map
            SetupMap();

            // set the time the game started
            networkTimeStarted.Value = NetworkManager.Singleton.LocalTime.TimeAsFloat;

            // start the game (server only)
            if (autoStart)
                gameState.Value = GameState.Running;
        }
    }


    void UpdateMusic()
    {

    }


    void Update()
    {
        UpdateTimer();

        if (IsServer)
        {
            switch (gameState.Value)
            {
                case GameState.Running:
                    // check if the game should be over
                    if (TimeElapsed >= gameDuration)
                    {
                        gameState.Value = GameState.Stopped;
                        EndScreenManager.Instance.ChefWins();
                    }

                    break;
            }
        }
    }

    string timeFormat(float time)
    {
        string format = "";
        int minutes = (int)(Mathf.Floor(time / 60.0f));
        int seconds = (int)(time - (60 * minutes));

        string secondsAsString;
        if (seconds >= 10)
        {
            secondsAsString = seconds.ToString();
        }
        else
        {
            secondsAsString = "0" + seconds.ToString();
        }

        format = minutes + ":" + secondsAsString;
        return format;
    }

    public float TimeElapsed
    {
        get
        {
            return NetworkManager.LocalTime.TimeAsFloat - networkTimeStarted.Value;
        }
    }

    public float TimeRemaining
    {
        get
        {
            return Mathf.Max(0f, gameDuration - TimeElapsed);
        }
    }

    public void UpdateTimer()
    {
        runningTime += Time.deltaTime;

        if (TimeRemaining < timestamps[currentTrack] && !canSwitchTrack)
        {
            canSwitchTrack = true;
            timeToChange = ((float)Math.Ceiling(runningTime / beatInterval)) * beatInterval;
        }

        if (canSwitchTrack && runningTime > timeToChange && currentTrack < 3)
        {
            currentTrack++;
            //Switch song
            Debug.Log("Song switching to track #: " + currentTrack);
            nextTimestamp = TimeRemaining - songInterval;
            tracks[currentTrack].GetComponent<TransitionMusic>().ChangeSong();
            if (currentTrack == 3)
            {
                tracks[currentTrack].GetComponent<TransitionMusic>().PlayFromStart();
            }
            canSwitchTrack = false;
        }

        //Format and update the timer
        string timerVar = timeFormat(TimeRemaining);
        SpoilMeter.Instance.transform.Find("Timer Text").GetComponent<TMPro.TextMeshProUGUI>().text = timerVar;
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

    private void OnSoupReceivedTrash(float amount, ulong throwerId)
    {
        if (gameState.Value == GameState.Stopped) return;

        if (amount < 0) return;

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
        yield return new WaitForSeconds(3f);

        InitializeGame();
    }
}
