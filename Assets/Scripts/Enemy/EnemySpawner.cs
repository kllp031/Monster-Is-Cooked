using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemySpawner : NetworkBehaviour 
{
    [Header("Settings")]
    [SerializeField] private GameObject _enemyPrefab; 
    [SerializeField] private int _maxEnemies = 10;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private LayerMask _playerLayer;   

    [Header("Debug Info Mạng")]
    [Networked] private int _currentCount { get; set; }
    [Networked] private NetworkBool _isActive { get; set; }
    [Networked] private TickTimer _spawnTimer { get; set; }

    private BoxCollider2D _spawnArea;
    // Đếm số lượng người chơi local đang đứng trong Trigger va chạm
    private int _localPlayersInTrigger = 0;

    private void Awake()
    {
        _spawnArea = GetComponent<BoxCollider2D>();
    }

    public override void Spawned()
    {
        base.Spawned();
        // Khởi tạo các giá trị ban đầu trên máy chủ (máy nắm State Authority)
        if (Object.HasStateAuthority)
        {
            _currentCount = 0;
            _isActive = false;
            _spawnTimer = TickTimer.None;
        }
    }

    // Vòng lặp mạng chạy đồng bộ cố định (FixedUpdateNetwork - FUN)
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        // CHỈ host mới xử lý logic sinh quái
        if (!Object.HasStateAuthority) return;

        if (_isActive && _currentCount < _maxEnemies)
        {
            // Kiểm tra xem đồng hồ đếm ngược hết hạn hoặc chưa chạy không
            if (_spawnTimer.ExpiredOrNotRunning(Runner))
            {
                SpawnEnemyNetworked();
                // Khởi tạo lại đồng hồ đếm ngược cho lượt đẻ quái tiếp theo
                _spawnTimer = TickTimer.CreateFromSeconds(Runner, _spawnInterval);
            }
        }
    }

    private void SpawnEnemyNetworked()
    {
        Vector2 spawnPos = GetRandomPositionInBounds();

        // 1. Sinh đối tượng mạng bằng NetworkRunner thay cho Instantiate
        NetworkObject newEnemyObj = Runner.Spawn(_enemyPrefab, spawnPos, Quaternion.identity, Object.InputAuthority);

        if (newEnemyObj != null)
        {
            // 2. Tự động tìm kiếm Player thực tế gần con quái này nhất trong Multiplayer
            Transform closestPlayer = FindClosestPlayer(spawnPos);

            if (newEnemyObj.TryGetComponent(out EnemyBase enemyScript))
            {
                enemyScript.SetTarget(closestPlayer);
            }

            if (newEnemyObj.TryGetComponent(out Health healthScript))
            {
                healthScript.SetupSpawner(this);
                _currentCount++; 
            }
        }
    }

    // Thuật toán quét tìm mục tiêu trong Multiplayer
    private Transform FindClosestPlayer(Vector2 spawnPos)
    {
        // Quét tất cả Collider thuộc lớp người chơi trong bán kính lớn
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(spawnPos, 30f, _playerLayer);
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                float dist = Vector2.Distance(spawnPos, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = hit.transform;
                }
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _localPlayersInTrigger++;

            // Nếu có ít nhất 1 người chơi bước vào, chuyển trạng thái mạng sang Active
            if (_localPlayersInTrigger > 0)
            {
                // Yêu cầu chiếm quyền điều khiển Spawner để sửa dữ liệu nếu máy local chưa có quyền
                Object.RequestStateAuthority();
                _isActive = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _localPlayersInTrigger--;
            if (_localPlayersInTrigger < 0) _localPlayersInTrigger = 0;

            // Khi không còn bất kỳ người chơi nào đứng trong vùng, tắt Spawner mạng
            if (_localPlayersInTrigger == 0)
            {
                Object.RequestStateAuthority();
                _isActive = false;
                _spawnTimer = TickTimer.None; // Reset timer mạng
            }
        }
    }

    // Hàm nhận tin nhắn từ Health.cs khi quái bị hạ gục
    public void OnEnemyDeath()
    {
        // Biến [Networked] chỉ được chỉnh sửa bởi máy giữ StateAuthority (Master Client)
        if (Object.HasStateAuthority)
        {
            _currentCount--;
            if (_currentCount < 0) _currentCount = 0;
        }
    }

    private Vector2 GetRandomPositionInBounds()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }
}