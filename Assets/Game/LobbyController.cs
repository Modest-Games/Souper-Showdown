using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class LobbyController : NetworkBehaviour
{
    // singleton class
    public static LobbyController Instance { get; private set; }

    public bool isDebugEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        //NetworkManager.Singleton.StartHost();
    }

    void Update()
    {
        
    }
}
