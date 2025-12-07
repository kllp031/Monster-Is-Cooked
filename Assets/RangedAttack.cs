using UnityEngine;

public class RangedAttack : EnemyAttackBase
{
    [Header("Ranged Specifics")]
    [SerializeField] private GameObject _bulletPrefab;
    private Transform _firePoint;

    private void Start()
    {
        _firePoint = transform;
    }

    public override void PerformAttack()
    {
        if (!CanAttack()) return;

        Shoot();
        ResetCooldown();
    }

    private void Shoot()
    {
        if (_bulletPrefab == null || _firePoint == null) return;

        var bulletObj = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
        Vector2 direction = Vector2.right; 
        if (GetComponent<EnemyBase>() != null)
        {
            Transform target = GetComponent<WalkingEnemy>().target;
            if (target != null)
            {
                direction = (target.position - _firePoint.position).normalized;
            }
        }
        bulletObj.GetComponent<BulletMove>().SetDirection(direction);
    }
}