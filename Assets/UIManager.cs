using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI connectedPlayersText;

    private void Awake()
    {
        // For ease of testing:
        Cursor.visible = true;
    }

    private void Update()
    {
        connectedPlayersText.text = $"Players in game: {PlayersManager.Instance.ConnectedPlayers}";
    }

    private void Start()
    {
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
    }
}
