using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRenderer : MonoBehaviour
{
    public GameObject Card;
    public NumberCounter NumberCounter;
    public int score;
    public string spoilerType;


    public CardRenderer(GameObject c, string n)
    {
        Card = c;
        score = 0;
        spoilerType = n;
    }

    public void setPosition(Vector3 p)
    {
        //Debug.Log("position should be: " + p);
        Card.transform.position = p;
    }


    public void setNumberCounter(NumberCounter nc)
    {
        NumberCounter = nc;
    }

    public void addScore(int val)
    {
        score += val;
        //scoreLabel.text = scoreValue.ToString();
        NumberCounter.Value = score;
    }

}
