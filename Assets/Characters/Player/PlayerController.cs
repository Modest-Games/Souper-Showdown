using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Idle,
    Moving,
    Dashing
}

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float dashDuration;
    public float dashForce;
    public float dashCooldown;

    private PlayerControlsMapping controls;
    private PlayerState playerState;
    private Vector2 movement;
    private Vector3 lookVector;
    private float timeOfLastDash;
    private Rigidbody rb;

    private void Awake()
    {
        controls = new PlayerControlsMapping();

        // map control inputs
        controls.Gameplay.Dash.performed += ctx => DashPerformed();
        controls.Gameplay.Move.performed += ctx => MovePerformed(ctx.ReadValue<Vector2>());
        controls.Gameplay.Move.canceled += ctx => MoveCancelled();
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
        playerState = PlayerState.Idle;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // calculate useful variables once
        float currentTime = Time.time;

        // switch on playerstate
        switch (playerState) {
            case PlayerState.Idle:
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

    private void MovePerformed(Vector2 newMovement)
    {
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
}
