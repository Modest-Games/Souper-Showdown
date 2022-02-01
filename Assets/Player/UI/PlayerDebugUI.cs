using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugUI : MonoBehaviour
{
    public GameObject playerObj;
    public GameObject playerStateTextObj;
    public GameObject carryStateTextObj;
    public GameObject numCarryablesTextObj;
    public GameObject isAliveTextObj;
    public GameObject canMoveTextObj;

    private PlayerController playerController;
    private Text playerStateText;
    private Text carryStateText;
    private Text numCarrablesText;
    private Text isAliveText;
    private Text canMoveText;

    void Start()
    {
        // setup variables
        playerController = playerObj.GetComponent<PlayerController>();
        playerStateText = playerStateTextObj.GetComponent<Text>();
        carryStateText = carryStateTextObj.GetComponent<Text>();
        numCarrablesText = numCarryablesTextObj.GetComponent<Text>();
        isAliveText = isAliveTextObj.GetComponent<Text>();
        canMoveText = canMoveTextObj.GetComponent<Text>();
    }

    void Update()
    {
        // reset rotation
        transform.rotation = Quaternion.Euler(90, 0, 0);

        // update the texts
        playerStateText.text = playerController.networkPlayerState.Value.ToString();
        carryStateText.text = playerController.networkCarryState.Value.ToString();
        numCarrablesText.text = playerController.reachableCollectables.Count.ToString();
        isAliveText.text = playerController.isAlive.ToString();
        canMoveText.text = playerController.canMove.ToString();
    }
}
