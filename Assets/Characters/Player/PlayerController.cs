using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;

    PlayerControlsMapping controls;
    private Vector2 movement;

    private void Awake()
    {
        controls = new PlayerControlsMapping();

        // map control inputs
        controls.Gameplay.Dash.performed += ctx => DashPerformed();
        controls.Gameplay.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => movement = Vector2.zero;
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
        
    }

    void Update()
    {
        // handle player movement
        Vector3 movementVec = new Vector3(movement.x, 0, movement.y) * Time.deltaTime * moveSpeed;
        transform.Translate(movementVec, Space.World);

        // rotate towards motion vector
        Vector3 lookLocation = transform.position + movementVec.normalized;
        transform.LookAt(Vector3.Lerp(transform.position + transform.forward, lookLocation, rotateSpeed * Time.deltaTime));
        //transform.rotation.SetFromToRotation(transform.rotation.eulerAngles, movementVec);

        // DEBUG
        // draw motion vector
        Debug.DrawRay(transform.position, movementVec.normalized * 2, Color.blue);
        // draw facing vector
        Debug.DrawRay(transform.position, transform.forward * 2, Color.green);
    }

    private void DashPerformed()
    {

    }
}
