using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class TrapManager : NetworkBehaviour
{
    public static TrapManager Instance { get; private set; }
    
    public List<TrapBox> traps;

    // network variables
    public NetworkVariable<int> networkNumTrapsPlaced = new NetworkVariable<int>();
    public NetworkVariable<int> numTrapsAllowed = new NetworkVariable<int>();

    public int NumTrapsRemaining
    {
        get
        {
            return numTrapsAllowed.Value - networkNumTrapsPlaced.Value;
        }
    }

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

    public TrapBox GetTrap(string trapName, bool doLog = false)
    {
        TrapBox foundTrapBox = traps.Find(x => x.name == trapName);

        if (doLog)
            Debug.LogFormat("Name: {0}, Found TrapBox: {1}", trapName, foundTrapBox);

        return foundTrapBox;
    }

    public TrapBox GetTrap(int index)
    {
        return traps[index];
    }

    public TrapBox GetNextTrap(string trapName)
    {
        // get the old trap's index
        int oldTrapIndex = traps.IndexOf(traps.Find(x => x.name == trapName));

        // get the next trap's index value
        int newIndex = oldTrapIndex + 1;

        // clamp new index
        if (newIndex >= traps.Count)
            newIndex = 0;

        return GetTrap(newIndex);
    }

    public TrapBox GetPreviousTrap(string trapName)
    {
        // get the old trap's index
        int oldTrapIndex = traps.IndexOf(traps.Find(x => x.name == trapName));

        // get the previous trap's index value
        int newIndex = oldTrapIndex - 1;

        // clamp new index
        if (newIndex < 0)
            newIndex = traps.Count - 1;

        return GetTrap(newIndex);
    }

    private void OnNumChefsChanged()
    {
        if (IsServer)
            numTrapsAllowed.Value = PlayersManager.Instance.NumberOfChefs;
    }

    private void OnEnable()
    {
        // setup event listeners
        PlayersManager.PlayerListChanged += OnNumChefsChanged;
        PlayerController.IsChefChanged += OnNumChefsChanged;
    }

    private void OnDisable()
    {
        // clear event listeners
        PlayersManager.PlayerListChanged -= OnNumChefsChanged;
        PlayerController.IsChefChanged -= OnNumChefsChanged;
    }
}
