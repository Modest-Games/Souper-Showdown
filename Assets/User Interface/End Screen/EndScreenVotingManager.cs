using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EndScreenVotingManager : NetworkBehaviour
{
    public int[] votes;
    
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

        foreach (PlayersManager.Player player in PlayersManager.Instance.players)
        {
            int playerVoteOption = PlayersManager.Instance.GetPlayerVote(player.networkObjId) - 1;

            if (playerVoteOption >= 0)
            {
                Debug.LogFormat("Player {0} voted for option {1}", player.networkObjId, playerVoteOption);
                votes[playerVoteOption]++;
            }
        }
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
