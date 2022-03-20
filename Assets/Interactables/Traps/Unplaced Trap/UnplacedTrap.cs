using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UnplacedTrap : MonoBehaviour
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

    [Button]
    public void RotateTrap()
    {
        rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
    }

    [Button]
    public bool SpawnTrap()
    {
        if (!canPlace)
            return false;

        Instantiate(crate, transform.position, crate.transform.localRotation);
        Instantiate(trap, transform.position, rotation);
        gameObject.SetActive(false);
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap")
            SetCanPlace(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Wall" || other.tag == "Player" || other.tag == "Trap")
            SetCanPlace(true);
    }

    private void SetCanPlace(bool newCanPlace)
    {
        canPlace = newCanPlace;
        lineRenderer.enabled = newCanPlace;
    }
}
