using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardRenderer : MonoBehaviour
{
    private NumberCounter numberCounter;
    private int score = 0;
    private ulong playerNetworkId;

    public void Setup(PlayersManager.Player player, int playerIndex)
    {
        // setup variables
        numberCounter = transform.Find("PlayerScore").GetComponent<NumberCounter>();

        // get the required data to render the card
        Character character = CharacterManager.Instance.GetCharacter(player.character);
        Sprite sprite = character.characterCard;
        Material material = character.UIParticleMaterial;
        string playerName = (playerIndex + 1).ToString();

        transform.Find("Backdrop").GetComponent<Image>().sprite = sprite;
        transform.Find("Border").GetComponent<Image>().sprite = sprite;
        transform.Find("UIParticle").Find("ParticleSystem").GetComponent<ParticleSystemRenderer>().material = material;
        transform.Find("PlayerName").GetComponent<TMPro.TextMeshProUGUI>().text = "Player " + playerName;

        // set the playernetworkId for reference
        playerNetworkId = player.networkObjId;
    }

    //public void setPosition(Vector3 p)
    //{
    //    //Debug.Log("position should be: " + p);
    //    Card.transform.position = p;
    //}

    //public void setNumberCounter(NumberCounter nc)
    //{
    //    numberCounter = nc;
    //}

    public void AddScore(int val)
    {
        score += val;
        //scoreLabel.text = scoreValue.ToString();
        numberCounter.Value = score;
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        numberCounter.Value = score;
    }

    private void OnPlayerScoreChanged(ulong _playerNetworkId, int newScore)
    {
        // check if the this card's player threw the object
        if (_playerNetworkId == playerNetworkId)
        {
            Debug.LogFormat("Player {0} score updating to: {1}", _playerNetworkId, newScore);
            SetScore(newScore);
        }
    }

    private void OnEnable()
    {
        // setup event listeners
        PlayerController.PlayerScoreChanged += OnPlayerScoreChanged;
    }

    private void OnDisable()
    {
        // clear even listeners
        PlayerController.PlayerScoreChanged -= OnPlayerScoreChanged;
    }
}
