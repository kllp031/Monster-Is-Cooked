using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; 

/// <summary>
/// EnemyBase-derived enemy class which walks in a direction until it hits a wall (Networked)
/// </summary>
public class WalkingEnemy : EnemyBase
{
    [Header("Settings")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float timeToIdle; 

    [Header("Attack")]
    public EnemyAttackBase enemyAttack;
    public float timeToAttack;

    [Header("Patrol Settings")]
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _minMoveTime = 2f;
    [SerializeField] private float _maxMoveTime = 5f;
    [SerializeField] private float _waitStayTime = 1.5f;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _wallCheckDistance = 1f;

    [Networked] private int _networkFacingDirection { get; set; } = 1;

    private float _patrolTimer;
    private bool _isPatrolWaiting;
    private Vector2 _moveDirection;
    private float idleTimer = 0f;
    private float attackTimer = 0f;

    [Header("Hurt Settings")]
    [SerializeField] private float _hurtTime = 2f;
    private float hurtTimer;

    [Header("Multiplayer AI Settings")]
    [SerializeField] private LayerMask _playerLayer; // Gán layer "Player" trong Inspector
    [SerializeField] private float _targetScanInterval = 0.5f; // Quét lại mục tiêu mỗi 0.5 giây
    private float _scanTimer = 0f;

    [Header("Sprite settings")]
    [SerializeField] private bool isFlip = false;

    protected override void Setup()
    {
        base.Setup();
        // rb và dropItem đã được xử lý ở base.Spawned() trong EnemyBase
        if (Object.HasStateAuthority)
        {
            _networkFacingDirection = 1;
        }
    }

    // KHÔNG dùng Update() cho logic mạng. Đưa toàn bộ vào FixedUpdateNetwork của EnemyBase.
    // Hàm này sẽ được base.FixedUpdateNetwork() gọi NẾU máy có StateAuthority
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Bộ đếm thời gian quét mục tiêu (Tránh việc quét mỗi frame gây sụt giảm hiệu năng - CPU spike)
            _scanTimer += Runner.DeltaTime;
            if (_scanTimer >= _targetScanInterval || target == null)
            {
                FindClosestPlayerNetworked();
                _scanTimer = 0f;
            }

            // Nếu đã tìm thấy người chơi, cho phép chạy FSM để quái di chuyển/tấn công
            if (target != null)
            {
                UpdateStateAndTimers();
            }
            else
            {
                // Nếu không có ai trong vùng, ép quái về trạng thái Idle hoặc Patrol
                if (currentEnemyState == EnemyState.Chase || currentEnemyState == EnemyState.Attack)
                {
                    currentEnemyState = EnemyState.Idle;
                    idleTimer = 0f;
                }

                // Chạy tuần tra tự động nếu không có người chơi xung quanh
                UpdateStateAndTimers();
            }
        }

        // Gọi base để thực thi di chuyển vị trí (MoveEnemy) lên mạng
        base.FixedUpdateNetwork();
    }

    // Hàm Render chạy liên tục mỗi frame trên TẤT CẢ các máy để xử lý đồ họa mượt mà (UI, Flip, Animation)
    public override void Render()
    {
        base.Render();
        // Áp dụng hướng quay mặt nhận được từ mạng lên Sprite cục bộ
        ApplySpriteFlip(_networkFacingDirection);
    }

    void UpdateStateAndTimers()
    {
        float distance = (target != null) ? Vector2.Distance(transform.position, target.position) : float.MaxValue;
        float dt = Runner.DeltaTime; // Dùng DeltaTime của Fusion thay cho Time.deltaTime

        switch (currentEnemyState)
        {
            case EnemyState.Idle:
                if (idleTimer >= timeToIdle)
                {
                    if (distance < chaseRange) currentEnemyState = EnemyState.Chase;
                    else currentEnemyState = EnemyState.Patrol;

                    idleTimer = 0;
                }
                else
                {
                    idleTimer += dt;
                }
                break;

            case EnemyState.Patrol:
                HandlePatrol(dt);
                if (distance < chaseRange)
                {
                    currentEnemyState = EnemyState.Idle;
                    idleTimer = 0;
                }
                break;

            case EnemyState.Chase:
                if (distance >= chaseRange)
                {
                    currentEnemyState = EnemyState.Idle;
                    idleTimer = 0;
                }
                if (distance <= attackRange)
                {
                    currentEnemyState = EnemyState.Attack;
                    attackTimer = 0;
                    NetworkFlip(target.transform.position.x > transform.position.x ? 1 : -1);
                }
                break;

            case EnemyState.Attack:
                if (attackTimer >= timeToAttack)
                {
                    currentEnemyState = EnemyState.Idle;
                    idleTimer = 0;
                    attackTimer = 0;
                }
                else
                {
                    attackTimer += dt;
                }
                break;

            case EnemyState.Hurt:
                if (hurtTimer >= _hurtTime)
                {
                    currentEnemyState = EnemyState.Idle;
                    idleTimer = 0;
                    hurtTimer = 0;
                }
                else
                {
                    hurtTimer += dt;
                }
                break;

            case EnemyState.Dead:
                break;
        }
    }

    protected override Vector3 GetMovement()
    {
        // Hàm này chỉ được base gọi khi có StateAuthority, an toàn để tính toán vị trí
        float dt = Runner.DeltaTime;

        if (currentEnemyState == EnemyState.Chase && target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            NetworkFlip((target.position - transform.position).x > 0 ? 1 : -1);
            return dir * moveSpeed * dt;
        }

        if (currentEnemyState == EnemyState.Patrol && !_isPatrolWaiting)
        {
            NetworkFlip(_moveDirection.x > 0 ? 1 : -1);
            return _moveDirection * _patrolSpeed * dt;
        }

        return Vector3.zero;
    }

    void FindClosestPlayerNetworked()
    {
        // Quét toàn bộ Collider thuộc layer Player trong bán kính đuổi theo (chaseRange)
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, chaseRange, _playerLayer);

        Transform closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                // Kiểm tra xem Player đó còn sống không (gắn với script Health mạng)
                if (hit.TryGetComponent(out Health playerHealth) && playerHealth.isDeath)
                    continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPlayer = hit.transform;
                }
            }
        }

        // Gán mục tiêu tìm được cho quái
        target = closestPlayer;
    }

    private void HandlePatrol(float dt)
    {
        _patrolTimer -= dt;
        if (_patrolTimer <= 0)
        {
            if (_isPatrolWaiting)
            {
                _isPatrolWaiting = false;
                SetRandomPatrolTime();
                PickRandomDirection();
            }
            else
            {
                _isPatrolWaiting = true;
                _patrolTimer = _waitStayTime;
                _moveDirection = Vector2.zero;
            }
        }

        if (!_isPatrolWaiting && CheckWallOrLedge())
        {
            PickRandomDirection();
            SetRandomPatrolTime();
        }
    }

    private void SetRandomPatrolTime()
    {
        _patrolTimer = Random.Range(_minMoveTime, _maxMoveTime);
    }

    private void PickRandomDirection()
    {
        _moveDirection = Random.insideUnitCircle.normalized;
        if (_moveDirection.x != 0)
        {
            NetworkFlip(_moveDirection.x > 0 ? 1 : -1);
        }
    }

    private bool CheckWallOrLedge()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDirection, _wallCheckDistance, _obstacleLayer);
        return hit.collider != null;
    }

    // Chỉ cập nhật biến Networked, không chỉnh localScale trực tiếp ở luồng AI
    private void NetworkFlip(int direction)
    {
        _networkFacingDirection = direction;
    }

    // Hàm thực thi đổi scale thật (Chỉ chạy trong Render() để đồng bộ mượt mà đồ họa)
    private void ApplySpriteFlip(int direction)
    {
        Vector3 scale = transform.localScale;
        if (isFlip)
            scale.x = Mathf.Abs(scale.x) * -direction;
        else
            scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    // Photon Fusion tự động quản lý vòng đời hủy đối tượng qua mạng,
    // Tuyệt đối KHÔNG dùng Destroy(gameObject) của Unity thường.
    public void OnNetworkDespawn()
    {
        if (Object != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}