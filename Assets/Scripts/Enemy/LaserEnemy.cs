using UnityEngine;

public class LaserEnemy : MonoBehaviour
{
    public enum TargetType { None, PlayerLaser, Player }

    [Header("Projectile Settings")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotateSpeed = 200f;
    [SerializeField] private float _detectRadius = 6f;

    private Rigidbody2D _rb;
    private bool _isUFOProjectile = false;
    private GameObject _UFOOwner;
    private Transform _target;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_isUFOProjectile)
        {
            SeekNearestPlayerLaser();
        }
        else
        {
            _rb.velocity = -transform.up * _speed;
        }

        if (transform.position.y < -5f)
        {
            Destroy(this.gameObject);
        }
    }

    //UFO Homing Logic
    void SeekNearestPlayerLaser()
    {
        GameObject[] playerLasers = GameObject.FindGameObjectsWithTag("PlayerLaser");
        if (playerLasers.Length == 0)
        {
            _rb.velocity = Vector2.down * _speed;
            return;
        }

        // Find the nearest player laser
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var laser in playerLasers)
        {
            float dist = Vector2.Distance(transform.position, laser.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = laser;
            }
        }

        if (nearest != null && minDistance <= _detectRadius)
        {
            Vector2 direction = ((Vector2)nearest.transform.position - _rb.position).normalized;

            float rotateAmount = Vector3.Cross(transform.up, direction).z;  //up to down
            _rb.angularVelocity = rotateAmount * _rotateSpeed;

            // Move forward in current facing direction
            _rb.velocity = transform.up * _speed;  //different direction        transform.translate
        }
        else
        {
            _rb.angularVelocity = 0f;
            _rb.velocity = Vector2.down * _speed;
        }
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.tag == "Player")
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
                player.Damage();
            
            
            Destroy(this.gameObject);
        }
        else if (hit.tag == "PlayerLaser")
        {
            Destroy(hit.gameObject);
            Destroy(gameObject);
        }
    }

    public void InitializeUFOProjectile(Vector3 direction, GameObject shooter)
    {
        _isUFOProjectile = true;
        _UFOOwner = shooter;
        _rb.velocity = direction.normalized * _speed;
        transform.up = -direction.normalized;
    }
}