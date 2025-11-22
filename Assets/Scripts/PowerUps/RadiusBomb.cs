using UnityEngine;

public class RadiusBomb : MonoBehaviour
{
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _explosionRadius = 2.5f;
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private AudioClip _explosionSound;
    [SerializeField] private float _explosionLifeTime = 1.5f;

    private bool _hasExploded = false;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasExploded)
        {
            transform.Translate(Vector3.up * _speed * Time.deltaTime);

            if (transform.position.y > 8f)
            {
                Explode();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasExploded) return;

        if (other.CompareTag("Enemy"))
        {
            Explode();
        }  
    }

    void Explode()
    {
        _hasExploded = true;

        bool isVisible = GetComponent<Renderer>()?.isVisible ?? false;

        if (_explosionEffect != null)
        {
            GameObject explosion = Instantiate(_explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, _explosionLifeTime);
        }

        if (_explosionSound != null)
        {
            _audioSource.PlayOneShot(_explosionSound);
        }
        else
        {
            Debug.LogWarning("Explosion sound is missing");
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                Destroy(hit.gameObject);
            }
        }

        Destroy(gameObject, 0.2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
