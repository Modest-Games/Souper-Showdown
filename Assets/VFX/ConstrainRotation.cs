using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainRotation : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(transform.InverseTransformDirection(Vector3.forward));
    }
}
