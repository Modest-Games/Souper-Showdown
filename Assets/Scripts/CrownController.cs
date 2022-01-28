using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] Frames;
    Vector3 endPosition;
    private static float xOffset = -56.3f;
    int prevPlayer = 0;


    public AudioClip[] soundEffects;

    AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    AudioClip playRandomSound(AudioClip[] arr)
    {
        return arr[(int)Random.Range(0, soundEffects.Length)];
    }


    public void setLeadPlayer(int i)
    {
        Debug.Log(Frames[i].transform.position);
        endPosition = Frames[i].transform.position;

        if (i != prevPlayer)
        {
            source.PlayOneShot(playRandomSound(soundEffects));
        }

        Vector3 finalPos = new Vector3(endPosition.x, transform.position.y, transform.position.z);
        StartCoroutine(SmoothMove(transform.position, finalPos, 0.7f));

        prevPlayer = i;
    }

    private IEnumerator SmoothMove(Vector3 sPos, Vector3 ePos, float dur)
    {

        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(sPos, ePos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
