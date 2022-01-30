using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Spawner : NetworkBehaviour
{
    public static Spawner Instance { get; private set; }

    [SerializeField] private GameObject objectPrefab;

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

    public void SpawnObject()
    {
        if (!IsServer) return;

        GameObject obj = Instantiate(objectPrefab);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}
