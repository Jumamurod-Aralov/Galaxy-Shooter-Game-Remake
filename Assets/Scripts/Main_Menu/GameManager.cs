using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _gameOver = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _gameOver == true)
        {
            RestartGame();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Game is Quitting...");
            Application.Quit();
        }
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ReallyGameOver()
    {
        _gameOver = true;
    }
}