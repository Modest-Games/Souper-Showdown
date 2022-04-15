using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableElement : MonoBehaviour
{

    public GameObject obj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnDisable()
    {
        obj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
