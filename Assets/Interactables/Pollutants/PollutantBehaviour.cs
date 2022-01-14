using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PollutantBehaviour : MonoBehaviour
{
    public enum PollutantState
    {
        Idle, 
        Airborn
    }

    public Pollutant pollutantObject;
    [ReadOnly] public PollutantState state;

    private TrailRenderer trail;
    private Vector3 throwStartPos;
    private Vector3 throwDestination;

    private void Awake()
    {
        // setup variables
        trail = gameObject.GetComponent<TrailRenderer>();
    }

    void Start()
    {
        RefreshMesh();
    }

    void Update()
    {
        switch (state)
        {
            case PollutantState.Idle:
                // do nothing (for now)
                break;

            case PollutantState.Airborn:
                //transform.position = Vector3.Lerp(throwStartPos, throwDestination, );
                break;
        }
    }

    [Button]
    public void RefreshMesh()
    {
        // check if there is an existing mesh
        Transform oldMesh = transform.Find("Mesh");
        if (oldMesh != null)
        {
            // remove the old mesh
            DestroyImmediate(oldMesh.gameObject);
        }

        // check if there is a mesh to render
        if (pollutantObject != null)
        {
            // instantiate the new mesh
            GameObject newMesh = Instantiate(pollutantObject.mesh, transform);
            newMesh.name = "Mesh";
        }
    }

    public Pollutant Pickup()
    {
        // destroy the gameobject
        Destroy(gameObject);

        return pollutantObject;
    }

    public void Throw(Vector3 throwDirection, float throwDistance)
    {
        // enable the trail renderer
        trail.emitting = true;
        state = PollutantState.Airborn;


    }
}
