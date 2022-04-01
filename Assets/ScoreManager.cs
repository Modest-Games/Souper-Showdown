using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;


public class ScoreManager : MonoBehaviour
{
    public GameObject cardRef;

    // Start is called before the first frame update
    void Start()
    {
        //for (int i = 0; i < totalSpoilerCount; i++)
        //{
        //    string name = "PLAYER " + (i + 1);
        //    AddPlayerCard(name, frames[i], particleMaterials[i], i);
        //    arrangeCards();
        //}

        // clear existing cards
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // add cards for each player in the player list
        for (int i = 0; i < PlayersManager.Instance.players.Count; i++)
        {
            // create and setup a new card for the player
            CardRenderer newCard = Instantiate(cardRef, transform).GetComponent<CardRenderer>();
            newCard.Setup(PlayersManager.Instance.players[i], i);
        }
    }

    //Adds new player card to deck
    //void AddPlayerCard(string playerName, Sprite sprite, Material material, int index)
    //{
    //    GameObject card = Instantiate(cardRef, new Vector3(0, 0, 0), Quaternion.identity);

    //    //Create new card
    //    CardRenderer newCard = new CardRenderer(cardRef, playerName);
    //    //Set card Image, particle, and name
    //    newCard.Card.transform.Find("Backdrop").GetComponent<Image>().sprite = sprite;
    //    newCard.Card.transform.Find("Border").GetComponent<Image>().sprite = sprite;
    //    newCard.Card.transform.Find("UIParticle").Find("ParticleSystem").GetComponent<ParticleSystemRenderer>().material = material;
    //    newCard.Card.transform.Find("PlayerName").GetComponent<TMPro.TextMeshProUGUI>().text = playerName;

    //    playerCards.Add(newCard);
    //    playerCards[index].Card = card;

    //    card.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
    //}
}
