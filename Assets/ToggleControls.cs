using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleControls : MonoBehaviour
{
    public static ToggleControls Instance { get; private set; }

    public GameObject image;

    private Animator animator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

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
