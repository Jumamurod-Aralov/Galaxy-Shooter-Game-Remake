using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _currentSpeed;

    [SerializeField] private float _sideAmplitude = 3f;
    [SerializeField] private float _sideFrequency = 3f;
    [SerializeField] private float _angleDirectionX = 1f;
    [SerializeField] private float _angleDirectionY = -1f;

    private enum MovementType { Straight, SideToSide, Angled }
    [SerializeField] private MovementType _movementType;

    private float _timeAlive = 0f;
    private Vector3 _spawnPos;

    [SerializeField] private float _rareMovementDuration = 10f;
    [SerializeField] private float _normalMovementDuration = 25f;
    private float _movementTimer = 0f;
    private bool _isRarePhase = false;

    [Header("Prefabs & FX")]
    [SerializeField] private GameObject _doubleLaserEnemy;
    [SerializeField] private AudioClip _explosionAudioClip;
    private Animator _explosionAnimation;

    [Header("UFO Settings")]
    [SerializeField] private bool _isUFO = false;
    [SerializeField] private int _UFOHealth = 2;
    private int _UFOCurrentHealth;
    [SerializeField] private GameObject _UFOProjectilePrefab;
    [SerializeField] private float _UFOShootInterval = 7f;
    private SpriteRenderer _UFOSpriteRenderer;

    [Header("Shield Settings")]
    [SerializeField] private bool _hasShield = false;
    [SerializeField] private GameObject _shieldVisual;

    [Header("Ramming Settings")]
    [SerializeField] private float _rammingSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _explodeDistance = 0.5f;
    private Transform _playerTarget;
    private bool _isRamming = false;

    [Header("Back Shooter Settings")]
    [SerializeField] private bool _isBackShooter = false;
    [SerializeField] private float _xAlignTolerance = 0.5f;
    private bool _isShootingBackward = false;

    [Header("Power-Up Detection")]
    [SerializeField] private float _powerUpDetectionRange = 3f;
    [SerializeField] private float _powerUpXAlignTolerance = 0.75f;
    private bool _isShootingPowerUp = false;

    [Header("Dodging Settings")]
    [SerializeField] private float _dodgeDetectionRange = 3.5f;
    [SerializeField] private float _dodgeSpeed = 4f;
    [SerializeField] private float _dodgeDuration = 0.4f;
    [SerializeField] private float _dodgeCooldown = 1.5f;
    private bool _isDodging = false;
    private float _nextDodgeTime = 0f;

    private Player _player;
    private SpawnManager _spawnManager;
    private Transform _ramTarget;

    public void Initialize()
    {
        _spawnPos = transform.position;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.GetComponent<Player>();

        _explosionAnimation = GetComponent<Animator>();

        if (_isUFO)
        {
            _UFOCurrentHealth = _UFOHealth;
            _UFOSpriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(UFOShootRoutine());
        }
        else
        {
            StartCoroutine(ShootRoutine());
        }

        float totalCycle = _normalMovementDuration + _rareMovementDuration;
        _movementTimer = Random.Range(0f, totalCycle);

        if (_movementTimer > _normalMovementDuration)
        {
            _isRarePhase = true;
            _movementType = (MovementType)Random.Range(1, System.Enum.GetValues(typeof(MovementType)).Length);
        }
        else
        {
            _isRarePhase = false;
            _movementType = MovementType.Straight;
        }

        if (_shieldVisual != null)
            _shieldVisual.SetActive(_hasShield);
    }

    public void SetSpawnManager(SpawnManager sm)
    {
        _spawnManager = sm;
    }

    public void SetSpeedRange(float min, float max)
    {
        _currentSpeed = Random.Range(min, max);
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (_isRamming && _playerTarget != null)
        {
            HandleRammingMovement();
            return;
        }

        if (!_isRamming && !_isShootingBackward && !_isShootingPowerUp)
            TryDodgePlayerLasers();

        if (!_isShootingBackward && _player != null && _isBackShooter)
        {
            float yDiff = _player.transform.position.y - transform.position.y;
            float xDiff = Mathf.Abs(_player.transform.position.x - transform.position.x);

            if (yDiff > 0.5f && xDiff <= _xAlignTolerance)
                StartCoroutine(ShootBackRoutine());
        }

        if (!_isRamming && !_isShootingBackward && !_isShootingPowerUp && !_isDodging)
        {
            UpdateMovementPhase();
            CalculateMovement();
        }

        if (!_isShootingPowerUp)
            DetectAndShootPowerUp();
    }

    private void UpdateMovementPhase()
    {
        _movementTimer += Time.deltaTime;

        if (_isRarePhase && _movementTimer > _rareMovementDuration)
        {
            _isRarePhase = false;
            _movementTimer = 0f;
            _movementType = MovementType.Straight;
        }
        else if (!_isRarePhase && _movementTimer > _normalMovementDuration)
        {
            _isRarePhase = true;
            _movementTimer = 0f;
            _movementType = (MovementType)Random.Range(1, System.Enum.GetValues(typeof(MovementType)).Length);
        }
    }

    private void CalculateMovement()
    {
        _timeAlive += Time.deltaTime;
        Vector3 pos = transform.position;

        switch (_movementType)
        {
            case MovementType.Straight:
                pos += Vector3.down * _currentSpeed * Time.deltaTime;
                break;

            case MovementType.SideToSide:
                pos.y -= _currentSpeed * Time.deltaTime;
                pos.x = _spawnPos.x + Mathf.Sin(_timeAlive * _sideFrequency) * _sideAmplitude;
                break;

            case MovementType.Angled:
                Vector3 dir = new Vector3(_angleDirectionX, _angleDirectionY, 0).normalized;
                pos += dir * _currentSpeed * Time.deltaTime;
                break;
        }

        transform.position = pos;

        if (transform.position.y < -5.5f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 9f, 0);
            _spawnPos = transform.position;
            _timeAlive = 0f;
        }
    }

    private void HandleRammingMovement()
    {
        if (_playerTarget == null) return;

        Vector3 direction = (_playerTarget.position - transform.position).normalized;
        transform.position += direction * _rammingSpeed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), _rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _playerTarget.position) <= _explodeDistance)
        {
            _playerTarget.GetComponent<Player>()?.Damage();
            _isRamming = false;
            WaitAnimation();
        }
    }

    private void TryDodgePlayerLasers()
    {
        if (_isDodging || Time.time < _nextDodgeTime) return;

        GameObject[] lasers = GameObject.FindGameObjectsWithTag("PlayerLaser");
        foreach (GameObject laser in lasers)
        {
            if (laser == null) continue;
            if (Vector3.Distance(transform.position, laser.transform.position) < _dodgeDetectionRange)
            {
                StartCoroutine(DodgeRoutine());
                break;
            }
        }
    }

    private IEnumerator DodgeRoutine()
    {
        _isDodging = true;
        _nextDodgeTime = Time.time + _dodgeCooldown;

        int dir = Random.value > 0.5f ? 1 : -1;
        float elapsed = 0f;
        while (elapsed < _dodgeDuration)
        {
            transform.Translate(Vector3.right * dir * _dodgeSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isDodging = false;
    }

    private void DetectAndShootPowerUp()
    {
        GameObject[] powerups = GameObject.FindGameObjectsWithTag("PowerUps");
        foreach (GameObject powerup in powerups)
        {
            if (powerup == null) continue;

            float yDiff = transform.position.y - powerup.transform.position.y;
            float xDiff = Mathf.Abs(transform.position.x - powerup.transform.position.x);

            if (yDiff > 0f && yDiff <= _powerUpDetectionRange && xDiff < _powerUpXAlignTolerance)
            {
                StartCoroutine(ShootPowerUpRoutine(powerup.transform));
                break;
            }
        }
    }

    private IEnumerator ShootPowerUpRoutine(Transform targetPowerUp)
    {
        _isShootingPowerUp = true;
        yield return new WaitForSeconds(0.2f);
        ShootLaser();
        yield return new WaitForSeconds(2f);
        _isShootingPowerUp = false;
    }

    private IEnumerator ShootBackRoutine()
    {
        _isShootingBackward = true;
        transform.rotation = Quaternion.Euler(0, 0, 180f);
        FireBackward();
        yield return new WaitForSeconds(1.5f);
        transform.rotation = Quaternion.Euler(0, 0, 0f);
        _isShootingBackward = false;
    }

    private IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            ShootLaser();
        }
    }

    private IEnumerator UFOShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_UFOShootInterval);
            UFOShootLaser();
        }
    }

    private void ShootLaser()
    {
        if (_doubleLaserEnemy == null) return;
        Instantiate(_doubleLaserEnemy, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }

    private void FireBackward()
    {
        if (_doubleLaserEnemy == null) return;
        GameObject shot = Instantiate(_doubleLaserEnemy, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        shot.transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    private void UFOShootLaser()
    {
        if (_UFOProjectilePrefab == null) return;
        GameObject shot = Instantiate(_UFOProjectilePrefab, transform.position + Vector3.down, Quaternion.identity);
        shot.GetComponent<LaserEnemy>()?.InitializeUFOProjectile(Vector3.down, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLaser"))
        {
            Destroy(other.gameObject);
            if (_hasShield)
            {
                _hasShield = false;
                _shieldVisual?.SetActive(false);
                return;
            }
            Damage(1);
        }

        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>()?.Damage();
            WaitAnimation();
        }
    }

    private void Damage(int amount)
    {
        if (_isUFO)
        {
            _UFOCurrentHealth -= amount;
            if (_UFOCurrentHealth <= 0)
            {
                _player?.AddToScore(10);
                WaitAnimation();
            }
            return;
        }

        _player?.AddToScore(10);
        WaitAnimation();
    }

    public void WaitAnimation()
    {
        _explosionAnimation?.SetTrigger("OnEnemyDeath");
        _shieldVisual?.SetActive(false);
        Destroy(gameObject, 2.5f);
        if (_explosionAudioClip != null)
            AudioSource.PlayClipAtPoint(_explosionAudioClip, Camera.main.transform.position);
    }

    public void StartRamming(Transform target)
    {
        _ramTarget = target;
        _playerTarget = target;
        _isRamming = true;
    }

    public void StopRamming()
    {
        _ramTarget = null;
        _playerTarget = null;
        _isRamming = false;
    }

    private void OnDestroy()
    {
        _spawnManager?.OnEnemyKilled();
    }

    public void SetShield(bool value)
    {
        _hasShield = value;

        if (_shieldVisual != null)
            _shieldVisual.SetActive(_hasShield);
    }
}