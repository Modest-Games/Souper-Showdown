using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTriggerAnimation : MonoBehaviour
{

    [SerializeField] private Animator faceAnim;
    [SerializeField] private string animName = "FaceAnim";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerFaceAnimation()
    {
        faceAnim.Play(animName, 0, 0.0f);
    }
}
