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

    void Start()
    {
        playerController = playerObj.GetComponent<PlayerController>();
        playerStateText = playerStateTextObj.GetComponent<Text>();
        carryStateText = carryStateTextObj.GetComponent<Text>();
    }

    void Update()
    {
        // reset rotation
        transform.rotation = Quaternion.Euler(90, 0, 0);

        // update the texts
        playerStateText.text = playerController.networkPlayerState.Value.ToString();
        carryStateText.text = playerController.carryState.ToString();
    }
}
