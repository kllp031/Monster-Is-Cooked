using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which handles enemy state translation to Animations
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The enemy component responsible for tracking the enemy's state")]
    public EnemyBase enemyComponent = null;
    [Tooltip("The animator to use to animate the enemy")]
    public Animator enemyAnimator = null;

    [Header("Animator Parameter Names")]
    public string IdleAnimatorParameter = "isIdle";
    public string PatrolAnimatorParameter = "isPatrol";
    public string ChaseAnimatorParameter = "isChase";
    public string AttackAnimatorParameter = "isAttack";
    public string HurtAnimatorParameter = "isHurt";
    public string DeadAnimatorParameter = "isDead";

    private void Start()
    {
        SetAnimatorState();
    }

    private void Update()
    {
        SetAnimatorState();
    }

    private void SetAnimatorState()
    {
        if (enemyComponent == null || enemyAnimator == null)
            return;

        var currentState = enemyComponent.currentEnemyState;
        var lastState = enemyComponent.lastEnemyState;

        if (currentState == lastState)
            return;

        enemyComponent.lastEnemyState = currentState;

        // Reset
        enemyAnimator.SetBool(IdleAnimatorParameter, false);
        enemyAnimator.SetBool(PatrolAnimatorParameter, false);
        enemyAnimator.SetBool(ChaseAnimatorParameter, false);
        enemyAnimator.SetBool(AttackAnimatorParameter, false);
        enemyAnimator.SetBool(HurtAnimatorParameter, false);
        enemyAnimator.SetBool(DeadAnimatorParameter, false);

        // Set new state
        switch (currentState)
        {
            case EnemyBase.EnemyState.Idle:
                enemyAnimator.SetBool(IdleAnimatorParameter, true);
                break;

            case EnemyBase.EnemyState.Patrol:
                enemyAnimator.SetBool(PatrolAnimatorParameter, true);
                break;

            case EnemyBase.EnemyState.Chase:
                enemyAnimator.SetBool(ChaseAnimatorParameter, true);
                break;

            case EnemyBase.EnemyState.Attack:
                enemyAnimator.SetBool(AttackAnimatorParameter, true);
                break;

            case EnemyBase.EnemyState.Hurt:
                enemyAnimator.SetBool(HurtAnimatorParameter, true);
                break;

            case EnemyBase.EnemyState.Dead:
                enemyAnimator.SetBool(DeadAnimatorParameter, true);
                break;
        }
    }
}
