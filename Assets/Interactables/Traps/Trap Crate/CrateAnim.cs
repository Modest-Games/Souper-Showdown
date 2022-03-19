using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CrateAnim : MonoBehaviour
{
    public Animator animator;

    [Button]
    public void PlayAnim()
    {
        animator.Play("Open");
    }
}
