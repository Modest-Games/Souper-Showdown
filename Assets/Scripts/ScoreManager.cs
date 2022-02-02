using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public int scoreValue = 0;
    public TMPro.TextMeshProUGUI scoreLabel;
    public NumberCounter NumberCounter;
    

    public PlayerStats()
    {
        scoreValue = 0;
    }

    public void setScoreLabel(TMPro.TextMeshProUGUI l)
    {
        scoreLabel = l;
    }

    public void setNumberCounter(NumberCounter nc)
    {
        NumberCounter = nc;
    }

    public void addScore(int val)
    {
        scoreValue += val;
        //scoreLabel.text = scoreValue.ToString();
        NumberCounter.Value = scoreValue;

    }

    
}

public class ScoreManager : MonoBehaviour
{
    public int timePerScore;
    float timeSinceLast;

    //ON FIRE variables
    public int onFireTime = 5;
    public float onFireParticles = 50;
    public float onFireVelocity = 0.5f;
    float defaultVelocity = -4.15f;

    //Crown variables
    public GameObject crown;
    bool crownActive = false;
    int leadPlayer = 0;


    public int playerMax = 4;
    public int highscoreMin;


    float defaultSize = 38.6f;

    public GameObject score;
    public GameObject scoreSmall;
    public GameObject canvas;

    public GameObject[] playerscoreGameObject;

    public TMPro.TextMeshProUGUI[] playerScoreGUI;
    public ParticleSystem[] playerParticleSystems;
    public bool[] canShrink;

    PlayerStats[] playerScore;


    public Transform[] players;

    float randomOffsetx, randomOffsety;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceLast = 0f;
        canShrink = new bool[4];
        playerScore = new PlayerStats[4];

        //playerscoreGameObject = new GameObject[4];

        for (int profile = 0; profile < transform.childCount; profile++)
        {
            //playerscoreGameObject[profile] = transform.GetChild(profile).Find("Frame").Find("PlayerScore").gameObject;
        }

        for (int i = 0; i < playerMax; i++)
        {
            playerScore[i] = new PlayerStats();
            playerScore[i].setScoreLabel(playerScoreGUI[i]);
            canShrink[i] = false;

            playerScore[i].setNumberCounter(playerscoreGameObject[i].GetComponent<NumberCounter>());
        }
    }


    int generateRandomValue()
    {
        return (int)Random.Range(1, 21) * 50;
    }

    int generateRandomPlayer()
    {
        return (int)Random.Range(0, 4);
    }

    IEnumerator GrowAndShrink(int i)
    {
        yield return new WaitForSeconds(0.5f);
        canShrink[i] = true;
    }

    void LerpShrink(int i)
    {
        if (playerScore[i].scoreLabel.fontSize >= defaultSize * 1.05f)
        {
            //Sizevairbale
            //playerScore[i].scoreLabel.fontSize *= 0.99f;
            if (playerScore[i].scoreLabel.fontSize < defaultSize)
            {
                playerScore[i].scoreLabel.fontSize = defaultSize;
            }
        }
    }

    void addScore(int value, int player)
    {
        randomOffsetx = Random.Range(-50, 50);
        randomOffsety = Random.Range(-50, 50);
        playerScore[player].addScore(value);


        //SIZE VARIABLE
        //playerScore[player].scoreLabel.fontSize = defaultSize * 1.5f;
        //StartCoroutine(GrowAndShrink(player));

        Vector3 spawn = new Vector3(0, 50, 50);

        GameObject labelType;

        if (value < highscoreMin)
        {
            labelType = scoreSmall;
        }
        else
        {
            labelType = score;
        }

        if (value > ((int) (highscoreMin * 1.5f)))
        {
            //Debug.Log("HIGH SCORE");
            ParticleSystem.EmissionModule emisssionModule;
            emisssionModule = playerParticleSystems[player].emission;
            emisssionModule.rateOverTime = onFireParticles;


            ParticleSystem.VelocityOverLifetimeModule veloModule;
            veloModule = playerParticleSystems[player].velocityOverLifetime;
            veloModule.radial = onFireVelocity;

            StartCoroutine(EndParticleTrigger(player));
        }

        GameObject newlabel = Instantiate(labelType, playerScore[player].scoreLabel.transform.position, Quaternion.Euler(90, 0, 0), canvas.transform);
        
        newlabel.GetComponent<ScoreAnimation>().setValue(value);
    }

    IEnumerator EndParticleTrigger(int i)
    {

        yield return new WaitForSeconds(onFireTime);

        ParticleSystem.EmissionModule emisssionModule;
        emisssionModule = playerParticleSystems[i].emission;
        emisssionModule.rateOverTime = 0;

        ParticleSystem.VelocityOverLifetimeModule veloModule;
        veloModule = playerParticleSystems[i].velocityOverLifetime;
        veloModule.radial = defaultVelocity;
    }

    void checkCrown()
    {
        //If temp no all players have a score of 0
        int tempHighestScore = -4;
        int tempHighestPlayer = -4;

        for (int p = 0; p < playerScore.Length; p++)
        {
            if (playerScore[p].scoreValue > 0)
            {
                if (playerScore[p].scoreValue > tempHighestScore)
                {
                    tempHighestPlayer = p;
                    tempHighestScore = playerScore[p].scoreValue;
                }
            }
        }

        if (tempHighestScore != -4)
        {
            crown.GetComponent<CrownController>().setLeadPlayer(tempHighestPlayer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < canShrink.Length; i++)
        {
            if(canShrink[i] == true)
            {
                LerpShrink(i);
            }
        }

        if (timeSinceLast > timePerScore)
        {
            timeSinceLast = 0f;
            int tempVal = generateRandomValue();
            int tempPlayer = generateRandomPlayer();
            Debug.Log("Adding a score of: " + tempVal + " to player #" + tempPlayer + ".");
            addScore(tempVal, tempPlayer);
            checkCrown();
        }

        timeSinceLast += Time.deltaTime;
    }
}
