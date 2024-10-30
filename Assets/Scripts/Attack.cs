using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject attackObject;
    public float playerAttackCooldown = 0.5f;
    public float enemyAttackCooldown = 1.0f;
    public bool canAttack = true;
    public bool isAttacking {  get; private set; }
    private BoxCollider2D attackArea;
    private AudioManager audioManager;

    private void Start()
    {
        if (attackObject != null)
        {
            attackArea = attackObject.GetComponent<BoxCollider2D>();
            attackObject.SetActive(false);
        }

        audioManager = FindObjectOfType<AudioManager>();
    }


    public void PlayerAttack()
    {
        if (canAttack)
        {
            StartCoroutine(PlayerAttackCoroutine());
        }
    }

    private IEnumerator PlayerAttackCoroutine()
    {
        isAttacking = true;
        canAttack = false;

        if (attackObject != null)
        {
            attackObject.SetActive(true);

            bool hitEnemy = false;

            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackArea.bounds.center, attackArea.bounds.size, 0f);
            foreach (Collider2D enemyCollider in hitEnemies)
            {
                if (enemyCollider.CompareTag("Enemy"))
                {
                    Enemy enemy = enemyCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage();
                        hitEnemy = true;
                    }
                }
            }

            if (hitEnemy)
            {
                audioManager.PlayPunchSound();
            } else
            {
                audioManager.PlayWhooshSound();
            }

            yield return new WaitForSeconds(0.2f);
            attackObject.SetActive(false);     
            
        }
      
        yield return new WaitForSeconds(playerAttackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    public void EnemyAttack(PlayerManager player, float damage, float delay, Enemy enemy)
    {
        if (canAttack)
        {
            StartCoroutine(EnemyAttackRoutine(player, damage, delay, enemy));
        }
    }

    private IEnumerator EnemyAttackRoutine(PlayerManager player, float damage, float delay, Enemy enemy)
    {
        isAttacking = true;
        canAttack = false;

        yield return new WaitForSeconds(delay);

        if (enemy.IsCollidingWithPlayer() && player != null)
        {
            player.TakeDamage(damage, transform.position);
        }

        // resume enemy movement
        enemy.SetCanMove(true);

        isAttacking = false;
        yield return new WaitForSeconds(enemyAttackCooldown);
        canAttack = true;
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public GameObject GetAttackObject()
    {
        if (attackObject != null)
        {
            return attackObject;
        } 
        else
        {
            return null;
        }
    }
}
