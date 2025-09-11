using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private GameObject _explosionUs;
    private SpawnManager _mainSpawner;
    private AudioSource _explosionAudio;

    private void Start()
    {
        _mainSpawner = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        if (_mainSpawner == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }

        _explosionAudio = GameObject.Find("DestroyExplosion").GetComponent<AudioSource>();
        if (_explosionAudio == null)
        {
            Debug.LogError("Explosion Audio is NULL");
        }
    }

    void Update()
    {
        transform.position += Vector3.down * 0.5f * Time.deltaTime;
        if (transform.position.y < -7f)
        {
            float randomX = Random.Range(-3f, 3f);
            transform.position = new Vector3(randomX, 9f, 0);
        }

        transform.Rotate(0f, 0f, 90f * Time.deltaTime, Space.Self);    
    }

    private void OnTriggerEnter2D(Collider2D enterOther)
    {
        if (enterOther.CompareTag("Laser"))
        {
            GameObject explosion = Instantiate(_explosionUs, transform.position, Quaternion.identity);
            Destroy(enterOther.gameObject);
            _mainSpawner.StartSpawning();

            Destroy(gameObject, 0.2f);
            _explosionAudio.Play();
        }
    }
}