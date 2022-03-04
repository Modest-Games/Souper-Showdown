using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public Transform gameOverText;

    private void OnGameStopped()
    {
        // enable the game over text
        gameOverText.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        GameController.GameStopped += OnGameStopped;
    }

    private void OnDisable()
    {
        GameController.GameStopped -= OnGameStopped;
    }
}
