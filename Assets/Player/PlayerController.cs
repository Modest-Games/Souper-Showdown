using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using System.Linq;

public enum PlayerState
{
    Idle,
    Moving,
    Dashing
}

public enum PlayerCarryState
{
    Empty,
    CarryingObject,
    CarryingPlayer
}

public class PlayerController : MonoBehaviour
{
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
    [SerializeField] [ReadOnly] private PlayerState playerState;
    [SerializeField] [ReadOnly] private PlayerCarryState carryState;
    
    [Header("Variables (ReadOnly)")]
    [SerializeField] [ReadOnly] private List<GameObject> reachableCollectables;
    [SerializeField] [ReadOnly] private Vector2 movement;
    [SerializeField] [ReadOnly] private Vector3 lookVector;
    [SerializeField] [ReadOnly] private float timeOfLastDash;

    private Transform holdLocation;
    private Rigidbody rb;
    private PlayerControlsMapping controls;

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

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    void Start()
    {
        // setup variables
        lookVector = transform.forward;
        timeOfLastDash = 0;
        carryState = PlayerCarryState.Empty;
        playerState = PlayerState.Idle;
        rb = GetComponent<Rigidbody>();
        holdLocation = transform.Find("HoldLocation");

        // refresh the character
        RefreshCharacter();
    }

    void Update()
    {
        // calculate useful variables once
        float currentTime = Time.time;

        // switch on playerstate
        switch (playerState) {
            case PlayerState.Idle:
                // do nothing (for now)
                break;

            case PlayerState.Moving:
                // handle player movement
                Vector3 movementVec = new Vector3(movement.x, 0, movement.y) * Time.deltaTime * moveSpeed;
                transform.Translate(movementVec, Space.World);
                //rb.MovePosition(rb.position + movementVec);

                // rotate towards motion vector
                lookVector = movementVec.normalized;
                transform.LookAt(Vector3.Lerp(
                    transform.position + transform.forward, transform.position + lookVector, rotateSpeed * Time.deltaTime));
                //transform.rotation.SetFromToRotation(transform.rotation.eulerAngles, movementVec);

                // DEBUG
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
                } else
                {
                    transform.Translate(lookVector * dashForce * Time.deltaTime, Space.World);

                    playerState = PlayerState.Dashing;
                }
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
        // update the movement vector
        movement = newMovement;

        // set the playerstate to moving if not dashing
        if (playerState != PlayerState.Dashing)
            playerState = PlayerState.Moving;
    }

    private void MoveCancelled()
    {
        // reset the movement vector
        movement = Vector2.zero;

        // set playerstate to idle if not dashing
        if (playerState != PlayerState.Dashing)
            playerState = PlayerState.Idle;
    }

    private void DashPerformed()
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

    private void GrabStarted()
    {
        // determine if can pickup
        bool canPickup = (carryState == PlayerCarryState.Empty) && (reachableCollectables.Count > 0) ;

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
}
