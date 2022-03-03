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
        Debug.Log("INSTANTIATED NEW PLAYER FROM PLAYERSMANAGER");
        
        PlayerController newPlayerController = newPlayerObj.GetComponent<PlayerController>();
        NetworkObject newPlayerNetworkObj = newPlayerObj.GetComponent<NetworkObject>();

        newPlayerController.BindControls(playerIndex);
        newPlayerNetworkObj.SpawnWithOwnership(clientId);
    }
}
