using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerTokenBehaviour : MonoBehaviour
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
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
