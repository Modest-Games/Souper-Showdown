using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

public class GetPlayersOnStart : MonoBehaviour
{
    private GameObject[] players;

    private void OnEnable()
    {
        GameController.GameStarted      += OnGameStarted;
    }

    private void OnGameStarted()
    {   
        // get camera target struct
        CinemachineTargetGroup camTargetGroup = GameObject.Find("CamTargetGroup").GetComponent<CinemachineTargetGroup>();

        // when the game starts, get all players 
        players = GameObject.FindGameObjectsWithTag("Player");
        
        // add them to the cineCam target group target list
        foreach(GameObject player in players) {
            camTargetGroup.AddMember(player.transform, 1f, 0f);
            Debug.Log("Start players " + player);
        }

    }

    private void OnDisable() 
    {
        GameController.GameStarted      -= OnGameStarted;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        // 
        //CinemachineTargetGroup cinemachineTargetGroup

        //GameObject camTargetGroup = GameObject.Find("CM vcam1").GetComponent<CinemachineTargetGroup>();

        //Debug.Log(camTargetGroup);
    }
}
