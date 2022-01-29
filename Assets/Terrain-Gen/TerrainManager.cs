using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    Transform terrainLeft;
    Transform terrainRight;

    private void Start()
    {
        terrainLeft = transform.GetChild(0);
        terrainRight = transform.GetChild(1);
    }

    [Button]
    public void GenerateAllTerrain()
    {
        terrainLeft.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
        terrainLeft.position = new Vector3(-17.5f, 0f, 0f);

        terrainRight.gameObject.GetComponent<coordinates_generator>().GenerateTerrain();
        terrainRight.position = new Vector3(17.5f, 0f, 0f);

    }
}
