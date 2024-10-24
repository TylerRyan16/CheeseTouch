using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // references
    private GameObject player;
    private PlayerManager playerManager;
    private SpriteRenderer spriteRenderer;

    // movement
    public float moveSpeed = 5f;

    // health
    public float currentHealth;
    public float maxHealth = 100f;

    // attack
    public float attackDamage = 10f;
    public float attackDelay = 0.5f;
    public float knockbackDistance = 2f;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;


    private void Start()
    {
        // set current health to max
        currentHealth = maxHealth;

        // initialize references
        player = GameObject.FindWithTag("Player");  
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = player.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
            FlipSprite();
            DespawnIfDead();
        }
    }

    public void MoveTowardsPlayer()
    {
        Vector2 playerPosition = playerManager.GetPlayerLocation();
        transform.position = Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);
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

    public Vector2 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void TakeDamage()
    {
        currentHealth = currentHealth - playerManager.GetDamage();
    }

    // deal damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D called");
        if (collision.gameObject.tag == "Player")
        {

            playerManager.TakeDamage(attackDamage);
            Debug.Log("Enemy hit the player, dealing " + attackDamage + " damage.");
        }
    }

}
