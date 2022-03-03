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
        if (IsServer)
        {
            // do scene switching
            switch (SceneManager.GetActiveScene().name)
            {
                case "Lobby":
                    NetworkManager.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
                    break;

                case "InGame":
                    NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
                    break;
            }
        }
    }

    private void OnEnable()
    {
        // subscribe to events
        UIManager.SceneSwitchRequested += OnSceneSwitchRequested;
    }

    private void OnDisable()
    {
        // unsubscribe from events
        UIManager.SceneSwitchRequested -= OnSceneSwitchRequested;
    }
}
