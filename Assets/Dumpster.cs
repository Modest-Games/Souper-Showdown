using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Dumpster : NetworkBehaviour
{
    public GameObject pollutantPrefab;

    public void SpawnPoulltant(Pollutant pollutant)
    {
        // spawn the new pollutant
        Vector3 spawnPos = new Vector3(transform.position.x + 1.0f, transform.position.y + 4.0f, transform.position.z);
        GameObject newPollutant = Instantiate(pollutantPrefab, spawnPos, Quaternion.identity);
        newPollutant.GetComponent<PollutantBehaviour>().pollutantObject = pollutant;
        newPollutant.GetComponent<NetworkObject>().Spawn();

        newPollutant.GetComponent<Rigidbody>().AddForce((-transform.forward.normalized * 2.5f) + (Vector3.up * 6f), ForceMode.Impulse);
    }
}
