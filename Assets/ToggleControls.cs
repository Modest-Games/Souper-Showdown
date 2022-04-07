using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleControls : MonoBehaviour
{
    public GameObject image;

    private Animator animator;

    void Start()
    {
        animator = image.GetComponent<Animator>();
    }


    public void DisableObject()
    {
        image.SetActive(false);
    }

    public void Toggle()
    {

        if (image.activeInHierarchy == false)
        {
            image.SetActive(true);
            animator.ResetTrigger("Drop");
        }

        else
        {
            Debug.Log("Drop recieved");
            animator.StopPlayback();
            animator.SetTrigger("Drop");
        }
    }
}
