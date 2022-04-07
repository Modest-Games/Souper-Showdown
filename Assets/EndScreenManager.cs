using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EndScreenManager : MonoBehaviour
{
    //Variables used for setting bar graph parameters
    public float maxHeight = 523.0f;
    public float backButtonHoldDuration;

    public GameObject BarGraphPanel;

    public GameObject endScreen; 

    public int playerNum = 0;
    public GameObject PlayerBarPrefab;
    public GameObject[] PlayerBars;

    public int endScreenDuration = 30;
    public float endScreenTimer = 0;
    public GameObject timerUI;

    public Transform GraphPanel;

    public int numPlayers;

    public Material[] spoilerMaterials;
    public Sprite[] spoilerIcons;
    public Color[] spoilerColors;

    //public int[] presetScores = new int[]{ 1200, 3600, 4300, 2000, 2500, 6000, 500};
    public string[] spoilers = new string[] { "Tomato", "Carrot", "Mushroom", "Eggplant", "Corn", "Onion", "Jalapeno" };
    public Sprite[] playerNumbers;

    private bool hasGameEnded;

    Dictionary<string, int> spoilerDictionary;

    private float timeWhenBackStarted;

    // Start is called before the first frame update
    void Start()
    {
        hasGameEnded = false;
        GameController.GameStopped += OnGameOver;
        endScreen.SetActive(false);

        // setup event listeners
        PlayerTokenBehaviour.BackActionStarted += BackActionStarted;
        PlayerTokenBehaviour.BackActionCancelled += BackActionCancelled;
    }

    private void OnDestroy()
    {
        // clear event listeners
        PlayerTokenBehaviour.BackActionStarted -= BackActionStarted;
        PlayerTokenBehaviour.BackActionCancelled -= BackActionCancelled;
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
                ResetEndScreen();
                //Restart Game
            }

            // check if the back button has been triggered
            if (BackButtonTriggered)
            {
                // TODO: exit the game session and return to the main menu
                //Debug.Log("Back button triggered");
            }
        }
    }

    private void BackActionStarted()
    {
        // code here will be called when a local player stops pressing the back button

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

        for(int i = 0; i < PlayerBars.Length - 0; i++)
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

    GameObject MakeBar(string spoiler, int score, Sprite playerNum) 
    {
        //Create player bar
        GameObject playerBar = Instantiate(PlayerBarPrefab, GraphPanel);

        int spoilerIndex;
        bool hasValue = spoilerDictionary.TryGetValue(spoiler, out spoilerIndex);

        if (!hasValue)
            Debug.Log("spoiler not found!");
        else
        {
            playerBar.transform.GetChild(0).GetComponent<PlayerBar>().setUpBar(score, 1.0f, spoilerMaterials[spoilerIndex], spoilerIcons[spoilerIndex], playerNum, spoilerColors[spoilerIndex]);
        }

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
        endScreen.SetActive(true);

        loadSpoilerLibrary();

        numPlayers = PlayersManager.Instance.players.Count;
        PlayerBars = new GameObject[numPlayers];

        List<PlayersManager.Player> players = PlayersManager.Instance.players;
        players.Sort((p2, p1) => PlayersManager.Instance.GetPlayerScore(p1.networkObjId).CompareTo(PlayersManager.Instance.GetPlayerScore(p2.networkObjId)));

        //Create bars
        for (int i = 0; i < numPlayers; i++)
        {
            //PlayerBars[i] = MakeBar(spoilers[i], presetScores[i], playerNumbers[0]);

            PlayersManager.Player currentPlayer = players[i];

            PlayerBars[i] = MakeBar(currentPlayer.character, PlayersManager.Instance.GetPlayerScore(currentPlayer.networkObjId), playerNumbers[0]);
        }

        //Sort bars
        //SortBars();

        //Give Verticality to bars
        setHeights();

        //Start Animating
        StartCoroutine(StaggerAnimation());

    }

    void ResetEndScreen()
    {
        endScreenTimer = 0;
        hasGameEnded = false;
        GameController.GameStopped += OnGameOver;
        endScreen.SetActive(false);
    }
}
