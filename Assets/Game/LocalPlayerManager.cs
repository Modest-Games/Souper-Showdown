using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LocalPlayerManager : MonoBehaviour
{
    public static LocalPlayerManager Instance { get; private set; }

    public List<PlayerInput> inputPlayers = new List<PlayerInput>();

    public ulong thisClientId;

    private PlayerInputManager inputManager;
    public bool canSpawn = false;
    public bool connected;

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

        inputManager = GetComponent<PlayerInputManager>();
    }

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += (byte[] arg1, ulong arg2, NetworkManager.ConnectionApprovedDelegate arg3) =>
        {
            //connected = true;
        };
    }

    private void Update()
    {
        connected = NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsConnectedClient;

        bool tempCanSpawn = Application.isFocused && connected && SceneManager.GetActiveScene().name == "Lobby";

        if (tempCanSpawn != canSpawn)
        {
            canSpawn = tempCanSpawn;

            if (canSpawn)
                inputManager.EnableJoining();
            else
                inputManager.DisableJoining();
        }
    }
}
