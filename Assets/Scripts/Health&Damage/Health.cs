using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class Health : NetworkBehaviour
{
    [Header("Team Settings")]
    public int teamId = 0;

    [Header("Health Settings")]
    public int maximumHealth = 1;

    // 1. Biến mạng đồng bộ máu. Khi máu đổi, gọi hàm OnHealthChanged để bật hiệu ứng đồ họa
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public int currentHealth { get; set; }
    private int _lastHealth;

    // 2. Đồng hồ bất tử mạng thay thế cho Time.time
    [Networked] public TickTimer invincibilityTimer { get; set; }
    [Tooltip("Invulnerability duration, in seconds, after taking damage")]
    public float invincibilityTime = 3f;

    // 3. Trạng thái chết đồng bộ mạng
    [Networked, OnChangedRender(nameof(OnDeathChanged))]
    public NetworkBool isDeath { get; set; }

    [Networked] public TickTimer knockbackTimer { get; set; }

    private EnemyBase enemyBase;
    private EnemySpawner mySpawner;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Invincibility Blink")]
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float blinkMinAlpha = 0.2f;
    private float _blinkTimer;
    private bool _blinkVisible = true;

    private Vector3 respawnPosition;

    // Thay thế Start() bằng Spawned() để khởi tạo dữ liệu mạng an toàn
    public override void Spawned()
    {
        base.Spawned();

        rb = GetComponent<Rigidbody2D>();
        enemyBase = GetComponent<EnemyBase>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        _lastHealth = currentHealth;
        SetRespawnPoint(transform.position);

        // Chỉ máy giữ quyền (State Authority) mới có quyền thiết lập máu ban đầu
        if (Object.HasStateAuthority)
        {
            if (gameObject.CompareTag("Player"))
                currentHealth = GetMaxHealth();
            else
                currentHealth = maximumHealth;
            isDeath = false;
        }
    }

    // Logic tính toán mạng (FixedUpdateNetwork - FUN)
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!Object.HasStateAuthority) return;

        // Xử lý hết lực Knockback đồng bộ qua mạng
        if (knockbackTimer.IsRunning && knockbackTimer.Expired(Runner))
        {
            rb.linearVelocity = Vector2.zero;
            knockbackTimer = TickTimer.None;
        }
    }

    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    // Returns max HP supporting both offline (PlayerDataManager) and online (PlayerDataManagerOnline).
    public int GetMaxHealth()
    {
        // Online manager on this player is authoritative in multiplayer; check it first.
        var pdmOnline = GetComponent<PlayerDataManagerOnline>();
        if (pdmOnline != null)
            return pdmOnline.CurrentMaxHealth;
        if (PlayerDataManager.Instance != null)
            return PlayerDataManager.Instance.CurrentMaxHealth;
        return maximumHealth;
    }

    public void Respawn()
    {
        if (!Object.HasStateAuthority) return;

        // Position is handled by KnightControllerOnline.
        invincibilityTimer = default;
        isDeath = false; // OnDeathChanged fires on all clients → plays Revive animation
        currentHealth = gameObject.CompareTag("Player") ? GetMaxHealth() : maximumHealth;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    // Lệnh nhận sát thương cực kỳ quan trọng
    public void TakeDamage(int damageAmount)
    {
        if (Object == null || !Object.IsValid) return;
        if (!Object.HasStateAuthority)
        {
            // Nếu Client chém trúng quái, gửi RPC báo Master Client trừ máu hộ
            RPC_RequestDamage(damageAmount);
            return;
        }

        // Kiểm tra bất tử bằng TickTimer
        bool isInvincible = !invincibilityTimer.ExpiredOrNotRunning(Runner);

        if (isInvincible || currentHealth <= 0 || isDeath) return;

        // Master Client đổi trạng thái FSM của quái, Fusion tự đồng bộ State Machine
        if (gameObject.CompareTag("Enemy") && enemyBase != null)
            enemyBase.currentEnemyState = EnemyBase.EnemyState.Hurt;

        // Kích hoạt thời gian bất tử mạng
        invincibilityTimer = TickTimer.CreateFromSeconds(Runner, invincibilityTime);

        currentHealth -= damageAmount;
        CheckDeath();
    }

    // Cầu nối RPC gửi từ máy chém quái lên máy chủ giữ quyền quái
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(int amount)
    {
        TakeDamage(amount);
    }

    public void Knockback(Vector2 dir, float knockbackForce)
    {
        if (Object == null || !Object.IsValid) return;
        if (isDeath) return;

        // Tác động vật lý mạng trực tiếp trên máy giữ quyền
        rb.linearVelocity = dir * knockbackForce;
        // Đặt đồng hồ mạng 0.1 giây để dừng lực đẩy
        knockbackTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
    }

    public void ReceiveHealing(int healingAmount)
    {
        if (!Object.HasStateAuthority) return;

        int maxHp = gameObject.CompareTag("Player") ? GetMaxHealth() : maximumHealth;
        currentHealth = Mathf.Min(currentHealth + healingAmount, maxHp);
    }

    [Header("Effects & Polish")]
    public GameObject deathEffect;
    public GameObject hitEffect;

    private void CheckDeath()
    {
        if (currentHealth <= 0 && !isDeath)
        {
            Die();
        }
    }

    private void Die()
    {
        isDeath = true; // OnDeathChanged fires on all clients → plays Death animation

        if (gameObject.CompareTag("Enemy"))
        {
            if (enemyBase != null) enemyBase.currentEnemyState = EnemyBase.EnemyState.Dead;
            if (mySpawner != null) mySpawner.OnEnemyDeath();

            // Spawn drop items trên mạng trước khi despawn (chỉ StateAuthority mới gọi được)
            var dropItem = GetComponent<DropItem>();
            Debug.Log($"[Health] Die → DropItem component = {(dropItem != null ? "tìm thấy" : "KHÔNG tìm thấy")} trên '{gameObject.name}'");
            if (dropItem != null)
                dropItem.DropNetworked(Runner, transform.position);

            Runner.Despawn(Object);
        }

        if (gameObject.CompareTag("Player"))
            GameOver();
    }

    public void GameOver()
    {
        if (!gameObject.CompareTag("Player")) return;

        if (GameManagerOnline.Instance != null)
            GameManagerOnline.Instance.RPC_NotifyPlayerDied(Object.Id);
        else if (GameManager.Instance != null)
            GameManager.Instance.EndLevel();
    }

    public void SetupSpawner(EnemySpawner spawner)
    {
        mySpawner = spawner;
    }

    // ==========================================
    // KHU VỰC THỰC THI ĐỒ HỌA/ÂM THANH TRÊN TẤT CẢ CÁC MÁY (RENDER LOGIC)
    // ==========================================

    public override void Render()
    {
        base.Render();

        if (spriteRenderer == null || !gameObject.CompareTag("Player")) return;

        bool isInvincible = !invincibilityTimer.ExpiredOrNotRunning(Runner);

        if (isInvincible)
        {
            _blinkTimer += Time.deltaTime;
            if (_blinkTimer >= blinkInterval)
            {
                _blinkTimer = 0f;
                _blinkVisible = !_blinkVisible;
                var c = spriteRenderer.color;
                c.a = _blinkVisible ? 1f : blinkMinAlpha;
                spriteRenderer.color = c;
            }
        }
        else
        {
            _blinkTimer = 0f;
            _blinkVisible = true;
            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
    }

    // Kích hoạt tự động khi biến currentHealth bị mạng thay đổi giá trị
    public void OnHealthChanged()
    {
        if (currentHealth < _lastHealth)
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }

            if (gameObject.CompareTag("Player"))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.playerHurt);
                if (animator != null) animator.SetTrigger("Hurt");
            }
        }

        // Cập nhật lại mốc máu cũ để so sánh cho lần sau
        _lastHealth = currentHealth;
    }

    public void OnDeathChanged()
    {
        if (!gameObject.CompareTag("Player")) return;

        if (isDeath)
        {
            if (animator != null)
            {
                animator.SetTrigger("Death");
                animator.SetBool("isDead", true);
            }

            if (deathEffect != null)
                Instantiate(deathEffect, transform.position, transform.rotation, null);
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.playerDie);

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (Object.HasInputAuthority)
                GetComponent<KnightControllerOnline>()?.OnLocalPlayerDied();
        }
        else
        {
            // Revive: chạy trên mọi client khi isDeath flip false (networked).
            // Clear isDead bool + Revive trigger để remote proxy thoát dead pose.
            if (animator != null)
            {
                animator.ResetTrigger("Death");
                animator.SetBool("isDead", false);
                animator.SetTrigger("Revive");
            }

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            if (Object.HasInputAuthority)
                GetComponent<KnightControllerOnline>()?.OnLocalPlayerRevived();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        base.Despawned(runner, hasStateAuthority);

        // CHỈ xử lý hiệu ứng tan biến cho ENEMY tại đây (Tránh việc nổ hiệu ứng 2 lần)
        if (gameObject.CompareTag("Enemy"))
        {
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, transform.rotation, null);
            }

            // Nếu quái vật có âm thanh chết riêng (enemyBase.PlayDieSound), bạn có thể gọi tại đây:
            if (enemyBase != null) enemyBase.PlayDieSound();
        }
    }
}
