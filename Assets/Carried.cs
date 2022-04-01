using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carried : MonoBehaviour
{
    public Transform target;
    public bool carried;

    void Awake()
    {
        target = null;
        carried = false;
    }

    void Update()
    {
        if (carried && target != null)
        {
            transform.position = target.position;
        }
    }
}
