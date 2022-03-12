using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UnplacedTrap : MonoBehaviour
{
    [SerializeField] private GameObject trap;
    private bool canPlace;

    private MeshRenderer meshRenderer;

    [SerializeField] private Material placeable;
    [SerializeField] private Material notPlaceable;

    void Start()
    {
        canPlace = true;

        meshRenderer = GetComponent<MeshRenderer>();
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
    }

    [Button]
    private void RotateTrap()
    {
        transform.Rotate(0, 90, 0);
    }

    [Button]
    private void SpawnTrap()
    {
        if (!canPlace)
            return;

        Instantiate(trap, transform.position, transform.localRotation);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        canPlace = false;
    }

    private void OnTriggerExit(Collider other)
    {
        canPlace = true;
    }
}
