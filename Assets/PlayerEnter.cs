using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnter : MonoBehaviour
{

    public delegate void PlayerEnterZoneDelegate(bool isVeggie);
    public event PlayerEnterZoneDelegate playersReady;
    public bool isChefZone;

    // Start is called before the first frame update
    void Awake()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ur mom");

        // this player is ready
        if (playersReady != null)
        {
            playersReady(isChefZone);
        }
    }

    void Update()
    {

    }
}
