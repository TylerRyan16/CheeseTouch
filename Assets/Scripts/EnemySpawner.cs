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
    public int currentEnemies;

    // Reference to the BoxCollider2D for the spawn area
    private BoxCollider2D spawnArea;

    // Start is called before the first frame update
    void Start()
    {
        // get spawn area
        spawnArea = GetComponent<BoxCollider2D>();

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (currentEnemies < maxEnemies)
        {
            SpawnEnemy();

            // wait for a random time between 4 and 8 seconds
            float spawnInterval = Random.Range(3f, 10f);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        // make sure enemy count is valid
        if (currentEnemies >= maxEnemies)
        {
            return;
        }

        // calculate the spawn area bounds based on the square (BoxCollider2D)
        Vector2 spawnPosition = GetRandomPositionInSpawnArea();

        // instantiate the enemy at the random position
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

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
}
