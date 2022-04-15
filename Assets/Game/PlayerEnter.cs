using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnter : MonoBehaviour
{
    public bool isChefZone;

    public delegate void PlayerEnterZoneDelegate(bool isChefZone);
    public static event PlayerEnterZoneDelegate playersReady;

    public delegate void PlayerLeaveZoneDelegate(bool isChefZone);
    public static event PlayerLeaveZoneDelegate playersNotReady;

    private void OnTriggerEnter(Collider other)
    {
        // this player is ready
        if (other.GetComponent<Collider>().tag == "Player" && playersReady != null)
        {
            playersReady(isChefZone);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        // this player is not ready
        if (other.GetComponent<Collider>().tag == "Player" && playersNotReady != null)
        {
            playersNotReady(isChefZone);
        }
    }

}
