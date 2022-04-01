using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class SoupPot_Behaviour : NetworkBehaviour
{
    public delegate void SoupPotDelegate(float influence, ulong throwerId);
    public static event SoupPotDelegate SoupReceivedTrash;

    public ParticleSystem ps;

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            switch (other.gameObject.tag)
            {
                case "Pollutant":
                    // make sure the pollutant is airborn
                        // destroy the trash
                        other.gameObject.GetComponent<SphereCollider>().isTrigger = false;
                        StartCoroutine(OnPollutantEnter(other.gameObject));

                        // call the received trash event
                        if (SoupReceivedTrash != null)
                        {
                            PollutantBehaviour otherPollutantBehaviour = other.gameObject.GetComponent<PollutantBehaviour>();
                            SoupReceivedTrash(
                                otherPollutantBehaviour.pollutantObject.effectAmount, otherPollutantBehaviour.throwerId.Value);
                        }

                    break;
            }
        }
    }

    private IEnumerator OnPollutantEnter(GameObject pollutant)
    {
        var liveMeshPlayerID = pollutant.GetComponent<PollutantBehaviour>().pollutantObject.playerID;
        if(liveMeshPlayerID != -1)
            RespawnPlayerFromLiveMeshClientRpc((ulong) liveMeshPlayerID);

        yield return new WaitForSeconds(0.25f);

        Destroy(pollutant);
        OnPollutantEnterClientRpc();

    }

    [ClientRpc]
    public void RespawnPlayerFromLiveMeshClientRpc(ulong playerID)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerID, out var playerToRespawn);
        if (playerToRespawn == null) return;

        playerToRespawn.GetComponentInParent<Rigidbody>().isKinematic = false;
        playerToRespawn.GetComponentInParent<SphereCollider>().enabled = true;
        playerToRespawn.transform.Find("Character").gameObject.SetActive(true);

        playerToRespawn.GetComponentInParent<PlayerController>().PlayerRandomSpawnPoint(false);

        CinemachineTargetGroup camTargetGroup = GameObject.Find("CineMachine Target Group").GetComponent<CinemachineTargetGroup>();
        camTargetGroup.AddMember(playerToRespawn.transform, 1f, 0f);
    }

    [ClientRpc]
    private void OnPollutantEnterClientRpc()
    {
        ps.Play();
    }
}
