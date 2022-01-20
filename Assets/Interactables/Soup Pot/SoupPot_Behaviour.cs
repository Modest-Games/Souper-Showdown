using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoupPot_Behaviour : MonoBehaviour
{
    public delegate void SoupPotDelegate();
    public static event SoupPotDelegate SoupReceivedTrash;
    public static event SoupPotDelegate SoupReceivedPlayer;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Pollutant":
                // destroy the trash
                Destroy(other.gameObject);

                // call the received trash event
                if (SoupReceivedTrash != null)
                    SoupReceivedTrash();

                Debug.Log("Soup received pollutant!");

                break;

            case "Character":
                // kill the player

                // call the received player event
                if (SoupReceivedPlayer != null)
                    SoupReceivedPlayer();

                Debug.Log("Soup received player!");

                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Pollutant":
                // destroy the trash
                Destroy(collision.gameObject);

                // call the received trash event
                if (SoupReceivedTrash != null)
                    SoupReceivedTrash();

                Debug.Log("Soup received pollutant!");

                break;

            case "Character":
                // kill the player

                // call the received player event
                if (SoupReceivedPlayer != null)
                    SoupReceivedPlayer();

                Debug.Log("Soup received player!");

                break;
        }
    }
}
