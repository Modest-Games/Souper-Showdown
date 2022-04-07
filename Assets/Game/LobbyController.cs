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

    // countdown variables
    public int countdownTime;
    private int countdownTimeOrig;
    public Text countdownTimer;

    // player manager (need this for the player list)
    PlayersManager playersManager;
    [SerializeField] GameObject playerManager;

    // total number of players (retrieved from the player list)
    private int numPlayers;
    // number of players in the chef zone
    private int numChefs;
    // number of players in the veggie zone
    private int numVeggies;

    // scene switcher
    public delegate void LobbyControllerDelegate();
    public static event LobbyControllerDelegate SwitchScene;

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

        playersManager = playerManager.GetComponent<PlayersManager>();

        numChefs = 0;
        numVeggies = 0;

        // store original value of countdown timer
        countdownTimeOrig = countdownTime;
    }

    void Start()
    {
        //NetworkManager.Singleton.StartHost();
        PlayerEnter.playersReady += OnPlayerEnter;
        PlayerEnter.playersNotReady += OnPlayerLeave;
    }

    public void OnPlayerEnter(bool isChefZone)
    {

        // get total number of players
        numPlayers = playersManager.players.Count;

        // handle player count
        if (isChefZone == true) {
            numChefs++;
        } else {
            numVeggies++;
        }

        Debug.Log("Total players " + numPlayers);
        Debug.Log("Total chefs " + numChefs);
        Debug.Log("Total veggies " + numVeggies);

        // if all players are on zones and there is at least one chef and one veggie, start countdown
        if (
            numChefs >= 1 &&
            numVeggies >= 1 &&
            numChefs + numVeggies == numPlayers
        ) {
            handleCountDown(true);
        }
    }

    public void OnPlayerLeave(bool isChefZone)
    {
        // update number of veggies and chefs
        if (isChefZone == true) {
            numChefs--;
        } else {
            numVeggies--;
        }
        
        Debug.Log("Total players " + numPlayers);
        Debug.Log("Total chefs " + numChefs);
        Debug.Log("Total veggies " + numVeggies);
        
        // interrupt coroutine
        handleCountDown(false);
    }

    // handles countdown coroutine and resets timers if interrupted
    public void handleCountDown(bool beginCountdown)
    {
        if (beginCountdown == true) {
            StartCoroutine(startCountdown());
        } else if (beginCountdown == false) {
            // when a player leaves their zone, interrupt the coroutine and reset countdown timer
            StopAllCoroutines();
            countdownTime = countdownTimeOrig;
            countdownTimer.text = countdownTime.ToString();
            countdownTimer.gameObject.SetActive(false);
            Debug.Log("Countdown interrupted and reset timer");
        }
    }

    public IEnumerator startCountdown()
    {
        Debug.Log("countdown started");

        // handles countdown timer
        countdownTimer.gameObject.SetActive(true);

        while (countdownTime > 0)
        {
            countdownTimer.text = countdownTime.ToString();
            yield return new WaitForSeconds(1.0f);
            countdownTime--;
        }

        if(SwitchScene != null && countdownTime == 0) 
            SwitchScene();

    }
}
