using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // enemy prefab to spawn
    public GameObject enemyPrefab;

    // max num enemies to spawn
    public int maxEnemies = 10;

    // current amount of enemies alive
    private int currentEnemies = 0;
    private int enemiesSpawned = 0;

    // flag to check if the first zombie has spawned yet (win condition)
    private bool firstSpawned = false;

    // Reference to the BoxCollider2D for the spawn area
    private BoxCollider2D spawnArea;

    // Start is called before the first frame update
    void Start()
    {
        // get spawn area
        spawnArea = GetComponent<BoxCollider2D>();

        StartCoroutine(SpawnEnemies());
    }
    private void Update()
    {
        CheckWinCondition();
    }

    private IEnumerator SpawnEnemies()
    {

        while (enemiesSpawned < maxEnemies)
        {
            SpawnEnemy();

            // wait for a random time between 3 and 8 seconds
            float spawnInterval = Random.Range(2f, 6f);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemiesSpawned >= maxEnemies)
        {
            return;
        }
        // trigger first spawned zombie
        if (!firstSpawned)
        {
            firstSpawned = true;
        }

        // calculate the spawn area bounds based on the square (BoxCollider2D)
        Vector2 spawnPosition = GetRandomPositionInSpawnArea();

        // instantiate the enemy at the random position
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        enemiesSpawned++;
        currentEnemies++;
    }

    public Vector2 GetRandomPositionInSpawnArea()
    {
        // grab spawn area bounds
        Bounds bounds = spawnArea.bounds;

        // get random bounds in spawn area
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        // return random position
        return new Vector2(randomX, randomY);

    }

    public int GetCurrentEnemies()
    {
        return currentEnemies;
    }

    public int GetEnemiesSpawned()
    {
        return enemiesSpawned;
    }

    public void LowerCurrentEnemies()
    {
        currentEnemies = Mathf.Max(0, currentEnemies - 1); 
    }

    public void CheckWinCondition()
    {
        if (enemiesSpawned == maxEnemies && firstSpawned && currentEnemies == 0)
        {
            Debug.Log("You win!");
        }
    }
}
