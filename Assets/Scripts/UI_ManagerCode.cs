using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_ManagerCode : MonoBehaviour
{
    [SerializeField] private Text _scoreText, _gameOverText, _resetGameText;
    [SerializeField] private Sprite[] _liveSprites;
    [SerializeField] private Image _livesImage;

    [SerializeField] private Text ammoCountText;
    [SerializeField] private Text lowAmmoErrorText;

    [SerializeField] private Text _waveInfoText;
    [SerializeField] private Text _waveCenterText;

    void Start()
    {
        _scoreText.text = "Score:" + 0;
        _gameOverText.gameObject.SetActive(false);
        _resetGameText.gameObject.SetActive(false);

        if (lowAmmoErrorText != null)
        {
            lowAmmoErrorText.gameObject.SetActive(false);
        }

        if (_waveCenterText != null)
            _waveCenterText.gameObject.SetActive(false);

        if (_waveInfoText != null)
            _waveInfoText.text = "Wave: 1                  Enemies: 0/0";
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

    public void UpdateAmmo(int ammoCount, int maxAmmo)
    {
        if (ammoCountText != null)
        {
            ammoCountText.text = "Ammo: " + ammoCount + "/" + maxAmmo;
        }

        if (ammoCount <= 3)
        {
            ammoCountText.color = Color.yellow;
        }
        else
        {
            ammoCountText.color = Color.white;
        }
    }

    public void ShowOutOfAmmoWarning(bool state)
    {
        if (lowAmmoErrorText != null)
        {
            lowAmmoErrorText.gameObject.SetActive(state);
        }
    }

    public IEnumerator FlashOutOfAmmoWarning()
    {
        if (lowAmmoErrorText == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            lowAmmoErrorText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.25f);
            lowAmmoErrorText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void UpdateWaveInfo(int waveNumber, int enemiesAlive, int enemiesAtWaveStart)
    {
        _waveInfoText.text = $"Wave: {waveNumber}              Enemies: {enemiesAlive}/{enemiesAtWaveStart}";
    }

    public IEnumerator ShowCenterMessage(string message)
    {
        if (_waveCenterText == null) yield break;

        _waveCenterText.gameObject.SetActive(true);

        //Reset alpha and set text before rendering
        _waveCenterText.canvasRenderer.SetAlpha(0f);
        _waveCenterText.text = message;

        yield return null;

        //Fade In
        _waveCenterText.CrossFadeAlpha(1f, 0.75f, false);
        yield return new WaitForSeconds(2.5f);

        //Fade Out
        _waveCenterText.CrossFadeAlpha(0f, 0.75f, false);
        yield return new WaitForSeconds(1f);

        _waveCenterText.gameObject.SetActive(false);
    }
}