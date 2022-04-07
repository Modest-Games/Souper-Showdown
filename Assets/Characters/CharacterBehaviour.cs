using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    public Transform looseArms;
    public Transform stiffArms;
    public Transform stillLegs;
    public Transform movingLegs;
    public MeshRenderer face;

    public Material idleFace;
    public Material dashingFace;

    public void UpdateLegs(PlayerController.PlayerState playerState)
    {
        if (playerState == PlayerController.PlayerState.Idle)
        {
            stillLegs.gameObject.SetActive(true);
            movingLegs.gameObject.SetActive(false);
        }
        else
        {
            stillLegs.gameObject.SetActive(false);
            movingLegs.gameObject.SetActive(true);
        }
    }

    public void UpdateArms(PlayerController.PlayerCarryState carryState)
    {
        if (carryState == PlayerController.PlayerCarryState.Empty)
        {
            looseArms.gameObject.SetActive(true);
            stiffArms.gameObject.SetActive(false);
        }
        else
        {
            stiffArms.gameObject.SetActive(true);
            looseArms.gameObject.SetActive(false);
        }
    }

    public void UpdateFace(PlayerController.PlayerState playerState)
    {
        if (playerState == PlayerController.PlayerState.Dashing)
        {
            face.material = dashingFace;
        }

        else
        {
            face.material = idleFace;
        }
    }
}
