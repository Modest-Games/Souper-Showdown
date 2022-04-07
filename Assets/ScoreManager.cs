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

    private void OnEnable()
    {
        GameController.GameStopped += OnGameOver;
    }

    private void OnGameOver()
    {
        gameObject.SetActive(false);

    }

    private void OnDisable()
    {
        GameController.GameStopped -= OnGameOver;
    }
}
