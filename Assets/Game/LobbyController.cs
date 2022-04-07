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

    public int countdownTime;
    public Text countdownTimer;

    // player manager (need this for the player list)
    PlayersManager playersManager;
    [SerializeField] GameObject manager;

    // total number of players (retrieved from the player list)
    private int numPlayers;
    // number of players in the chef zone
    private int numChefs;
    // number of players in the veggie zone
    private int numVeggies;

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

        playersManager = manager.GetComponent<PlayersManager>();
    }

    void Start()
    {
        //NetworkManager.Singleton.StartHost();
        FindObjectOfType<PlayerEnter>().playersReady += OnPlayerEnter;
    }

    public void OnPlayerEnter(bool isChefZone)
    {
        Debug.Log("player entered and I'm an event");
        Debug.Log(isChefZone);
        // get total number of players
        numPlayers = playersManager.players.Count;

        // handle player count
        if (isChefZone == true) {
            numChefs++;
        } else {
            numVeggies++;
        }

        // if all players are on zones and there is at least one chef and one veggie, start countdown
        if (
            numChefs >= 1 &&
            numVeggies >= 1 &&
            numChefs + numVeggies == numPlayers
        ) {
            handleCountDown(true);
        }
    }

    // handles countdown coroutine and resets timers if interrupted
    public void handleCountDown(bool beginCountdown)
    {
        if (beginCountdown == true) {
            StartCoroutine(startCountdown());
        } else if (beginCountdown == false) {
            StopCoroutine(startCountdown());
        }
    }

    public IEnumerator startCountdown()
    {
        int countdownTime = 5;
        Debug.Log("coroutine started");

        // handles countdown timer
        countdownTimer.gameObject.SetActive(true);

        while (countdownTime > 0)
        {
            countdownTimer.text = countdownTime.ToString();
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        // if(PlayersReady != null && countdownTime == 0) 
        //     PlayersReady();

    }
}
