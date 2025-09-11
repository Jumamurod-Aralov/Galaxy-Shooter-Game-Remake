using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_ManagerCode : MonoBehaviour
{
    [SerializeField] private Text _scoreText, _gameOverText, _resetGameText;
    [SerializeField] private Sprite[] _liveSprites;
    [SerializeField] private Image _livesImage;
    
    void Start()
    {
        _scoreText.text = "Score:" + 0;
        _gameOverText.gameObject.SetActive(false);
    }

    public void UpdateScore(int newScore)
    {
        _scoreText.text = "Score: " + newScore;
    }

    public void UpdateLives(int currentLives)
    {
        if (currentLives < 0 || currentLives > _liveSprites.Length) { return; }
        _livesImage.sprite = _liveSprites[currentLives];
    }

    public void GameOverTextOn()
    {
        _resetGameText.gameObject.SetActive(true);
        StartCoroutine(FlickerTextRoutine());
    }

    IEnumerator FlickerTextRoutine()
    {
        while(true)
        {
            _gameOverText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            _gameOverText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
        }
    }
}