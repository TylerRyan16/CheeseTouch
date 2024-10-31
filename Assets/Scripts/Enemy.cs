using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    // references
    private GameObject player;
    private PlayerManager playerManager;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Knockback knockbackScript;
    private Attack attackScript;
    private EnemySpawner enemySpawner;
    private Level2Spawner levelTwoSpawner;
    public GameObject textSpawnArea;
    private ScoreManager scoreManager;
    private bool scoreAdded = false;
    private AudioManager audioManager;

    // movement
    public float moveSpeed = 50f;
    public bool canMove = true;

    // health
    public float currentHealth;
    private float maxHealth = 25f;
    private bool isInvincible = false;
    public float invincibilityDuration = 0.2f;

    // attack
    public float attackDamage = 25f;
    public float attackDelay = 0.65f;
    public float attackCooldown = 0.5f;
    private bool isCollidingWithPlayer = false;
    private Coroutine attackCoroutine;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        InitializeSpawnerAndHealth();


        // initialize references
        scoreManager = FindObjectOfType<ScoreManager>();
        knockbackScript = GetComponent<Knockback>();
        attackScript = GetComponent<Attack>();
        rb = GetComponent<Rigidbody2D>();
       
        player = GameObject.FindWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = player.GetComponent<PlayerManager>();
    }

    private void InitializeSpawnerAndHealth()
    {
        // Get the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Basketball_Court")
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner != null)
            {
                int enemiesSpawned = enemySpawner.GetEnemiesSpawned();
                maxHealth = 25 + (enemiesSpawned / 7) * 8;
            }
        }
        else if (currentSceneName == "Level2")
        {
            levelTwoSpawner = FindObjectOfType<Level2Spawner>();
            if (levelTwoSpawner != null)
            {
                int enemiesSpawned = levelTwoSpawner.GetEnemiesSpawned();
                maxHealth = 25 + (enemiesSpawned / 7) * 8;
            }
        }

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player != null && canMove)
        {
            MoveTowardsPlayer();
            FlipSprite();
        }
    }

    public void MoveTowardsPlayer()
    {
        Vector2 playerPosition = playerManager.GetPlayerLocation();
        Vector2 newPosition = Vector2.MoveTowards(rb.position, playerPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    public void FlipSprite()
    {
        if (transform.position.x < player.transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;

            // continuous attack loop
            if (attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(EnemyAttackLoop());
            }

            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            isCollidingWithPlayer = false;
        }
    }

    private IEnumerator EnemyAttackLoop()
    {
        while (isCollidingWithPlayer)
        {
            if (attackScript != null && attackScript.CanAttack())
            {
                // disable movement during attack and perform attack
                SetCanMove(false);
                attackScript.EnemyAttack(playerManager, attackDamage, attackDelay, this);

                // wait until the attack cooldown has finished
                yield return new WaitForSeconds(attackDelay);

                // re-enable movement after attack cooldown
                SetCanMove(true);
            }

            // ensure we wait slightly before checking again to prevent spamming
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void TakeDamage()
    {
        if (isInvincible) return;

        float damage = playerManager.GetDamage();
        currentHealth -= damage;
        knockbackScript.TakeKnockback(player.transform.position);

        if (currentHealth > 0)
        {
            audioManager.PlayZombieHurtSound();
        }

        DespawnIfDead();

        StartCoroutine(InvincibilityCooldown());
    }

    private IEnumerator InvincibilityCooldown()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    public void DespawnIfDead()
    {
        if (currentHealth <= 0)
        {
            // Check which scene is active and call the appropriate spawner's LowerCurrentEnemies method
            if (SceneManager.GetActiveScene().name == "Level2")
            {
                // Use Level2Spawner if we are in Level 2
                Level2Spawner levelTwoSpawner = FindObjectOfType<Level2Spawner>();
                if (levelTwoSpawner != null)
                {
                    levelTwoSpawner.LowerCurrentEnemies();
                }
            }
            else
            {
                // Use EnemySpawner for other scenes
                if (enemySpawner != null)
                {
                    enemySpawner.LowerCurrentEnemies();
                }
            }

            // Destroy enemy game object and update score
            Destroy(gameObject);
            audioManager.PlayZombieDeathSound();

            if (!scoreAdded)
            {
                scoreManager.AddScore(10);
                scoreAdded = true;
            }
        }
    }

    public bool IsCollidingWithPlayer()
    {
        return isCollidingWithPlayer;
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    private void SpawnDamageText(float damage)
    {
        // Find the TextMeshPro component within the damageTextPrefab (already part of the enemy prefab)
        TextMeshProUGUI textMesh = textSpawnArea.GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh != null)
        {
            // Randomize the text position within the spawn area bounds
            Bounds spawnBounds = textSpawnArea.GetComponent<Renderer>().bounds;
            Vector3 randomPosition = new Vector3(
                Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                Random.Range(spawnBounds.min.y, spawnBounds.max.y),
                spawnBounds.center.z
            );

            // Set the text position, text value, and activate the object
            textMesh.transform.position = randomPosition;
            textMesh.text = damage.ToString();
            textMesh.gameObject.SetActive(true);

            // Start coroutine to deactivate after a delay
            StartCoroutine(DeactivateDamageText(textMesh));
        }
        else
        {
            Debug.LogError("TextMeshPro component not found in the textSpawnArea.");
        }
    }

    // Coroutine to deactivate the damage text after 0.5 seconds
    private IEnumerator DeactivateDamageText(TextMeshProUGUI textMesh)
    {
        yield return new WaitForSeconds(0.5f);
        textMesh.gameObject.SetActive(false);
    }
}
