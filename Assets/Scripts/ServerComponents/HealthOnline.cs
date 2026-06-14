using UnityEngine;
using Fusion;

/// <summary>
/// Networked health for online players. Replaces Health.cs on OnlinePlayer prefab.
/// State authority (host) owns currentHealth and isDeath — all damage RPCs route through it.
/// Visual/audio feedback runs locally on every client via RPC_OnDamageTaken / RPC_OnDeath.
/// </summary>
public class HealthOnline : NetworkBehaviour
{
    [Header("Team Settings")]
    public int teamId = 0;

    [Header("Health Settings")]
    public int maximumHealth = 1;
    [Tooltip("Invulnerability duration in seconds after taking damage")]
    public float invincibilityTime = 3f;
    [Networked] public int CurrentHealth { get; set; }
    [Networked] public NetworkBool IsDeath { get; set; }
    [Networked] private TickTimer invincibilityTimer { get; set; }
    [Networked] public NetworkBool IsInvincibleNetworked { get; private set; }

    public bool IsInvincible => IsInvincibleNetworked;

    [Header("Effects & Polish")]
    public GameObject deathEffect;
    public GameObject hitEffect;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerDataManagerOnline dataManager;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dataManager = GetComponent<PlayerDataManagerOnline>();

        if (Object.HasStateAuthority)
        {
            int maxHp = dataManager != null ? dataManager.CurrentMaxHealth : maximumHealth;
            CurrentHealth = maxHp;
            IsDeath = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && IsInvincibleNetworked && invincibilityTimer.ExpiredOrNotRunning(Runner))
            IsInvincibleNetworked = false;
    }

    // Called by DamageOnline (or any attacker) — only state authority processes this
    public void TakeDamage(int damageAmount)
    {
        //Debug Log
        if (!Object.HasStateAuthority)
        {
            print($"[HealthOnline][{gameObject.name}] TakeDamage called on non-state authority. Ignored.");
            return;
        }
        if (IsInvincible)
        {
            print($"[HealthOnline][{gameObject.name}] is invincible and ignored damage.");
            return;
        }
        if (CurrentHealth <= 0 || IsDeath)
        {
            print($"[HealthOnline][{gameObject.name}] is already dead and ignored damage.");
            return;
        }
        if (IsDeath)
        {
            print($"[HealthOnline][{gameObject.name}] is already dead and ignored damage.");
            return;
        }
        // if (IsInvincible || CurrentHealth <= 0 || IsDeath) return;

        invincibilityTimer = TickTimer.CreateFromSeconds(Runner, invincibilityTime);
        IsInvincibleNetworked = true;
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            IsDeath = true;
            RPC_OnDeath();
        }
        else
        {
            RPC_OnDamageTaken(transform.position);
        }
    }

    public void ReceiveHealing(int healingAmount)
    {
        if (!Object.HasStateAuthority) return;

        int max = dataManager != null ? dataManager.CurrentMaxHealth : maximumHealth;

        CurrentHealth = Mathf.Min(CurrentHealth + healingAmount, max);
    }

    public void Knockback(Vector2 dir, float knockbackForce)
    {
        if (IsDeath || rb == null) return;
        rb.linearVelocity = dir * knockbackForce;
        // Stop knockback velocity after a short delay
        Runner.StartCoroutine(StopVelocityAfter(0.1f));
    }

    private System.Collections.IEnumerator StopVelocityAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // Broadcast hurt feedback to all clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnDamageTaken(Vector3 position)
    {
        if (hitEffect != null)
            Instantiate(hitEffect, position, Quaternion.identity);

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (gameObject.CompareTag("Player") && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerHurt);
    }

    // Broadcast death to all clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnDeath()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (gameObject.CompareTag("Player"))
        {
            if (animator != null)
            {
                animator.SetTrigger("Death");
                animator.SetBool("isDead", true);
            }

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.playerDie);

            // Only trigger GameOver on the local machine that owns this player
            if (Object.HasInputAuthority)
                GameOver();
        }
    }

    private void GameOver()
    {
        if (GameManagerOnline.Instance != null)
            GameManagerOnline.Instance.RPC_NotifyPlayerDied(Object.Id);
    }

    [ContextMenu("Test Take Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(1);
    }
}
