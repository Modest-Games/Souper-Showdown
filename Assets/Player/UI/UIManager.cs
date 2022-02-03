using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI connectedPlayersText;

    [SerializeField] private Button startGameButton;

    [SerializeField] public Dropdown characterSelector;

    public Toggle isChefToggle;

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

        // setup character selector
        characterSelector.itemText.text = "Character";
        foreach (Character character in CharacterManager.Instance.characterList)
        {
            characterSelector.options.Add(new Dropdown.OptionData(character.characterName));
        }
        
    }

    private void Update()
    {
        connectedPlayersText.text = $"Players in game: {PlayersManager.Instance.ConnectedPlayers}";
    }

    private void Start()
    {
        hasServerStarted = false;

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

        startGameButton.onClick.AddListener(() =>
        {
            if (!hasServerStarted) return;
            GameController.Instance.InitializeGame();
        });
    }
}
