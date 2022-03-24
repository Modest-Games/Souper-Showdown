using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    public Transform looseArms;
    public Transform stiffArms;
    public Transform stillLegs;
    public Transform movingLegs;


    public void UpdateLegs(PlayerController.PlayerState playerState)
    {
        if (playerState == PlayerController.PlayerState.Idle)
        {
            Debug.Log("Not moving");
            stillLegs.gameObject.SetActive(true);
            movingLegs.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Moving");
            stillLegs.gameObject.SetActive(false);
            movingLegs.gameObject.SetActive(true);
        }
    }

    public void UpdateArms(PlayerController.PlayerCarryState carryState)
    {
        if (carryState == PlayerController.PlayerCarryState.Empty)
        {
            Debug.Log("Loose arms engaged");
            looseArms.gameObject.SetActive(true);
            stiffArms.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Stiff arms engaged");
            stiffArms.gameObject.SetActive(true);
            looseArms.gameObject.SetActive(false);
        }
    }


}
