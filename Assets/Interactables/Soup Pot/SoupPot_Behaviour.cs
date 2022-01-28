using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SoupPot_Behaviour : NetworkBehaviour
{
    public delegate void SoupPotDelegate();
    public static event SoupPotDelegate SoupReceivedTrash;
    public static event SoupPotDelegate SoupReceivedPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            switch (other.gameObject.tag)
            {
                case "Pollutant":
                    // make sure the pollutant is airborn
                    if (other.gameObject.GetComponent<PollutantBehaviour>().state
                        == PollutantBehaviour.PollutantState.Airborn)
                    {
                        // destroy the trash
                        Destroy(other.gameObject);

                        // call the received trash event
                        if (SoupReceivedTrash != null)
                            SoupReceivedTrash();
                    }

                    break;

                case "Character":
                    // kill the player

                    // call the received player event
                    if (SoupReceivedPlayer != null)
                        SoupReceivedPlayer();

                    break;
            }
        }
    }
}
