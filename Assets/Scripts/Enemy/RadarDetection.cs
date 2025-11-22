using UnityEngine;

public class RadarDetection : MonoBehaviour
{
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private Enemy _enemyParent;

    void Awake()
    {
        if (_enemyParent == null)
            _enemyParent = GetComponentInParent<Enemy>();
    }

    // Called once when player enters radar zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;

        // Start ramming toward player
        _enemyParent?.StartRamming(other.transform);
    }

    // Called once when player exits radar zone
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(_playerTag)) return;

        // Stops ramming and resume normal movement
        _enemyParent?.StopRamming();
    }
}