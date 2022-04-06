using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine.SceneManagement;

public class UIManager : NetworkBehaviour
{
    public delegate void UIDelegate();
    public static event UIDelegate SceneSwitchRequested;
    public static event UIDelegate GameJoined;
    
    public static UIManager Instance { get; private set; }

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI connectedPlayersText;

    [SerializeField] private TextMeshProUGUI roomCodeText;

    [SerializeField] private Button startGameButton;

    [SerializeField] private TMP_InputField networkAddressInput;

    [SerializeField] private Transform characterSelectionUI;

    [SerializeField] private Image souperLogo;

    [SerializeField] private GameObject mainMenuUI;

    [SerializeField] private GameObject menuControls;

    [SerializeField] private Image menuBlur;

    [SerializeField] private Button controlsButton;

    private bool hasServerStarted;
    private bool isClient;

    private void Awake()
    {
        Cursor.visible = true;

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        Application.targetFrameRate = 60;

        hasServerStarted = false;
        isClient = false;

        SceneManager.sceneLoaded += (Scene newScene, LoadSceneMode loadSceneMode) =>
        {
            BindUI();
        };

        BindUIEvents();
    }

    private void OnSceneChanged(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        BindUI();
    }

    private void BindUI()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        Transform networkUICanvas = GameObject.Find("NetworkUI").transform;     // probably bad

        if (networkUICanvas == null)
            return;

        startHostButton = networkUICanvas.Find("Start Host").GetComponentInChildren<Button>();
        startClientButton = networkUICanvas.Find("Start Client").GetComponentInChildren<Button>();
        connectedPlayersText = networkUICanvas.Find("Players").GetComponent<TextMeshProUGUI>();
        startGameButton = networkUICanvas.Find("Start Game").GetComponentInChildren<Button>();
        controlsButton = networkUICanvas.Find("Controls Toggle").GetComponentInChildren<Button>();
        networkAddressInput = networkUICanvas.Find("NetworkAddressInput").GetComponent<TMP_InputField>();

        BindUIEvents();
    }

    private void BindUIEvents()
    {
        startHostButton.onClick.RemoveAllListeners();
        startHostButton.onClick.AddListener(async() =>
        {
            if (RelayManager.Instance.IsRelayEnabled)
                await RelayManager.Instance.SetupRelay();

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
            }

            else
            {
                Debug.Log("Host not started!");
            }
        });

        startClientButton.onClick.RemoveAllListeners();
        startClientButton.onClick.AddListener(async() =>
        {
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(networkAddressInput.text))
                await RelayManager.Instance.JoinRelay(networkAddressInput.text);

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
                hasServerStarted = true;
                isClient = true;
                if (GameJoined != null)
                    GameJoined();
            }

            else
            {
                Debug.Log("Client not started!");
                hasServerStarted = false;
            }
        });

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
            if (GameJoined != null)
                GameJoined();
        };

        startGameButton.onClick.RemoveAllListeners();
        startGameButton.onClick.AddListener(() =>
        {
            if (SceneSwitchRequested != null)
                SceneSwitchRequested();
        });
    }

    private void UpdateButtonVisibilities(bool isConnected)
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        connectedPlayersText.gameObject.SetActive(isConnected);
        roomCodeText.gameObject.SetActive(isConnected);
        startHostButton.gameObject.SetActive(!isConnected);
        startClientButton.gameObject.SetActive(!isConnected);
        networkAddressInput.gameObject.SetActive(!isConnected);
        startGameButton.gameObject.SetActive(isConnected);
        controlsButton.gameObject.SetActive(isConnected);
        mainMenuUI.gameObject.SetActive(!isConnected);
        characterSelectionUI.gameObject.SetActive(isConnected);
        souperLogo.gameObject.SetActive(!isConnected);
        menuControls.gameObject.SetActive(isConnected);
        menuBlur.gameObject.SetActive(!isConnected);
    }

    private void Update()
    {
        if (PlayersManager.Instance != null && hasServerStarted)
            connectedPlayersText.text = $"Players in game: {PlayersManager.Instance.players.Count}";

        if (hasServerStarted && !isClient)
            roomCodeText.text = $"ROOM CODE: {RelayManager.Instance.roomCode}";

        else if (hasServerStarted && isClient)
            roomCodeText.text = $"ROOM CODE: {networkAddressInput.text}";

        UpdateButtonVisibilities(hasServerStarted);
    }
}
