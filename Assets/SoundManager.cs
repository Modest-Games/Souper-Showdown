using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //public static AudioPlayer audioPlayerInstance;

    public AudioClip[] audioClips;
    public AudioSource sourcePrefab;

    private void Awake()
    {
        //if (audioPlayerInstance == null) { audioPlayerInstance = this; }
    }

    public void PlayClip(AudioClip sound, bool loop, float volume)
    {
        AudioSource temp;

        temp = Instantiate(sourcePrefab);
        Destroy(temp, sound.length + 1.0f);

        if (loop)
        {
            temp.loop = true;
        }

        temp.clip = sound;
        temp.volume = volume;
        temp.Play();
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
