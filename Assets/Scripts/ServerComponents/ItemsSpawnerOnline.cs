using Fusion;
using UnityEngine;

public class ItemsSpawnerOnline : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private PickupItemOnline _itemPrefab;
    [SerializeField] private int _maxItems = 5;
    [SerializeField] private float _spawnInterval = 3f;

    [Networked] private int CurrentCount { get; set; }
    [Networked] private NetworkBool _isActive { get; set; }
    [Networked] private TickTimer SpawnTimer { get; set; }

    private BoxCollider2D _spawnArea;
    private int _localPlayersInTrigger = 0;

    private void Awake()
    {
        _spawnArea = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _localPlayersInTrigger++;
        //Debug.Log($"[Spawner] Player ENTERED zone → _localPlayersInTrigger = {_localPlayersInTrigger}, RequestStateAuthority");
        Object.RequestStateAuthority();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _localPlayersInTrigger = Mathf.Max(0, _localPlayersInTrigger - 1);
        //Debug.Log($"[Spawner] Player EXITED zone → _localPlayersInTrigger = {_localPlayersInTrigger}, RequestStateAuthority");
        Object.RequestStateAuthority();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        _isActive = _localPlayersInTrigger > 0;

        if (!_isActive)
        {
            //Debug.Log("[Spawner] Không spawn: _isActive = false");
            return;
        }
        if (CurrentCount >= _maxItems)
        {
            //Debug.Log($"[Spawner] Không spawn: CurrentCount ({CurrentCount}) >= _maxItems ({_maxItems})");
            return;
        }
        if (!SpawnTimer.ExpiredOrNotRunning(Runner))
        {
            //Debug.Log($"[Spawner] Không spawn: Timer chưa hết ({SpawnTimer.RemainingTime(Runner):F1}s còn lại)");
            return;
        }

        SpawnItem();
        SpawnTimer = TickTimer.CreateFromSeconds(Runner, _spawnInterval);
    }

    private void SpawnItem()
    {
        Vector2 pos = GetRandomPositionInBounds();
        //Debug.Log($"[Spawner] Đang spawn item tại {pos}...");
        PickupItemOnline newItem = Runner.Spawn(_itemPrefab, pos, Quaternion.identity);
        if (newItem != null)
        {
            newItem.SetupItemSpawner(this);
            CurrentCount++;
            //Debug.Log($"[Spawner] Spawn thành công → CurrentCount = {CurrentCount}");
        }
        else
        {
            //Debug.LogWarning("[Spawner] Runner.Spawn trả về null!");
        }
    }

    private Vector2 GetRandomPositionInBounds()
    {
        Bounds b = _spawnArea.bounds;
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }

    public void OnItemCollected()
    {
        if (!Object.HasStateAuthority) return;
        CurrentCount = Mathf.Max(0, CurrentCount - 1);
        //Debug.Log($"[Spawner][StateAuth] OnItemCollected → CurrentCount = {CurrentCount}");
    }
}
