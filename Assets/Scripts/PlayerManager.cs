using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // player stats
    public float health;
    public float speed;
    public float attack;
    public float stamina;

    // some sprint variables
    public float accelerationTime = 0.2f;
    private float currentSpeed;
    private float accelerationVelocity = 0.0f;
    public float sprintMultiplier = 1.5f;

    // gravity variables
    public bool isJumping = false;
    public float gravity;
    public float jumpPower = 2f;
    public float smoothSpeed = 2f;

    // stamina variables
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 3f;
    private float maxStamina;

    // max health for calculating health bar percentages
    public float healthRegenRate = 0f;
    private float maxHealth;

    // what direction we are facing
    private Vector3 moveDirection;

    // audio variables
    public AudioManager audioManager;
    public bool isOnGrass = true;

    // to send to audio manager
    private bool isRunning = false;
    private bool isMoving = false;

    // map bounds top & bottom
    private float topYValue = -1.15f;
    private float botYValue = -43.0f;

    // reference to sprite renderer for later
    private SpriteRenderer mySpriteRenderer;

    // animation variables
    private Animator animator;

    // reference to player sprite
    private Sprite playerSprite;

    // reference to character selector to set playerSprite
    private CharacterSelector characterSelector;

    // reference to health bar UI element
    public Image healthBar;

    // reference to stamina bar UI element
    public Image staminaBar;

    // if game is paused
    public PauseMenu pauseMenu;

    // attack variables
    public GameObject attackObject;
    private BoxCollider2D attackArea;
    public float attackCooldown = 1.0f;
    public bool canAttack = true;


    private void Start()
    {
        // get attack box collider
        attackArea = GetComponent<BoxCollider2D>();

        // get animator
        animator = GetComponent<Animator>();

        // find character selector
        characterSelector = FindObjectOfType<CharacterSelector>();

        // find the pause menu
        pauseMenu = FindObjectOfType<PauseMenu>();

        // get reference to sprite renderer
        mySpriteRenderer = GetComponent<SpriteRenderer>();

        if (characterSelector != null)
        { 
            // get selected player sprite
            playerSprite = characterSelector.GetCurrentCharacter();
            // get selected character stats
            var characterStats = characterSelector.GetCurrentStats();

            // apply stats to player
            health = characterStats.health;
            speed = characterStats.speed;
            attack = characterStats.attack;
            stamina = characterStats.stamina;
            maxHealth = characterStats.health;
            maxStamina = characterStats.stamina;

            // apply selected sprite to player
            mySpriteRenderer.sprite = playerSprite;
        }
        else
        {
            health = 100f;
            speed = 5f;
            attack = 5f;
            stamina = 100f;
            maxHealth = 100f;
            maxStamina = 100f;
            Debug.LogError("CharacterSelector not found in the scene. Supplementing default player stats.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseMenu.IsPaused())
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(5);
        }

        // attack
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(Attack());
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }

        if (isMoving && audioManager.CanPlayStep())
        {
            audioManager.PlayFootstep(isRunning, isOnGrass, transform);
        }

        if (health < maxHealth)
        {
            RegenerateHealth();
        }
    }

    public void HandleMovement()
    {
        // check if sprinting, create target speed to reach
        float targetSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            // increase target speed while sprinting
            targetSpeed = speed * sprintMultiplier;
            isRunning = true;
            DecreaseStamina();
            // trigger running animation
            animator.SetBool("isRunning", isRunning);
        } else
        {
            if (stamina < maxStamina)
            {
                isRunning = false;
                RegenerateStamina();
                // trigger walking animation
                animator.SetBool("isRunning", isRunning);

            }
        }

        // smoothly accelerate
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref accelerationVelocity, accelerationTime);


        // get move current move direction
        // move x < 0 if we are facing left, + otherwise
        float moveX = Input.GetAxisRaw("Horizontal");
        // same with y
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // set is moving
        isMoving = moveX != 0 || moveY != 0;

        // play walking animation
        animator.SetBool("isWalking", isMoving);

        // increase current player position based on move direction & speed
        transform.position += moveDirection * (currentSpeed * 2) * Time.deltaTime;

        // clamp player on screen (vertically for now)
        // TO DO - CLAMP HORIZONTALLY
        float clampedY = Mathf.Clamp(transform.position.y, botYValue, topYValue);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);

        // flip sprite if we are facing left
        if (moveX < 0)
        {
            mySpriteRenderer.flipX = true;
        } else if (moveX > 0)
        {
            mySpriteRenderer.flipX = false;
        }
    }

    public void Jump()
    {
        isJumping = true;
        StartCoroutine(JumpCoroutine());
    }

    private IEnumerator JumpCoroutine()
    {
        // save start position
        Vector2 startJumpPosition = new Vector2(transform.position.x, transform.position.y);

        // save horizontal movement data
        Vector3 savedMoveDirection = moveDirection;
        float savedSpeed = currentSpeed;
        
        // define the peak of the jump
        Vector2 peakPosition = new Vector2(transform.position.x, transform.position.y + jumpPower);

        // define jump times
        float jumpTime = 0.2f;
        float elapsedTime = 0;

        float horizontalMultiplier = 2.2f;

        while (elapsedTime < jumpTime)
        {
            // lerp the player's position from start to final position based on elapsed time
            transform.position = new Vector3(startJumpPosition.x + (savedMoveDirection.x * savedSpeed * elapsedTime * horizontalMultiplier),
                                          Mathf.Lerp(startJumpPosition.y, peakPosition.y, (elapsedTime / jumpTime)),
                                          transform.position.z);

            // increase elapsed time by the time passed in this frame
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // reset elapsed time
        elapsedTime = 0;

        // move player back down to the start position
        while (elapsedTime < jumpTime)
        {
            // lerp the player's position from peak position back to start position
            transform.position = new Vector3(startJumpPosition.x + (savedMoveDirection.x * savedSpeed * (elapsedTime + jumpTime) * horizontalMultiplier),
                                          Mathf.Lerp(peakPosition.y, startJumpPosition.y, (elapsedTime / jumpTime)),
                                          transform.position.z);

            // increase elapsed time by the time passed in this frame
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // move player back to start position
        transform.position = new Vector3(startJumpPosition.x + (savedMoveDirection.x * savedSpeed * 2 * jumpTime * horizontalMultiplier),
                                      startJumpPosition.y,
                                      transform.position.z);

        isJumping = false;
    }

    public Vector3 GetPlayerLocation()
    {
        return transform.position;
    }

    private IEnumerator Attack()
    {
        // enable the attack area
        attackObject.SetActive(true);
        canAttack = false;

        // check for any enemies within attack area
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackArea.bounds.center, attackArea.bounds.size, 0f);

        // iterate through all hit enemies and apply damage
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                // get enemy script to apply damage
                Enemy enemy = enemyCollider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    enemy.TakeDamage();
                }
            }
        }


        // wait for 0.2 seconds
        yield return new WaitForSeconds(0.2f);
        attackObject.SetActive(false);

        canAttack = true;
    }

    public void TakeDamage(float damage)
    {
        // decrease health
        health -= damage;

        // ensure health doesnt go above max or below 0
        health = Mathf.Clamp(health, 0, maxHealth);

        // update health bar fill based on percentage
        healthBar.fillAmount = health / maxHealth;

        Knockback();
    }

    public void Knockback()
    {
        if (IsFacingRight())
        {
            StartCoroutine(KnockbackRoutine(-2));
            
        } else
        {
            StartCoroutine(KnockbackRoutine(2));
        }
    }

    private IEnumerator KnockbackRoutine(float knockbackDistance)
    {
        // knockback duration
        float duration = 0.5f;
        float elapsedTime = 0;
        Vector2 originalPosition = transform.position;
        Vector2 targetPosition = new Vector3(transform.position.x + knockbackDistance, transform.position.y);

        // wait until knockback duration is over
        while (elapsedTime < duration)
        {
            // interpolate from original pos to end pos over time
            transform.position = Vector2.Lerp(originalPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure final position is exact

    }

    public bool IsFacingRight()
    {
        return !mySpriteRenderer.flipX;
    }
    public float GetDamage()
    {
        return attack;
    }

    // lower stamina while sprinting
    public void DecreaseStamina()
    {
        stamina -= staminaDrainRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // update stamina bar
        staminaBar.fillAmount = stamina / maxStamina;
    }

    // regenerate stamina when not sprinting
    public void RegenerateStamina()
    {
        stamina += staminaRegenRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // update stamina bar
        staminaBar.fillAmount = stamina / maxStamina;
    }

    public void RegenerateHealth()
    {
        health += healthRegenRate * Time.deltaTime;
        health = Mathf.Clamp(health, 0, maxHealth);

        // update health bar
        healthBar.fillAmount = health / maxHealth;
    }
}
