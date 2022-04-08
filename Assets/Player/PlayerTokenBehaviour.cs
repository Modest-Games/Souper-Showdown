using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerTokenBehaviour : MonoBehaviour
{
    public delegate void PlayerTokenActionDelegate();
    public delegate void PlayerTokenVoteActionDelegate(int voteOption, ulong senderNetworkId);
    public static event PlayerTokenActionDelegate BackActionStarted;
    public static event PlayerTokenActionDelegate BackActionCancelled;
    public static event PlayerTokenVoteActionDelegate PlayerVoted;

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

        // bind back action events
        playerInput.actions["Back"].started += ctx => { if (BackActionStarted != null) BackActionStarted(); };
        playerInput.actions["Back"].canceled += ctx => { if (BackActionCancelled != null) BackActionCancelled(); };

        // bind scene change events
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneLoadMode)
    {
        if (scene.name == "Lobby")
            Destroy(gameObject);
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