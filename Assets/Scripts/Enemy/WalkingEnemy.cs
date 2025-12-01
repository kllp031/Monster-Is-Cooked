using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyBase-derived enemy class which walks in a direction until it hits a wall
/// </summary>
public class WalkingEnemy : EnemyBase
{
    [Header("Settings")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public float timeToIdle; //time to change state

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
    private float _patrolTimer;       
    private bool _isPatrolWaiting;
    private Vector2 _moveDirection;
    private float idleTimer = 0f;
    private float attackTimer = 0f;

    [Header("Hurt Settings")]
    [SerializeField] private float _hurtTime = 2f;
    private float hurtTimer;

    [Header("Sprite settings")]
    [SerializeField] private bool isFlip = false;

    protected override void Setup()
    {
        base.Setup();
        rb = GetComponent<Rigidbody2D>();
        dropItem = GetComponent<DropItem>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    void UpdateState()
    {
        float distance = Vector2.Distance(transform.position, target.position);

        switch (currentEnemyState)
        {
            case EnemyState.Idle:
                if (idleTimer >= timeToIdle )
                {
                    if (distance < chaseRange) currentEnemyState = EnemyState.Chase;
                    else currentEnemyState = EnemyState.Patrol;

                    idleTimer = 0;
                }
                else
                {
                    idleTimer += Time.deltaTime;
                }
                break;

            case EnemyState.Patrol:
                HandlePatrol();
                if (distance < chaseRange) currentEnemyState = EnemyState.Idle;
                break;

            case EnemyState.Chase:
                if (distance >= chaseRange) currentEnemyState = EnemyState.Idle;
                if (distance <= attackRange)
                {
                    currentEnemyState = EnemyState.Attack;
                    if (target.transform.position.x > transform.position.x)
                    {
                        Flip(1);
                    }
                    else
                        Flip(-1);
                }
                break;

            case EnemyState.Attack:
                //enemyAttack.PerformAttack();
                //enemy will call attack in animation event

                if (attackTimer >= timeToAttack)
                {
                    if (distance > attackRange) currentEnemyState = EnemyState.Idle;
                    attackTimer = 0;
                }
                else
                {
                    attackTimer += Time.deltaTime;
                }
                break;

            case EnemyState.Hurt:
                //hurt, health handle
                if (hurtTimer >= _hurtTime)
                {
                    currentEnemyState = EnemyState.Idle;
                    hurtTimer = 0;
                }
                else
                {
                    hurtTimer += Time.deltaTime;
                }
                break;
            case EnemyState.Dead:
                //dead, health handle
                break;
        }
    }

    protected override Vector3 GetMovement()
    {
        if (currentEnemyState == EnemyState.Chase)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            Flip((target.position - transform.position).x > 0 ? 1 : -1);
            return dir * moveSpeed * Time.deltaTime;
        }

        //patrol
        if (currentEnemyState == EnemyState.Patrol && !_isPatrolWaiting)
        {
            Flip(_moveDirection.x > 0 ? 1 : -1);
            return _moveDirection * _patrolSpeed * Time.deltaTime;
        }

        return Vector3.zero;
    }

    private void HandlePatrol()
    {
        _patrolTimer -= Time.deltaTime;
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
            Flip(_moveDirection.x > 0 ? 1 : -1);
        }
    }
    private bool CheckWallOrLedge()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDirection, _wallCheckDistance, _obstacleLayer);
        if (hit.collider != null) Debug.Log("var");
        return hit.collider != null;
    }
    private void Flip(int direction)
    {
        Vector3 scale = transform.localScale;
        if(isFlip)
            scale.x = Mathf.Abs(scale.x) * -direction;
        else
            scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }
}
