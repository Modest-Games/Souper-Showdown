using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Unity.Netcode;

public class TerrainManager : NetworkBehaviour
{
    public static TerrainManager Instance { get; private set; }

    Transform terrainLeft;
    Transform terrainRight;

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

        terrainLeft = transform.GetChild(0);
        terrainRight = transform.GetChild(1);
    }

    public void GenerateAllTerrain()
    {
        terrainLeft.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
        terrainRight.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
    }
}
