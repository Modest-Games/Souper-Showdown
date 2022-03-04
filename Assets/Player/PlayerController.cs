using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Samples;
using NaughtyAttributes;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    public delegate void PlayerDelegate();
    public static event PlayerDelegate PlayerCreated;

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
        CarryingPlayer
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
    //public bool isChef = false;

    [Header("State (ReadOnly)")]
    [SerializeField] [ReadOnly] public PlayerState playerState;
    //[SerializeField] [ReadOnly] public PlayerCarryState carryState;
    [SerializeField] [ReadOnly] public PlayerCarryState lastKnownState;
    [SerializeField] [ReadOnly] public bool isAlive;

    [Header("Variables (ReadOnly)")]
    [SerializeField] [ReadOnly] public List<GameObject> reachableCollectables;
    [SerializeField] [ReadOnly] private Vector2 movement;
    [SerializeField] [ReadOnly] private Vector3 lookVector;
    [SerializeField] [ReadOnly] private float timeOfLastDash;

    [SerializeField] private GameObject pollutantPrefab;

    private LineRenderer aimIndicator;
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Transform debugCanvasObj;
    public bool canMove;
    private GameObject dazeIndicator;
    private CharacterBehaviour characterBehaviour;

    private bool justThrew;
    private float timeDazed;
    private bool characterInitialized;
    private bool controlsBound;
    private bool isRefreshingCharacter;
    private bool justSwitchedCharacters;

    public NetworkVariable<int> playerIndex = new NetworkVariable<int>();
    private GameObject heldObject;
    private Pollutant currentlyHeld;

    //public NetworkVariable<bool> networkIsChef = new NetworkVariable<bool>();
    //public NetworkString networkCharacterName = new NetworkString();
    //public NetworkVariable<char> networkCharacterName = new NetworkVariable<char>();

    public NetworkVariable<Unity.Collections.FixedString64Bytes> networkCharacterName = new NetworkVariable<Unity.Collections.FixedString64Bytes>();
    public NetworkVariable<bool> networkIsChef = new NetworkVariable<bool>();
    public NetworkVariable<PlayerCarryState> networkCarryState = new NetworkVariable<PlayerCarryState>();
    public NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private void Awake()
    {
        characterInitialized = false;
        isAlive = true;
        lookVector = transform.forward;
        timeOfLastDash = 0;
        justThrew = false;
        controlsBound = false;
        justSwitchedCharacters = false;
    }

    private void Start()
    {
        bool isChef = (IsClient && IsOwner) ? false : networkIsChef.Value;

        //Physics.IgnoreLayerCollision(7, 10);

        characterObject = (IsClient && IsOwner) ?
            CharacterManager.Instance.GetRandomCharacter() :
            CharacterManager.Instance.GetCharacter(networkCharacterName.Value.ToString());
        canMove = true;
        aimIndicator = transform.Find("ThrowIndicator").GetComponent<LineRenderer>();
        debugCanvasObj = transform.GetComponentInChildren<PlayerDebugUI>().transform;
        characterBehaviour = transform.Find("Character").GetComponent<CharacterBehaviour>();
        //carryState = PlayerCarryState.Empty;
        lastKnownState = networkCarryState.Value;
        rb = GetComponent<Rigidbody>();
        dazeIndicator = transform.Find("DazeIndicatorHolder").gameObject;
        heldObject = transform.Find("Held Object").gameObject;

        //looseArms = ;
        //stiffArms = null;

        //looseArms = characterObject.characterPrefab.transform.Find("Flaccid").gameObject;
        //stiffArms = characterObject.characterPrefab.transform.Find("Stiff").gameObject;

        // setup variables
        if (IsClient && IsOwner)
        {
            // set character name
            Debug.Log(characterObject.characterName);
            UpdateCharacterNameServerRpc(characterObject.characterName);

            // NETWORKING:
            UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);
            UpdatePlayerStateServerRpc(PlayerState.Idle);

            // Spawn the player
            //PlayerRandomSpawnPoint(isChef);
        }

        if (PlayerCreated != null)
            PlayerCreated();

        // setup debugging
        debugCanvasObj.gameObject.SetActive(LobbyController.Instance.isDebugEnabled);

        Debug.LogFormat("{2} initialized: IsClient: {0}, IsOwner: {1}, IsChef: {3}", IsClient, IsOwner, OwnerClientId, isChef);
    }

    public void BindControls()
    {
        if (IsClient && IsOwner)
        {
            playerInput = LocalPlayerManager.Instance.inputPlayers.Find(
                p => p.playerIndex == playerIndex.Value);

            // map control inputs
            playerInput.actions["Dash"].performed += ctx => DashPerformed();
            playerInput.actions["Move"].performed += ctx => MovePerformed(ctx.ReadValue<Vector2>());
            playerInput.actions["Move"].canceled += ctx => MoveCancelled();
            playerInput.actions["Grab"].started += ctx => GrabStarted();
            playerInput.actions["Grab"].canceled += ctx => GrabCancelled();
            playerInput.actions["Throw"].canceled += ctx => ThrowPerformed();
            playerInput.actions["Throw"].started += ctx => ThrowStarted();
            playerInput.actions["Next Character"].performed += ctx => NextCharacterPerformed();
            playerInput.actions["Previous Character"].performed += ctx => PreviousCharacterPerformed();

            Debug.Log("Binding controls to client " + OwnerClientId + " on playerIndex: " + playerIndex);
            controlsBound = true;
        }
    }

    private void PlayerRandomSpawnPoint(bool isChef)
    {
        rb.position = TerrainManager.Instance.GetRandomSpawnLocation(isChef);
    }

    void Update()
    {
        // check if the character needs to be refreshed
        if (!characterInitialized && characterObject != null)
        {
            RefreshCharacter();
            characterInitialized = true;
        }

        else
        {
            // update the player visuals
            UpdateClientVisuals();
        }

        // check if the controls need to be bound
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
        // get the state values depending on if this is a networked client or a local player
        PlayerCarryState carryStateVal = networkCarryState.Value;
        PlayerState playerStateVal = (IsClient && IsOwner) ? playerState : networkPlayerState.Value;


        if (lastKnownState != carryStateVal)
        {
            // update the arms on the character
            characterBehaviour.UpdateArms(carryStateVal);

            // update last state
            lastKnownState = carryStateVal;
        }


        switch (carryStateVal)
        {
            case PlayerCarryState.Empty:
                // Set "Held Object" to inactive
                heldObject.SetActive(false);
                break;

            case PlayerCarryState.CarryingObject:
                // Set "Held Object" to active
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
                // show the daze indicator
                dazeIndicator.SetActive(true);
                break;

            default:
                // hide the daze indicator
                dazeIndicator.SetActive(false);
                break;
        }
    }

    private void PlayerMovement()
    {
        // calculate useful variables once
        float currentTime = Time.time;
        float deltaTime = Time.fixedDeltaTime;

        // switch on playerstate
        switch (playerState)
        {
            case PlayerState.Idle:
                // clear rotatitonal velocity
                rb.angularVelocity = Vector3.zero;
                break;

            case PlayerState.Moving:
                // handle player movement
                Vector3 movementVec = new Vector3(movement.x, 0, movement.y) * deltaTime * moveSpeed;
                //rb.AddForce(movementVec, ForceMode.Impulse);
                //transform.Translate(movementVec, Space.World);
                rb.MovePosition(rb.position + movementVec);

                // rotate towards motion vector
                lookVector = movementVec.normalized;
                lookVector.y = 0f; // remove any y angle from the look vector

                transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + lookVector, rotateSpeed * deltaTime));
                // transform.rotation.SetFromToRotation(transform.rotation.eulerAngles, movementVec);

                // DEBUG:
                // draw motion vector
                Debug.DrawRay(transform.position, movementVec.normalized * 2, Color.blue);

                // draw facing vector
                Debug.DrawRay(transform.position, transform.forward * 2, Color.green);
                break;

            case PlayerState.Dashing:
                // check if the dash should be complete
                if ((currentTime - timeOfLastDash) >= dashDuration)
                {
                    // complete the dash and update the player state (depending on if moving or not)
                    PlayerState newPlayerState = (movement.magnitude == 0) ? PlayerState.Idle : PlayerState.Moving;
                    UpdatePlayerStateServerRpc(newPlayerState);
                    playerState = newPlayerState;
                }

                else
                {
                    // calculate the dash vector
                    Vector3 dashVector = rb.position + (lookVector * dashForce * deltaTime);

                    // TEMP: to cause the network to send a transform update
                    //transform.Translate(transform.forward * 0.01f);

                    // continue performing the dash
                    //transform.Translate(lookVector * dashForce * Time.deltaTime, Space.World);
                    rb.MovePosition(dashVector);
                    //transform.Translate(dashVector, Space.Self);
                    //rb.AddForce(lookVector * dashForce, ForceMode.Impulse);
                    //rb.velocity = lookVector * dashForce;

                    // look at direction of motion
                    transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + lookVector, rotateSpeed * deltaTime));

                    Debug.DrawLine(rb.position, rb.position + (lookVector * 4), Color.red);

                    // update the player state
                    //UpdatePlayerStateServerRpc(PlayerState.Dashing);
                }

                break;

            case PlayerState.Dazed:
                // clear rotatitonal velocity
                rb.angularVelocity = Vector3.zero;

                // check if the daze should be over
                if (timeDazed >= dazeDuration)
                {
                    // end the daze
                    playerState = PlayerState.Idle;
                    UpdatePlayerStateServerRpc(PlayerState.Idle);
                }

                // if the daze is still going
                else
                {
                    // update time dazed
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
            // switch on the other object's tag
            switch (other.tag)
            {
                case "Pollutant":
                    // add the pollutant to the list of reachable collectables
                    if (!reachableCollectables.Contains(other.gameObject) && !justThrew)
                    {
                        reachableCollectables.Add(other.gameObject);
                    }

                    //Debug.Log("ENTER: " + other.gameObject.GetInstanceID());
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
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(other.gameObject.tag); //TEMP UNDO

        // only get booped if not a chef
        if (IsOwner && IsClient && !networkIsChef.Value)
        {
            switch (other.gameObject.tag)
            {
                case "Player":
                    // get the other player's PlayerController
                    PlayerController otherPC = other.GetComponentInParent<PlayerController>();

                    // get the other player's state (if they are a local player, then use the appropriate player state)
                    PlayerState otherPlayerState = (otherPC.IsClient && otherPC.IsOwner)
                        ? otherPC.playerState : otherPC.networkPlayerState.Value;

                    // check if the other player is a chef
                    if (otherPC.networkIsChef.Value && otherPlayerState == PlayerState.Dashing)
                    {
                        // get rekt
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
            // switch on the other object's tag
            switch (other.tag)
            {
                case "Pollutant":
                    // remove the pollutant from the list of reachable collectables
                    reachableCollectables.Remove(other.gameObject);
                    //Debug.Log("EXIT: " + other.gameObject.GetInstanceID());
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
                    break;
            }
        }
    }

    [NaughtyAttributes.Button("Refresh Character", EButtonEnableMode.Editor)]
    private void RefreshCharacter()
    {
        isRefreshingCharacter = true;
        Debug.Log("REFRESH CHARACTER CALLED");

        // check if there is a character mesh ready
        GameObject newCharacterMesh = characterObject == null ?
            CharacterManager.Instance.GetCharacter(0).characterPrefab : characterObject.characterPrefab;

        // remove the old mesh
        Destroy(transform.Find("Character").gameObject);

        // instantiate the new mesh
        GameObject newMesh = Instantiate(newCharacterMesh, transform);

        // enable the chef hat if this player is a chef
        transform.Find("ChefHat").gameObject.SetActive(networkIsChef.Value);

        newMesh.name = "Character";
        characterBehaviour = newMesh.GetComponent<CharacterBehaviour>();
        isRefreshingCharacter = false;
    }

    private void OnBoop()
    {
        // make sure the player can be booped
        bool canGetBooped = (playerState != PlayerState.Dazed) && (playerState != PlayerState.Ungrounded);

        if (canGetBooped)
        {
            Debug.Log(gameObject.name + " was booped!");

            // reset timeDazed
            timeDazed = 0f;

            // drop what's being held (if anything)
            GrabCancelled();

            // update the player state
            playerState = PlayerState.Dazed;
            UpdatePlayerStateServerRpc(PlayerState.Dazed);
        }
    }

    [NaughtyAttributes.Button("Refresh Reachable Collectables")]
    private void RefreshReachableCollectables()
    {
        for (int i = 0; i < reachableCollectables.Count; i++)
        {
            // remove missing reachable collectables
            if (reachableCollectables[i] == null)
                reachableCollectables.RemoveAt(i);
        }

        Debug.Log(reachableCollectables);
    }

    public void MovePerformed(Vector2 newMovement)
    {
        if (Application.isFocused && IsClient && IsOwner)
        {
            // update the movement vector
            movement = newMovement;

            // set the playerstate to moving if not dashing
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
            // reset the movement vector
            movement = Vector2.zero;

            // set playerstate to idle if not dashing
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
            // calculate the time since the last dash, and if the player can dash
            float timeSinceDashCompleted = (Time.time - timeOfLastDash) - dashDuration;
            bool canDash = (playerState == PlayerState.Idle || playerState == PlayerState.Moving)
                && timeSinceDashCompleted >= dashCooldown && networkCarryState.Value == PlayerCarryState.Empty;

            // make sure the player is not already dashing
            if (canDash)
            {
                Debug.LogFormat("Started dash for {0}s", dashDuration);
                timeOfLastDash = Time.time;

                // set the playerstate to dashing
                UpdatePlayerStateServerRpc(PlayerState.Dashing);
                playerState = PlayerState.Dashing;
            }
        }
    }

    public void GrabStarted()
    {
        if (IsClient && IsOwner)
        {
            // refresh reachable collectables
            RefreshReachableCollectables();

            // determine if can pickup
            bool canPickup = (networkCarryState.Value == PlayerCarryState.Empty) && (reachableCollectables.Count > 0);

            // if the player can pickup
            if (canPickup)
            {
                // sort reachableCollectables by distance
                reachableCollectables = reachableCollectables.OrderBy(
                    r => Vector3.Distance(transform.position, r.transform.position)).ToList();

                // get the nearest reachable collectable
                GameObject nearestReachableCollectable = reachableCollectables[0];

                reachableCollectables.Remove(nearestReachableCollectable);
                var netObj = nearestReachableCollectable.GetComponent<NetworkObject>();

                // Spawn new pollutant:
                OnGrabServerRpc(netObj.NetworkObjectId);
            }
        }
    }

    public void GrabCancelled()
    {
        if (IsClient && IsOwner)
        {
            // determine if can drop
            bool canDrop = (networkCarryState.Value == PlayerCarryState.CarryingObject) || (networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            // if the player can drop
            if (canDrop)
            {
                // hide the aim indicator (in case throw is being held)
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
            // determine if can throw
            bool canThrow =
            (networkPlayerState.Value == PlayerState.Idle || networkPlayerState.Value == PlayerState.Moving) &&
            (networkCarryState.Value == PlayerCarryState.CarryingObject || networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            if (canThrow)
            {
                // show the aim indicator
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

                // Play a throwing animation:
                // ...

                StartCoroutine(TempDisablePickup());
                StartCoroutine(TempDisableMovement());
                OnThrowServerRpc();
            }
        }
    }

    public void NextCharacterPerformed()
    {
        if (IsClient && IsOwner && !isRefreshingCharacter && !justSwitchedCharacters && SceneManager.GetActiveScene().name == "Lobby")
        {
            UpdateCharacterNameServerRpc(CharacterManager.Instance.GetNextCharacter(characterObject.characterName).characterName);
            StartCoroutine(TempDisableCharacterSwitch());
        }
    }

    public void PreviousCharacterPerformed()
    {
        if (IsClient && IsOwner && !isRefreshingCharacter && !justSwitchedCharacters && SceneManager.GetActiveScene().name == "Lobby")
        {
            UpdateCharacterNameServerRpc(CharacterManager.Instance.GetNextCharacter(characterObject.characterName).characterName);
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
        // stop moving
        rb.velocity = Vector3.zero;

        canMove = false;
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
        if (characterInitialized && !isRefreshingCharacter)
            RefreshCharacter();
    }

    private void OnCharacterNameChanged(
        Unity.Collections.FixedString64Bytes oldVal, Unity.Collections.FixedString64Bytes newVal)
    {
        characterObject = CharacterManager.Instance.GetCharacter(newVal.ToString());
        
        
        if (characterInitialized && !isRefreshingCharacter)
            RefreshCharacter();
    }

    private void OnEnable()
    {
        // enable controls
        //controls.Gameplay.Enable();

        // setup event listeners
        GameController.DebugEnabled     += OnDebugEnabled;
        GameController.DebugDisabled    += OnDebugDisabled;
        GameController.GameStarted      += OnGameStarted;
        GameController.GamePaused       += OnGamePaused;
        GameController.GameResumed      += OnGameResumed;
        GameController.GameStopped      += OnGameStopped;
        GameController.GameCreated      += OnGameCreated;

        // setup network event listeners
        networkIsChef.OnValueChanged += OnIsChefChanged;
        networkCharacterName.OnValueChanged += OnCharacterNameChanged;
    }

    private void OnDisable()
    {
        // disable controls
        //controls.Gameplay.Disable();

        // clear event listeners
        GameController.DebugEnabled     -= OnDebugEnabled;
        GameController.DebugDisabled    -= OnDebugDisabled;
        GameController.GameStarted      -= OnGameStarted;
        GameController.GamePaused       -= OnGamePaused;
        GameController.GameResumed      -= OnGameResumed;
        GameController.GameStopped      -= OnGameStopped;
        GameController.GameCreated      -= OnGameCreated;

        // clear network event listeners
        networkIsChef.OnValueChanged -= OnIsChefChanged;
        networkCharacterName.OnValueChanged += OnCharacterNameChanged;
    }

    private Pollutant GetPollutantObject(string type)
    {
        var pollutantObject = ObjectSpawner.Instance.pollutantList.Find(x => x.type == type);

        if (pollutantObject == null)
        {
            pollutantObject = ObjectSpawner.Instance.deadBodyList.Find(x => x.type == type);
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
    public void OnThrowServerRpc()
    {
        if (networkCarryState.Value == PlayerCarryState.Empty) return;

        Vector3 throwPos = transform.position;
        throwPos.y += 2f;

        var forwardOffset = 1.05f;
        if (networkPlayerState.Value == PlayerState.Moving)
        {
            forwardOffset = 1.55f;
        }

        throwPos += (transform.forward) * forwardOffset;

        var thrownObj = Instantiate(pollutantPrefab, throwPos, Quaternion.Euler(0, transform.localEulerAngles.y, 90));
        var thrownObjBehaviour = thrownObj.GetComponent<PollutantBehaviour>();
        thrownObjBehaviour.pollutantObject = currentlyHeld;
        thrownObjBehaviour.meshInitialized = false;

        thrownObj.GetComponent<NetworkObject>().Spawn();
        thrownObj.GetComponent<Rigidbody>().AddForce((transform.forward.normalized * throwForce) + (Vector3.up * 6f), ForceMode.Impulse);
        
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
}
