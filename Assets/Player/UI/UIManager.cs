using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public delegate void UIDelegate();
    public static event UIDelegate SceneSwitchRequested;

    public static UIManager Instance { get; private set; }

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI connectedPlayersText;

    [SerializeField] private Button startGameButton;

    [SerializeField] private Button switchSceneButton;

    [SerializeField] private Dropdown characterSelector;

    public Toggle isChefToggle;
    public string chosenCharacterName;
    public int chosenCharacterIndex;

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

        isChefToggle.isOn = false;
    }

    private void OnEnable()
    {
        // setup event listeners
        NetworkManager.Singleton.SceneManager.OnLoad += OnSceneChanged; ;
    }

    // called when the scene changes
    private void OnSceneChanged(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        // rebind the UI buttons
        BindUI();
    }

    private void BindUI()
    {
        // get the network UI canvas
        Transform networkUICanvas = GameObject.Find("NetworkUI").transform;     // probably bad

        // bind buttons
        startHostButton = networkUICanvas.Find("Start Host").GetComponentInChildren<Button>();
        startServerButton = networkUICanvas.Find("Start Server").GetComponentInChildren<Button>();
        startClientButton = networkUICanvas.Find("Start Client").GetComponentInChildren<Button>();
        connectedPlayersText = networkUICanvas.Find("Players").GetComponent<TextMeshProUGUI>();
        startGameButton = networkUICanvas.Find("Start Game").GetComponentInChildren<Button>();
        switchSceneButton = networkUICanvas.Find("Switch Scene").GetComponentInChildren<Button>();
        characterSelector = networkUICanvas.Find("CharacterSelect").GetComponentInChildren<Dropdown>();

        startHostButton.onClick.AddListener(() =>
        {
            if(NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
            }

            else
            {
                Debug.Log("Host not started!");
            }
        });

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

        switchSceneButton.onClick.AddListener(() =>
        {
            // invoke the sceneswitchtriggered event
            if (SceneSwitchRequested != null)
                SceneSwitchRequested();
        });

        startGameButton.onClick.AddListener(() =>
        {
            if (!hasServerStarted) return;
            GameController.Instance.InitializeGame();
        });
    }

    private void Update()
    {
        connectedPlayersText.text = $"Players in game: {PlayersManager.Instance.ConnectedPlayers}";
    }

    private void Start()
    {
        // setup character selector
        foreach (Character character in CharacterManager.Instance.characterList)
        {
            characterSelector.options.Add(new Dropdown.OptionData(character.characterName));
        }
        characterSelector.RefreshShownValue();

        characterSelector.onValueChanged.AddListener((chosen) =>
        {
            //Debug.Log(characterSelector.options[chosen].text);
            chosenCharacterName = characterSelector.options[chosen].text;
            chosenCharacterIndex = chosen;
        });

        hasServerStarted = false;

        BindUI();
    }
}
