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
    public float healthRegenRate = 3f;
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



    private void Start()
    {
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

        // define the peak of the jump
        Vector2 peakPosition = new Vector2(transform.position.x, transform.position.y + 2);

        // define jump times
        float jumpTime = 0.2f;
        float elapsedTime = 0;

        while (elapsedTime < jumpTime)
        {
            // lerp the player's position from start to final position based on elapsed time
            transform.position = Vector3.Lerp(startJumpPosition, peakPosition, (elapsedTime / jumpTime));

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
            transform.position = Vector3.Lerp(peakPosition, startJumpPosition, (elapsedTime / jumpTime));

            // increase elapsed time by the time passed in this frame
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // move player back to start position
        transform.position = startJumpPosition;

        isJumping = false;
    }

    public Vector3 GetPlayerLocation()
    {
        return transform.position;
    }

    public void TakeDamage(float damage)
    {
        // decrease health
        health -= damage;

        // ensure health doesnt go above max or below 0
        health = Mathf.Clamp(health, 0, maxHealth);

        // update health bar fill based on percentage
        healthBar.fillAmount = health / maxHealth;
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
