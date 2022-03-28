using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerTokenBehaviour : MonoBehaviour
{
    public delegate void PlayerTokenDelegate();
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
    }

    private void OnDisable()
    {
        LocalPlayerManager.Instance.inputPlayers.Remove(playerInput);
    }
}
