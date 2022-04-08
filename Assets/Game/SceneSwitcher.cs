using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneSwitcher : NetworkBehaviour
{
    public static SceneSwitcher Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnSceneSwitchRequested()
    {
        // do scene switching
        switch (SceneManager.GetActiveScene().name)
        {
            case "Lobby":
                SwitchToInGame();
                break;

            case "InGame":
                SwitchToLobby();
                break;
        }
    }

    public void SwitchToInGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        Debug.Log("Switching to InGame scene");
    }

    public void SwitchToLobby()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        Debug.Log("Switching to Lobby scene");
    }

    private void OnEnable()
    {
        // subscribe to events
        UIManager.SceneSwitchRequested += OnSceneSwitchRequested;
        LobbyController.SwitchScene += OnSceneSwitchRequested;
    }

    private void OnDisable()
    {
        // unsubscribe from events
        UIManager.SceneSwitchRequested -= OnSceneSwitchRequested;
        LobbyController.SwitchScene -= OnSceneSwitchRequested;
    }
}
