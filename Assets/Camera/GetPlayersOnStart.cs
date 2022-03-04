using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;
using static PlayerController;

public class GetPlayersOnStart : MonoBehaviour
{
    private GameObject[] players;
    private CinemachineVirtualCamera VCam;
    private GameObject camObj;

    private Vector3 startPos;
    private Quaternion startRot;

    // force camera to this position on start
    public void Start()
    {
        // startPos = new Vector3(5.0f, 16.0f, -69.0f);
        // startRot = new Quaternion(8, 0, 0, 1);

        // Debug.Log("start from cinemachine");

        // camObj = GameObject.Find("CineMachine VCam");
        // VCam = camObj.GetComponent<CinemachineVirtualCamera>();
        // VCam.ForceCameraPosition(startPos, startRot);
    }

    // handles players joining game
    private void OnEnable()
    {
        GameController.GameStarted      += OnGameStarted;
        PlayerController.PlayerCreated  += OnGameStarted;
    }

    private void OnGameStarted()
    {
        // add a brain to the main camera if it doesn't have one already
        Camera.main.gameObject.TryGetComponent<CinemachineBrain>(out var brain);
        if (brain == null) 
        {
            Camera.main.gameObject.AddComponent<CinemachineBrain>();
        }

        // get camera target struct
        CinemachineTargetGroup camTargetGroup = GameObject.Find("CineMachine Target Group").GetComponent<CinemachineTargetGroup>();

        // when the game starts, get all players
        players = GameObject.FindGameObjectsWithTag("Player");

        // add them to the cineCam target group target list
        foreach(GameObject player in players) {
            camTargetGroup.AddMember(player.transform, 1f, 0f);
        }

        // update camera settings to center on characters
        // VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 900;
        // VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.5f;
        // VCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.5f;
    }

    private void OnDisable()
    {
        GameController.GameStarted      -= OnGameStarted;
        PlayerController.PlayerCreated  -= OnGameStarted;
    }

}
