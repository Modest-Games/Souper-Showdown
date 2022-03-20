using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CrateAnim : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        StartCoroutine(DestroyObjectCoroutine());
    }

    public IEnumerator DestroyObjectCoroutine()
    {
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }
}
