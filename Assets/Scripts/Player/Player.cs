using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _fireRate = 0.5f, _canFire = -1f;
    [SerializeField] private float _speed, _normalSpeed = 5f, _shiftSpeed = 1.8f;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.0f, 0);
    [SerializeField] private int _lives = 3, _shieldCount = 3;

    [Header("Prefabs")]
    [SerializeField] private GameObject _laserPrefab, _tripleLaser, _homingLaserPrefab;
    [SerializeField] private GameObject _secondLaser, _bombPrefab;
    [SerializeField] private GameObject _shieldVisual;

    [Header("Power-Up Status")]
    [SerializeField] private bool _is3xShotActive = false;
    [SerializeField] private bool _isShieldInAction = false;
    [SerializeField] private bool _isMultiShotActive = false;
    [SerializeField] private bool _isBombActive = false;
    [SerializeField] private bool _isHomingActive = false;
    private bool _isFrozen = false;  // Player State

    [Header("Power-Up Durations")]
    [SerializeField] private float _multiShotDuration = 5f;
    [SerializeField] private float _bombDuration = 10f;
    [SerializeField] private float _homingDuration = 8f;

    [Header("Score & UI")]
    [SerializeField] private int _score;
    [SerializeField] private UI_ManagerCode _manageUI;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameObject[] _engineFire; // Engine Damage Fire

    [Header("Thruster / Boost System")]
    [SerializeField] private float _boostCharge = 6f;
    [SerializeField] private float _boostCooldown = 5f;
    [SerializeField] private Slider _boostSlider;
    private float _currentBoost;
    private bool _isCoolingDown = false;

    [Header("Magnet Settings")]
    [SerializeField] private float _magnetDuration = 5f;
    [SerializeField] private float _magnetCooldown = 10f;
    private bool _isMagnetActive = false;
    private bool _isOnCoolDown = false;

    [Header("Ammo Settings")]
    [SerializeField] private int _maxAmmo = 15, _ammoCount;

    // Cached Components and Coroutines
    private AudioSource _laserAudioFX;
    private SpawnManager _spawnManager;
    private SpriteRenderer _shieldRenderer;
    private Coroutine _shieldCoroutine;
    private Coroutine _tripleShotCoroutine;
    private Coroutine _speedBoostCoroutine;
    private Coroutine _multiShotCoroutine;
    private Coroutine _bombCoroutine;
    private Coroutine _homingCoroutine;

    void Start()
    {
        // Initial setup
        transform.position = new Vector3(0, -2f, 0);

        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        if (_spawnManager == null) { Debug.LogError("The Spawn Manager is NULL!"); }

        _manageUI = GameObject.Find("Canvas").GetComponent<UI_ManagerCode>();
        if (_manageUI == null) { Debug.LogError("The UI Manager is NULL!"); }

        _laserAudioFX = GameObject.Find("LaserShootSound").GetComponent<AudioSource>();
        if (_laserAudioFX == null) { Debug.LogError("Laser Sound is NULL"); }

        // Boost Init
        _currentBoost = _boostCharge;
        _boostSlider.maxValue = _boostCharge;
        _boostSlider.value = _currentBoost;

        // Shield Renderer Caching
        Transform shieldData = transform.Find("Shield");
        if (shieldData != null)
        {
            _shieldRenderer = shieldData.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Shield child object not found");
        }

        // Ammo Init
        _ammoCount = _maxAmmo;
        _manageUI.UpdateAmmo(_ammoCount, _maxAmmo);
    }

    void Update()
    {
        CalculateMovement();

        // Firing Input
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            if (_ammoCount > 0)
            {
                FireLaser();
                _manageUI.ShowOutOfAmmoWarning(false);
            }
            else
            { 
                StartCoroutine(_manageUI.FlashOutOfAmmoWarning()); 
            }
        }

        // Magnet Input
        if (Input.GetKey(KeyCode.C) && !_isOnCoolDown)
        {
            if (!_isMagnetActive)
            {
                StartCoroutine(ActivateMagnet());
            }
        }

        // UI Updates
        _boostSlider.value = _currentBoost;
        ChangeSliderColor();
    }

    void CalculateMovement()
    {
        if (_isFrozen) return;

        float horizontalInput = Input.GetAxis("Horizontal"); 
        float verticalInput = Input.GetAxis("Vertical");

        // BOOST DRAIN
        if (Input.GetKey(KeyCode.LeftShift) && !_isCoolingDown && _currentBoost > 0)
        {
            _speed = _normalSpeed * _shiftSpeed;

            // Drain boost, use Mathf.Max to prevent negative values
            _currentBoost = Mathf.Max(0, _currentBoost - Time.deltaTime);
        }
        // BOOST RECHARGE
        else
        {
            // Only reset speed if boost is currently active speed
            if (_speed != _normalSpeed && Input.GetKeyUp(KeyCode.LeftShift))
            {
                _speed = _normalSpeed;
            }

            if (!_isCoolingDown && _currentBoost < _boostCharge)
            {
                // Recharge boost, use Mathf.Min to prevent overcharge
                _currentBoost = Mathf.Min(_boostCharge, _currentBoost + Time.deltaTime);
            }
        }

        // OVERHEAT / COOLDOWN CHECK
        if (_currentBoost <= 0 && !_isCoolingDown)
        {
            _speed = _normalSpeed; // Ensure speed resets if boost runs out
            _isCoolingDown = true;
            StartCoroutine(CoolDownRoutine());
        }

        // Apply movement
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
        transform.Translate(direction * _speed * Time.deltaTime);

        // Clamping
        float clampedX = transform.position.x;
        float clampedY = Mathf.Clamp(transform.position.y, -3.9f, 2f);

        if (clampedX > 10) clampedX = -10;
        else if (clampedX < -10) clampedX = 10;

        transform.position = new Vector3(clampedX, clampedY, 0);
    }

    IEnumerator CoolDownRoutine()
    {
        // Wait for the required cooldown duration
        yield return new WaitForSeconds(_boostCooldown);

        // Linear Recharge Phase
        float elapsed = 0f;
        float rechargeDuration = 2f; // Time for the bar to fill up

        while (elapsed < rechargeDuration)
        {
            elapsed += Time.deltaTime;
            _currentBoost = Mathf.Lerp(0, _boostCharge, elapsed / rechargeDuration);
            _boostSlider.value = _currentBoost;
            yield return null;
        }

        // Reset state
        _currentBoost = _boostCharge;
        _boostSlider.value = _boostCharge;
        _isCoolingDown = false;
    }

    void FireLaser()
    {
        // Ammo check
        if (_ammoCount <= 0)
        {
            _manageUI.ShowOutOfAmmoWarning(true);
            return;
        }

        // Reset fire rate timer
        _canFire = Time.time + _fireRate;

        int ammoUsed = 0;
        // Determine base layer types (Standard or Homing)
        GameObject standardLaserPrefab = _isHomingActive ? _homingLaserPrefab : _laserPrefab;

        // Firing Priority
        if (_isBombActive)
        {
            Instantiate(_bombPrefab, transform.position + new Vector3(0, 0.7f, 0), Quaternion.identity);
            ammoUsed = 0; // Bomb is free
        }
        else if (_isMultiShotActive && _ammoCount >= 5)
        {
            MultiAngleShotLaser(standardLaserPrefab);
            ammoUsed = 5;
        }
        else if (_is3xShotActive && _ammoCount >= 3)
        {
            Instantiate(_tripleLaser, transform.position + _offset, Quaternion.identity);
            ammoUsed = 3;
        }
        else // Base or Homing Shot
        {
            Instantiate(standardLaserPrefab, transform.position + _offset, Quaternion.identity);
            ammoUsed = 1;
        }

        // Apply Ammp Change & UI Update
        _ammoCount -= ammoUsed;
        _manageUI.UpdateAmmo(_ammoCount, _maxAmmo);

        if (_ammoCount <= 0)
        {
            _manageUI.ShowOutOfAmmoWarning(true);
        }

        _laserAudioFX.Play();
    }

    public void MultiAngleShotLaser(GameObject prefabToUse)
    {
        float[] angles = { -45f, -15f, 0f, 15f, 45f };

        foreach (float angle in angles)
        {
            Quaternion beamRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            
            // Calculate spawn position relative to the player's forward direction
            Vector3 rotatedOffset = beamRotation * _offset;
            Vector3 spawnPos = transform.position + rotatedOffset;

            Instantiate(prefabToUse, spawnPos, beamRotation);
        }
    }

    public void GetShieldActive()
    {
        // Stop any currently running routine to prevent unexpected turn-off
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
        }
        _isShieldInAction = true;
        _shieldVisual.SetActive(true);
        _shieldCount = 3;
        _shieldRenderer.color = Color.white;

        // Start new routine for turn-off
        _shieldCoroutine = StartCoroutine(ShieldTurnOffRoutine());
    }

    IEnumerator ShieldTurnOffRoutine()
    {
        yield return new WaitForSeconds(10f);
        _isShieldInAction = false;
        _shieldVisual.SetActive(false);
        _shieldCoroutine = null;
    }

    public void Damage()
    {
        if (_isShieldInAction)
        {
            _shieldCount -= 1;

            if (_shieldCount > 0)
            {
                GetShieldColorValue();
            }
            else
            {
                _shieldCount = 0;
                _isShieldInAction = false;
                _shieldVisual.SetActive(false);
            }
            return;
        }

        _lives -= 1;

        CameraShake shake = Camera.main.GetComponent<CameraShake>();
        if (shake != null)
        {
            StartCoroutine(shake.Shake(0.4f, 0.25f));
        }

        if (_lives == 2)
        {
            _engineFire[UnityEngine.Random.Range(0, _engineFire.Length)].SetActive(true);
        }
        if (_lives == 1)
        {
            _engineFire[0].SetActive(true);
            _engineFire[1].SetActive(true);
        }

        _manageUI.UpdateLives(_lives);

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();
            _manageUI.GameOverTextOn();
            _gameManager.ReallyGameOver();
            Destroy(this.gameObject);
        }
    }

    // POWER UP ACTIVATION
    public void TripleShotActive()
    {
        if (_tripleShotCoroutine != null) StopCoroutine(_tripleShotCoroutine);
        _is3xShotActive = true;
        _tripleShotCoroutine = StartCoroutine(PowerDownRoutine(5f, () => _is3xShotActive = false));
    }

    public void PlusSpeedActive()
    {
        if (_speedBoostCoroutine != null) StopCoroutine(_speedBoostCoroutine);
        _normalSpeed = 10f;
        _speedBoostCoroutine = StartCoroutine(PowerDownRoutine(5f, () => { _normalSpeed = 5f; }));
    }

    public void MultiShotActive()
    {
        if (_multiShotCoroutine != null) StopCoroutine(_multiShotCoroutine);
        _isMultiShotActive = true;
        _multiShotCoroutine = StartCoroutine(PowerDownRoutine(_multiShotDuration, () => _isMultiShotActive = false));
    }

    public void ActivateRadiusBomb()
    {
        if (_bombCoroutine != null) StopCoroutine(_bombCoroutine);
        _isBombActive = true;
        _bombCoroutine = StartCoroutine(PowerDownRoutine(_bombDuration, () => _isBombActive = false));
    }
    public void ActivateHomingMode()
    {
        if (_homingCoroutine != null) StopCoroutine(_homingCoroutine);
        _isHomingActive = true;
        _homingCoroutine = StartCoroutine(PowerDownRoutine(_homingDuration, () => _isHomingActive = false));
    }

    // Generic Coroutine for PowerDowns
    IEnumerator PowerDownRoutine(float duration, System.Action onPowerDown)
    {
        yield return new WaitForSeconds(duration);
        onPowerDown.Invoke();
    }

    // Magnet Coroutine 
    IEnumerator ActivateMagnet()
    {
        _isMagnetActive = true;
        _isOnCoolDown = true;

        PowerUps.SetMagnetState(true);
        yield return new WaitForSeconds(_magnetDuration);

        PowerUps.SetMagnetState(false); // Stop magnet effect
        _isMagnetActive = false;

        // Wait before next activation
        yield return new WaitForSeconds(_magnetCooldown);
        _isOnCoolDown = false;
    }

    // Utility Methods
    public void AddToScore(int scoreValue)
    {
        _score += scoreValue;
        if (_manageUI != null)
        {
            _manageUI.UpdateScore(_score);
        }
    }

    public void ChangeSliderColor()
    {
        Image fill = _boostSlider.fillRect.GetComponent<Image>();

        if (_isCoolingDown)
        {
            fill.color = Color.red;
        }
        else if (_currentBoost < _boostCharge * 0.3f)
        {
            fill.color = Color.yellow;
        }
        else
        {
            fill.color = Color.green;
        }
    }

    public void AddAmmo()
    {
        _ammoCount = _maxAmmo;
        _manageUI.UpdateAmmo(_ammoCount, _maxAmmo);
        _manageUI.ShowOutOfAmmoWarning(false);
    }

    public void HealthPlus()
    {
        if (_lives < 3)
        {
            _lives++;
        }

        if (_lives == 3)
        {
            _engineFire[0].SetActive(false);
            _engineFire[1].SetActive(false);
        }
        else if (_lives == 2)
        {
            _engineFire[UnityEngine.Random.Range(0, _engineFire.Length)].SetActive(false);
        }

        _manageUI.UpdateLives(_lives);
    }

    public void NegativePowerUp()
    {
        if (!_isFrozen)
        {
            StartCoroutine(NegativePowerUpRoutine(3f)); //Freeze for 3 seconds
        }
    }

    IEnumerator NegativePowerUpRoutine(float duration)
    {
        _isFrozen = true;
        Debug.Log("Player frozen for " + duration + " seconds!");

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.color = Color.cyan;

        yield return new WaitForSeconds(duration);

        _isFrozen = false;
        Debug.Log("Player Unfrozen.");

        if (sprite != null)
            sprite.color = Color.white;
    }
    public void GetShieldColorValue()
    {
        if (_shieldCount == 2)
        {
            _shieldRenderer.color = Color.yellow;
        }
        else if (_shieldCount == 1)
        {
            _shieldRenderer.color = Color.red;
        }
    }
}