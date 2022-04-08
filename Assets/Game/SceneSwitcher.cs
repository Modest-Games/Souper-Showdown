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
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("InGame", LoadSceneMode.Single);

        var traps = GameObject.FindGameObjectsWithTag("Trap");
        var pollutants = GameObject.FindGameObjectsWithTag("Pollutant");
        var projectiles = GameObject.FindGameObjectsWithTag("Projectile");

        foreach (GameObject obj in traps)
        {
            Destroy(obj);
        }

        foreach (GameObject obj in pollutants)
        {
            Destroy(obj);
        }

        foreach (GameObject obj in projectiles)
        {
            Destroy(obj);
        }
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
