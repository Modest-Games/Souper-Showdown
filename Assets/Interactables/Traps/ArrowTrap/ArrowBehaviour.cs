using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArrowBehaviour : NetworkBehaviour
{
    [Header("Config")]
    public GameObject arrowMesh;
    public float travelDistance = 5f;
    public float travelSpeed = 1f;
    [Tooltip("How long the arrow should be visible after colliding")]
    public float expireTime = 0.5f;

    private Transform meshHolder;
    private Vector3 startPosition;
    private float distanceTravelled = 0f;
    private bool shouldUpdate;
    private bool hasCollided = false;
    private float timeSinceCollision = 0f;

    void Start()
    {
        // setup variables
        shouldUpdate = true; // TEMP
        startPosition = transform.position;
        meshHolder = transform.Find("Mesh");

        // refresh the mesh
        RefreshMesh();
    }

    void Update()
    {
        if (!IsServer) return;

        // make sure the arrow should be updating
        if (shouldUpdate)
        {
            if (hasCollided)
            {
                // if the arrow has collided, check if it should expire
                if (timeSinceCollision >= expireTime)
                {
                    //destroy the arrow
                    Destroy(gameObject);

                }
                else
                {
                   DoFadeOut();
                }

            }
                
            else
            {
                // check if the arrow has travelled enough
                if (distanceTravelled >= travelDistance)
                {
                    // destroy the arrow
                    Destroy(gameObject);

                }
   
                else
                {
                    // continue moving forward
                    Move();
                }
            }
        }
    }

    private void DoFadeOut()
    {
        // add delta time to the timeSinceCollision
        timeSinceCollision += Time.deltaTime;
    }

    private void Move()
    {
        // calculate how much the arrow should move
        float moveAmount = travelSpeed / 10f;

        transform.Translate(transform.forward * moveAmount, Space.World);

        // update distance travelled
        distanceTravelled += moveAmount;
    }

    public void RefreshMesh()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Soup Pot") || other.CompareTag("Wall"))
        {
            hasCollided = true;
        }

        if(other.CompareTag("Player"))
        {
            var playerController = other.gameObject.GetComponent<PlayerController>();

            // Don't kill Chef player with it's own trap: 
            if (playerController.networkIsChef.Value == true) return;

            // Send message to client that got hit to respawn itself:
            playerController.KillPlayerClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { other.gameObject.GetComponent<NetworkObject>().OwnerClientId }
                }
            });
        }
    }
}
