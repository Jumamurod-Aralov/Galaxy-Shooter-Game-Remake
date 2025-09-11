using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _speed = 5f, _fireRate = 0.5f, _canFire = -1f;
    [SerializeField] private GameObject _laserPrefab, _tripleLaser, _shieldVisual, _enemyLaser;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.0f, 0);
    [SerializeField] private int _lives = 3;
    
    [Header("Power-Up")]
    [SerializeField] private bool _is3xShotActive = false, _isSpeedBoostActive = false, _isShieldInAction = false;

    [SerializeField] private int _score;
    [SerializeField] private UI_ManagerCode _manageUI;
    [SerializeField] private GameManager _gameManager;

    [Header("Engine Damage Fire")]
    [SerializeField] private GameObject[] _engineFire;

    private AudioSource _laserAudioFX, _explosionAudio;
    private SpawnManager _spawnManager;
        
    void Start()
    {
        transform.position = new Vector3(0, -2f, 0);
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL!");
        }

        if (_manageUI == null)
        {
            Debug.LogError("The UI Manager is NULL!");
        }

        _laserAudioFX = GameObject.Find("LaserShootSound").GetComponent<AudioSource>();
        if (_laserAudioFX == null)
        {
            Debug.LogError("Laser Sound is NULL");
        }
    }

    void Update()
    {
        CalculateMovement();
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            FireLaser();
        }
    }

    void CalculateMovement ()
    {
        float horizontalInput = Input.GetAxis("Horizontal"), verticalInput = Input.GetAxis("Vertical");

        transform.Translate(Vector3.right * horizontalInput * _speed * Time.deltaTime);
        transform.Translate(Vector3.up * verticalInput * _speed * Time.deltaTime);

        if (transform.position.y >= 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y <= -3.9f)
        {
            transform.position = new Vector3(transform.position.x, -3.9f, 0);
        }

        if (transform.position.x > 10)
        {
            transform.position = new Vector3(-10, transform.position.y, 0);
        }
        else if (transform.position.x < -10)
        {
            transform.position = new Vector3(10, transform.position.y, 0);
        }
    }

    void FireLaser()
    {
        _canFire = Time.time + _fireRate;
        
        if (_is3xShotActive == true)
        {
            Instantiate(_tripleLaser, transform.position + _offset, Quaternion.identity);
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + _offset, Quaternion.identity);
        }

        _laserAudioFX.Play();
    }

    public void Damage()
    {
        if (_isShieldInAction == true)
        {
            _isShieldInAction = false;
            _shieldVisual.SetActive(false);
            Debug.Log("Shield is Deactivated!");
            return;
        }
        
        _lives -= 1;
        if (_lives == 2)
        {
            int engineNumber = _engineFire.Length;
            _engineFire[Random.Range(0, engineNumber)].SetActive(true);
        }
        if (_lives == 1)
        {
            _engineFire[0].SetActive(true);
            _engineFire[1].SetActive(true);
        }

        _manageUI.UpdateLives(_lives);
        Debug.Log("Lives : " + _lives);

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();    
            Debug.Log("Player is Dead!");

            _manageUI.GameOverTextOn();
            _gameManager.ReallyGameOver();

            Destroy(this.gameObject);
        }
    }

    public void TripleShotActive()
    {
        _is3xShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _is3xShotActive = false;
    }

    public void PlusSpeedActive()
    {
        _isSpeedBoostActive = true;
        _speed *= 2;
        StartCoroutine(PlusSpeedPowerDownRoutine());  
    }

    IEnumerator PlusSpeedPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpeedBoostActive = false;
        _speed /= 2;
    }

    public void GetShieldActive()
    {
        _isShieldInAction = true;
        _shieldVisual.SetActive(true);
        StartCoroutine(ShieldTurnOffRoutine());
    }

    IEnumerator ShieldTurnOffRoutine()
    {
        yield return new WaitForSeconds(8.0f);
        _isShieldInAction = false;
        _shieldVisual.SetActive(false);
        Debug.Log("Shield is Expired!");
    }

    public void AddToScore()
    {
        _score += 10;
        if (_manageUI != null)
        {
            _manageUI.UpdateScore(_score);
        }
    }
}