using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManagerOnline : NetworkBehaviour
{
    [SerializeField] LevelDesignOnline levelDesign;
    // Spawning Settings
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<Vector2> spawnPositions = new();
    [SerializeField] List<PlayerRef> spawnedPlayers = new();

    [Networked] public int CollectedMoney { get; set; }
    [Networked] public float LevelStartTime { get; set; }
    [Networked] public bool LevelStarted { get; set; }
    [Networked] public bool GameStarted { get; set; }
    [Networked] public int LevelNumber { get; set; }

    // Scene-local singleton để UI dễ truy cập (không persistent, mỗi scene load lại).
    public static GameManagerOnline Instance { get; private set; }

    // Global event broadcast khi level được bắt đầu qua RPC (mọi client đều nhận).
    // UI subscribe để ẩn start screen đồng bộ.
    public static event Action OnLevelStarted;
    public static event Action<bool> OnLevelEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        if (NetworkManager.Instance == null) return;
        if (playerPrefab == null || spawnPositions.Count == 0) return;
        //RPC_AnnounceReady(NetworkManager.Instance.NetworkRunner.LocalPlayer);
        NetworkManager.Instance.NetworkRunner.Spawn(playerPrefab, spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count())], Quaternion.identity, NetworkManager.Instance.NetworkRunner.LocalPlayer);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Gọi từ MasterClient để bắt đầu level trên tất cả clients.
    /// Shared mode: mọi client có quyền gọi (RpcSources.All), nhưng caller tự gate
    /// (chỉ MasterClient mới nên gọi — EndStartUI đã check).
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartLevel()
    {
        Debug.Log("[GameManagerOnline] RPC_StartLevel received on " +
                  (NetworkManager.Instance != null && NetworkManager.Instance.NetworkRunner != null
                      ? NetworkManager.Instance.NetworkRunner.LocalPlayer.ToString()
                      : "unknown"));

        if (GameManager.Instance != null && GameManager.Instance.GameStarted)
        {
            GameManager.Instance.StartCurrentLevel();
        }
        else
        {
            Debug.LogWarning("[GameManagerOnline] RPC_StartLevel: GameManager null or GameStarted == false.");
        }

        OnLevelStarted?.Invoke();
    }

    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    //public void RPC_AnnounceReady(PlayerRef player)
    //{
    //    if (spawnedPlayers.Contains(player) || playerPrefab == null || spawnPositions.Count == 0) return;
    //    Debug.Log("Announce ready: " + player.PlayerId);
    //    var newPlayer = NetworkManager.Instance.NetworkRunner.Spawn(playerPrefab, spawnPositions[currentSpawnPosition], Quaternion.identity, player);
    //    if (newPlayer != null)
    //    {
    //        spawnedPlayers.Add(player);
    //        RPC_AnnounceSpawned(player);
    //    }

    //}

    //[Rpc(RpcSources.All, RpcTargets.All)]
    //public void RPC_AnnounceSpawned([RpcTarget] PlayerRef player)
    //{
    //    // Show announcement "Wait for Room Master to start the level"
    //    Debug.Log("Spawned successfully!");
    //}

    public void EndLevel()
    {

    }

    public LevelDetailOnline GetCurrentLevelDetail()
    {
        if (levelDesign == null) return null;
        return levelDesign.GetLevelDetail(LevelNumber);
    }

}
