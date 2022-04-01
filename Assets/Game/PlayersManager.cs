using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Linq;

public class PlayersManager : NetworkBehaviour
{
    [System.Serializable]
    public struct Player
    {
        public Player(ulong _clientIndex, int _controllerIndex, ulong _networkObjId)
        {
            clientIndex = _clientIndex;
            controllerIndex = _controllerIndex;
            networkObjId = _networkObjId;
            character = "";
        }

        public Player(ulong _clientIndex, int _controllerIndex, ulong _networkObjId, string _character)
        {
            clientIndex = _clientIndex;
            controllerIndex = _controllerIndex;
            networkObjId = _networkObjId;
            character = _character;
        }

        public static Player Parse(Unity.Collections.FixedString64Bytes input)
        {
            string[] str = input.ToString().Split(':');
            return new Player(ulong.Parse(str[0]), int.Parse(str[1]), ulong.Parse(str[2]));
        }

        public ulong clientIndex;
        public int controllerIndex;
        public ulong networkObjId;
        public string character;
    }

    public delegate void PlayersManagerDelegate();
    public static event PlayersManagerDelegate PlayerListChanged;

    public static PlayersManager Instance { get; private set; }

    public GameObject playerPrefab;

    public List<Player> players = new List<Player>();

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

    private void Update()
    {
        if (players.Count > 0)
            Debug.LogFormat("Player 1 score: {0}", GetPlayerScore(0));
    }

    public void UpdatePlayerInList(ulong clientId, int controllerId, ulong networkObjId, string character)
    {
        int index = players.IndexOf(players.Find(p => p.clientIndex == clientId && p.controllerIndex == controllerId));

        if (index != -1)
        {
            players[index] = new Player(clientId, controllerId, networkObjId, character);
            
            // sort the list
            players.Sort((p1, p2) => p1.networkObjId.CompareTo(p2.networkObjId));
            
            // fire the playerListChanged event
            if (PlayerListChanged != null)
                PlayerListChanged();
        }
    }

    public void AddPlayerToList(ulong clientId, int controllerID, ulong networkObjId, string character)
    {
        players.Add(new Player(clientId, controllerID, networkObjId, character));

        // sort the list
        players.Sort((p1, p2) => p1.networkObjId.CompareTo(p2.networkObjId));

        // fire the playerListChanged event
        if (PlayerListChanged != null)
            PlayerListChanged();
    }

    public void RemovePlayerFromList(ulong networkObjId)
    {
        players.Remove(players.Find((player) => player.networkObjId == networkObjId));

        // fire the playerListChanged event
        if (PlayerListChanged != null)
            PlayerListChanged();
    }

    public Player GetPlayerFromList(ulong networkObjId)
    {
        return players.Find(p => p.networkObjId == networkObjId);
    }

    public int GetPlayerIndex(ulong networkObjId)
    {
        return players.IndexOf(players.Find(p => p.networkObjId == networkObjId));
    }

    public int GetPlayerScore(int playerIndex)
    {
        return GetPlayerScore(players[playerIndex].networkObjId);
    }

    public int GetPlayerScore(ulong networkObjId)
    {
        return GetNetworkObject(networkObjId).GetComponent<PlayerController>().networkScore.Value;
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

    private void UIManager_GameJoined()
    {
        NetworkObject.DestroyWithScene = false;
    }

    private void OnEnable()
    {
        // setup event listeners
        UIManager.GameJoined += UIManager_GameJoined;
    }

    private void OnDisable()
    {
        // clear event listeners
        UIManager.GameJoined += UIManager_GameJoined;
    }
}
