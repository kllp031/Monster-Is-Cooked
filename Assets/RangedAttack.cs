using System;
using UnityEngine;

public class RangedAttack : EnemyAttackBase
{
    public enum ShootType
    {
        SingleToTarget,
        CircleSpread
    }

    [Header("Ranged Specifics")]
    [SerializeField] private ShootType _shootType = ShootType.SingleToTarget;
    [SerializeField] private GameObject _bulletPrefab;
    private Transform _firePoint;

    [Header("CircleSpread Settings")]
    [SerializeField] private int _bulletCount = 6;
    private void Start()
    {
        _firePoint = transform;
    }

    public override void PerformAttack()
    {
        if (!CanAttack()) return;

        if (_shootType == ShootType.SingleToTarget)
        {
            Shoot();
        }
        else if (_shootType == ShootType.CircleSpread)
        {
            ShootCircle();
        }
        ResetCooldown();
    }

    private void Shoot()
    {
        if (_bulletPrefab == null || _firePoint == null) return;

        Vector2 direction = Vector2.right; 
        if (GetComponent<EnemyBase>() != null)
        {
            Transform target = GetComponent<WalkingEnemy>().target;
            if (target != null)
            {
                direction = (target.position - _firePoint.position).normalized;
            }
        }
        ShootBullet(direction);
    }

    private void ShootBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<BulletMove>().SetDirection(direction);
    }

    private void ShootCircle()
    {
        if (_bulletPrefab == null || _bulletCount <= 0) return;

        float angleStep = 360f / _bulletCount;
        float angle = 0f;

        for (int i = 0; i < _bulletCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            dir = dir.normalized;
            SpawnBullet(dir);

            angle += angleStep;
        }
    }
    private void SpawnBullet(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject bulletObj = Instantiate(_bulletPrefab, _firePoint.position, rotation);
        var bullet = bulletObj.GetComponent<BulletMove>();
        if (bullet != null)
        {
            //since the bullet is rotated, move it along its sprite direction.
            bullet.SetDirection(Vector2.up);
        }
    }
}