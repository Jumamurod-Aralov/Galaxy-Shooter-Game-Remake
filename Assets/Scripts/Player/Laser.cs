using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private int _laserSpeed = 8;
    private Vector3 _moveDirection;

    void Start()
    {
        _moveDirection = transform.up;
    }

    void Update()
    {
        LaserMovement();
    }

    void LaserMovement()
    {        
        transform.Translate(Vector3.up * _laserSpeed * Time.deltaTime, Space.Self);

        if (transform.position.y > 8f || transform.position.y < -8 || transform.position.x > 10 || transform.position.x < -10)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            Destroy(this.gameObject);
        }
    }
}