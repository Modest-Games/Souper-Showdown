using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;
using NaughtyAttributes;
using Unity.Netcode.Components;

public class PollutantBehaviour : NetworkBehaviour
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

    public Rigidbody rb;

    void Awake()
    {
        // setup variables
        trail = gameObject.GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody>();

        //RefreshMesh();
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

    private void OnCollisionEnter(Collision collision)
    {
        // switch on other tag
        switch (collision.gameObject.tag)
        {
            // if colliding with the ground
            case "Ground":
                // stop being airborn
                trail.emitting = false;
                state = PollutantState.Idle;
                break;
        }
    }

    [ClientRpc]
    public void OnThrowClientRpc()
    {
        StartCoroutine(ThrowEffectsDelay());
    }

    public IEnumerator ThrowEffectsDelay()
    {
        yield return new WaitForSeconds(0.1f);

        trail.emitting = true;
        state = PollutantState.Airborn;
    }
}
