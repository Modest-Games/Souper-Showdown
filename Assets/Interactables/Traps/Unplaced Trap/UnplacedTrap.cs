using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnplacedTrap : NetworkBehaviour
{
    [SerializeField] private GameObject trap;
    private bool validPlacement;
    public bool canPlace;

    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;

    [SerializeField] private Material placeable;
    [SerializeField] private Material invalid;
    [SerializeField] private Material notPlaceable;
    [SerializeField] private GameObject crate;

    private Quaternion rotation;

    void Start()
    {
        meshRenderer = transform.GetComponent<MeshRenderer>();
        lineRenderer = transform.GetComponentInChildren<LineRenderer>();

        canPlace = true;
        validPlacement = true;
    }

    void Update()
    {
        //if (canPlace)
        //    meshRenderer.material = validPlacement ? placeable : invalid;
        //else
        //    meshRenderer.material = notPlaceable;

        meshRenderer.material = validPlacement ? placeable : invalid;
        lineRenderer.enabled = validPlacement;

        // correct the trap's rotation
        transform.rotation = Quaternion.Euler(transform.InverseTransformDirection(Vector3.forward) + rotation.eulerAngles);
        //transform.Rotate(rotation.eulerAngles, Space.World);
    }

    private void OnEnable()
    {
        // setup event listeners
        if (TrapManager.Instance == null) return;
        TrapManager.Instance.networkNumTrapsPlaced.OnValueChanged += OnNumTrapsChanged;
    }

    private void OnDisable()
    {
        // clear event listeners
        if (TrapManager.Instance == null) return;
        TrapManager.Instance.networkNumTrapsPlaced.OnValueChanged -= OnNumTrapsChanged;

        canPlace = false;
    }

    private void OnNumTrapsChanged(int oldNum, int newNum)
    {
        canPlace = newNum < TrapManager.Instance.numTrapsAllowed.Value;
    }

    public void RotateTrap()
    {
        rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
    }

    public bool SpawnTrap()
    {
        // ensure the trap can be placed here
        if (!invalid)
            return false;

        // spawn the trap
        RequestTrapSpawnServerRpc(transform.position, rotation);

        gameObject.SetActive(false);
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (SceneManager.GetActiveScene().name != "InGame")
            return;

        // don't collide with self
        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerController>().NetworkObjectId != NetworkObjectId)
            return;

        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap" || other.tag == "Soup Pot")
        {
            validPlacement = false;
            canPlace = TrapManager.Instance.networkNumTrapsPlaced.Value < TrapManager.Instance.numTrapsAllowed.Value;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (SceneManager.GetActiveScene().name != "InGame")
            return;

        // don't collide with self
        if (other.tag == "Player" && other.gameObject.GetComponent<PlayerController>().NetworkObjectId != NetworkObjectId)
            return;

        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap" || other.tag == "Soup Pot")
        {
            validPlacement = true;
            canPlace = TrapManager.Instance.networkNumTrapsPlaced.Value < TrapManager.Instance.numTrapsAllowed.Value;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTrapSpawnServerRpc(Vector3 trapPosition, Quaternion trapRotation)
    {
        // don't place if the trap if the maxinum number of traps is reached
        if (TrapManager.Instance.networkNumTrapsPlaced.Value >= TrapManager.Instance.numTrapsAllowed.Value)
            return;

        GameObject newTrap = Instantiate(trap, trapPosition, trapRotation);
        newTrap.GetComponent<NetworkObject>().Spawn();
        TrapManager.Instance.networkNumTrapsPlaced.Value++;
    }
}
