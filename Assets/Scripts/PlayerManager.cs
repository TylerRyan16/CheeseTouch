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

    // stamina variables
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 3f;
    private float maxStamina;

    // max health for calculating health bar percentages
    private float maxHealth;

    // what direction we are facing
    private Vector3 moveDirection;
    
    // map bounds top & bottom
    private float topYValue = -1.15f;
    private float botYValue = -43.0f;

    // reference to sprite renderer for later
    private SpriteRenderer mySpriteRenderer;

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
    }

    public void HandleMovement()
    {
        // check if sprinting
        float targetSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            // increase target speed while sprinting
            targetSpeed = speed * sprintMultiplier;
            DecreaseStamina();
        } else
        {
            if (stamina < maxStamina)
            {
                RegenerateStamina();
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

        // increase current player position based on move direction & speed
        transform.position += moveDirection * (currentSpeed * 2) * Time.deltaTime;

        // clamp player on screen
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

    public void Heal(float healingAmount)
    {
        health += healingAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        healthBar.fillAmount = health / maxHealth;
    }
}
