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
    
    public int struggleCount;
    public NetworkVariable<ulong> networkHeldPlayerID = new NetworkVariable<ulong>();

    void Start()
    {
        // setup variables
        playerController = GetComponent<PlayerController>();
    }

    public void BindControls(PlayerInput playerInput)
    {
        playerInput.actions["Struggle"].performed += ctx => OnStrugglePerformed();
    }

    public void UnbindControls(PlayerInput playerInput)
    {
        playerInput.actions["Struggle"].performed -= ctx => OnStrugglePerformed();
    }

    private void OnStrugglePerformed()
    {
        // ensure the player is able to struggle
        if (playerController.networkPlayerState.Value != PlayerController.PlayerState.Ungrounded)
        {
            // reset the times struggled
            if (struggleCount != 0)
                struggleCount = 0;

            return;
        }

        //Debug.LogFormat("PlayerID: {0} struggled, held by: {1}, count: {2}",
        //    NetworkObjectId, networkHeldPlayerID.Value, struggleCount);

        struggleCount++;

        // check if struggled enough
        if (struggleCount >= struggleRequirement)
        {
            BreakFree();
            struggleCount = 0;
        }
    }

    public void BreakFree()
    {
        // ensure the player is able to break free
        if (playerController.networkPlayerState.Value != PlayerController.PlayerState.Ungrounded)
        {
            // reset the times struggled
            if (struggleCount != 0)
                struggleCount = 0;

            return;
        }

        // get the holder player controller
        PlayerController holderPC = GetNetworkObject(networkHeldPlayerID.Value).GetComponent<PlayerController>();
        holderPC.OnDropServerRpc(holderPC.GetComponent<Rigidbody>().velocity);

        // reset the released time
        playerController.OnReleasedServerRpc();

        Debug.LogFormat("PlayerID: {0} broke free from player {1}!", NetworkObjectId, networkHeldPlayerID.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHeldPlayerIDServerRpc(ulong heldPlayerId)
    {
        networkHeldPlayerID.Value = heldPlayerId;
    }
}
