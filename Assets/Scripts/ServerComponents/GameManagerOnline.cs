using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManagerOnline : NetworkBehaviour
{
    [SerializeField] LevelDesignOnline levelDesign;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<Vector2> spawnPositions = new();

    [Networked, OnChangedRender(nameof(OnCollectedMoneyRender))]
    public int CollectedMoney { get; set; }
    [Networked] public float LevelStartTime { get; set; }
    [Networked] public bool LevelStarted { get; set; }
    [Networked] public bool GameStarted { get; set; }
    [Networked] public int LevelNumber { get; set; }
    [Networked] public int RetryReadyCount { get; set; }
    [Networked] public bool IsWaitingForRetry { get; set; }

    public static GameManagerOnline Instance { get; private set; }
    public bool IsReady { get; private set; }

    public static event Action OnLevelStarted;
    public static event Action<bool> OnLevelEnd;
    public static event Action OnCollectedMoneyChanged;
    public static event Action OnNextLevelReady;
    // Fires when the networked GameManagerOnline finishes spawning — level detail is now available.
    public static event Action OnReady;

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
        if (Object.HasStateAuthority)
        {
            GameStarted = true;
            LevelNumber = 0;
        }

        IsReady = true;
        OnReady?.Invoke();

        if (NetworkManager.Instance == null) return;
        if (playerPrefab == null || spawnPositions.Count == 0) return;
        NetworkManager.Instance.NetworkRunner.Spawn(
            playerPrefab,
            spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)],
            Quaternion.identity,
            NetworkManager.Instance.NetworkRunner.LocalPlayer);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnCollectedMoneyRender() => OnCollectedMoneyChanged?.Invoke();

    /// <summary>
    /// Called by MasterClient (gated in EndStartUIOnline). Sets networked state on StateAuthority,
    /// then broadcasts OnLevelStarted to all clients so UI hides the start screen in sync.
    /// Uses Runner.SimulationTime (not Time.time) so CustomersSpawnerOnline elapsed-time calc is correct.
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartLevel()
    {
        //Debug.Log("[GameManagerOnline] RPC_StartLevel on " + (Runner != null ? Runner.LocalPlayer.ToString() : "unknown"));

        if (Object.HasStateAuthority)
        {
            if (!GameStarted)
            {
                //Debug.LogWarning("[GameManagerOnline] RPC_StartLevel: GameStarted == false.");
                return;
            }

            // Sum bonus money from all spawned players so every player's BonusMoney attribute counts.
            int bonusMoney = 0;
            foreach (var pdm in FindObjectsByType<PlayerDataManagerOnline>(FindObjectsSortMode.None))
                bonusMoney += pdm.CurrentBonusMoney;

            CollectedMoney = bonusMoney;
            LevelStartTime = Runner.SimulationTime;
            LevelStarted = true;
            RetryReadyCount = 0;
            IsWaitingForRetry = false;
        }

        OnLevelStarted?.Invoke();
    }

    /// <summary>
    /// Called by CustomersSpawnerOnline (MasterClient) when all customers have left.
    /// Only StateAuthority writes networked state and sends the end broadcast.
    /// </summary>
    public void EndLevel()
    {
        if (!Object.HasStateAuthority) return;
        if (!LevelStarted || !GameStarted)
        {
            //Debug.LogWarning("[GameManagerOnline] EndLevel: no level in progress.");
            return;
        }

        LevelStarted = false;

        var levelDetail = GetCurrentLevelDetail();
        bool win = levelDetail != null && CollectedMoney >= levelDetail.TargetMoney;
        bool isLastLevel = !levelDesign.CheckValidLevel(LevelNumber + 1);

        //Debug.Log($"[GameManagerOnline] EndLevel — win:{win} lastLevel:{isLastLevel}");
        RPC_BroadcastLevelEnd(win, win && isLastLevel);
    }

    /// <summary>
    /// Received on all clients. Each client earns money for its own local player
    /// (valid because each PlayerDataManagerOnline has StateAuthority over itself).
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastLevelEnd(bool win, bool gameOver)
    {
        if (Object.HasStateAuthority)
        {
            RetryReadyCount = 0;
            IsWaitingForRetry = !win;
        }

        LocalPlayerData.Instance?.Data?.EarnMoney(CollectedMoney);

        OnLevelEnd?.Invoke(win);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerPressedRetry()
    {
        RetryReadyCount++;
    }

    /// <summary>
    /// Called by any player when they die. Routes to StateAuthority (MasterClient) which
    /// checks if all players are dead and ends the level immediately if so.
    /// deadPlayerId is passed explicitly to avoid race conditions where IsDeath may not
    /// have replicated to MasterClient yet at the time this RPC arrives.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_NotifyPlayerDied(NetworkId deadPlayerId)
    {
        if (!LevelStarted || !GameStarted) return;

        foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (!h.CompareTag("Player")) continue;
            if (h.Object.Id == deadPlayerId) continue;
            if (!h.isDeath) return;
        }

        EndLevel();
    }

    /// <summary>
    /// Called by MasterClient to advance LevelNumber.
    /// [Networked] LevelNumber replicates automatically — no RPC needed.
    /// </summary>
    public void NextLevel()
    {
        if (!Object.HasStateAuthority) return;
        if (levelDesign.CheckValidLevel(LevelNumber + 1))
            LevelNumber++;
        RPC_BroadcastNextLevel();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastNextLevel()
    {
        OnNextLevelReady?.Invoke();
    }

    public LevelDetailOnline GetCurrentLevelDetail()
    {
        if (levelDesign == null) return null;
        return levelDesign.GetLevelDetail(LevelNumber);
    }
}
