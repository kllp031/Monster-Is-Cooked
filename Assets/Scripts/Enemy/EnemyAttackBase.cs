using UnityEngine;

public abstract class EnemyAttackBase: MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] protected float _attackCooldown = 1.5f;

    protected float _lastAttackTime;
    public abstract void PerformAttack();

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackCooldown;
    }

    protected void ResetCooldown()
    {
        _lastAttackTime = Time.time;
    }
}
