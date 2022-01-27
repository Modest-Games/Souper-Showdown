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

    public void Pickup()
    {
        // destroy the gameobject
        Destroy(gameObject);
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

    [ServerRpc(RequireOwnership = false)]
    public void OnPickupServerRpc()
    {
        transform.position = new Vector3(0, -100, 0);

        // Position of throwable is always determined by the server, this means physics properties should be changed
        // by the server as well:
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDropServerRpc(Vector3 playerPos, Vector3 playerForward, Vector3 playerVelocity, Vector3 lookVector, float throwForce, bool isThrown)
    {
        // Position of throwable is always determined by the server, this means physics properties should be changed
        // by the server as well:
        rb.useGravity = true;
        rb.isKinematic = false;

        var dropPosition = new Vector3(playerPos.x, playerPos.y + 2.5f, playerPos.z);
        dropPosition += (playerForward * 0.50f);

        var obj = Instantiate(gameObject, dropPosition, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();

        var newThrowable = obj.GetComponent<PollutantBehaviour>();
        
        if (isThrown)
        {
            newThrowable.rb.AddForce(playerForward.normalized * throwForce, ForceMode.Impulse);
            newThrowable.OnThrowClientRpc();
        }

        else
        {
            newThrowable.rb.velocity = playerVelocity;
        }

        Destroy(gameObject);
    }


    [ClientRpc]
    public void OnThrowClientRpc()
    {
        StartCoroutine(ThrowEffectsDelay());
    }

    public IEnumerator ThrowEffectsDelay()
    {
        yield return new WaitForSeconds(.1f);

        trail.emitting = true;
        state = PollutantState.Airborn;
    }
}
