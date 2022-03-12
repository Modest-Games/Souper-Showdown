using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldObject : MonoBehaviour
{
    public GameObject heldObject;
    public bool meshInitialized;

    void Awake()
    {
        meshInitialized = false;
    }

    void Update()
    {
        if (!meshInitialized && heldObject != null)
        {
            RefreshMesh();
            meshInitialized = true;
        }
    }

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
        GameObject newMesh = Instantiate(heldObject, transform);
        newMesh.name = "Mesh";
    }
}
