using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TMPro;

public class PlayerHUD : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playersName = new NetworkVariable<NetworkString>();

    private bool overlaySet = false;

    private void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playersName.Value))
        {
            SetHUD();
            overlaySet = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            playersName.Value = $"Player {OwnerClientId}";


        }
    }

    public void SetHUD()
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = playersName.Value;
    }
}

// Custom implementation for strings for NetworkVariables, move later:
public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }

    public static implicit operator string (NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() {  info = new FixedString32Bytes(s)};
}
