using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleControls : MonoBehaviour
{
    public GameObject image;

    public void Toggle()
    {
        Debug.Log("hi");

        if (image.activeInHierarchy == false)
        {
            image.SetActive(true);
        }

        else
        {
            image.SetActive(false);
        }
    }
}
