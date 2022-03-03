using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SoupPot_Behaviour : NetworkBehaviour
{
    public delegate void SoupPotDelegate(float influence);
    public static event SoupPotDelegate SoupReceivedTrash;
    public static event SoupPotDelegate SoupReceivedPlayer;

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
                            SoupReceivedTrash(other.gameObject.GetComponent<PollutantBehaviour>().pollutantObject.effectAmount);

                    break;
            }
        }
    }

    private IEnumerator OnPollutantEnter(GameObject pollutant)
    {
        yield return new WaitForSeconds(0.25f);

        Destroy(pollutant);
        OnPollutantEnterClientRpc();

    }

    [ClientRpc]
    private void OnPollutantEnterClientRpc()
    {
        ps.Play();
    }
}
