using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnplacedTrap : NetworkBehaviour
{
    [SerializeField] private GameObject trap;
    private bool canPlace;

    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;

    [SerializeField] private Material placeable;
    [SerializeField] private Material notPlaceable;
    [SerializeField] private GameObject crate;

    private Quaternion rotation;

    void Start()
    {
        meshRenderer = transform.GetComponent<MeshRenderer>();
        lineRenderer = transform.GetComponentInChildren<LineRenderer>();
        
        SetCanPlace(true);
    }

    void Update()
    {
        if (canPlace)
        {
            meshRenderer.material = placeable;
        }

        else
        {
            meshRenderer.material = notPlaceable;
        }

        // correct the trap's rotation
        transform.rotation = Quaternion.Euler(transform.InverseTransformDirection(Vector3.forward) + rotation.eulerAngles);
        //transform.Rotate(rotation.eulerAngles, Space.World);
    }

    private void OnDisable()
    {
        SetCanPlace(true);
    }

    public void RotateTrap()
    {
        rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
    }

    public bool SpawnTrap()
    {
        // ensure the trap can be placed here
        if (!canPlace)
            return false;

        // spawn the trap
        SpawnTrapServerRpc(transform.position, rotation);

        gameObject.SetActive(false);
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap" || other.tag == "Soup Pot")
            SetCanPlace(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap" || other.tag == "Soup Pot")
            SetCanPlace(true);
    }

    private void SetCanPlace(bool newCanPlace)
    {
        canPlace = newCanPlace;
        lineRenderer.enabled = newCanPlace;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnTrapServerRpc(Vector3 trapPosition, Quaternion trapRotation)
    {
        GameObject newTrap = Instantiate(trap, trapPosition, trapRotation);
        newTrap.GetComponent<NetworkObject>().Spawn();
    }
}
