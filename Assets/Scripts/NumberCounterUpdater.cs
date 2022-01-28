using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumberCounterUpdater : MonoBehaviour
{
    public NumberCounter NumberCounter;

    public void SetValue(int value)
    {
        NumberCounter.Value = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
