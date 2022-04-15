using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionMusic : MonoBehaviour
{
    [SerializeField] private AudioSource musicToChange = null;


    public void PlayFromStart()
    {
        MusicManager mm = FindObjectOfType<MusicManager>();
        mm.CurrentSong.Play();
    }

    public void ChangeSong()
    {
        MusicManager mm = FindObjectOfType<MusicManager>();
        if (mm == null) return;
        if (mm.CurrentSong == null)
        {
            mm.CurrentSong = musicToChange;
            mm.CurrentSong.mute = false;
            return;
        }

        mm.CurrentSong.mute = true;
        mm.CurrentSong = musicToChange;
        mm.CurrentSong.mute = false;
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
