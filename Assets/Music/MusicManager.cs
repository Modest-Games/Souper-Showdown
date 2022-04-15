using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    [SerializeField] private AudioSource currentSong = null;

    public AudioSource CurrentSong
    {
        get => currentSong;
        set => currentSong = value;
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
