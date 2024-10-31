using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    // map bounds top & bottom
    private float LEFT_X_VALUE;
    private float RIGHT_X_VALUE;
    private float TOP_Y_VALUE;
    private float BOT_Y_VALUE;

    // animation states
    private string currentState;
    const string PLAYER_IDLE = "GregIdle";
    const string PLAYER_WALK = "GregWalk";
    const string PLAYER_RUN = "GregRun";
    const string PLAYER_ATTACK = "GregPunch";


    // references
    private SpriteRenderer mySpriteRenderer;
    private Animator animator;
    private Sprite playerSprite;
    private CharacterSelector characterSelector;
    public Image healthBar;
    public Image staminaBar;
    public PauseMenu pauseMenu;
    public AudioManager audioManager;
    private Attack attackScript;
    private Knockback knockbackScript;
    private EnemySpawner enemySpawner;

    // player stats
    public float health;
    public float speed;
    public float attack;
    public float stamina;
    private float originalSpeed;


    // some movement variables
    private Vector3 moveDirection;
    public float accelerationTime = 0.2f;
    private float currentSpeed;
    private float accelerationVelocity = 0.0f;
    public float sprintMultiplier = 1.5f;

    // max health for calculating health bar percentages
    public float healthRegenRate = 0f;
    private float maxHealth;

    // stamina variables
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 3f;
    private float maxStamina;

    // gravity / jump variables
    public bool isJumping = false;
    public float gravity;
    public float jumpPower = 2f;
    public float smoothSpeed = 2f;

    // audio / animation identifiers
    public bool isOnGrass = true;
    private bool isRunning = false;
    private bool isMoving = false;

    private bool levelTwoLoaded = false;

    private void Start()
    {
        // get script references
        attackScript = GetComponent<Attack>();
        knockbackScript = GetComponent<Knockback>();
        enemySpawner = FindObjectOfType<EnemySpawner>();

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
            SetPlayerStats();
        }
        else
        {
            FillDefaultStats();
        }

        SetSceneSpecificSettings();

    }

    private void SetSceneSpecificSettings()
    {
        // Determine if current scene is Level2
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level2")
        {
            // Double the speed for Level2
            speed *= 1.2f;
            originalSpeed = speed;

            // Set specific boundaries for Level2
            LEFT_X_VALUE = -60.0f;
            RIGHT_X_VALUE = 64.0f;
            TOP_Y_VALUE = -5.0f;
            BOT_Y_VALUE = -31.0f;
        }
        else
        {
            // Default boundaries and speed
            LEFT_X_VALUE = -75.0f;
            RIGHT_X_VALUE = 45.0f;
            TOP_Y_VALUE = -1.15f;
            BOT_Y_VALUE = -43.0f;
            originalSpeed = speed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        // attack
        if (Input.GetMouseButtonDown(0) && attackScript.CanAttack())
        {

            ChangeAnimationState(PLAYER_ATTACK);
            attackScript.PlayerAttack();
        }

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }

        // audio stuff
        if (isMoving && audioManager.CanPlayStep())
        {
            audioManager.PlayFootstep(isRunning, isOnGrass, transform);
        }

        // health regen
        if (health < maxHealth)
        {
            //RegenerateHealth();
        }
    }

    private void FixedUpdate()
    {
        // movement
        if (!pauseMenu.IsPaused())
        {
            HandleMovement();
        }
    }

    public void HandleMovement()
    {
        // Determine target speed based on sprinting or normal movement
        float targetSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            targetSpeed = originalSpeed * sprintMultiplier; // Sprinting
            isRunning = true;
            DecreaseStamina();
        }
        else
        {
            isRunning = false;
            if (stamina < maxStamina)
            {
                RegenerateStamina();
            }
        }

        // Smooth transition to target speed
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref accelerationVelocity, accelerationTime);

        // Get movement direction
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // Set animation based on movement
        isMoving = moveX != 0 || moveY != 0;
        if (!attackScript.isAttacking)
        {
            if (!isMoving) ChangeAnimationState(PLAYER_IDLE);
            else if (isRunning) ChangeAnimationState(PLAYER_RUN);
            else ChangeAnimationState(PLAYER_WALK);
        }

        // Apply movement and clamp within boundaries
        transform.position += moveDirection * (currentSpeed * 2) * Time.deltaTime;
        float clampedX = Mathf.Clamp(transform.position.x, LEFT_X_VALUE, RIGHT_X_VALUE);
        float clampedY = Mathf.Clamp(transform.position.y, BOT_Y_VALUE, TOP_Y_VALUE);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        // Check for scene transition near the boundary
        if (Mathf.Abs(transform.position.x - RIGHT_X_VALUE) < 0.1f && enemySpawner.CheckWinCondition() && !levelTwoLoaded)
        {
            levelTwoLoaded = true;
            SceneManager.LoadScene("Level2");
        }

        // flip sprite if we are facing left
        if (moveX < 0)
        {
            mySpriteRenderer.flipX = true;
        }
        else if (moveX > 0)
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

    public void TakeDamage(float damage, Vector2 enemyPosition)
    {
        audioManager.PlayZombieAttackSound();

        // decrease health
        health -= damage;

        if (health <= 0)
        {
            audioManager.PlayGameOverSound();
            pauseMenu.ActivateLoseScreen();
        }

        // ensure health doesnt go above max or below 0
        health = Mathf.Clamp(health, 0, maxHealth);

        // update health bar fill based on percentage
        healthBar.fillAmount = health / maxHealth;

        if (knockbackScript.CanBeKnockedBack())
        {
            knockbackScript.TakeKnockback(enemyPosition);
        }
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


    public bool EnemyOnRight(Vector2 enemyPosition)
    {
        return enemyPosition.x > transform.position.x;
    }

    public float GetDamage()
    {
        return attack;
    }

    public Vector3 GetPlayerLocation()
    {
        return transform.position;
    }


    public void SetPlayerStats()
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


    private void FillDefaultStats()
    {
        health = 100f;
        speed = 5f;
        attack = 25f;
        stamina = 100f;
        maxHealth = 100f;
        maxStamina = 100f;
        Debug.LogError("CharacterSelector not found in the scene. Supplementing default player stats.");
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;
    }


}


