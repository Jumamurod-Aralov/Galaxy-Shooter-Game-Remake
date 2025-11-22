using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [SerializeField] private float _speedDown = 1f;
    [SerializeField] private float _magnetSpeed = 6f;

    [Header("PowerUp ID (0-8)")] //0-Triple //1-Speed //2-Shield //3-Ammo //4-Health //5-MultiShot //6-RadiusBomb //7-StopPowerUp //8-HomingPlayerLaser
    [SerializeField] private int _powerUpID;

    // Magnet Effect
    private static bool _isMagnetActive = false; // Static state: applies to ALL PowerUps

    // Cached Components
    private Transform _playerTransform;
    private AudioSource _powerUpSound;

    private void Start()
    {
        // Get Player Transform (used for magnet effect)
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            _playerTransform = playerGO.transform;
        }
        else
        {
            Debug.LogError("Player Transform is NULL in PowerUps");
        }

        GameObject soundObj = GameObject.Find("PowerUpSound");
        if (soundObj != null)
        {
            _powerUpSound = soundObj.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogError("PowerUpSound is NULL in PowerUps");
        }
    }

    void Update()
    {
        if (_isMagnetActive && _playerTransform != null)
        {
            // Vector3.MoveTowards is a great choice for controlled attraction
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, _magnetSpeed * Time.deltaTime);
        } 
        else
        {
            // Normal downward movement
            transform.Translate(Vector3.down * _speedDown * Time.deltaTime);
        }

        // Boundary check
        if (transform.position.y < -5.5f)
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            
            if (player != null)
            {
                // The switch statement is efficient for ID-based logic
                switch (_powerUpID)
                {
                    case 0: player.TripleShotActive(); break;
                    case 1: player.PlusSpeedActive(); break;
                    case 2: player.GetShieldActive(); break;
                    case 3: player.AddAmmo(); break;
                    case 4: player.HealthPlus(); break;
                    case 5: player.MultiShotActive(); break;
                    case 6: player.ActivateRadiusBomb(); break;
                    case 7: player.NegativePowerUp(); break;
                    case 8: player.ActivateHomingMode(); break;
                    default: Debug.LogWarning("Unknown Power-Up ID: " + _powerUpID); break;
                }
            }

            if (_powerUpSound != null)
            {
                _powerUpSound.PlayOneShot(_powerUpSound.clip); // Use PlayOneShot to avoid audio clipping issues
            }

            Destroy(this.gameObject);
        }

        // Check for collision with enemy laser to destroy power-up
        if (other.CompareTag("EnemyLaser"))
        {
            Destroy(this.gameObject);
            Destroy(other.gameObject);  // Destroy the enemy laser as well
        }
    }

    // Static method for player to toggle magnet
    public static void SetMagnetState(bool state)
    {
        _isMagnetActive = state;
    }
}