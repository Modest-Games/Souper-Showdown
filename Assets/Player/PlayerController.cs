using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;
using Cinemachine;
using NetcodeString = Unity.Collections.FixedString64Bytes;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    public delegate void PlayerDelegate();
    public static event PlayerDelegate PlayerCreated;

    public delegate void PlayerScoreDelegate(ulong playerNetworkId, int newScore);
    public static event PlayerScoreDelegate PlayerScoreChanged;

    public delegate void CharacterChangedDelegate(string oldCharName, string newCharName);
    public static event CharacterChangedDelegate CharacterChanged;

    public enum ArmState
    {
        Stiff,
        Loose
    }

    public enum PlayerState
    {
        Idle,
        Moving,
        Dashing,
        Dazed,
        Ungrounded
    }

    public enum PlayerCarryState
    {
        Empty,
        CarryingObject,
        CarryingPlayer,
        PlacingTrap
    }

    [Header("Config")]
    public Vector3 holdPosition;
    public float throwForce;
    public float moveSpeed;
    public float rotateSpeed;
    public float dashDuration;
    public float dashForce;
    public float dashCooldown;
    public float dazeDuration;

    [Header("Character")]
    public Character characterObject;
    private GameObject legs;

    [Header("State (ReadOnly)")]
    [SerializeField] public PlayerState playerState;
    [SerializeField] public PlayerCarryState lastKnownState;
    [SerializeField] public bool isAlive;

    [Header("Variables (ReadOnly)")]
    [SerializeField] public List<GameObject> reachableCollectables;
    [SerializeField] private Vector2 movement;
    [SerializeField] private Vector3 lookVector;
    [SerializeField] private float timeOfLastDash;

    [SerializeField] private GameObject pollutantPrefab;

    private LineRenderer aimIndicator;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Transform debugCanvasObj;
    public bool canMove;
    private GameObject dazeIndicator;
    private CharacterBehaviour characterBehaviour;
    private VisualEffect vfx;
    private UnplacedTrap trapPlacer;

    private bool justThrew;
    private float timeDazed;
    private bool controlsBound;
    private bool justSwitchedCharacters;
    private bool justSwitchedTrapPlacementMode;
    private bool justRotatedTrap;
    private bool justPlacedTrap;

    private GameObject heldObject;
    private Pollutant currentlyHeld;

    private bool startHasRan = false;

    public NetworkVariable<int> playerIndex = new NetworkVariable<int>();

    public NetworkVariable<NetcodeString> networkCharacterName = new NetworkVariable<NetcodeString>();
    public NetworkVariable<bool> networkIsChef = new NetworkVariable<bool>();
    public NetworkVariable<int> networkTrapRotation = new NetworkVariable<int>();
    public NetworkVariable<NetcodeString> networkSelectedTrap = new NetworkVariable<NetcodeString>();
    public NetworkVariable<PlayerCarryState> networkCarryState = new NetworkVariable<PlayerCarryState>();
    public NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();
    public NetworkVariable<int> networkScore = new NetworkVariable<int>();

    public int numberVeggies;
    public int numberChefs;
    private IEnumerator lobbyCountdown;

    private void Awake()
    {
        isAlive = true;
        lookVector = transform.forward;
        timeOfLastDash = 0;
        justThrew = false;
        controlsBound = false;
        justSwitchedCharacters = false;
        justRotatedTrap = false;
        justPlacedTrap = false;
    }

    private void Start()
    {
        if (!startHasRan)
        {
            bool isChef = (IsClient && IsOwner) ? false : networkIsChef.Value;

            characterObject = (IsClient && IsOwner) ?
                CharacterManager.Instance.GetRandomCharacter() :
                CharacterManager.Instance.GetCharacter(networkCharacterName.Value.ToString());

            rb = GetComponent<Rigidbody>();
            canMove = true;

            dazeIndicator = transform.Find("DazeIndicatorHolder").gameObject;
            heldObject = transform.Find("Held Object").gameObject;
            aimIndicator = transform.Find("ThrowIndicator").GetComponent<LineRenderer>();
            characterBehaviour = transform.Find("Character").GetComponent<CharacterBehaviour>();
            debugCanvasObj = transform.GetComponentInChildren<PlayerDebugUI>().transform;
            trapPlacer = transform.Find("Trap Placer").GetComponentInChildren<UnplacedTrap>();
            vfx = transform.Find("VFX").GetComponent<VisualEffect>();

            lastKnownState = networkCarryState.Value;

            // setup variables
            if (IsClient && IsOwner)
            {
                UpdateCharacterNameServerRpc(characterObject.characterName);
                UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);
                UpdatePlayerStateServerRpc(PlayerState.Idle);
            }

            if (IsClient && !IsOwner)
                RefreshCharacter();

            if (PlayerCreated != null)
                PlayerCreated();

            debugCanvasObj.gameObject.SetActive(LobbyController.Instance.isDebugEnabled);

            // add the player to the list of players in the players manager
            PlayersManager.Instance.AddPlayerToList(OwnerClientId, playerIndex.Value, NetworkObjectId, networkCharacterName.Value.ToString());

            startHasRan = true;
        }
        
    }

    public void BindControls()
    {
        if (IsClient && IsOwner)
        {
            playerInput = LocalPlayerManager.Instance.inputPlayers.Find(
                p => p.playerIndex == playerIndex.Value);

            // Map control inputs:
            playerInput.actions["Dash"].performed += ctx => DashPerformed();
            playerInput.actions["Move"].performed += ctx => MovePerformed(ctx.ReadValue<Vector2>());
            playerInput.actions["Move"].canceled += ctx => MoveCancelled();
            playerInput.actions["Grab"].started += ctx => GrabStarted();
            playerInput.actions["Grab"].canceled += ctx => GrabCancelled();
            playerInput.actions["Throw"].canceled += ctx => ThrowPerformed();
            playerInput.actions["Throw"].started += ctx => ThrowStarted();
            playerInput.actions["Next Character"].performed += ctx => NextCharacterPerformed();
            playerInput.actions["Previous Character"].performed += ctx => PreviousCharacterPerformed();

            // Map chef control inputs:
            playerInput.actions["Toggle Trap Mode"].performed += ctx => ToggleTrapModePerformed();
            playerInput.actions["Next Trap"].performed += ctx => NextTrapPerformed();
            playerInput.actions["Previous Trap"].performed += ctx => PreviousTrapPerformed();
            playerInput.actions["Rotate Trap"].performed += ctx => RotateTrapPerformed();
            playerInput.actions["Place Trap"].performed += ctx => PlaceTrapPerformed();

            controlsBound = true;
        }
    }

    public void TeleportPlayer(Vector3 pos)
    {
        if (IsClient && IsOwner)
        {
            var adjustedPos = new Vector3(pos.x, 0, pos.z);
            GetComponent<ClientNetworkTransform>().Teleport(adjustedPos, Quaternion.identity, Vector3.one);

            playerState = PlayerState.Idle;
            UpdatePlayerStateServerRpc(PlayerState.Idle);
        }

        vfx.Play();
    }

    public void PlayerRandomSpawnPoint(bool isChef)
    {
        TeleportPlayer(TerrainManager.Instance.GetRandomSpawnLocation(isChef));
    }

    void Update()
    {
        UpdateClientVisuals();

        if (!controlsBound)
        {
            BindControls();
        }
    }

    private void FixedUpdate()
    {
        if (IsClient && IsOwner)
        {
            if (isAlive && canMove)
            {
                PlayerMovement();
            }
        }
    }

    private void UpdateClientVisuals()
    {
        // Get the state values depending on if this is a networked client or a local player:
        PlayerCarryState carryStateVal = networkCarryState.Value;
        PlayerState playerStateVal = (IsClient && IsOwner) ? playerState : networkPlayerState.Value;

        UpdateTrapPlacerVisuals();

        if (lastKnownState != carryStateVal)
        {
            characterBehaviour.UpdateArms(carryStateVal);

            lastKnownState = carryStateVal;
        }

        switch (carryStateVal)
        {
            case PlayerCarryState.Empty:
                heldObject.SetActive(false);

                break;

            case PlayerCarryState.CarryingObject:
                heldObject.SetActive(true);
                heldObject.transform.localRotation = Quaternion.Euler(0, 0, 90);

                break;

            case PlayerCarryState.CarryingPlayer:
                // Carrying Player

                break;
        }

        switch (playerStateVal)
        {
            case PlayerState.Dazed:
                dazeIndicator.SetActive(true);
                break;
            default:
                dazeIndicator.SetActive(false);
                break;
        }

        characterBehaviour.UpdateLegs(playerStateVal);
    }

    private void PlayerMovement()
    {
        float currentTime = Time.time;
        float deltaTime = Time.fixedDeltaTime;

        switch (playerState)
        {
            case PlayerState.Idle:
                rb.angularVelocity = Vector3.zero;

                legs = characterObject.characterPrefab.transform.GetChild(3).gameObject;
                legs.SetActive(false);
                break;

            case PlayerState.Moving:
                Vector3 movementVec = new Vector3(movement.x, 0, movement.y) * deltaTime * moveSpeed;

                rb.MovePosition(rb.position + movementVec);

                lookVector = movementVec.normalized;
                lookVector.y = 0f;

                transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + lookVector, rotateSpeed * deltaTime));

                Debug.DrawRay(transform.position, movementVec.normalized * 2, Color.blue);
                Debug.DrawRay(transform.position, transform.forward * 2, Color.green);

                break;

            case PlayerState.Dashing:
                if ((currentTime - timeOfLastDash) >= dashDuration)
                {
                    PlayerState newPlayerState = (movement.magnitude == 0) ? PlayerState.Idle : PlayerState.Moving;
                    UpdatePlayerStateServerRpc(newPlayerState);
                    playerState = newPlayerState;
                }

                else
                {
                    Vector3 dashVector = rb.position + (lookVector * dashForce * deltaTime);

                    rb.MovePosition(dashVector);
                    transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + lookVector, rotateSpeed * deltaTime));

                    Debug.DrawLine(rb.position, rb.position + (lookVector * 4), Color.red);
                }

                break;

            case PlayerState.Dazed:
                // Clear rotatitonal velocity:
                rb.angularVelocity = Vector3.zero;

                if (timeDazed >= dazeDuration)
                {
                    playerState = PlayerState.Idle;
                    UpdatePlayerStateServerRpc(PlayerState.Idle);
                }

                else
                {
                    timeDazed += deltaTime;
                }

                break;

            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner && IsClient)
        {
            switch (other.tag)
            {
                case "Pollutant":
                    if (!reachableCollectables.Contains(other.gameObject) && !justThrew)
                    {
                        reachableCollectables.Add(other.gameObject);
                    }

                    break;

                case "Player":

                    if (!reachableCollectables.Contains(other.gameObject) && !justThrew)
                    {
                        if (networkIsChef.Value)
                        {
                            reachableCollectables.Add(other.gameObject);
                        }
                    }

                    break;
            }
        }

        if (IsServer)
        {
            switch (other.tag)
            {
                case "ChefZone":
                    if (networkIsChef.Value == false)
                    {
                        networkIsChef.Value = true;
                    }

                    // if everyone's on their tiles, start game
                    numberChefs++;

                    if (numberVeggies == (PlayersManager.Instance.players.Count - 1) && numberChefs == 1) 
                        lobbyCountdown = LobbyController.Instance.startCountdown();
                        StartCoroutine(lobbyCountdown);

                    break;

                case "VeggieZone":
                    
                    numberVeggies++;

                    if (numberVeggies == (PlayersManager.Instance.players.Count - 1) && numberChefs == 1) 
                    {
                        Debug.Log("Vegetable is on veggie zone");
                        //startCountdown();
                    }

                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsOwner && IsClient && !networkIsChef.Value)
        {
            switch (other.gameObject.tag)
            {
                case "Player":
                    PlayerController otherPC = other.GetComponentInParent<PlayerController>();

                    // Get the state values depending on if this is a networked client or a local player:
                    PlayerState otherPlayerState = (otherPC.IsClient && otherPC.IsOwner)
                        ? otherPC.playerState : otherPC.networkPlayerState.Value;

                    if (otherPC.networkIsChef.Value && otherPlayerState == PlayerState.Dashing)
                    {
                        OnBoop();
                    }

                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsOwner && IsClient)
        {
            switch (other.tag)
            {
                case "Pollutant":
                    reachableCollectables.Remove(other.gameObject);
                    break;

                case "Player":
                    reachableCollectables.Remove(other.gameObject);
                    break;
            }
        }

        if (IsServer)
        {
            switch (other.tag)
            {
                case "ChefZone":
                    if (networkIsChef.Value == true)
                    {
                        networkIsChef.Value = false;
                    }

                    numberChefs--;
                    if (numberVeggies == (PlayersManager.Instance.players.Count - 1) && numberChefs == 1) 
                        lobbyCountdown = LobbyController.Instance.startCountdown();
                        StopCoroutine(lobbyCountdown);
                        GameObject.Find("Countdown").SetActive(false);

                    break;
                
                case "VeggieZone":
                    
                    numberVeggies--;

                    break;
            }
        }
    }

    private void RefreshCharacter()
    {

        // Check if there is a character mesh ready:
        GameObject newCharacterMesh = characterObject == null ?
            CharacterManager.Instance.GetCharacter(0).characterPrefab : characterObject.characterPrefab;

        Destroy(transform.Find("Character").gameObject);

        GameObject newMesh = Instantiate(newCharacterMesh, transform);

        newMesh.name = "Character";
        characterBehaviour = newMesh.GetComponent<CharacterBehaviour>();

        // update the player in the list of players in the players manager
        PlayersManager.Instance.UpdatePlayerInList(OwnerClientId, playerIndex.Value, NetworkObjectId, networkCharacterName.Value.ToString());
    }

    private void RefreshChefHat()
    {
        transform.Find("ChefHat").gameObject.SetActive(networkIsChef.Value);
    }

    private void UpdateTrapPlacerVisuals()
    {
        if (!networkIsChef.Value)
        {
            trapPlacer.gameObject.SetActive(false);
            return;
        }

        switch (networkCarryState.Value)
        {
            case PlayerCarryState.PlacingTrap:
                trapPlacer.gameObject.SetActive(true);

                break;

            default:
                trapPlacer.gameObject.SetActive(false);

                break;
        }
    }

    private void OnBoop()
    {
        bool canGetBooped = (playerState != PlayerState.Dazed) && (playerState != PlayerState.Ungrounded);

        if (canGetBooped)
        {
            timeDazed = 0f;

            GrabCancelled();

            playerState = PlayerState.Dazed;
            UpdatePlayerStateServerRpc(PlayerState.Dazed);
        }
    }

    private void RefreshReachableCollectables()
    {
        for (int i = 0; i < reachableCollectables.Count; i++)
        {
            if (reachableCollectables[i] == null)
                reachableCollectables.RemoveAt(i);

            if (reachableCollectables[i].CompareTag("Player"))
            {
                var playerState = reachableCollectables[i].GetComponentInParent<PlayerController>().networkPlayerState;

                if (playerState.Value != PlayerState.Dazed)
                    reachableCollectables.RemoveAt(i);
            } 
        }
    }

    public void MovePerformed(Vector2 newMovement)
    {
        if (Application.isFocused && IsClient && IsOwner)
        {
            movement = newMovement;

            if (playerState == PlayerState.Idle || playerState == PlayerState.Moving)
            {
                UpdatePlayerStateServerRpc(PlayerState.Moving);
                playerState = PlayerState.Moving;
            }
        }
    }

    public void MoveCancelled()
    {
        if (Application.isFocused && IsClient && IsOwner)
        {
            movement = Vector2.zero;

            if (playerState != PlayerState.Dashing)
            {
                UpdatePlayerStateServerRpc(PlayerState.Idle);
                playerState = PlayerState.Idle;
            }
        }
    }

    public void DashPerformed()
    {
        if (Application.isFocused && IsClient && IsOwner)
        {
            // Calculate the time since the last dash, and if the player can dash:
            float timeSinceDashCompleted = (Time.time - timeOfLastDash) - dashDuration;
            bool canDash = (playerState == PlayerState.Idle || playerState == PlayerState.Moving)
                && timeSinceDashCompleted >= dashCooldown && networkCarryState.Value == PlayerCarryState.Empty;

            if (canDash)
            {
                timeOfLastDash = Time.time;

                UpdatePlayerStateServerRpc(PlayerState.Dashing);
                playerState = PlayerState.Dashing;
            }
        }
    }

    public void ToggleTrapModePerformed()
    {
        if (IsClient && IsOwner && Application.isFocused)
        {
            if (SceneManager.GetActiveScene().name != "InGame")
                return;

            if (GameController.Instance.gameState.Value != GameController.GameState.Running)
                return;

            if (!networkIsChef.Value)
                return;

            if (justSwitchedTrapPlacementMode)
                return;

            switch (networkCarryState.Value)
            {
                case PlayerCarryState.Empty:
                    UpdatePlayerCarryStateServerRpc(PlayerCarryState.PlacingTrap);
                    StartCoroutine(TempDisableTrapPlacementToggle());
                    break;

                case PlayerCarryState.PlacingTrap:
                    UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);
                    StartCoroutine(TempDisableTrapPlacementToggle());
                    break;
            }
        }
    }

    public void NextTrapPerformed()
    {
        if (IsClient && IsOwner && networkIsChef.Value && Application.isFocused)
        {

        }
    }

    public void PreviousTrapPerformed()
    {
        if (IsClient && IsOwner && networkIsChef.Value && Application.isFocused)
        {

        }
    }

    public void RotateTrapPerformed()
    {
        if (IsClient && IsOwner && networkIsChef.Value && Application.isFocused)
        {
            if (SceneManager.GetActiveScene().name != "InGame")
                return;

            if (GameController.Instance.gameState.Value != GameController.GameState.Running)
                return;

            if (networkCarryState.Value != PlayerCarryState.PlacingTrap)
                return;

            if (justRotatedTrap)
                return;

            trapPlacer.RotateTrap();
            StartCoroutine(TempDisableTrapRotation());
        }
    }

    public void PlaceTrapPerformed()
    {
        if (IsClient && IsOwner && networkIsChef.Value && Application.isFocused)
        {
            if (SceneManager.GetActiveScene().name != "InGame")
                return;

            if (GameController.Instance.gameState.Value != GameController.GameState.Running)
                return;

            if (networkCarryState.Value != PlayerCarryState.PlacingTrap)
                return;

            if (justPlacedTrap)
                return;

            if (trapPlacer.SpawnTrap())
                StartCoroutine(TempDisableTrapPlacement());
        }
    }

    public void GrabStarted()
    {
        if (IsClient && IsOwner && Application.isFocused)
        {
            RefreshReachableCollectables();

            bool canPickup = (networkCarryState.Value == PlayerCarryState.Empty) && (reachableCollectables.Count > 0);

            if (canPickup)
            {
                for (int i = 0; i < reachableCollectables.Count; i++)
                {
                    if (reachableCollectables[i].CompareTag("Player") && networkIsChef.Value)
                    {
                        var otherPlayer = reachableCollectables[i];

                        PlayerController otherPC = otherPlayer.GetComponentInParent<PlayerController>();
                        if (!(otherPC.IsClient && otherPC.IsOwner))
                        {
                            var playerID = otherPlayer.GetComponentInParent<NetworkObject>().NetworkObjectId;
                            HideGrabbedPlayerServerRpc(playerID);
                            OnPlayerGrabServerRpc(otherPC.networkCharacterName.Value, (int) playerID);
                        }

                        return;
                    }
                }

                reachableCollectables = reachableCollectables.OrderBy(
                    r => Vector3.Distance(transform.position, r.transform.position)).ToList();

                GameObject nearestReachableCollectable = reachableCollectables[0];

                reachableCollectables.Remove(nearestReachableCollectable);
                var netObj = nearestReachableCollectable.GetComponent<NetworkObject>();

                OnGrabServerRpc(netObj.NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnPlayerGrabServerRpc(NetcodeString characterName, int playerID)
    {
        string pollutantType = characterName.ToString();
        pollutantType += "Live";
        currentlyHeld = GetPollutantObject(pollutantType);
        currentlyHeld.playerID = playerID;

        var heldObjectBehaviour = heldObject.GetComponent<HeldObject>();
        heldObjectBehaviour.heldObject = currentlyHeld.mesh;
        heldObjectBehaviour.meshInitialized = false;

        // Repeat logic on client-side:
        SetHeldObjectClientRpc(pollutantType);

        networkCarryState.Value = PlayerCarryState.CarryingObject;
    }

    [ServerRpc(RequireOwnership = false)]
    private void HideGrabbedPlayerServerRpc(ulong playerID)
    {
        HideGrabbedPlayerClientRpc(playerID);
    }

    [ClientRpc]
    private void HideGrabbedPlayerClientRpc(ulong playerID)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerID, out var playerToHide);
        if (playerToHide == null) return;

        CinemachineTargetGroup camTargetGroup = GameObject.Find("CineMachine Target Group").GetComponent<CinemachineTargetGroup>();
        camTargetGroup.RemoveMember(playerToHide.transform);

        var playerController = playerToHide.GetComponentInParent<PlayerController>();
        playerController.playerState = PlayerState.Idle;
        playerController.UpdatePlayerStateServerRpc(PlayerState.Idle);

        playerToHide.GetComponentInParent<Rigidbody>().isKinematic = true;
        playerToHide.GetComponentInParent<SphereCollider>().enabled = false;
        playerToHide.transform.Find("Character").gameObject.SetActive(false);
    }

    public void GrabCancelled()
    {
        if (IsClient && IsOwner)
        {
            bool canDrop = (networkCarryState.Value == PlayerCarryState.CarryingObject) || (networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            if (canDrop)
            {
                aimIndicator.gameObject.SetActive(false);

                StartCoroutine(TempDisablePickup());

                Vector3 playerVelocity = 2f * new Vector3(movement.x, 0, movement.y);
                OnDropServerRpc(playerVelocity);
            }
        }
    }

    public void ThrowStarted()
    {
        if (IsClient && IsOwner)
        {
            bool canThrow =
            (networkPlayerState.Value == PlayerState.Idle || networkPlayerState.Value == PlayerState.Moving) &&
            (networkCarryState.Value == PlayerCarryState.CarryingObject || networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            if (canThrow)
            {
                aimIndicator.gameObject.SetActive(true);
            }
        }
    }

    public void ThrowPerformed()
    {
        if (IsClient && IsOwner)
        {
            bool canThrow =
            (networkPlayerState.Value == PlayerState.Idle || networkPlayerState.Value == PlayerState.Moving) &&
            (networkCarryState.Value == PlayerCarryState.CarryingObject || networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            if (canThrow)
            {
                aimIndicator.gameObject.SetActive(false);

                StartCoroutine(TempDisablePickup());
                StartCoroutine(TempDisableMovement());
                OnThrowServerRpc(transform.forward);
            }
        }
    }

    public void NextCharacterPerformed()
    {
        if (IsClient && IsOwner && !justSwitchedCharacters && SceneManager.GetActiveScene().name == "Lobby")
        {
            // dont switch when the window isn't focused
            if (!Application.isFocused)
                return;

            UpdateCharacterNameServerRpc(CharacterManager.Instance.GetNextCharacter(characterObject.characterName).characterName);
            StartCoroutine(TempDisableCharacterSwitch());
        }
    }

    public void PreviousCharacterPerformed()
    {
        if (IsClient && IsOwner && !justSwitchedCharacters && SceneManager.GetActiveScene().name == "Lobby")
        {
            // dont switch when the window isn't focused
            if (!Application.isFocused)
                return;

            UpdateCharacterNameServerRpc(CharacterManager.Instance.GetPreviousCharacter(characterObject.characterName).characterName);
            StartCoroutine(TempDisableCharacterSwitch());
        }
    }

    private void OnGameCreated()
    {
        if (IsClient && IsOwner)
        {
            canMove = false;
            PlayerRandomSpawnPoint(networkIsChef.Value);
        }
    }

    private void OnGameStarted()
    {
        canMove = true;
    }

    private void OnGamePaused()
    {
        canMove = false;
    }

    private void OnGameResumed()
    {
        canMove = true;
    }

    private void OnGameStopped()
    {
        if (IsClient && IsOwner)
        {
            rb.velocity = Vector3.zero;
            canMove = false;
        }
    }

    private void OnDebugEnabled()
    {
        debugCanvasObj.gameObject.SetActive(true);
    }

    private void OnDebugDisabled()
    {
        debugCanvasObj.gameObject.SetActive(false);
    }

    private void OnIsChefChanged(bool oldVal, bool newVal)
    {
        RefreshChefHat();
        UpdateTrapPlacerVisuals();
    }

    private void OnCharacterNameChanged(
        NetcodeString oldVal, NetcodeString newVal)
    {
        characterObject = CharacterManager.Instance.GetCharacter(newVal.ToString());
        
        RefreshCharacter();

        vfx.Play();

        if (CharacterChanged != null)
            CharacterChanged(oldVal.ToString(), newVal.ToString());
    }

    private void OnSelectedTrapChanged(
        NetcodeString oldVal, NetcodeString newVal)
    {

    }

    private void SoupPot_Behaviour_OnSoupReceivedTrash(float influence, ulong throwerId)
    {
        if (IsServer && throwerId == NetworkObjectId)
            networkScore.Value = Mathf.Max(0, Mathf.RoundToInt(networkScore.Value + influence));
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        if (PlayerScoreChanged != null)
            PlayerScoreChanged(NetworkObjectId, newScore);
    }

    private void OnEnable()
    {
        // Setup game controller event listeners:
        GameController.DebugEnabled     += OnDebugEnabled;
        GameController.DebugDisabled    += OnDebugDisabled;
        GameController.GameStarted      += OnGameStarted;
        GameController.GamePaused       += OnGamePaused;
        GameController.GameResumed      += OnGameResumed;
        GameController.GameStopped      += OnGameStopped;
        GameController.GameCreated      += OnGameCreated;

        // Setup network event listeners:
        networkIsChef.OnValueChanged += OnIsChefChanged;
        networkCharacterName.OnValueChanged += OnCharacterNameChanged;
        SoupPot_Behaviour.SoupReceivedTrash += SoupPot_Behaviour_OnSoupReceivedTrash;
        networkScore.OnValueChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        GameController.DebugEnabled     -= OnDebugEnabled;
        GameController.DebugDisabled    -= OnDebugDisabled;
        GameController.GameStarted      -= OnGameStarted;
        GameController.GamePaused       -= OnGamePaused;
        GameController.GameResumed      -= OnGameResumed;
        GameController.GameStopped      -= OnGameStopped;
        GameController.GameCreated      -= OnGameCreated;

        networkIsChef.OnValueChanged -= OnIsChefChanged;
        networkCharacterName.OnValueChanged -= OnCharacterNameChanged;
        SoupPot_Behaviour.SoupReceivedTrash -= SoupPot_Behaviour_OnSoupReceivedTrash;
        networkScore.OnValueChanged -= OnScoreChanged;
    }

    new private void OnDestroy()
    {
        // remove the player from the players list in players manager
        PlayersManager.Instance.RemovePlayerFromList(NetworkObjectId);
    }

    private Pollutant GetPollutantObject(string type)
    {
        var pollutantObject = ObjectSpawner.Instance.pollutantList.Find(x => x.type == type);

        if (pollutantObject == null)
        {
            pollutantObject = ObjectSpawner.Instance.deadBodyList.Find(x => x.type == type);
        }

        if (pollutantObject == null)
        {
            pollutantObject = ObjectSpawner.Instance.liveBodyList.Find(x => x.type == type);
        }

        return pollutantObject;
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnGrabServerRpc(ulong objToPickupID)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(objToPickupID, out var objToPickup);
        if (objToPickup == null || objToPickup.transform.parent != null) return;

        string pollutantType = objToPickup.GetComponent<PollutantBehaviour>().pollutantObject.type;
        currentlyHeld = GetPollutantObject(pollutantType);

        var heldObjectBehaviour = heldObject.GetComponent<HeldObject>();
        heldObjectBehaviour.heldObject = currentlyHeld.mesh;
        heldObjectBehaviour.meshInitialized = false;

        // Repeat logic on client-side:
        SetHeldObjectClientRpc(pollutantType);
        
        Destroy(objToPickup.gameObject);
        networkCarryState.Value = PlayerCarryState.CarryingObject;
    }

    [ClientRpc]
    public void SetHeldObjectClientRpc(string pollutantType)
    {
        currentlyHeld = GetPollutantObject(pollutantType);

        var heldObjectBehaviour = heldObject.GetComponent<HeldObject>();
        heldObjectBehaviour.heldObject = currentlyHeld.mesh;
        heldObjectBehaviour.meshInitialized = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDropServerRpc(Vector3 playerVelocity)
    {
        if (networkCarryState.Value == PlayerCarryState.Empty) return;

        Vector3 dropPos = transform.position;
        dropPos.y += 2f;
        dropPos += (transform.forward);

        var droppedObj = Instantiate(pollutantPrefab, dropPos, Quaternion.Euler(0, transform.localEulerAngles.y, 90));
        var droppedObjBehaviour = droppedObj.GetComponent<PollutantBehaviour>();
        droppedObjBehaviour.pollutantObject = currentlyHeld;
        droppedObjBehaviour.meshInitialized = false;

        droppedObj.GetComponent<NetworkObject>().Spawn();
        droppedObj.GetComponent<Rigidbody>().velocity = playerVelocity;

        networkCarryState.Value = PlayerCarryState.Empty;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnThrowServerRpc(Vector3 playerForward)
    {
        if (networkCarryState.Value == PlayerCarryState.Empty) return;

        Vector3 throwPos = transform.position;
        throwPos.y += 2f;

        var forwardOffset = 1.05f;
        if (networkPlayerState.Value == PlayerState.Moving)
        {
            forwardOffset = 1.55f;
        }

        throwPos += (playerForward) * forwardOffset;

        var thrownObj = Instantiate(pollutantPrefab, throwPos, Quaternion.Euler(0, transform.localEulerAngles.y, 90));
        var thrownObjBehaviour = thrownObj.GetComponent<PollutantBehaviour>();
        thrownObjBehaviour.pollutantObject = currentlyHeld;
        thrownObjBehaviour.meshInitialized = false;
        thrownObjBehaviour.throwerId.Value = NetworkObjectId; // set the throwerid of the pollutant to this object's owner id

        thrownObj.GetComponent<NetworkObject>().Spawn();
        thrownObj.GetComponent<Rigidbody>().AddForce((playerForward.normalized * throwForce) + (Vector3.up * 6f), ForceMode.Impulse);
        
        thrownObjBehaviour.OnThrowClientRpc();

        networkCarryState.Value = PlayerCarryState.Empty;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBodyServerRpc()
    {
        var deadBody = Instantiate(pollutantPrefab, new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), transform.rotation);
        var deadBodyBehaviour = deadBody.GetComponent<PollutantBehaviour>();
        deadBodyBehaviour.pollutantObject = characterObject.deadCharacter;
        deadBodyBehaviour.meshInitialized = false;

        deadBody.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    public void KillPlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(RespawnTiming());
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerCarryStateServerRpc(PlayerCarryState newState)
    {
        networkCarryState.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerStateServerRpc(PlayerState newState)
    {
        networkPlayerState.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateIsChefServerRpc(bool newValue)
    {
        networkIsChef.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCharacterNameServerRpc(string newName)
    {
        networkCharacterName.Value = newName;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSelectedTrapServerRpc(string newTrap)
    {
        networkSelectedTrap.Value = newTrap;
    }

    public IEnumerator TempDisablePickup()
    {
        justThrew = true;

        yield return new WaitForSeconds(0.50f);

        justThrew = false;
    }

    public IEnumerator TempDisableMovement()
    {
        canMove = false;

        yield return new WaitForSeconds(0.50f);

        canMove = true;
    }


    public IEnumerator RespawnTiming()
    {
        GrabCancelled();
        canMove = false;

        yield return new WaitForSeconds(0.1f);

        SpawnBodyServerRpc();
        PlayerRandomSpawnPoint(false);

        yield return new WaitForSeconds(0.90f);

        canMove = true;
    }

    public IEnumerator TempDisableCharacterSwitch()
    {
        justSwitchedCharacters = true;

        yield return new WaitForSeconds(0.50f);

        justSwitchedCharacters = false;
    }

    public IEnumerator TempDisableTrapPlacementToggle()
    {
        justSwitchedTrapPlacementMode = true;

        yield return new WaitForSeconds(0.5f);

        justSwitchedTrapPlacementMode = false;
    }

    public IEnumerator TempDisableTrapRotation()
    {
        justRotatedTrap = true;

        yield return new WaitForSeconds(0.25f);

        justRotatedTrap = false;
    }

    public IEnumerator TempDisableTrapPlacement()
    {
        justPlacedTrap = true;

        yield return new WaitForSeconds(0.5f);

        justPlacedTrap = false;
    }
}
