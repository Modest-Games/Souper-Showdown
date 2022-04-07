using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class StruggleBehaviour : NetworkBehaviour
{
    [Tooltip("The number of times needed to struggle in order to break free")]
    public int struggleRequirement;

    private PlayerController playerController;
    
    public NetworkVariable<int> networkStruggleCount = new NetworkVariable<int>();
    public NetworkVariable<ulong> networkHeldPlayerID = new NetworkVariable<ulong>();

    void Start()
    {
        // setup variables
        playerController = GetComponent<PlayerController>();

        // setup event listeners
        networkStruggleCount.OnValueChanged += OnStruggleCountChanged;
    }

    new private void OnDestroy()
    {
        // clear event listeners
        networkStruggleCount.OnValueChanged -= OnStruggleCountChanged;
    }

    public void BindControls(PlayerInput playerInput)
    {
        playerInput.actions["Struggle"].performed += ctx => OnStrugglePerformed();
    }

    private void OnStrugglePerformed()
    {
        // ensure the player is able to struggle
        if (playerController.networkPlayerState.Value != PlayerController.PlayerState.Ungrounded)
        {
            // reset the times struggled
            if (networkStruggleCount.Value != 0)
                ResetStruggleCountServerRpc();

            return;
        }

        Debug.LogFormat("PlayerID: {0} struggled, held by: {1}, count: {2}",
            NetworkObjectId, networkHeldPlayerID.Value, networkStruggleCount.Value);

        StruggleServerRpc();
    }

    public void BreakFree()
    {
        // ensure the player is able to break free
        if (playerController.networkPlayerState.Value != PlayerController.PlayerState.Ungrounded)
        {
            // reset the times struggled
            if (networkStruggleCount.Value != 0)
                ResetStruggleCountServerRpc();

            return;
        }

        // get the holder player controller
        PlayerController holderPC = GetNetworkObject(networkHeldPlayerID.Value).GetComponent<PlayerController>();
        holderPC.OnDropServerRpc(holderPC.GetComponent<Rigidbody>().velocity);

        // reset the released time
        playerController.OnReleasedServerRpc();

        Debug.LogFormat("PlayerID: {0} broke free from player {1}!", NetworkObjectId, networkHeldPlayerID.Value);
    }

    private void OnStruggleCountChanged(int oldVal, int newVal)
    {
        // check if struggled enough
        if (networkStruggleCount.Value >= struggleRequirement)
        {
            BreakFree();
            networkStruggleCount.Value = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StruggleServerRpc()
    {
        networkStruggleCount.Value = networkStruggleCount.Value + 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetStruggleCountServerRpc()
    {
        networkStruggleCount.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHeldPlayerIDServerRpc(ulong heldPlayerId)
    {
        networkHeldPlayerID.Value = heldPlayerId;
    }
}
