using System.Collections;
using System.Linq;  
using UnityEngine;

public class HomingLaser : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 6f;               // Forward speed
    [SerializeField] private float _detectionRange = 8f;      // How far to search for targets
    [SerializeField] private float _rotateSpeedDeg = 360f;    // Degrees per second

    [Header("Targeting")]
    [SerializeField] private string[] _targetTags = new string[] { "Enemy", "Enemy_UFO" };
    [SerializeField] private float _targetLostRangeFactor = 1.5f;

    [Header("Lifetime & Recheck")]
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private float _recheckInterval = 0.25f;

    private Transform _target;
    private float _squareDetectionRange;

    void Start()
    {
        // Cache squared detection range for faster comparison
        _squareDetectionRange = _detectionRange * _detectionRange;

        // Start looking for targets
        StartCoroutine(TargetSeekRoutine());
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        // Always move forward regardless of target status
        MoveForward();

        if (_target == null)
            return;

        // Check if the target was destroyed externally
        if (_target.gameObject == null)
        {
            _target = null;
            return;
        }

        Vector2 toTarget = _target.position - transform.position;

        // Check if the target has moved too far away (Lost Check)
        if (toTarget.sqrMagnitude > _squareDetectionRange * _targetLostRangeFactor)
        {
            _target = null;  // Target lost, search again next interval
            return;
        }

        // Calculate rotation toward target
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;

        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _rotateSpeedDeg * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    private void MoveForward()
    {
        // Use transform.up for movement, which respects the rotation
        transform.position += transform.up * _speed * Time.deltaTime;
    }

    IEnumerator TargetSeekRoutine()
    {
        while (true)
        {
            if (_target == null)
                FindClosestTarget();

            yield return new WaitForSeconds(_recheckInterval);
        }
    }

    void FindClosestTarget()
    {
        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        // Optimized search - Find all target objects once
        var allTargets = _targetTags.SelectMany(tag => GameObject.FindGameObjectsWithTag(tag)).ToList();

        foreach (var close in allTargets)
        {
            if (close == null) continue;

            float distSqr = (currentPos - close.transform.position).sqrMagnitude; // Use square magnitude for performance

            if (distSqr < closestDistSqr && distSqr <= _squareDetectionRange)
            {
                closestDistSqr = distSqr;
                closest = close.transform;
            }
        }

        _target = closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object's tag is one of our targets
        if (_targetTags.Contains(other.tag))
        {
            // Apply Damage Logic

            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.WaitAnimation();
            }
            else
            {
                Destroy(other.gameObject);
            }

            // Destroy the laser itself on hit
            Destroy(gameObject);
        }
        
    }
}