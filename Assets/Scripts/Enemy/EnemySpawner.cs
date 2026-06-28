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

    [Header("//Debug Info Mạng")]
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

        if (!Object.HasStateAuthority)
        {
            // Khi host cũ thoát, master client mới tự tiếp quản authority
            if (Runner != null && Runner.IsSharedModeMasterClient)
                Object.RequestStateAuthority();
            return;
        }

        // Tính _isActive từ local trigger count SAU KHI có authority
        // (Không gán trong OnTriggerEnter vì RequestStateAuthority() là async)
        _isActive = _localPlayersInTrigger > 0;

        if (!_isActive)
        {
            _spawnTimer = TickTimer.None;
            return;
        }

        if (_currentCount < _maxEnemies && _spawnTimer.ExpiredOrNotRunning(Runner))
        {
            SpawnEnemyNetworked();
            _spawnTimer = TickTimer.CreateFromSeconds(Runner, _spawnInterval);
        }
    }

    private void SpawnEnemyNetworked()
    {
        Vector2 spawnPos = GetRandomPositionInBounds();

        // 1. Sinh đối tượng mạng bằng NetworkRunner thay cho Instantiate
        NetworkObject newEnemyObj = Runner.Spawn(_enemyPrefab, spawnPos, Quaternion.identity, Object.InputAuthority);

        if (newEnemyObj != null)
        {
            if (newEnemyObj.TryGetComponent(out Health healthScript))
            {
                healthScript.SetupSpawner(this);
                _currentCount++; 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _localPlayersInTrigger++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _localPlayersInTrigger = Mathf.Max(0, _localPlayersInTrigger - 1);
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