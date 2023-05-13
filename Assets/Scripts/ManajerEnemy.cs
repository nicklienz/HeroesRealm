using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManajerEnemy : MonoBehaviour
{
    public static ManajerEnemy instance;
    public static ManajerEnemy Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerEnemy>();
            }
            
            return instance;
        }
    }
    // List of enemy prefabs
    public List<GameObject> enemyPrefabs;

    // List of spawn points
    public List<Transform> spawnPoints;

    // Spawn rate
    public float spawnRate = 2f;

    // Maximum number of enemies
    public int maxEnemies = 5;

    // Number of enemies spawned
    private int numEnemies = 0;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    // Spawn enemies coroutine
    private IEnumerator SpawnEnemies()
    {
        // Keep spawning enemies as long as the spawner is active
        while (true)
        {
            // Wait for the spawn rate
            yield return new WaitForSeconds(spawnRate);

            // Spawn an enemy if the maximum number of enemies has not been reached
            if (numEnemies < maxEnemies)
            {
                // Get a random enemy prefab
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

                // Get a random spawn point that doesn't have an enemy
                List<Transform> availableSpawnPoints = new List<Transform>();
                foreach (Transform spawnPoint in spawnPoints)
                {
                    Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, 0);
                    bool isAvailable = true;
                    foreach (Collider collider in colliders)
                    {
                        if (collider.tag == "Enemy")
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                    if (isAvailable)
                    {
                        availableSpawnPoints.Add(spawnPoint);
                    }
                }
                if (availableSpawnPoints.Count > 0)
                {
                    Transform spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

                    // Spawn the enemy
                    GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                    // Increase the number of enemies
                    numEnemies++;
                }
            }
        }
    }

    // Remove enemy from the count when it dies
    public void RemoveEnemy()
    {
        numEnemies--;
    }
}

