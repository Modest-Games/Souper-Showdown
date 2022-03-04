using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ArrowTrapBehaviour : NetworkBehaviour
{
    [Header("Config")]
    public GameObject arrowTrapMesh;
    public GameObject arrowPrefab;
    public float timeBetweenShots;
    [Header("Arrow Config")]
    public float arrowTravelDistance;
    public float arrowTravelSpeed;
    public float arrowExpireTime;

    private Transform meshHolder;
    private float timeSinceLastShot;
    private bool shouldUpdate;

    void Start()
    {
        // setup variables
        shouldUpdate = true; // TEMP
        timeSinceLastShot = timeBetweenShots;
        meshHolder = transform.Find("Mesh");
    }

    void Update()
    {
        if (IsServer)
        {
            if (shouldUpdate)
            {
                // check if should shoot
                if (timeSinceLastShot >= timeBetweenShots)
                {
                    Shoot();

                }
                else
                {
                    timeSinceLastShot += Time.deltaTime;
                }
            }
        }
    }

    public void Shoot()
    {
        // create an arrow
        GameObject newArrow = Instantiate(arrowPrefab, new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z), transform.rotation);
        newArrow.GetComponent<NetworkObject>().Spawn();

        ArrowBehaviour newArrowBehaviour = newArrow.GetComponent<ArrowBehaviour>();

        // configure the arrow
        newArrowBehaviour.travelDistance = arrowTravelDistance;
        newArrowBehaviour.travelSpeed = arrowTravelSpeed;
        newArrowBehaviour.expireTime = arrowExpireTime;

        // reset time since last shot
        timeSinceLastShot = 0f;
    }
}
