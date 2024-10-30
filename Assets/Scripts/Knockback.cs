using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackDistance = 1f;
    public float knockbackCooldown = 1.0f;
    public bool canBeKnockedBack = true;
    private Attack attackScript;

    private void Start()
    {
        attackScript = GetComponent<Attack>();
    }
    public void TakeKnockback(Vector2 enemyPosition)
    {
        if (canBeKnockedBack && (attackScript.GetAttackObject() == null || !attackScript.isAttacking))
        {
            Debug.Log("getting knocked back...");
            float direction = EnemyOnRight(enemyPosition) ? -1 : 1;
            StartCoroutine(KnockbackRoutine(direction * knockbackDistance));
            StartCoroutine(KnockbackCooldown());
        }
    }

    private IEnumerator KnockbackRoutine(float knockbackDistance)
    {
        float duration = 0.1f;
        float elapsedTime = 0;
        Vector2 originalPosition = transform.position;
        Vector2 targetPosition = new Vector2(transform.position.x + knockbackDistance, transform.position.y);

        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    private IEnumerator KnockbackCooldown()
    {
        canBeKnockedBack = false;
        yield return new WaitForSeconds(knockbackCooldown);
        canBeKnockedBack = true;
    }

    public bool CanBeKnockedBack()
    {
        return canBeKnockedBack;
    }


    private bool EnemyOnRight(Vector2 enemyPosition)
    {
        return enemyPosition.x > transform.position.x;
    }
}
