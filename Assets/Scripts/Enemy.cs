using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _enemySpeed = 1.5f;
    [SerializeField] private GameObject _doubleLaserEnemy;
    private Player _player;
    private Animator _explosionAnimation;
    private AudioSource _explosionAudio;
    private bool _enemyDestroyedStopShooting = false;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Player - NULL");
        }

        _explosionAnimation = GetComponent<Animator>();
        if (_explosionAnimation == null)
        {
            Debug.LogError("Animator - NULL");
        }

        _explosionAudio = GameObject.Find("DestroyExplosion").GetComponent<AudioSource>();
        if (_explosionAudio == null)
        {
            Debug.LogError("AudioSource - NULL");
        }

        StartCoroutine(ShootRoutine());
    }

    void Update()
    {
        CalculateMovement();
    }

    void CalculateMovement()
    {
        transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime);

        if (transform.position.y < -5.5f)
        {
            float randomX = Random.Range(-8f, 8f);
            transform.position = new Vector3(randomX, 9f, 0);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }
            WaitAnimation();
        }

        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);
            if (_player != null)
            {
                _player.AddToScore();
            }
            WaitAnimation();
        }
    }

    void WaitAnimation()
    {
        _enemyDestroyedStopShooting = true;
        WaitAnimThenDestroy();
        _explosionAudio.Play();
    }

    void WaitAnimThenDestroy()
    {
        _explosionAnimation.SetTrigger("OnEnemyDeath");
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        float secForAnim = 158 / 60f;
        yield return new WaitForSeconds(secForAnim);
        Destroy(this.gameObject);
    }

    void ShootLaser()
    {
        Vector3 _shootOffset = new Vector3(0, 0.5f, 0);
        GameObject laser = Instantiate(_doubleLaserEnemy, transform.position + _shootOffset, Quaternion.identity);
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (_enemyDestroyedStopShooting == false)
            {
                ShootLaser();
            }
            break;
        }
    }
}