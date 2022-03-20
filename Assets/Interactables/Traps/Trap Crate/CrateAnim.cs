using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CrateAnim : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        Destroy(gameObject, 1f);
    }
}
