using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [Header("Configuration")]
    private Transform _target; 
    [SerializeField] private EnemyAttackBase _melee_attack;
    [SerializeField] private EnemyAttackBase _ranged_attack;
    [SerializeField] private float rangedMeleeAttack;

    private void Start()
    {
        _target = GetComponent<WalkingEnemy>().target;
    }

    public void PerformBestAttack()
    {
        if (_target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, _target.position);

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
