using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains settings for and handles the control of an enemy
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How fast this enemy moves")]
    public float moveSpeed = 2f;

    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }

    [Tooltip("The state the enemy is in for animation playback")]
    public EnemyState currentEnemyState;
    public EnemyState lastEnemyState = EnemyState.Idle;

    [Header("References")]
    public Transform target;  // Player
    protected Rigidbody2D rb;
    protected DropItem dropItem;

    [Header("Audio Settings")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip hurtSound;
    [SerializeField] protected AudioClip dieSound;

    public void PlayAttackSound()
    {
        if(attackSound != null)
            SoundManager.Instance.PlaySFX(attackSound);
    }
    public void PlayHurtSound()
    {
        if (hurtSound != null)
            SoundManager.Instance.PlaySFX(hurtSound);
    }
    public void PlayDieSound()
    {
        if (dieSound != null)
            SoundManager.Instance.PlaySFX(dieSound);
    }

    public void SetTarget(Transform player)
    {
        target = player;
    }

    protected virtual void Start()
    {
        Setup();
    }

    protected virtual void Update()
    {
        // Every frame, get the desired movement of this enemy, then move it.
        Vector3 movement = GetMovement();
        MoveEnemy(movement);
    }

    protected virtual void Setup()
    {

    }

    protected virtual Vector3 GetMovement()
    {
        return Vector3.zero;
    }

    protected virtual void MoveEnemy(Vector3 movement)
    {
        transform.position = transform.position + movement;
    }
}
