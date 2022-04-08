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

        // bind vote action events
        playerInput.actions["VoteOption1"].performed += ctx => { VoteForOption(1); };
        playerInput.actions["VoteOption2"].performed += ctx => { VoteForOption(2); };
        playerInput.actions["VoteOption3"].performed += ctx => { VoteForOption(3); };
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneLoadMode)
    {
        if (scene.name == "Lobby")
            Destroy(gameObject);
    }

    private void VoteForOption(int option)
    {
        if (PlayerVoted != null)
            PlayerVoted(option, PlayersManager.Instance.GetPlayerFromList(
                LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex).networkObjId);

        Debug.LogFormat("Finding player with clientId: {0}, networkObjectId: {1}", LocalPlayerManager.Instance.thisClientId, playerInput.playerIndex);
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