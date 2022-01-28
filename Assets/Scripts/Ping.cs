using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ping : MonoBehaviour
{

    public float expansionScalar;
    public float duration;
    
    float endTime;
    float scaleInterval;
    float opacInterval;
    float currentOpac;

    // Start is called before the first frame update
    void Start()
    {

        currentOpac = 255f;
        endTime = 0f;
        scaleInterval = expansionScalar / duration * Time.deltaTime;
        opacInterval = 255f / duration * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        endTime += Time.deltaTime;
        float temp = transform.localScale.x + scaleInterval;
        transform.localScale = new Vector3(temp, temp, temp);

        Color tmp = gameObject.GetComponent<SpriteRenderer>().color;
        currentOpac -= 0.35f;
        tmp.a = currentOpac;

        //Debug.Log(tmp.a);
        GetComponent<SpriteRenderer>().color = tmp;

        if (endTime > duration)
        {
            Destroy(gameObject);
        }
    }
}
