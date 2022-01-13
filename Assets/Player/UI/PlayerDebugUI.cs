using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugUI : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject playerStateTextObj;
    public GameObject carryStateTextObj;

    private PlayerController playerController;
    private Text playerStateText;
    private Text carryStateText;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
        playerController = playerObj.GetComponent<PlayerController>();
        playerStateText = playerStateTextObj.GetComponent<Text>();
        carryStateText = carryStateTextObj.GetComponent<Text>();
    }

    void Update()
    {
        // reset z rotation
        transform.rotation = initialRotation;

        // update the texts
        playerStateText.text = playerController.playerState.ToString();
        carryStateText.text = playerController.carryState.ToString();
    }
}
