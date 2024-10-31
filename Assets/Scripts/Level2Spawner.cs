using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Spawner : MonoBehaviour
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
    public GameObject spawnObject1;
    public GameObject spawnObject2;
    private BoxCollider2D spawnArea1;
    private BoxCollider2D spawnArea2;


    private PauseMenu pauseMenu;

    public GameObject nextLevelIndicator;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        spawnArea1 = spawnObject1.GetComponent<BoxCollider2D>();
        spawnArea2 = spawnObject2.GetComponent<BoxCollider2D>();


        // get spawn area
        if (spawnArea1 == null || spawnArea2 == null)
        {
            Debug.LogError("Spawn areas not assigned.");
            return;
        }

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
        BoxCollider2D selectedSpawnArea = (Random.Range(0, 2) == 0) ? spawnArea1 : spawnArea2;

        // grab spawn area bounds
        Bounds bounds = selectedSpawnArea.bounds;

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

    public bool CheckWinCondition()
    {
        if (enemiesSpawned == maxEnemies && firstSpawned && currentEnemies == 0)
        {
            nextLevelIndicator.SetActive(true);
            return true;
        }

        return false;
    }
}
