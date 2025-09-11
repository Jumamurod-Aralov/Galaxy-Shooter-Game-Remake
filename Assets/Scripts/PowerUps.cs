using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUps : MonoBehaviour
{
    [SerializeField] private float _speedDown = 1.0f;

    [Header("Power Up ID (0-1-2)")] //0-TripleShot //1-SpeedBoost //2-ShieldPowerUp
    [SerializeField] private int _powerUpID;

    private AudioSource _powerUpSound;

    void Update()
    {
        transform.Translate(Vector3.down * _speedDown * Time.deltaTime);
        if (transform.position.y < -5.5f)
        {
            Destroy(this.gameObject);
        }        

        _powerUpSound = GameObject.Find("PowerUpSound").GetComponent<AudioSource>();
        if (_powerUpSound == null)
        {
            Debug.LogError("PowerUp Sound is NULL");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                switch (_powerUpID)
                {
                    case 0:
                        player.TripleShotActive();
                        _powerUpSound.Play();
                        Debug.Log("Triple Shot Power-Up Collected by the Player!");
                        break;
                    case 1:
                        player.PlusSpeedActive();
                        _powerUpSound.Play();
                        Debug.Log("Speed Power-Up Collected by the Player!");
                        break;
                    case 2:
                        player.GetShieldActive();
                        _powerUpSound.Play();
                        Debug.Log("Shield Power-Up Collected by the Player!");
                        break;
                    default:
                        Debug.LogWarning("Unknown Power-Up ID");
                        break;
                }
            }
            Destroy(this.gameObject);
        }
    }
}