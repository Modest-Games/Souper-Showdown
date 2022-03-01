using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    public Transform looseArms;
    public Transform stiffArms;
    public Transform legs;
    public Animator animator;

    public void UpdateLegs(PlayerController.PlayerState carryState)
    {
        switch (carryState)
        {
            case PlayerController.PlayerState.Idle:
                animator.StartPlayback();
                break;
            case PlayerController.PlayerState.Dazed:
                animator.StopPlayback();
                break;
            case PlayerController.PlayerState.Moving:
                animator.StopPlayback();
                break;
            default:
                break;
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
