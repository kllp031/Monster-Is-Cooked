using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; 

/// <summary>
/// This class contains settings for and handles the control of an enemy in a network environment
/// </summary>
public abstract class EnemyBase : NetworkBehaviour
{
    [Header("Settings")]
    [Tooltip("How fast this enemy moves")]
    public float moveSpeed = 2f;

    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }

    // Biến mạng lưu trạng thái để đồng bộ Animation sang tất cả Client
    [Networked]
    public EnemyState currentEnemyState { get; set; }

    public EnemyState lastEnemyState = EnemyState.Idle;

    [Header("References")]
    public Transform target;  // Player
    protected Rigidbody2D rb;
    protected DropItem dropItem;

    [Header("Audio Settings")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip hurtSound;
    [SerializeField] protected AudioClip dieSound;

    public void PlayAttackSound()
    {
        if (attackSound != null)
            SoundManager.Instance.PlaySFX(attackSound);
    }
    public void PlayHurtSound()
    {
        if (hurtSound != null)
            SoundManager.Instance.PlaySFX(hurtSound);
    }
    public void PlayDieSound()
    {
        if (dieSound != null)
            SoundManager.Instance.PlaySFX(dieSound);
    }

    public void SetTarget(Transform player)
    {
        target = player;
    }

    // Thay thế Start() bằng Spawned() của Fusion
    public override void Spawned()
    {
        base.Spawned();
        rb = GetComponent<Rigidbody2D>();
        dropItem = GetComponent<DropItem>();
        Setup();
    }

    // Thay thế Update() bằng FixedUpdateNetwork (FUN) để đồng bộ theo nhịp Tick mạng.
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!Object.HasStateAuthority)
        {
            if (Runner != null && Runner.IsSharedModeMasterClient)
                Object.RequestStateAuthority();
        }

        // CỰC KỲ QUAN TRỌNG: Chỉ máy có quyền (Master Client/Spawner) mới được di chuyển quái
        if (Object.HasStateAuthority)
        {
            Vector3 movement = GetMovement();
            MoveEnemy(movement);
        }
    }

    protected virtual void Setup()
    {
        // Khởi tạo các giá trị ban đầu qua mạng nếu cần
        if (Object.HasStateAuthority)
        {
            currentEnemyState = EnemyState.Idle;
        }
    }

    protected virtual Vector3 GetMovement()
    {
        return Vector3.zero;
    }

    protected virtual void MoveEnemy(Vector3 movement)
    {
        // Nếu dùng Rigidbody2D (khuyên dùng cho 2D Combat), bạn nên sửa thành rb.velocity hoặc rb.MovePosition
        // Ở đây tạm thời giữ nguyên logic dịch chuyển transform của bạn, nhưng bọc trong FUN phía trên
        transform.position = transform.position + movement;
    }
}