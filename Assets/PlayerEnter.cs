using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnter : MonoBehaviour
{

    public delegate void PlayerEnterZoneDelegate(bool isChefZone);
    public static event PlayerEnterZoneDelegate playersReady;
    public bool isChefZone;

    public delegate void PlayerLeaveZoneDelegate();
    public static event PlayerLeaveZoneDelegate playersNotReady;

    private void OnTriggerEnter(Collider other)
    {
        // this player is ready
        if (playersReady != null)
        {
            playersReady(isChefZone);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        // this player is not ready
        if (playersNotReady != null)
        {
            playersNotReady();
        }
    }

}
