using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Samples;
using NaughtyAttributes;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerController : NetworkBehaviour
{
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
    public float moveSpeed;
    public float rotateSpeed;
    public float dashDuration;
    public float dashForce;
    public float dashCooldown;
    public float throwForce;
    public float dazeDuration;

    [Header("Character")]
    public Character characterObject;
    //public bool isChef = false;

    [Header("State (ReadOnly)")]
    [SerializeField] [ReadOnly] public PlayerState playerState;
    //[SerializeField] [ReadOnly] public PlayerCarryState carryState;
    [SerializeField] [ReadOnly] public Pollutant carriedObject;
    [SerializeField] [ReadOnly] public bool isAlive;

    [Header("Variables (ReadOnly)")]
    [SerializeField] [ReadOnly] public List<GameObject> reachableCollectables;
    [SerializeField] [ReadOnly] private Vector2 movement;
    [SerializeField] [ReadOnly] private Vector3 lookVector;
    [SerializeField] [ReadOnly] private float timeOfLastDash;

    private Transform holdLocation;
    private LineRenderer aimIndicator;
    private Rigidbody rb;
    private PlayerControlsMapping controls;
    private Transform debugCanvasObj;
    public bool canMove;
    private GameObject dazeIndicator;

    private GameObject heldObject;
    private bool justThrew;
    private float timeDazed;

    //public NetworkVariable<bool> networkIsChef = new NetworkVariable<bool>();
    private NetworkVariable<bool> networkIsChef = new NetworkVariable<bool>();
    public NetworkVariable<PlayerCarryState> networkCarryState = new NetworkVariable<PlayerCarryState>();
    public NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private void Awake()
    {
        isAlive = true;
        lookVector = transform.forward;
        timeOfLastDash = 0;
        //carryState = PlayerCarryState.Empty;
        justThrew = false;
        controls = new PlayerControlsMapping();
    }

    private void Start()
    {
        bool isChef = (IsClient && IsOwner) ? UIManager.Instance.isChefToggle.isOn : networkIsChef.Value;
        canMove = GameController.Instance.gameState.Value == GameController.GameState.Running;
        aimIndicator = transform.Find("ThrowIndicator").GetComponent<LineRenderer>();
        debugCanvasObj = transform.GetComponentInChildren<PlayerDebugUI>().transform;
        //carryState = PlayerCarryState.Empty;
        rb = GetComponent<Rigidbody>();
        holdLocation = transform.Find("HoldLocation");
        dazeIndicator = transform.Find("DazeIndicatorHolder").gameObject;

        // setup variables
        if (IsClient && IsOwner)
        {
            // set isChef
            UpdateIsChefServerRpc(isChef);

            // map control inputs
            controls.Gameplay.Dash.performed += ctx => DashPerformed();
            controls.Gameplay.Move.performed += ctx => MovePerformed(ctx.ReadValue<Vector2>());
            controls.Gameplay.Move.canceled += ctx => MoveCancelled();
            controls.Gameplay.Grab.started += ctx => GrabStarted();
            controls.Gameplay.Grab.canceled += ctx => GrabCancelled();
            controls.Gameplay.Throw.canceled += ctx => ThrowPerformed();
            controls.Gameplay.Throw.started += ctx => ThrowStarted();

            // Spawn the player
            rb.position = TerrainManager.Instance.GetRandomSpawnLocation(isChef);

            // NETWORKING:
            UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);
            UpdatePlayerStateServerRpc(PlayerState.Idle);
        }

        // refresh the character
        RefreshCharacter(isChef);

        // setup debugging
        debugCanvasObj.gameObject.SetActive(FindObjectOfType<GameController>().isDebugEnabled);

        Debug.LogFormat("{2} initialized: IsClient: {0}, IsOwner: {1}, IsChef: {3}", IsClient, IsOwner, OwnerClientId, isChef);
    }

    void Update()
    {
        // update the player visuals
        UpdateClientVisuals();
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

        switch (carryStateVal)
        {
            case PlayerCarryState.Empty:
                // Set "Held Object" to inactive
                transform.GetChild(1).gameObject.SetActive(false);
                break;

            case PlayerCarryState.CarryingObject:
                // Set "Held Object" to active
                transform.GetChild(1).gameObject.SetActive(true);
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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);

        // only get booped if not a chef
        if (IsOwner && IsClient && !networkIsChef.Value)
        {
            switch (collision.gameObject.tag)
            {
                case "Player":
                    // get the other player's PlayerController
                    PlayerController otherPC = collision.gameObject.GetComponent<PlayerController>();

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
    }

    [NaughtyAttributes.Button("Refresh Character", EButtonEnableMode.Editor)]
    private void RefreshCharacter(bool isChef)
    {
        // check if there is an existing mesh
        Transform oldCharacter = transform.Find("Character");
        if (oldCharacter != null)
        {
            // remove the old mesh
            DestroyImmediate(oldCharacter.gameObject);
        }

        // instantiate the new mesh
        GameObject newMesh = Instantiate(characterObject.characterPrefab, transform);

        // enable the chef hat if this player is a chef
        transform.Find("ChefHat").gameObject.SetActive(isChef);

        newMesh.name = "Character";
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

    private void MovePerformed(Vector2 newMovement)
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

    private void MoveCancelled()
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

    private void DashPerformed()
    {
        if (Application.isFocused && IsClient && IsOwner)
        {
            // calculate the time since the last dash, and if the player can dash
            float timeSinceDashCompleted = (Time.time - timeOfLastDash) - dashDuration;
            bool canDash = playerState != PlayerState.Dashing && timeSinceDashCompleted >= dashCooldown
                && networkCarryState.Value == PlayerCarryState.Empty;

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

    private void GrabStarted()
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

                // Store the nearest reachable collectable for dropping purposes:
                heldObject = nearestReachableCollectable;

                // Call OnPickup ServerRpc:
                nearestReachableCollectable.GetComponent<PollutantBehaviour>().OnPickupServerRpc();

                reachableCollectables.Remove(nearestReachableCollectable);

                // update the carryState
                UpdatePlayerCarryStateServerRpc(PlayerCarryState.CarryingObject);
            }
        }
    }

    private void GrabCancelled()
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
                var playerVelocity = 2f * new Vector3(movement.x, 0, movement.y);
                heldObject.GetComponent<PollutantBehaviour>().OnDropServerRpc(transform.position, transform.forward, playerVelocity, lookVector, throwForce, false);

                // update the carryState
                UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);
            }
        }
    }

    private void ThrowStarted()
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

    private void ThrowPerformed()
    {
        if (IsClient && IsOwner)
        {
            // determine if can throw
            bool canThrow =
            (networkPlayerState.Value == PlayerState.Idle || networkPlayerState.Value == PlayerState.Moving) &&
            (networkCarryState.Value == PlayerCarryState.CarryingObject || networkCarryState.Value == PlayerCarryState.CarryingPlayer);

            // if the player can throw
            if (canThrow)
            {
                StartCoroutine(TempDisablePickup());
                var playerVelocity = 2f * new Vector3(movement.x, 0, movement.y);
                heldObject.GetComponent<PollutantBehaviour>().OnDropServerRpc(transform.position, transform.forward, playerVelocity, lookVector, throwForce, true);

                // set the carry state to empty
                UpdatePlayerCarryStateServerRpc(PlayerCarryState.Empty);

                // hide the aim indicator
                aimIndicator.gameObject.SetActive(false);
            }
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
        RefreshCharacter(newVal);
    }

    private void OnEnable()
    {
        // enable controls
        controls.Gameplay.Enable();

        // setup event listeners
        GameController.DebugEnabled     += OnDebugEnabled;
        GameController.DebugDisabled    += OnDebugDisabled;
        GameController.GameStarted      += OnGameStarted;
        GameController.GamePaused       += OnGamePaused;
        GameController.GameResumed      += OnGameResumed;
        GameController.GameStopped      += OnGameStopped;

        // setup network event listeners
        networkIsChef.OnValueChanged += OnIsChefChanged;
    }

    private void OnDisable()
    {
        // disable controls
        controls.Gameplay.Disable();

        // clear event listeners
        GameController.DebugEnabled     -= OnDebugEnabled;
        GameController.DebugDisabled    -= OnDebugDisabled;
        GameController.GameStarted      -= OnGameStarted;
        GameController.GamePaused       -= OnGamePaused;
        GameController.GameResumed      -= OnGameResumed;
        GameController.GameStopped      -= OnGameStopped;

        // clear network event listeners
        networkIsChef.OnValueChanged -= OnIsChefChanged;
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

    public IEnumerator TempDisablePickup()
    {
        justThrew = true;

        yield return new WaitForSeconds(0.50f);

        justThrew = false;
    }
}
