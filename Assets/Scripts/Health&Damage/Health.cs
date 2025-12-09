using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This class handles the health state of a game object.
/// 
/// Implementation Notes: 2D Rigidbodies must be set to never sleep for this to interact with trigger stay damage
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    [Header("Health Settings")]
    [Tooltip("The default health value")]
    public int defaultHealth = 1;
    [Tooltip("The maximum health value")]
    public int maximumHealth = 1;
    [Tooltip("The current in game health value")]
    public int currentHealth = 1;
    [Tooltip("Invulnerability duration, in seconds, after taking damage")]
    public float invincibilityTime = 3f;
    public bool isDeath = false;

    private EnemyBase enemyBase;
    private Rigidbody2D rb;
    private Animator animator;
    void Start()
    {
        SetRespawnPoint(transform.position);
        rb = GetComponent<Rigidbody2D>();
        enemyBase = GetComponent<EnemyBase>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        InvincibilityCheck();
    }

    // The specific game time when the health can be damaged again
    private float timeToBecomeDamagableAgain = 0;
    // Whether or not the health is invincible
    public bool isInvincible = false;

    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time)
        {
            isInvincible = false;
        }
    }

    // The position that the health's gameobject will respawn at
    private Vector3 respawnPosition;

    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    void Respawn()
    {
        transform.position = respawnPosition;
        currentHealth = defaultHealth;
        //GameManager.UpdateUIElements();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || currentHealth <= 0)
        {
            return;
        }
        else
        {
            if(gameObject.tag == "Enemy") 
                enemyBase.currentEnemyState = EnemyBase.EnemyState.Hurt;

            if (gameObject.tag == "Player")
                animator.SetTrigger("Hurt");

            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;
            isInvincible = true;
            currentHealth -= damageAmount;
            CheckDeath();
        }
        //GameManager.UpdateUIElements();
    }

    public void Knockback(Vector2 dir, float knockbackForce)
    {
        rb.linearVelocity = dir * knockbackForce;
        Debug.Log("dir knockback x: " + dir.x + "y: " + dir.y);
        StartCoroutine(StopMoveAfterTime(0.1f));
    }

    IEnumerator StopMoveAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        rb.linearVelocity = Vector2.zero;
        Debug.Log("stop knockback");
    }

    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        CheckDeath();
        //GameManager.UpdateUIElements();
    }

    [Header("Effects & Polish")]
    [Tooltip("The effect to create when this health dies")]
    public GameObject deathEffect;
    [Tooltip("The effect to create when this health is damaged (but does not die)")]
    public GameObject hitEffect;

    bool CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    void Die()
    {
        Debug.Log("Die");
        if (gameObject.tag == "Enemy") enemyBase.currentEnemyState = EnemyBase.EnemyState.Dead;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }
        //GameManager.UpdateUIElements();
    }

    public void GameOver()
    {
        /*if (GameManager.instance != null && gameObject.tag == "Player")
        {
            GameManager.instance.GameOver();
        }*/
    }
}
