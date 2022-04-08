using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Relay;

public class EndScreenManager : NetworkBehaviour
{
    public static EndScreenManager Instance { get; private set; }

    //Variables used for setting bar graph parameters
    public float maxHeight = 523.0f;
    public float backButtonHoldDuration;

    public GameObject BarGraphPanel;

    public GameObject endScreen;
    public GameObject chefScore;
    public GameObject highlightBubble;

    public Scene lobbyScene;

    public int playerNum = 0;
    public GameObject PlayerBarPrefab;
    public GameObject[] PlayerBars;

    public int endScreenDuration = 30;
    public float endScreenTimer = 0;

    public GameObject timerUI;


    public GameObject chefWins;
    public GameObject spoilersWin;

    public Transform GraphPanel;

    public int numPlayers;

    public Material[] spoilerMaterials;
    public Sprite[] spoilerIcons;
    public Color[] spoilerColors;

    public string[] spoilers = new string[] { "Tomato", "Carrot", "Mushroom", "Eggplant", "Corn", "Onion", "Jalapeno" };
    public Sprite[] playerNumbers;

    private bool hasGameEnded;

    Dictionary<string, int> spoilerDictionary;

    private float timeWhenBackStarted;

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

    // Start is called before the first frame update
    void Start()
    {
        hasGameEnded = false;
        endScreen.SetActive(false);

        // setup event listeners
        PlayerTokenBehaviour.BackActionStarted += BackActionStarted;
        PlayerTokenBehaviour.BackActionCancelled += BackActionCancelled;
        GameController.GameStopped += OnGameOver;
    }

    new private void OnDestroy()
    {
        // clear event listeners
        PlayerTokenBehaviour.BackActionStarted -= BackActionStarted;
        PlayerTokenBehaviour.BackActionCancelled -= BackActionCancelled;
        GameController.GameStopped -= OnGameOver;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasGameEnded)
        {
            endScreenTimer += Time.deltaTime;

            int timeRemaining = endScreenDuration - (int)endScreenTimer;
            timerUI.GetComponent<TMPro.TextMeshProUGUI>().text = timeRemaining.ToString();

            if (endScreenTimer >= endScreenDuration)
            {
                RestartGame();
                //Restart Game
            }

            if (BackButtonHeldAmount > 0)
            {
                float opacityValue = 255.0f * BackButtonHeldAmount;

                Color bubble = highlightBubble.GetComponent<Image>().color;
                bubble.a = opacityValue;
                Debug.Log(opacityValue);
                highlightBubble.GetComponent<Image>().color = bubble;
            }

            // check if the back button has been triggered
            if (BackButtonTriggered)
            {
                //TODO: exit the game session and return to the main menu
                Debug.Log("Back button triggered");

                NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
                NetworkManager.Singleton.Shutdown();

                //NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>().DisconnectLocalClient();

                SceneManager.LoadScene("Lobby");
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Lobby"));
            }
        }
    }

    private void BackActionStarted()
    {
        // code here will be called when a local player stops pressing the back button
        Debug.Log("Back button starts");
        // ensure another player isn't already holding the back button
        if (timeWhenBackStarted > 0f)
            return;

        // set the time when the back button starting being held
        timeWhenBackStarted = Time.time;
    }

    private void BackActionCancelled()
    {
        // code here will be called when a local player starts pressing the back button

        timeWhenBackStarted = -1f;
    }

    private bool BackButtonTriggered
    {
        get
        {
            return (timeWhenBackStarted > 0f) ? (Time.time - timeWhenBackStarted) >= backButtonHoldDuration : false;
        }
    }

    public float BackButtonHeldAmount
    {
        get
        {
            return (timeWhenBackStarted > 0f) ? Mathf.InverseLerp(0f, backButtonHoldDuration, (Time.time - timeWhenBackStarted)) : 0f;
        }
    }

    //ADD WHENEVER NEW VEGGIE CREATED
    void loadSpoilerLibrary()
    {
        //Initialize dictionary
        spoilerDictionary = new Dictionary<string, int>();
        spoilerDictionary.Add("Tomato", 0);
        spoilerDictionary.Add("Carrot", 1);
        spoilerDictionary.Add("Mushroom", 2);
        spoilerDictionary.Add("Eggplant", 3);
        spoilerDictionary.Add("Corn", 4);
        spoilerDictionary.Add("Onion", 5);
        spoilerDictionary.Add("Jalapeno", 6);
    }

    void positionBars()
    {
        SortBars();
    }

    int getScoreFromBar(GameObject p)
    {
        return p.transform.GetChild(0).GetComponent<PlayerBar>().score;
    }

    void setHeights()
    {
        float maxScore = getScoreFromBar(PlayerBars[0]);
        float minScore = getScoreFromBar(PlayerBars[PlayerBars.Length - 1]);

        for (int i = 0; i < PlayerBars.Length - 0; i++)
        {
            float val = Mathf.InverseLerp(minScore, maxScore, getScoreFromBar(PlayerBars[i]));
            val = (val / 2.0f) + 0.45f;
            setBarHeight(PlayerBars[i], val);
        }

    }

    void setBarHeight(GameObject bar, float h)
    {
        bar.transform.GetChild(0).GetComponent<PlayerBar>().setHeight(h);
    }

    void SortBars()
    {
        List<GameObject> list1 = new List<GameObject>();
        list1 = PlayerBars.OfType<GameObject>().ToList();
        list1.Sort((element2, element1) => element1.transform.GetChild(0).GetComponent<PlayerBar>().score.CompareTo(element2.transform.GetChild(0).GetComponent<PlayerBar>().score));

        PlayerBars = list1.ToArray();


        foreach (GameObject pb in PlayerBars)
        {
            Debug.Log(pb);
            //Place bar in panel
            //pb.transform.SetParent(GameObject.FindGameObjectWithTag("BarGraph").transform, false);
            pb.transform.SetParent(GraphPanel, false);
        }
    }

    public void ChefWins()
    {
        Debug.Log("Chef team wins!");
        chefWins.SetActive(true);
        spoilersWin.SetActive(false);
    }

    public void SpoilersWin()
    {
        Debug.Log("Spoiler team wins!");
        chefWins.SetActive(false);
        spoilersWin.SetActive(true);
    }

    GameObject MakeBar(string spoiler, int score, Sprite playerNum)
    {
        //Create player bar
        GameObject playerBar = Instantiate(PlayerBarPrefab, GraphPanel);

        int spoilerIndex;
        if (spoiler == "Tomato")
        {
            spoilerIndex = 0;
        }

        else if (spoiler == "Carrot")
        {
            spoilerIndex = 1;
        }

        else if (spoiler == "Mushroom")
        {
            spoilerIndex = 2;
        }

        else if (spoiler == "Eggplant")
        {
            spoilerIndex = 3;
        }

        else if (spoiler == "Corn")
        {
            spoilerIndex = 4;
        }

        else if (spoiler == "Onion")
        {
            spoilerIndex = 5;
        }

        else
        {
            spoilerIndex = 6;
        }

        playerBar.transform.GetChild(0).GetComponent<PlayerBar>().setUpBar(score, 1.0f, spoilerMaterials[spoilerIndex], spoilerIcons[spoilerIndex], playerNum, spoilerColors[spoilerIndex]);

        //playerNum++;
        return playerBar;
    }

    private IEnumerator StaggerAnimation()
    {
        //Triggers animations consecutively, leaving short delay for the winner
        for (int i = PlayerBars.Length - 1; i > 0; i--)
        {
            PlayerBars[i].transform.GetChild(0).GetComponent<PlayerBar>().startAnimating();
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1.0f);
        PlayerBars[0].transform.GetChild(0).GetComponent<PlayerBar>().startAnimating();
    }

    private void OnGameOver()
    {
        // ensure that the game hasn't already ended ended
        if (hasGameEnded)
            return;

        // show the end screen
        hasGameEnded = true;
        //endScreen = GameObject.Find("End Screen");
        endScreen.SetActive(true);

        loadSpoilerLibrary();

        numPlayers = PlayersManager.Instance.players.Count;
        PlayerBars = new GameObject[numPlayers]; //numPlayers

        List<PlayersManager.Player> players = PlayersManager.Instance.players;

        //collect total chef score and add to UI
        int totalChefScore = 0;
        foreach (PlayersManager.Player p in players)
        {
            if (GetNetworkObject(p.networkObjId).GetComponent<PlayerController>().networkIsChef.Value)
            {
                totalChefScore += PlayersManager.Instance.GetPlayerScore(p.networkObjId);
            }
        }
        chefScore.GetComponent<TMPro.TextMeshProUGUI>().text = totalChefScore.ToString();

        players.Sort((p2, p1) => PlayersManager.Instance.GetPlayerScore(p1.networkObjId).CompareTo(PlayersManager.Instance.GetPlayerScore(p2.networkObjId)));


        for (int i = 0; i < numPlayers; i++)
        {
            PlayersManager.Player p = PlayersManager.Instance.players[i];
            if (!GetNetworkObject(p.networkObjId).GetComponent<PlayerController>().networkIsChef.Value)
            {
                PlayerBars[i] = MakeBar(p.character, PlayersManager.Instance.GetPlayerScore(p.networkObjId), playerNumbers[i]);
            }
        }

        //Sort bars
        //SortBars();

        //Give Verticality to bars
        setHeights();

        //Start Animating
        StartCoroutine(StaggerAnimation());

        SpoilMeter.Instance.gameObject.SetActive(false);
    }

    void RestartGame()
    {
        endScreenTimer = 0;
        hasGameEnded = false;
        endScreen.SetActive(false);

        SceneSwitcher.Instance.SwitchToInGame();
    }
}
