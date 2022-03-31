using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerTokenBehaviour : MonoBehaviour
{
    public delegate void PlayerTokenDelegate(PlayersManager.Player player);
    public static event PlayerTokenDelegate PlayerJoined;
    public static event PlayerTokenDelegate PlayerQuit;
    
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        Debug.Log(playerInput.playerIndex);

        PlayersManager.Instance.RequestPlayerServerRpc(
            playerInput.playerIndex, LocalPlayerManager.Instance.thisClientId);
    }

    private void OnEnable()
    {
        LocalPlayerManager.Instance.inputPlayers.Add(playerInput);
        if (PlayerJoined != null)
            PlayerJoined(new PlayersManager.Player(LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex));

        // send player connected rpc to server
        PlayersManager.Instance.PlayerConnectedServerRpc(LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex);
    }

    private void OnDisable()
    {
        LocalPlayerManager.Instance.inputPlayers.Remove(playerInput);
        if (PlayerQuit != null)
            PlayerQuit(new PlayersManager.Player(LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex));
    }

    private void OnDestroy()
    {
        // send player disconnected rpc to server
        if (PlayersManager.Instance != null)
            PlayersManager.Instance.PlayerDisconnectedServerRpc(LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex);
    }
}
