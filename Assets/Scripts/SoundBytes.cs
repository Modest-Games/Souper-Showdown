using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBytes : MonoBehaviour
{
    // Start is called before the first frame update


    public AudioClip uiMeterFill1;
    public AudioClip uiMeterFill2;
    public AudioClip uiMeterFill3;
    public AudioClip uiMeterFill4;

    public AudioClip smallFill1;
    public AudioClip smallFill2;
    public AudioClip bigFill1;
    public AudioClip bigFill2;

    public AudioClip bubbles1;
    public AudioClip bubbles2;

    public AudioClip bubbles3;
    public AudioClip bubbles4;

    AudioSource source;


    void Start()
    {
        source = GetComponent<AudioSource>();
    }


    public void playSmallClip()
    {
        int rand = (int)Random.Range(0, 2);
        if (rand == 0)
        {
            source.PlayOneShot(uiMeterFill1);
        }
        else
        {
            source.PlayOneShot(uiMeterFill2);
        }
        StartCoroutine(SmallBubbleDelay());
    }

    IEnumerator SmallBubbleDelay()
    {
        yield return new WaitForSeconds(1);
        int rand = (int)Random.Range(0, 2);
        if (rand == 0)
        {
            source.PlayOneShot(uiMeterFill3);
        }
        else
        {
            source.PlayOneShot(uiMeterFill4);
        }
    }

    IEnumerator LargeBubbleDelay()
    {
        yield return new WaitForSeconds(1.5f);
        int rand = (int)Random.Range(0, 2);
        if (rand == 0)
        {
            source.PlayOneShot(bubbles3);
        }
        else
        {
            source.PlayOneShot(bubbles4);
        }
    }

    public void playBigClip()
    {
        int rand = (int)Random.Range(0, 2);
        if (rand == 0)
        {
            source.PlayOneShot(bigFill1);
        }
        else
        {
            source.PlayOneShot(bigFill2);
        }
        StartCoroutine(LargeBubbleDelay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
