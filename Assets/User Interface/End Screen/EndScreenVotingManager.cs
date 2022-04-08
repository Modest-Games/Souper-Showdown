using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class EndScreenVotingManager : NetworkBehaviour
{
    public int[] votes;

    public Transform option1;
    public Transform option2;
    public Transform option3;

    public static EndScreenVotingManager Instance { get; private set; }

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

    private void OnPlayerVoted()
    {
        votes = new int[3];

        for (int i = 0; i < PlayersManager.Instance.players.Count; i++)
        {
            PlayersManager.Player player = PlayersManager.Instance.players[i];
            int playerVoteOption = PlayersManager.Instance.GetPlayerVote(player.networkObjId) - 1;

            if (playerVoteOption >= 0)
            {
                Debug.LogFormat("Player {0} voted for option {1}", i + 1, playerVoteOption);
                votes[playerVoteOption]++;
            }
        }

        UpdateVoteIndicators();
    }

    private void UpdateVoteIndicators()
    {
        option1.GetComponent<TMP_Text>().text = votes[0].ToString();
        option2.GetComponent<TMP_Text>().text = votes[1].ToString();
        option3.GetComponent<TMP_Text>().text = votes[2].ToString();
    }

    void Start()
    {
        // setup event listeners
        PlayerVoter.PlayerVoted += OnPlayerVoted;
    }

    new private void OnDestroy()
    {
        // clear event listeners
        PlayerVoter.PlayerVoted -= OnPlayerVoted;
    }
}
