using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerToken : NetworkBehaviour
{
    private void OnDisable()
    {
        PlayersManager.Instance.PlayerDisconnectedServerRpc(OwnerClientId);
    }

    //private void OnDestroy()
    //{
    //    PlayersManager.Instance.PlayerDisconnectedServerRpc(OwnerClientId);
    //}
}
