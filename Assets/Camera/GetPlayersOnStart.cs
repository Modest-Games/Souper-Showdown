using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

public class GetPlayersOnStart : MonoBehaviour
{
    private GameObject[] players;
    private CinemachineVirtualCamera VCam;
    private GameObject camObj;

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
        camObj = GameObject.Find("CineMachine VCam");
        VCam = camObj.GetComponent<CinemachineVirtualCamera>();
        VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 900;
        VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.5f;
        VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.5f;

    }

    private void OnDisable() 
    {
        GameController.GameStarted      -= OnGameStarted;
    }

}
