using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;


public class PlayersManager : NetworkBehaviour
{
    public struct Player
    {
        public Player(ulong _clientIndex, int _controllerIndex)
        {
            clientIndex = _clientIndex;
            controllerIndex = _controllerIndex;
        }

        public ulong clientIndex;
        public int controllerIndex;
    }

    public static PlayersManager Instance { get; private set; }

    public GameObject playerPrefab;

    public NetworkList<Unity.Collections.FixedString64Bytes> _players;

    public List<string> players = new List<string>();

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

        _players = new NetworkList<Unity.Collections.FixedString64Bytes>();
    }

    private void OnPlayersChanged(NetworkListEvent<Unity.Collections.FixedString64Bytes> changeEvent)
    {
        // clear the list
        players.Clear();

        foreach (Unity.Collections.FixedString64Bytes _player in _players)
        {
            players.Add(_player.ToString());
        }
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
    public void PlayerConnectedServerRpc(ulong clientId, int playerIndex)
    {
        Debug.LogFormat("Adding player: {0}:{1}", clientId, playerIndex);

        // add the player to the list of players
        _players.Add(string.Format("{0}:{1}", clientId, playerIndex));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDisconnectedServerRpc(ulong clientId, int playerIndex)
    {
        Debug.LogFormat("Removing player: {0}:{1}", clientId, playerIndex);

        // remove the player from the list of players
        _players.RemoveAt(_players.IndexOf(string.Format("{0}:{1}", clientId, playerIndex)));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDisconnectedServerRpc(ulong clientId)
    {
        int playersRemoved = 0;
        int size = _players.Count;

        // loop through each player in the list of players
        for (int i = 0; i < size; i++)
        {
            //Debug.Log(_players[i - playersRemoved].ToString().Split(':')[0] == clientId.ToString());

            // only remove the player if their client id matches the one given to this RPC
            if (_players[i - playersRemoved].ToString().Split(':')[0] == clientId.ToString())
            {
                // compensate for previous players already removed from the list (index offset)
                _players.RemoveAt(i - playersRemoved);
                playersRemoved++;
            }
        }

        Debug.LogFormat("Removing all {1} players from client: {0}", clientId, playersRemoved);
    }

    private void OnEnable()
    {
        // setup event listeners
        _players.OnListChanged += OnPlayersChanged;
    }

    private void OnDisable()
    {
        // clear event listeners
        _players.OnListChanged -= OnPlayersChanged;
    }
}
