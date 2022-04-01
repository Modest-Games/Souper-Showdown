using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerIndicator : NetworkBehaviour
{
    public Transform text;

    void Update()
    {
        transform.LookAt(transform.position + (Vector3.forward * 10) + (Vector3.down * 4));
    }

    private void OnPlayersListChanged()
    {
        int newNum = PlayersManager.Instance.GetPlayerIndex(NetworkObjectId);
        text.GetComponent<TMP_Text>().text = "P" + (newNum+1).ToString();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode sceneLoadMode)
    {
        if (scene.name == "InGame")
            gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // set up event listeners
        PlayersManager.PlayerListChanged += OnPlayersListChanged;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        // clear event listeners
        PlayersManager.PlayerListChanged -= OnPlayersListChanged;
    }
}
