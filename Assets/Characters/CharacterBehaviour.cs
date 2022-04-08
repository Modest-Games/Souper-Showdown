using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    public Animator armsController;

    public Transform stillLegs;
    public Transform movingLegs;

    public MeshRenderer face;

    public Material idleFace;
    public Material dashingFace;

    public void UpdateLegs(PlayerController.PlayerState playerState, bool canMove)
    {
        if (playerState == PlayerController.PlayerState.Idle)
        {
            stillLegs.gameObject.SetActive(true);
            movingLegs.gameObject.SetActive(false);
        }

        else
        {
            if (!canMove) return;

            stillLegs.gameObject.SetActive(false);
            movingLegs.gameObject.SetActive(true);
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

    public void AnimateArms(string animation)
    {
        armsController.Play(animation);
    }
}
