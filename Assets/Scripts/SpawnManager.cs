using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _whatToSpawn, _enemyContainer;

    [Header("Power Up Spawner")]
    [SerializeField] private GameObject[] _powerUp;

    private bool _stopSpawning = false;

    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        while (_stopSpawning == false)
        {
            Vector3 posToSpawn = new Vector3(Random.Range(-9f, 9f), 9f, 0);
            GameObject newEnemy = Instantiate(_whatToSpawn, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
        }
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        while (_stopSpawning == false)
        {
            Vector3 posToSpawnPowerUp = new Vector3(Random.Range(-8f, 8f), 9f, 0);
            GameObject newPowerUpTripleShot = Instantiate(_powerUp[Random.Range(0,3)], posToSpawnPowerUp, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(8f, 12f));
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }
}