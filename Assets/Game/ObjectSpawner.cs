using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.Netcode;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject pollutantPrefab;
    public Pollutant[] pollutants;

    [Header("Config")]
    public bool useSquareExclusion;
    public Vector2 squareExclusion;
    public float roundExclusionRadius;
    public Vector2 spawnBounds;
    public float defaultYValue;

    [Button("Spawn Pollutant")]
    public void SpawnPollutant()
    {
        Vector3 spawnLocation = GetSpawnLocation();

        // spawn the new pollutant
        GameObject newPollutant = Instantiate(pollutantPrefab, spawnLocation, Quaternion.identity);

        newPollutant.GetComponent<PollutantBehaviour>().pollutantObject = pollutants[Random.Range(0, pollutants.Length - 1)];
        newPollutant.GetComponent<NetworkObject>().Spawn();

        Debug.Log("Spawned pollutant at " + spawnLocation);
    }

    [Header("Testing")]
    [SerializeField] private int manualPollutantsToSpawn;
    [Button("Spawn Multiple Pollutants")]
    private void ManualSpawnManyPollutants()
    {
        SpawnManyPollutants(manualPollutantsToSpawn);
    }

    public void SpawnManyPollutants(int numPollutants)
    {
        for (int i = 0; i < numPollutants; i++)
        {
            SpawnPollutant();
        }
    }

    private Vector3 GetSpawnLocation()
    {
        Vector3 returnVec;

        if (useSquareExclusion)
        {
            float xVal = Random.Range(squareExclusion.x / 2f, spawnBounds.x / 2f) * (Random.Range(0, 2) == 1 ? 1f : -1f);
            //float yMin = Mathf.Max(0f, (squareExclusion.y / 2f) - Mathf.Abs(xVal));
            //float yMin = Mathf.Abs(xVal) > squareExclusion.x / 2f ? 0f : squareExclusion.y / 2f;
            float yVal = Random.Range(0f, spawnBounds.y / 2f) * (Random.Range(0, 2) == 1 ? 1f : -1f);
            returnVec = new Vector3(xVal, defaultYValue, yVal);
        } else
        {
            float xVal = Random.Range(0f, spawnBounds.x / 2f) * (Random.Range(0, 2) == 1 ? 1f : -1f);
            float yMin = Mathf.Max(0f, roundExclusionRadius - Mathf.Abs(xVal));
            float yVal = Random.Range(yMin, spawnBounds.y / 2f) * (Random.Range(0, 2) == 1 ? 1f : -1f);
            returnVec = new Vector3(xVal, defaultYValue, yVal);
        }

        return returnVec;
    }
}
