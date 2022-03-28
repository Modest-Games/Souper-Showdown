using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public GameObject cardRef;
    public Sprite[] frames;
    public Material[] particleMaterials;

    public int totalSpoilerCount;
    //public List<PlayerCard> playerCards = new List<PlayerCard>();
    public List<CardRenderer> playerCards = new List<CardRenderer>();

    public Transform leftPoint;
    public Transform rightPoint;

    //Arrange cards uniformly between "Spawn1" and "Spawn2" in canvas
    void arrangeCards()
    {
        if (playerCards.Count == 1)
        {
            // Debug.Log("Arranging one card!");
            float centerPos = leftPoint.position.x + ((rightPoint.position.x - leftPoint.position.x) / 2.0f);
            Vector3 position = new Vector3(centerPos, leftPoint.position.y, leftPoint.position.z);
            playerCards[0].setPosition(position);
        }
        else if (playerCards.Count == 2)
        {
            //Debug.Log("Arranging two cards!");
            playerCards[0].setPosition(leftPoint.position);
            playerCards[1].setPosition(rightPoint.position);
        }
        else
        {
            //Debug.Log("Arranging two+ cards!");
            float spacing = (rightPoint.position.x - leftPoint.position.x) / totalSpoilerCount;
            for (int i = 0; i < playerCards.Count; i++)
            {
                Vector3 newPos = new Vector3(leftPoint.position.x + (i * spacing) + (spacing / 2.0f), leftPoint.position.y, leftPoint.position.z);
                playerCards[i].setPosition(newPos);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < totalSpoilerCount; i++)
        {
            string name = "PLAYER " + (i + 1);
            AddPlayerCard(name, frames[i], particleMaterials[i], i);
            arrangeCards();
        }
    }

    //Adds new player card to deck
    void AddPlayerCard(string n, Sprite s, Material m, int index)
    {
        GameObject card = Instantiate(cardRef, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

        //Create new card
        CardRenderer newCard = new CardRenderer(cardRef, n);
        //Set card Image, particle, and name
        newCard.Card.transform.Find("Backdrop").GetComponent<Image>().sprite = s;
        newCard.Card.transform.Find("Border").GetComponent<Image>().sprite = s;
        newCard.Card.transform.Find("UIParticle").Find("ParticleSystem").GetComponent<ParticleSystemRenderer>().material = m;
        newCard.Card.transform.Find("PlayerName").GetComponent<TMPro.TextMeshProUGUI>().text = n;


        
        playerCards.Add(newCard);
        playerCards[index].Card = card;

        card.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
