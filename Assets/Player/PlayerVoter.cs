using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerVoter : NetworkBehaviour
{
    public delegate void PlayerVoteDelegate();
    public static event PlayerVoteDelegate PlayerVoted;

    public NetworkVariable<int> networkVoteOption = new NetworkVariable<int>();

    public void BindControls(PlayerInput playerInput)
    {
        playerInput.actions["VoteOption1"].performed += ctx => OnVotePerformed(1);
        playerInput.actions["VoteOption2"].performed += ctx => OnVotePerformed(2);
        playerInput.actions["VoteOption3"].performed += ctx => OnVotePerformed(3);
    }

    public void UnbindControls(PlayerInput playerInput)
    {
        playerInput.actions["VoteOption1"].performed -= ctx => OnVotePerformed(1);
        playerInput.actions["VoteOption2"].performed -= ctx => OnVotePerformed(2);
        playerInput.actions["VoteOption3"].performed -= ctx => OnVotePerformed(3);
    }

    private void OnVotePerformed(int option)
    {
        // ensure the window is focused
        if (!Application.isFocused)
            return;

        VoteServerRpc(option);
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteServerRpc(int option)
    {
        networkVoteOption.Value = option;
    }

    private void OnVoteOptionChanged(int oldVal, int newVal)
    {
        if (PlayerVoted != null)
            PlayerVoted();
    }

    private void Start()
    {
        // setup event listeners
        networkVoteOption.OnValueChanged += OnVoteOptionChanged;
    }

    new private void OnDestroy()
    {
        // clear event listeners
        networkVoteOption.OnValueChanged -= OnVoteOptionChanged;
    }
}
