using UnityEngine;
using System.Collections;

public class BossEnemyStandalone : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopYPosition = 3f;
    [SerializeField] private float spawnYPosition = 10f;
    
    [Header("Combat Settings")]
    [SerializeField] private int totalLives = 10;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float playerDetectionRadius = 8f;
    [SerializeField] private GameObject doubleLaserPrefab;

    [Header("Visual & Audio")]
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionAudioClip;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentLives;
    private bool hasStopped = false;
    private Transform playerTransform;
    private Vector3 spawnPos;

    private bool isDead = false;

    private SpawnManager _spawnManager;

    void Start()
    {
        currentLives = totalLives;
        spawnPos = new Vector3(0f, spawnYPosition, 0f);
        transform.position = spawnPos;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerTransform = playerGO.transform;

        StartCoroutine(ShootRoutine());
    }

    void Update()
    {
        if (isDead) return;

        MoveBoss();
        DetectAndRotateToPlayer();
    }

    private void MoveBoss()
    {
        if (!hasStopped)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, stopYPosition, 0f), moveSpeed * Time.deltaTime);

            if (transform.position.y <= stopYPosition)
            {
                hasStopped = true;
                transform.rotation = Quaternion.Euler(0f, 0f, 180f); // Face downward
            }
        }
    }

    private void DetectAndRotateToPlayer()
    {
        if (!hasStopped || playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= playerDetectionRadius)
        {
            Vector3 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitUntil(() => hasStopped);

        while (!isDead)
        {
            if (playerTransform != null && doubleLaserPrefab != null)
            {
                Vector3 shootOffset = transform.up * 1.5f;
                Instantiate(doubleLaserPrefab, transform.position + shootOffset, transform.rotation);
            }

            yield return new WaitForSeconds(fireInterval);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentLives -= amount;
        StartCoroutine(FlashDamage());

        if (currentLives <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (explosionAudioClip != null)
            AudioSource.PlayClipAtPoint(explosionAudioClip, Camera.main.transform.position);

        // Award player points
        Player player = playerTransform?.GetComponent<Player>();
        if (player != null)
            player.AddToScore(1000);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerLaser"))
        {
            Destroy(other.gameObject);
            TakeDamage(1);
        }

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            player?.Damage();
            TakeDamage(999); // Instant death if collides with player
        }
    }

    public void SetSpawnManager(SpawnManager sm)
    {
        _spawnManager = sm;
    }
}