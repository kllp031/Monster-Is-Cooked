using System;
using UnityEngine;
using Fusion;

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
    [SerializeField] private Transform _firePoint;

    [Header("CircleSpread Settings")]
    [SerializeField] private int _bulletCount = 6;

    private void Start()
    {
        if (_firePoint == null) _firePoint = transform;
    }

    public override void PerformAttack()
    {
        // 1. CHỈ máy nắm quyền sinh quái (Master Client) mới được tính toán bắn đạn
        if (Object != null && !Object.HasStateAuthority) return;

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

        // KIẾN THỨC KINH ĐIỂN: Lấy target từ lớp cha EnemyBase thay vì ép cứng lớp con WalkingEnemy
        EnemyBase baseEnemy = GetComponent<EnemyBase>();
        if (baseEnemy != null)
        {
            Transform currentTarget = baseEnemy.target;
            if (currentTarget != null)
            {
                direction = (currentTarget.position - _firePoint.position).normalized;
            }
        }

        ShootBullet(direction);
    }

    private void ShootBullet(Vector2 direction)
    {
        // 2. Tạo góc quay cục bộ cho viên đạn hướng về phía Player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 3. SINH ĐẠN QUA MẠNG: Thay thế hoàn toàn Instantiate
        NetworkObject bulletNetObj = Runner.Spawn(_bulletPrefab, _firePoint.position, rotation, Object.InputAuthority);

        if (bulletNetObj != null && bulletNetObj.TryGetComponent(out BulletMove bullet))
        {
            // Thiết lập hướng di chuyển cho script đạn mạng
            bullet.SetDirection(direction);
        }
    }

    private void ShootCircle()
    {
        if (_bulletPrefab == null || _bulletCount <= 0) return;

        float angleStep = 360f / _bulletCount;
        float angle = 0f;

        for (int i = 0; i < _bulletCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            SpawnBulletNetworked(dir);

            angle += angleStep;
        }
    }

    private void SpawnBulletNetworked(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 4. Sinh quái đạn vòng tròn qua mạng bằng lệnh của Runner
        NetworkObject bulletNetObj = Runner.Spawn(_bulletPrefab, _firePoint.position, rotation, Object.InputAuthority);

        if (bulletNetObj != null && bulletNetObj.TryGetComponent(out BulletMove bullet))
        {
            // Vì đạn bắn vòng tròn xoay theo trục, ta truyền thẳng hướng vector hoặc Vector2.up tùy logic BulletMove cũ
            bullet.SetDirection(direction);
        }
    }
}