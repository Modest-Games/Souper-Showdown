using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;
using NaughtyAttributes;
using Unity.Netcode.Components;
using Cinemachine;

public class PollutantBehaviour : NetworkBehaviour
{
    public enum PollutantState
    {
        Idle, 
        Airborn
    }

    // Assigned by object spawner:
    public Pollutant pollutantObject;

    public bool meshInitialized;

    [ReadOnly] public PollutantState state;

    public NetworkVariable<ulong> throwerId = new NetworkVariable<ulong>();

    private TrailRenderer trail;

    public Rigidbody rb;

    void Awake()
    {
        // setup variables
        trail = gameObject.GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody>();
        meshInitialized = false;
    }

    void Start()
    {
        if (!IsServer) return;

        SetPollutantObjectClientRpc(pollutantObject.type);
    }

    void Update()
    {
        // Check if the mesh needs to be refreshed
        if (!meshInitialized && pollutantObject != null)
        {
            RefreshMesh();
            meshInitialized = true;
        }
    }

    [Button]
    private void RefreshMesh()
    {
        Transform oldMesh = transform.Find("Mesh");
        if (oldMesh != null)
        {
            DestroyImmediate(oldMesh.gameObject);
        }

        GameObject newMesh = Instantiate(pollutantObject.mesh, transform);
        newMesh.name = "Mesh";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
            return;

        switch (collision.gameObject.tag)
        {
            case "Ground":
                trail.emitting = false;
                state = PollutantState.Idle;

                if (pollutantObject.playerID != -1)
                {
                    ReplaceLiveMeshClientRpc((ulong) pollutantObject.playerID);
                    gameObject.SetActive(false);
                }

                break;
        }
    }

    [ClientRpc]
    public void ReplaceLiveMeshClientRpc(ulong playerID)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerID, out var playerToTeleport);
        if (playerToTeleport == null) return;

        playerToTeleport.GetComponentInParent<Rigidbody>().isKinematic = false;
        playerToTeleport.GetComponentInParent<SphereCollider>().enabled = true;
        playerToTeleport.transform.Find("Character").gameObject.SetActive(true);

        var playerController = playerToTeleport.GetComponent<PlayerController>();
        playerController.TeleportPlayer(transform.position);

        CinemachineTargetGroup camTargetGroup = GameObject.Find("CineMachine Target Group").GetComponent<CinemachineTargetGroup>();
        camTargetGroup.AddMember(playerToTeleport.transform, 1f, 0f);

        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void SetPollutantObjectClientRpc(string type)
    {
        pollutantObject = ObjectSpawner.Instance.pollutantList.Find(x => x.type == type);

        if (pollutantObject == null)
        {
            pollutantObject = ObjectSpawner.Instance.deadBodyList.Find(x => x.type == type);
        }

        if (pollutantObject == null)
        {
            pollutantObject = ObjectSpawner.Instance.liveBodyList.Find(x => x.type == type);
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
