using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.Netcode;

public class ObjectSpawner : NetworkBehaviour
{
    public static ObjectSpawner Instance;

    public GameObject pollutantPrefab;
    public List<Pollutant> pollutantList;
    public List<Pollutant> deadBodyList;
    public List<Pollutant> liveBodyList;
    public Dumpster[] dumpsters = new Dumpster[4];

    [Header("Config")]
    public bool useSquareExclusion;
    public Vector2 squareExclusion;
    public float roundExclusionRadius;
    public Vector2 spawnBounds;
    public float defaultYValue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [Button("Spawn Pollutant")]
    public void SpawnPollutant()
    {
        var pollutantIndex = Random.Range(0, pollutantList.Count);
        dumpsters[Random.Range(0, 4)].SpawnPoulltant(pollutantList[pollutantIndex]);
    }

    public void SpawnManyPollutants(int numPollutants)
    {
        StartCoroutine(CycleDumpsters());
    }

    public IEnumerator CycleDumpsters()
    {
        var numPollutants = 0;

        while (numPollutants < 4)
        {
            SpawnPollutant();
            yield return new WaitForSeconds(1.00f);
            numPollutants++;
        }
        
    }
}
