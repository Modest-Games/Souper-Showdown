using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EndScreenVotingManager : NetworkBehaviour
{
    [System.Serializable]
    public class Vote
    {
        public Vote (int _voteOption, ulong _networkPlayerId)
        {
            voteOption = _voteOption;
            networkPlayerId = _networkPlayerId;
        }

        public int voteOption;
        public ulong networkPlayerId;
    }

    public List<Vote> votes = new List<Vote>();
    //public NetworkList<Vote> networkVotes = new NetworkList<Vote>();

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

    void Start()
    {
        // setup event listeners
        PlayerTokenBehaviour.PlayerVoted += PlayerVotedServerRpc;
    }

    new private void OnDestroy()
    {
        // clear event listeners
        PlayerTokenBehaviour.PlayerVoted += PlayerVotedServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerVotedServerRpc(int voteOption, ulong senderNetworkId)
    {
        PlayerVotedClientRpc(voteOption, senderNetworkId);
    }

    [ClientRpc]
    private void PlayerVotedClientRpc(int voteOption, ulong senderNetworkId)
    {
        bool found = false;

        // try to find the voter in the list
        for (int i = 0; i < votes.Count; i++)
        {
            if (votes[i].networkPlayerId == senderNetworkId)
            {
                found = true;
                votes[i] = new Vote(voteOption, senderNetworkId);
            }
        }

        // if not found, add a new vote to the list
        if (!found)
            votes.Add(new Vote(voteOption, senderNetworkId));
    }
}
