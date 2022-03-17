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

    public static UIManager Instance { get; private set; }

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI connectedPlayersText;

    [SerializeField] private Button startGameButton;

    [SerializeField] private TMP_InputField networkAddressInput;

    private bool hasServerStarted;

    private void Awake()
    {
        // For ease of testing:
        Cursor.visible = true;

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
    
    private void Start()
    {
        hasServerStarted = false;

        // setup event listeners
        SceneManager.sceneLoaded += (Scene newScene, LoadSceneMode loadSceneMode) =>
        {
            Debug.Log("Rebinding UIManager buttons due to scene switch");
            // re-enable the scene switcher
            //GameObject.FindObjectOfType<SceneSwitcher>().gameObject.SetActive(true);

            BindUI();
        };

        //NetworkManager.Singleton.SceneManager.OnLoad += OnSceneChanged;

        BindUIEvents();
        UpdateNetworkAddress(networkAddressInput.text);
    }

    // called when the scene changes
    private void OnSceneChanged(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        // rebind the UI buttons
        BindUI();
    }

    private void BindUI()
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        // get the network UI canvas
        Transform networkUICanvas = GameObject.Find("NetworkUI").transform;     // probably bad

        if (networkUICanvas == null)
            return;

        // bind buttons
        startHostButton = networkUICanvas.Find("Start Host").GetComponentInChildren<Button>();
        startServerButton = networkUICanvas.Find("Start Server").GetComponentInChildren<Button>();
        startClientButton = networkUICanvas.Find("Start Client").GetComponentInChildren<Button>();
        connectedPlayersText = networkUICanvas.Find("Players").GetComponent<TextMeshProUGUI>();
        startGameButton = networkUICanvas.Find("Start Game").GetComponentInChildren<Button>();
        networkAddressInput = networkUICanvas.Find("NetworkAddressInput").GetComponent<TMP_InputField>();


        BindUIEvents();
    }

    private void BindUIEvents()
    {
        startHostButton.onClick.RemoveAllListeners();
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
            }

            else
            {
                Debug.Log("Host not started!");
            }
        });

        startServerButton.onClick.RemoveAllListeners();
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
            }

            else
            {
                Debug.Log("Server not started!");
            }
        });

        startClientButton.onClick.RemoveAllListeners();
        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
            }

            else
            {
                Debug.Log("Host not started!");
            }
        });

        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        startGameButton.onClick.RemoveAllListeners();
        startGameButton.onClick.AddListener(() =>
        {
            // invoke the sceneswitchtriggered event
            if (SceneSwitchRequested != null)
                SceneSwitchRequested();
        });
    }

    public void UpdateNetworkAddress(string newAddress)
    {
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = newAddress;
    }

    private void UpdateButtonVisibilities(bool isConnected)
    {
        if (SceneManager.GetActiveScene().name == "InGame")
            return;

        startHostButton.gameObject.SetActive(!isConnected);
        startClientButton.gameObject.SetActive(!isConnected);
        startServerButton.gameObject.SetActive(!isConnected);
        networkAddressInput.gameObject.SetActive(!isConnected);
        startGameButton.gameObject.SetActive(isConnected);
    }

    private void Update()
    {
        connectedPlayersText.text = $"Players in game: {PlayersManager.Instance.ConnectedPlayers}";
        UpdateButtonVisibilities(hasServerStarted); // should probably not be done on the update
    }
}
