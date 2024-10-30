using System.Collections;
using UnityEngine;
using TMPro;

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
    public GameObject textSpawnArea;
    private ScoreManager scoreManager;
    private bool scoreAdded = false;

    // movement
    public float moveSpeed = 50f;
    public bool canMove = true;

    // health
    public float currentHealth;
    private float maxHealth = 30f;
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
        // initialize references
        scoreManager = FindObjectOfType<ScoreManager>();
        knockbackScript = GetComponent<Knockback>();
        attackScript = GetComponent<Attack>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        rb = GetComponent<Rigidbody2D>();
       
        player = GameObject.FindWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = player.GetComponent<PlayerManager>();

        // dynamically calc health based on how many enemies spawned
        int enemiesSpawned = enemySpawner.GetEnemiesSpawned();
        maxHealth = 100 + (enemiesSpawned / 7) * 12;
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
        //SpawnDamageText(damage);
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
            enemySpawner.LowerCurrentEnemies();
            Destroy(gameObject);
            if (scoreAdded == false)
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
