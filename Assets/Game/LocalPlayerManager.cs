using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerManager : MonoBehaviour
{
    public static LocalPlayerManager Instance { get; private set; }

    public List<PlayerInput> inputPlayers = new List<PlayerInput>();

    public ulong thisClientId;

    private PlayerInputManager inputManager;

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

        inputManager = GetComponent<PlayerInputManager>();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
            inputManager.EnableJoining();
        else
            inputManager.DisableJoining();
    }
}
