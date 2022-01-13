using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PollutantBehaviour : MonoBehaviour
{
    public Pollutant pollutantObject;

    void Start()
    {
        RefreshMesh();
    }

    void Update()
    {
        
    }

    [Button]
    private void RefreshMesh()
    {
        // check if there is an existing mesh
        Transform oldMesh = transform.Find("Mesh");
        if (oldMesh != null)
        {
            // remove the old mesh
            DestroyImmediate(oldMesh.gameObject);
        }

        // instantiate the new mesh
        GameObject newMesh = Instantiate(pollutantObject.mesh, transform);
        newMesh.name = "Mesh";
    }
}
