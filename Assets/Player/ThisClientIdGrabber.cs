using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ThisClientIdGrabber : NetworkBehaviour
{
    void Start()
    {
        if (IsClient && IsOwner)
        {
            // send this client id to the local player manager
            LocalPlayerManager.Instance.thisClientId = OwnerClientId;
        }
    }
}
