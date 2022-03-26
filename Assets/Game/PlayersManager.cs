using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; private set; }

    public GameObject playerPrefab;

    private NetworkVariable<int> connectedClients = new NetworkVariable<int>();
    private NetworkVariable<int> connectedPlayers = new NetworkVariable<int>();

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

    public int ConnectedClients
    {
        get
        {
            return connectedClients.Value;
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                Debug.Log($"{id} just connected...");
                connectedClients.Value++;
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
            {
                Debug.Log($"{id} just disconnected...");
                connectedClients.Value--;
            };
        }

        PlayerTokenBehaviour.PlayerJoined += () =>
        {
            PlayerConnectedServerRpc();
        };

        PlayerTokenBehaviour.PlayerQuit += () =>
        {
            PlayerDisconnectedServerRpc();
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerServerRpc(int playerIndex, ulong clientId)
    {
        // instantiate a new player
        GameObject newPlayerObj = Instantiate(playerPrefab);
        Debug.LogFormat("Spawning a new player object for client: {0}, playerIndex: {1}", clientId, playerIndex);
        
        PlayerController newPlayerController = newPlayerObj.GetComponent<PlayerController>();
        NetworkObject newPlayerNetworkObj = newPlayerObj.GetComponent<NetworkObject>();

        newPlayerController.playerIndex.Value = playerIndex;
        newPlayerNetworkObj.SpawnWithOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerConnectedServerRpc()
    {
        // increment the number of connected players
        connectedPlayers.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDisconnectedServerRpc()
    {
        // decrement the number of connected players
        connectedPlayers.Value--;
    }
}
