using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Unity.Netcode;

public class TerrainManager : NetworkBehaviour
{
    public static TerrainManager Instance { get; private set; }

    [Header("Config")]
    public float chefSpawnRadius;

    private Transform terrainLeft;
    private Transform terrainRight;
    public readonly List<Vector3> playerSpawnLocations = new List<Vector3>();

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

        // setup variables
        GetSpawnLocations();
        terrainLeft = transform.GetChild(0);
        terrainRight = transform.GetChild(1);
    }

    private void GetSpawnLocations()
    {
        Transform spawnLocationContainer = transform.Find("PlayerSpawnLocations");
        
        for (int i = 0; i < spawnLocationContainer.childCount; i++)
        {
            // add the current spawn location to the list
            playerSpawnLocations.Add(spawnLocationContainer.GetChild(i).transform.position);
        }
    }

    public void GenerateAllTerrain()
    {
        terrainLeft.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
        terrainRight.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
    }

    public Vector3 GetRandomSpawnLocation(bool isChef)
    {
        if (isChef)
        {
            // return a random position from the center using chefSpawnRadius
            Vector2 randomSpawn2D = Random.insideUnitCircle * chefSpawnRadius;
            return new Vector3(randomSpawn2D.x, 0f, randomSpawn2D.y);

        } else
        {
            // return the position of a random spawn location
            Vector3 randomSpawn = playerSpawnLocations[Random.Range(0, playerSpawnLocations.Count - 1)];
            return randomSpawn;
        }
    }
}
