using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrapBehaviour : MonoBehaviour
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

        // refresh the mesh
        RefreshMesh();
    }

    void Update()
    {
        if (shouldUpdate)
        {
            // check if should shoot
            if (timeSinceLastShot >= timeBetweenShots)
            {
                Shoot();

            } else
            {
                timeSinceLastShot += Time.deltaTime;
            }
        }
    }

    public void RefreshMesh()
    {

    }

    public void Shoot()
    {
        // create an arrow
        GameObject newArrow = Instantiate(arrowPrefab, transform.position, transform.rotation);
        ArrowBehaviour newArrowBehaviour = newArrow.GetComponent<ArrowBehaviour>();

        // configure the arrow
        newArrowBehaviour.travelDistance = arrowTravelDistance;
        newArrowBehaviour.travelSpeed = arrowTravelSpeed;
        newArrowBehaviour.expireTime = arrowExpireTime;

        // reset time since last shot
        timeSinceLastShot = 0f;
    }
}
