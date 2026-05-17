using UnityEngine;
using Fusion; 

public abstract class EnemyAttackBase : NetworkBehaviour 
{
    [Header("Attack Settings")]
    [SerializeField] protected float _attackCooldown = 1.5f;

    [Networked] protected TickTimer _attackCooldownTimer { get; set; }

    public abstract void PerformAttack();

    public bool CanAttack()
    {
        return _attackCooldownTimer.ExpiredOrNotRunning(Runner);
    }

    protected void ResetCooldown()
    {
        if (Object.HasStateAuthority)
        {
            _attackCooldownTimer = TickTimer.CreateFromSeconds(Runner, _attackCooldown);
        }
    }
}