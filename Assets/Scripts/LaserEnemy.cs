using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnemy : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.down * 4f *  Time.deltaTime);
        if (transform.position.y < -5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D laserHit)
    {
        if (laserHit.tag == "Player")
        {
            Player player = laserHit.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
                Destroy(this.gameObject);
            }

        }
    }
}