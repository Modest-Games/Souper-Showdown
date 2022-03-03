using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

public class GetPlayersOnStart : MonoBehaviour
{
    private GameObject[] players;
    public CinemachineVirtualCamera VCam;

    private void OnEnable()
    {
        GameController.GameStarted      += OnGameStarted;
    }

    private void OnGameStarted()
    {   
        // get camera target struct
        CinemachineTargetGroup camTargetGroup = GameObject.Find("CineMachine Target Group").GetComponent<CinemachineTargetGroup>();

        // when the game starts, get all players 
        players = GameObject.FindGameObjectsWithTag("Player");
        
        // add them to the cineCam target group target list
        foreach(GameObject player in players) {
            camTargetGroup.AddMember(player.transform, 1f, 0f);
        }

        // update camera settings to center on characters
        //CinemachineVirtualCamera CineMachine = GameObject.Find("CineMachine VCam");

    }

    private void OnDisable() 
    {
        GameController.GameStarted      -= OnGameStarted;
    }

}
