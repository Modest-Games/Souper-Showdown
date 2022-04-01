using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbyController : NetworkBehaviour
{
    // singleton class
    public static LobbyController Instance { get; private set; }

    public bool isDebugEnabled;

    //public int countdownTime;
    //public Text countdownTimer;

    public delegate void LobbyControllerDelegate();
    public static event LobbyControllerDelegate PlayersReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        //NetworkManager.Singleton.StartHost();
    }

    void Update()
    {
        
    }

    public IEnumerator startCountdown()
    {
        int countdownTime = 5;

        // handles countdown timer
        countdownTimer.gameObject.SetActive(true);

        while (countdownTime > 0)
        {
            countdownTimer.text = countdownTime.ToString();
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        if(PlayersReady != null && countdownTime == 0) 
            PlayersReady();

    }
}
