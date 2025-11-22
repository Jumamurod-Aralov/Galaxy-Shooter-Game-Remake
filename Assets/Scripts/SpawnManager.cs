using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _whatToSpawn;
    [SerializeField] private GameObject _enemyContainer;

    [Header("Power Up Spawner")]
    [SerializeField] private GameObject[] _powerUp;

    private float _multiShotCoolDown = 25f, _nextMultiShotTime = 0f;
    private float _radiusBombCoolDown = 20f, _nextRadiusBombTime = 0f;

    [Header("Enemy Speed Range (per wave)")]
    [SerializeField] private float _baseEnemySpeed = 0.5f;
    [SerializeField] private float _maxEnemySpeed = 1.5f;
    [SerializeField] private float _speedIncreasePerWave = 0.25f;

    private int _currentWave = 1;
    private int _enemiesToSpawn = 16;
    private int _enemiesAlive;
    private int _enemiesAtWaveStart;

    [SerializeField] private UI_ManagerCode _uiManager;
    private bool _stopSpawning = false;

    [Header("UFO Enemy Settings")]
    [SerializeField] private GameObject _ufoEnemyPrefab;
    [SerializeField][Range(0f, 1f)] private float _ufoSpawnChance = 0.2f;

    [Header("Spawn Control")]
    [SerializeField] private int _maxEnemiesOnScreen = 8;
    [SerializeField] private Vector2 _enemySpawnDelayRange = new Vector2(5f, 7f);

    [Header("BOSS Settings")]
    [SerializeField] private GameObject _bossPrefab;
    private bool _isBossAlive = false;

    public void StartSpawning()
    {
        StartCoroutine(WaveAndBossCycleRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator WaveAndBossCycleRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (!_stopSpawning)
        {
            if (_uiManager != null)
                StartCoroutine(_uiManager.ShowCenterMessage($"Wave {_currentWave} Starting"));

            yield return new WaitForSeconds(3f);

            _enemiesAtWaveStart = _enemiesToSpawn;
            _enemiesAlive = _enemiesToSpawn;
            _uiManager?.UpdateWaveInfo(_currentWave, _enemiesAlive, _enemiesAtWaveStart);

            for (int i = 0; i < _enemiesToSpawn; i++)
            {
                if (_stopSpawning || _enemyContainer == null) yield break;

                while (_enemyContainer != null && _enemyContainer.transform.childCount >= _maxEnemiesOnScreen)
                    yield return new WaitForSeconds(1f);

                Vector3 posToSpawn = new Vector3(Random.Range(-9f, 9f), 9f, 0);
                GameObject prefabToSpawn = Random.value <= _ufoSpawnChance ? _ufoEnemyPrefab : _whatToSpawn;
                GameObject newEnemy = Instantiate(prefabToSpawn, posToSpawn, Quaternion.identity);
                newEnemy.transform.parent = _enemyContainer.transform;

                Enemy enemy = newEnemy.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Initialize();
                    enemy.SetSpeedRange(_baseEnemySpeed, _maxEnemySpeed);
                    enemy.SetSpawnManager(this);

                    bool giveShield = Random.value <= 0.25f;
                    enemy.SetShield(giveShield);
                }

                yield return new WaitForSeconds(Random.Range(_enemySpawnDelayRange.x, _enemySpawnDelayRange.y));
            }

            yield return new WaitUntil(() => _enemiesAlive <= 0);
            StartCoroutine(_uiManager?.ShowCenterMessage($"Wave {_currentWave} Cleared"));

            yield return new WaitForSeconds(3f);

            // BOSS WAVE
            Debug.Log("BOSS WAVE COMMENCING");
            StartCoroutine(_uiManager?.ShowCenterMessage("BOSS INCOMING! Prepare for Battle!"));
            yield return new WaitForSeconds(2f);
            SpawnBoss();
            yield return new WaitUntil(() => !_isBossAlive);
            StartCoroutine(_uiManager?.ShowCenterMessage("BOSS DEFEATED! Victory!"));

            yield return new WaitForSeconds(5f);

            _currentWave++;
            _enemiesToSpawn = Mathf.Min(_enemiesToSpawn * 2, 256);
            _baseEnemySpeed += _speedIncreasePerWave;
            _maxEnemySpeed = Mathf.Min(_maxEnemySpeed + _speedIncreasePerWave, 6f);
            yield return new WaitForSeconds(3f);
        }
    }

    void SpawnBoss()
    {
        if (_bossPrefab == null) { Debug.LogError("Boss Prefab is not assigned!"); return; }

        Vector3 posToSpawn = new Vector3(0, 8f, 0);
        GameObject bossObject = Instantiate(_bossPrefab, posToSpawn, Quaternion.identity);
        bossObject.transform.parent = _enemyContainer.transform;

        BossEnemyStandalone boss = bossObject.GetComponent<BossEnemyStandalone>();
        if (boss != null)
        {
            boss.SetSpawnManager(this);
            _isBossAlive = true;
        }
        else
        {
            Debug.LogError("Boss prefab missing BossEnemyStandalone component!");
            Destroy(bossObject);
        }
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(3f);
        while (!_stopSpawning)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f), 9f, 0);
            GameObject prefabToSpawn = _powerUp[Random.Range(0, _powerUp.Length)];

            // Handle cooldowns
            if (prefabToSpawn == _powerUp[5] && Time.time < _nextMultiShotTime) prefabToSpawn = null;
            if (prefabToSpawn == _powerUp[6] && Time.time < _nextRadiusBombTime) prefabToSpawn = null;

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, pos, Quaternion.identity);
                if (prefabToSpawn == _powerUp[5]) _nextMultiShotTime = Time.time + _multiShotCoolDown;
                if (prefabToSpawn == _powerUp[6]) _nextRadiusBombTime = Time.time + _radiusBombCoolDown;
            }

            yield return new WaitForSeconds(Random.Range(6f, 10f));
        }
    }

    public void OnPlayerDeath() => _stopSpawning = true;
    public void OnEnemyKilled()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        _uiManager?.UpdateWaveInfo(_currentWave, _enemiesAlive, _enemiesAtWaveStart);
    }
    public void OnBossDeath()
    {
        _isBossAlive = false;
        Debug.Log("Boss defeated.");
    }
}
