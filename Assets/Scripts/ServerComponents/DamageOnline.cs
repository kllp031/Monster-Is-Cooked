using UnityEngine;
using Fusion;

/// <summary>
/// Networked damage dealer. Mirrors Damage.cs but targets HealthOnline instead of Health.
/// Collision detection runs on all clients; only state authority of the target processes damage.
/// Attach this instead of Damage.cs on attack effects used by online players.
/// </summary>
public class DamageOnline : MonoBehaviour
{
    [Header("Team Settings")]
    public int teamId = 0;

    [Header("Damage Settings")]
    public int damageAmount = 1;
    public int knockbackForce = 50;
    [Tooltip("Destroy this GameObject after dealing damage")]
    public bool destroyAfterDamage = true;
    public bool dealDamageOnTriggerEnter = false;
    public bool dealDamageOnTriggerStay = false;
    public bool dealDamageOnCollision = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dealDamageOnTriggerEnter)
        {
            DealDamage(collision.gameObject);
            DealKnockback(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (dealDamageOnTriggerStay)
            DealDamage(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dealDamageOnCollision)
            DealDamage(collision.gameObject);
    }

    private void DealDamage(GameObject target)
    {
        HealthOnline health = target.GetComponent<HealthOnline>();
        if (health == null) return;
        if (health.teamId == teamId) return;

        // TakeDamage guards itself with HasStateAuthority — safe to call from any client
        if (gameObject.CompareTag("PlayerAttack"))
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.metalHit);

            // Walk up to the player root to read their networked attack stat
            PlayerDataManagerOnline attackerData = GetComponentInParent<PlayerDataManagerOnline>();
            int attack = attackerData != null ? attackerData.CurrentAttack : damageAmount;
            health.TakeDamage(attack);
        }
        else
        {
            health.TakeDamage(damageAmount);
        }

        if (destroyAfterDamage)
            Destroy(gameObject);
    }

    private void DealKnockback(GameObject target)
    {
        HealthOnline health = target.GetComponent<HealthOnline>();
        if (health == null) return;
        if (health.teamId == teamId) return;

        Vector2 dir = (target.transform.position - transform.position).normalized;

        // Notify the online controller about hurt state so animations play correctly
        if (target.CompareTag("Player"))
        {
            KnightControllerOnline controller = target.GetComponent<KnightControllerOnline>();
            if (controller != null)
                controller.SetIsHurting(true);
        }

        health.Knockback(dir, knockbackForce);
    }
}
