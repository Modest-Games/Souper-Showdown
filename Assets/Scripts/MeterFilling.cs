using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeterFilling : MonoBehaviour
{
    public Transform[] nodes;
    public GameObject soupMask;
    public ParticleSystem pSystem;
    public GameObject fillLine;
    public GameObject greenThreshold;

    public GameObject strainedFace;
    public GameObject shockedFace;
    public GameObject sleepyFace;
    public GameObject soundManager;

    public float fillLIneOffset = 0.011f;
    public float moveSpeed;
    public float x_offset;

    float Timer;
    float changePercentTimer;
    public float changeEvery;
    static Vector3 currentPos;
    int currentNode;
    float prevPercent;

    public int defaultParticleSpawnRate = 5;

    //Particle Boost variables
    float burstConstant = 90.0f;
    float extraParticles = 0;

    //Liquid wobble variables
    float sineWobbleMax = 10f;
    float currentWobble = 10f;

    //Green Threshold Anim Variables
    float targetAlpha = 0f;
    bool colorAnimActive = false;
    float lerpVal = 0f;

    //Face animation variables
    public GameObject animatorComp;
    float faceTimer = 0f;
    bool checkforFace = false;
    float idleTimer = 0f;
    float idleInterval = 5.0f;

    Transform startNode;
    Transform endNode;


    // Start is called before the first frame update
    void Start()
    {
        changePercentTimer = 0f;
        CheckNode();
        startNode = nodes[0];
        endNode = nodes[nodes.Length-1];
        //Debug.Log(startNode.position);
        disableFace();
    }

    void enableFace(string face, float animDuration)
    {
        //Debug.Log("Face: " + face + " active for " + animDuration + " seconds!");

        faceTimer = Time.time + animDuration;
        checkforFace = true;
        if (face == "shocked")
        {
            shockedFace.gameObject.SetActive(true);
            sleepyFace.gameObject.SetActive(false);
            strainedFace.gameObject.SetActive(false);
        }
        else if (face == "sleepy")
        {
            sleepyFace.gameObject.SetActive(true);
            shockedFace.gameObject.SetActive(false);
            strainedFace.gameObject.SetActive(false);
        }
        else if (face == "strained")
        {
            strainedFace.gameObject.SetActive(true);
            sleepyFace.gameObject.SetActive(false);
            shockedFace.gameObject.SetActive(false);
        }
    }

    void disableFace()
    {
        strainedFace.gameObject.SetActive(false);
        sleepyFace.gameObject.SetActive(false);
        shockedFace.gameObject.SetActive(false);
        checkforFace = false;
    }

    void CheckNode()
    {
        Timer = 0;
        currentPos = nodes[currentNode].transform.position;
    }

    //Percent value between 0 and 1; 0 being nothing, 1 being a full meter
    void setPercent(float percent)
    {
        setColor(percent);

        //Adding to the meter (net positive)
        if (prevPercent < percent)
        {
            //Animate face
            if (percent < 0.5f)
            {
                enableFace("strained", 2f);
            }
            else if (percent < 1.0f)
            {
                enableFace("shocked", 3f);
            }

            //Add animation
            animatorComp.GetComponent<FaceTriggerAnimation>().TriggerFaceAnimation();


            //Add burst of particles
            extraParticles = burstConstant;
            currentWobble = sineWobbleMax;

            //Sound effect
            if (Mathf.Abs(prevPercent - percent) < 0.3f)
            {
                soundManager.GetComponent<SoundBytes>().playSmallClip();
            }
            else
            {
                soundManager.GetComponent<SoundBytes>().playBigClip();
            }
        }

        float destination =  (endNode.position.x - startNode.position.x) * percent + startNode.position.x;
        currentPos.x = destination;

        if (percent < 0.33f)
        {
            setRow(0);
        }
        else if (percent < 0.66f)
        {
            setRow(1);
        }
        else if (percent < 1.00f)
        {
            setRow(2);
        }

        prevPercent = percent;
    }


    void setColor(float percent)
    {
        targetAlpha = 255.0f * percent;
        //lerpVal = 0;
        colorAnimActive = true;
    }


    void animateColor()
    {
        Color tmp = greenThreshold.GetComponent<SpriteRenderer>().color;

        if (Mathf.Abs(tmp.a - targetAlpha) < 0.1)
        {
            tmp.a = targetAlpha;
            colorAnimActive = false;
        }

        lerpVal += 0.5f * Time.deltaTime;
        float alpha = Mathf.Lerp(tmp.a, targetAlpha, lerpVal);
        float finalVal = (alpha / 255.0f);
        tmp.a = finalVal;

        //Debug.Log(finalVal);
        greenThreshold.GetComponent<SpriteRenderer>().color = tmp;

    }


    void setRow(int i)
    {
        var tex = pSystem.textureSheetAnimation;
        tex.rowIndex = i;
    }

    void setParticleBoost(float extra)
    {
        ParticleSystem.EmissionModule emisssionModule;
        emisssionModule = pSystem.emission;
        emisssionModule.rateOverTime = new ParticleSystem.MinMaxCurve(defaultParticleSpawnRate + extra, defaultParticleSpawnRate + 5.0f + extra);

    }


    void sineWobble()
    {
        float sineWave = Mathf.Sin(Time.time * 13.0f);
        float finalNumber = sineWave * currentWobble;
        //Debug.Log(currentWobble);
        currentWobble *= 0.9985f;
        transform.rotation = Quaternion.Euler(-90f, 0f, 7.786f + finalNumber);
    } 


    // Update is called once per frame0.4748172
    void Update()
    {
        sineWobble();

        if (colorAnimActive)
        {
            animateColor();
        }

        if (checkforFace)
        {
            if (Time.time > faceTimer)
            {
                disableFace();
            }
        }

        if(Time.time > idleTimer)
        {
            int coin = (int)Random.Range(0, 2);
            if (coin == 0)
            {
                enableFace("sleepy", Random.Range(1.0f, 4.0f));
            }
            idleTimer = Time.time + idleInterval + Random.Range(0, idleInterval);
        }

        Timer += Time.deltaTime * moveSpeed;
        changePercentTimer += Time.deltaTime;

        if (changePercentTimer > changeEvery)
        {
            changePercentTimer = 0f;
            float randomInt = (float)Random.Range(0.0f, 1.0f);
            //Debug.Log("Meter set to %" + randomInt);
            setPercent(randomInt);
        }

        if (extraParticles > 0) {
            extraParticles *= 0.998f;
            setParticleBoost(extraParticles);
            //Debug.Log(extraParticles + " particles!");
            if (extraParticles < 0.5f)
            {
                extraParticles = 0;
            }
        }

        transform.position = Vector3.Lerp(transform.position, currentPos, Timer);
        soupMask.transform.position = new Vector3((transform.position.x - x_offset), soupMask.transform.position.y, soupMask.transform.position.z);
        pSystem.transform.position = new Vector3((transform.position.x), soupMask.transform.position.y, soupMask.transform.position.z);
    }
}
