using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerController : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Dashing,
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

    [Header("Character")]
    public Character characterObject;

    [Header("State (ReadOnly)")]
    [SerializeField] [ReadOnly] public PlayerState playerState;
    [SerializeField] [ReadOnly] public PlayerCarryState carryState;
    [SerializeField] [ReadOnly] public Pollutant carriedObject;
    [SerializeField] [ReadOnly] public bool isAlive;
    
    [Header("Variables (ReadOnly)")]
    [SerializeField] [ReadOnly] private List<GameObject> reachableCollectables;
    [SerializeField] [ReadOnly] private Vector2 movement;
    [SerializeField] [ReadOnly] private Vector3 lookVector;
    [SerializeField] [ReadOnly] private float timeOfLastDash;
    [SerializeField] private Vector2 defaultPositionRange = new Vector2(-5, 5);

    private Transform holdLocation;
    private Rigidbody rb;
    private PlayerControlsMapping controls;
    private Transform debugCanvasObj;

    private void Awake()
    {
        controls = new PlayerControlsMapping();

        // map control inputs
        controls.Gameplay.Dash.performed    += ctx => DashPerformed();
        controls.Gameplay.Move.performed    += ctx => MovePerformed(ctx.ReadValue<Vector2>());
        controls.Gameplay.Move.canceled     += ctx => MoveCancelled();
        controls.Gameplay.Grab.started      += ctx => GrabStarted();
        controls.Gameplay.Grab.canceled     += ctx => GrabCancelled();
        controls.Gameplay.Throw.started     += ctx => ThrowStarted();
    }

    private void Start()
    {
        // setup variables
        debugCanvasObj = transform.GetComponentInChildren<PlayerDebugUI>().transform;
        isAlive = true;
        lookVector = transform.forward;
        timeOfLastDash = 0;
        carryState = PlayerCarryState.Empty;
        playerState = PlayerState.Idle;
        rb = GetComponent<Rigidbody>();
        holdLocation = transform.Find("HoldLocation");

        // refresh the character
        RefreshCharacter();

        // setup debugging
        debugCanvasObj.gameObject.SetActive(FindObjectOfType<GameController>().isDebugEnabled);

        // Random Spawn Position:
        transform.position = new Vector3(Random.Range(defaultPositionRange.x, defaultPositionRange.y), 0, Random.Range(defaultPositionRange.x, defaultPositionRange.y));
    }

    void Update()
    {
        if (IsClient && IsOwner)
        {
            if (isAlive)
            {
                PlayerMovement();
            } else
            {
                // do dead things
            }
        }
    }

    private void PlayerMovement()
    {
        // calculate useful variables once
        float currentTime = Time.time;

        // switch on playerstate
        switch (playerState)
        {
            case PlayerState.Idle:
                // clear rotatitonal velocity
                rb.angularVelocity = Vector3.zero;

                // play idle animation, etc.
                break;

            case PlayerState.Moving:
                // handle player movement
                Vector3 movementVec = new Vector3(movement.x, 0, movement.y) * Time.deltaTime * moveSpeed;
                //rb.AddForce(movementVec, ForceMode.Impulse);
                //transform.Translate(movementVec, Space.World);
                rb.MovePosition(rb.position + movementVec);

                // rotate towards motion vector
                lookVector = movementVec.normalized;
                lookVector.y = 0f; // remove any y angle from the look vector

                transform.LookAt(Vector3.Lerp(transform.position + transform.forward, transform.position + lookVector, rotateSpeed * Time.deltaTime));
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
                    // complete the dash
                    playerState = (movement.magnitude == 0) ? PlayerState.Idle : PlayerState.Moving;
                }

                else
                {
                    Debug.DrawLine(rb.position, rb.position + (lookVector * 4), Color.red);
                    Debug.Log(dashForce);

                    // continue performing the dash
                    //transform.Translate(lookVector * dashForce * Time.deltaTime, Space.World);
                    //rb.MovePosition(rb.position + lookVector * dashForce * Time.deltaTime);
                    rb.AddForce(lookVector * dashForce, ForceMode.Impulse);
                    //rb.velocity = lookVector * dashForce;

                    playerState = PlayerState.Dashing;
                }

                break;

            case PlayerState.Ungrounded:
                // play a flail animation, etc.
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        // switch on the other object's tag
        switch (other.tag)
        {
            case "Pollutant":
                // add the pollutant to the list of reachable collectables
                reachableCollectables.Add(other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // switch on the other object's tag
        switch (other.tag)
        {
            case "Pollutant":
                // remove the pollutant from the list of reachable collectables
                reachableCollectables.Remove(other.gameObject);
                break;
        }
    }

    [Button]
    private void RefreshCharacter()
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
        newMesh.name = "Character";
    }

    private void MovePerformed(Vector2 newMovement)
    {
        if (Application.isFocused)
        {
            // update the movement vector
            movement = newMovement;

            // set the playerstate to moving if not dashing
            if (playerState != PlayerState.Dashing)
                playerState = PlayerState.Moving;
        }
    }

    private void MoveCancelled()
    {
        if (Application.isFocused)
        {
            // reset the movement vector
            movement = Vector2.zero;

            // set playerstate to idle if not dashing
            if (playerState != PlayerState.Dashing)
                playerState = PlayerState.Idle;
        }
    }

    private void DashPerformed()
    {
        if (Application.isFocused)
        {
            // calculate the time since the last dash, and if the player can dash
            float timeSinceDashCompleted = (Time.time - timeOfLastDash) - dashDuration;
            bool canDash = playerState != PlayerState.Dashing && timeSinceDashCompleted >= dashCooldown;

            // make sure the player is not already dashing
            if (canDash)
            {
                timeOfLastDash = Time.time;

                // set the playerstate to dashing
                playerState = PlayerState.Dashing;
            }
        }
    }

    private void GrabStarted()
    {
        // determine if can pickup
        bool canPickup = (carryState == PlayerCarryState.Empty) && (reachableCollectables.Count > 0) ;

        Debug.Log("CanPickup: " + canPickup);

        // if the player can pickup
        if (canPickup)
        {
            // sort reachableCollectables by distance
            reachableCollectables = reachableCollectables.OrderBy(
                r => Vector3.Distance(transform.position, r.transform.position)).ToList();

            // get the nearest reachable collectable
            GameObject nearestReachableCollectable = reachableCollectables[0];

            // disable physics on the collectable
            Rigidbody collectableRb = nearestReachableCollectable.GetComponent<Rigidbody>();
            collectableRb.isKinematic = true;
            collectableRb.useGravity = false;

            // parent the collectable to the HoldLocation and reset it's local transform
            nearestReachableCollectable.transform.SetParent(holdLocation);
            nearestReachableCollectable.transform.localPosition = Vector3.zero;
            nearestReachableCollectable.transform.localRotation = Quaternion.identity;

            // update the carryState
            carryState = PlayerCarryState.CarryingObject;
        }
    }

    private void GrabCancelled()
    {
        // determine if can drop
        bool canDrop = (carryState == PlayerCarryState.CarryingObject) || (carryState == PlayerCarryState.CarryingPlayer);

        // if the player can drop
        if (canDrop)
        {
            // drop whatever is in the holdLocation
            Transform dropable = holdLocation.GetChild(0);

            // detach the dropable from the player
            dropable.SetParent(null);

            // enable physics on the dropable
            Rigidbody dropableRb = dropable.GetComponent<Rigidbody>();
            dropableRb.isKinematic = false;
            dropableRb.useGravity = true;

            // set the dropable's velocity to the player's current velocity
            dropableRb.velocity = 2f * new Vector3(movement.x, 0, movement.y);

            // update the carryState
            carryState = PlayerCarryState.Empty;
        }
    }

    private void ThrowStarted()
    {
        // determine if can throw
        bool canThrow = 
            (playerState == PlayerState.Idle || playerState == PlayerState.Moving) &&
            (carryState == PlayerCarryState.CarryingObject || carryState == PlayerCarryState.CarryingPlayer);

        // if the player can throw
        if (canThrow)
        {
            // throw whatever is in the holdLocation
            Transform throwable = holdLocation.GetChild(0);

            // detach the throwable from the player
            throwable.SetParent(null);

            // enable physics on the throwable
            Rigidbody throwableRb = throwable.GetComponent<Rigidbody>();
            throwableRb.isKinematic = false;
            throwableRb.useGravity = true;

            // apply a 'throw' velocity to the throwable in the forward vector
            throwableRb.AddForce(lookVector.normalized * throwForce, ForceMode.Impulse);

            // set the carry state to empty
            carryState = PlayerCarryState.Empty;
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

    private void OnEnable()
    {
        // enable controls
        controls.Gameplay.Enable();

        // subscribe to events
        GameController.DebugEnabled += OnDebugEnabled;
        GameController.DebugDisabled += OnDebugDisabled;
    }

    private void OnDisable()
    {
        // disable controls
        controls.Gameplay.Disable();

        // unsubscribe from events
        GameController.DebugEnabled -= OnDebugEnabled;
        GameController.DebugDisabled -= OnDebugDisabled;
    }
}
