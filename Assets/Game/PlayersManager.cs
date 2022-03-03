using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance { get; private set; }

    public GameObject playerPrefab;

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

    public int ConnectedPlayers
    {
        get
        {
            return connectedPlayers.Value;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (IsServer)
            {
                Debug.Log($"{id} just connected...");
                connectedPlayers.Value++;
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (IsServer)
            {
                Debug.Log($"{id} just disconnected...");
                connectedPlayers.Value--;
            }
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
}
