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
