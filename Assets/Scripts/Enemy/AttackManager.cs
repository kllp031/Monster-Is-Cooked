using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyAttackBase _melee_attack;
    [SerializeField] private EnemyAttackBase _ranged_attack;
    [SerializeField] private float rangedMeleeAttack;

    private Transform Target => GetComponent<WalkingEnemy>().target;

    public void PerformBestAttack()
    {
        Transform target = Target;
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        EnemyAttackBase bestAttack = SelectBestAttack(distanceToTarget);

        if (bestAttack != null && bestAttack.CanAttack())
        {
            bestAttack.PerformAttack();
        }
    }    

    private EnemyAttackBase SelectBestAttack(float distance)
    {
        if (distance > rangedMeleeAttack)
        {
            return _ranged_attack;
        }
        else
        {
            return _melee_attack;
        }
    }
}
