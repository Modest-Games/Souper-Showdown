using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreAnimation : MonoBehaviour
{
    public TMPro.TextMeshProUGUI label;

    public AudioClip[] soundEffects;

    public AudioClip[] collectFX;

    AudioSource source;

    public int value;


    public void setValue(int v)
    {
        value = v;
        label.text = value.ToString();
    }

    AudioClip playRandomSound(AudioClip[] arr)
    {
        return arr[(int)Random.Range(0, soundEffects.Length)];
    }

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();

        label.text = value.ToString();

        source.PlayOneShot(playRandomSound(soundEffects));
        source.PlayOneShot(playRandomSound(collectFX));

        transform.Rotate(new Vector3(0, 0, Random.Range(-30f, 30f)));
        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {

        yield return new WaitForSeconds(3.0f);
        DeleteSelf();
    }

    void DeleteSelf()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
