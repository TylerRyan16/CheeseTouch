using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // references
    private GameObject player;
    private PlayerManager playerManager;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // movement
    public float moveSpeed = 10f;
    public bool canMove = true;

    // health
    public float currentHealth;
    public float maxHealth = 100f;

    // attack
    public float attackDamage = 10f;
    public float attackDelay = 0.5f;
    public float knockbackDistance = 1f;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;
    private bool isCollidingWithPlayer = false;


    private void Start()
    {
        // set current health to max
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();


        // initialize references
        player = GameObject.FindWithTag("Player");  
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = player.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
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
        // Move towards the player
        Vector2 playerPosition = playerManager.GetPlayerLocation();
        Vector2 newPosition = Vector2.MoveTowards(rb.position, playerPosition, moveSpeed * Time.deltaTime);

        // Apply movement using Rigidbody2D's MovePosition
        rb.MovePosition(newPosition);
    }

    public void FlipSprite()
    {
        Vector2 playerPosition = GetPlayerPosition();


        if (transform.position.x < playerPosition.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }


    public Vector2 GetPlayerPosition()
    {
        return player.transform.position;
    }



    // COLLISION - ATTACK
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("colliding...");
            isCollidingWithPlayer = true;
            StartCoroutine(AttackPlayerWithDelay());
        }
    }

    private IEnumerator AttackPlayerWithDelay()
    {
        canMove = false;
        yield return new WaitForSeconds(0.5f);

        if (isCollidingWithPlayer)
        {
            playerManager.TakeDamage(attackDamage, transform.position);
            Debug.Log("Damaged the player!");
        }
        canMove = true;
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isCollidingWithPlayer = false;
        }
    }

    public void TakeDamage()
    {
        currentHealth = currentHealth - playerManager.GetDamage();

        StartCoroutine(TakeKnockback());

        DespawnIfDead();
    }

    private IEnumerator TakeKnockback()
    {
        // disable movement for short time to act as stun
        canMove = false;

        if (IsPlayerToRight())
        {
            yield return StartCoroutine(KnockbackRoutine(-knockbackDistance));
        }
        else
        {
            yield return StartCoroutine(KnockbackRoutine(knockbackDistance));
        }

        // wait 0.3s to act as stun
        yield return new WaitForSeconds(0.4f);

        // allow movement again
        canMove = true;
    }

    private IEnumerator KnockbackRoutine(float knockbackDistance)
    {
        float duration = 0.1f;
        float elapsedTime = 0;
        Vector2 originalpos = transform.position;
        Vector2 targetPos = new Vector2(transform.position.x + knockbackDistance, transform.position.y);

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(originalpos, targetPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    // DESPAWNING
    public void DespawnIfDead()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            return;
        }

    }

    // GETTERS
    private bool IsPlayerToRight()
    {
        return player.transform.position.x > transform.position.x;
    }

}
