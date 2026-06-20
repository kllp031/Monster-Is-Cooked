using UnityEngine;
using UnityEngine.Events;
using Fusion;

/// <summary>
/// Per-player networked data manager. Attach to OnlinePlayer prefab.
/// Fresh start each session — no PlayerPrefs. Stats come from PlayerStatsConfig.
/// State authority (host) owns all [Networked] values; upgrades route through RPCs.
/// </summary>
public class PlayerDataManagerOnline : NetworkBehaviour
{
    public enum StatType { Health, Speed, Attack, BonusMoney }

    [Header("Data Source")]
    [SerializeField] private PlayerStatsConfig statsConfig;

    [Header("Events (local client only)")]
    public UnityEvent onHealthUpgrade = new UnityEvent();
    public UnityEvent onSpeedUpgrade = new UnityEvent();
    public UnityEvent onAttackUpgrade = new UnityEvent();
    public UnityEvent onMoneyBonusUpgrade = new UnityEvent();
    public UnityEvent onMoneyChanged = new UnityEvent();

    // ==================== NETWORKED STATE ====================

    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public int HealthLevel { get; set; }
    [Networked] public int SpeedLevel { get; set; }
    [Networked] public int AttackLevel { get; set; }
    [Networked] public int BonusMoneyLevel { get; set; }
    [Networked] public int TotalMoney { get; set; }

    // ==================== DERIVED RUNTIME VALUES ====================
    // Read these instead of [Networked] props so callers don't need to know level indices.

    public int CurrentMaxHealth => StatValue(HealthLevel).health;
    public float CurrentSpeed => StatValue(SpeedLevel).speed;
    public int CurrentAttack => StatValue(AttackLevel).attack;
    public int CurrentBonusMoney => StatValue(BonusMoneyLevel).bonusMoney;

    // ==================== LIFECYCLE ====================

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            HealthLevel = 1;
            SpeedLevel = 1;
            AttackLevel = 1;
            BonusMoneyLevel = 1;
            TotalMoney = 0;
        }

        if (Object.HasInputAuthority)
        {
            LocalPlayerData.Register(this);
            RPC_SetPlayerName(NetworkManager.Instance != null ? NetworkManager.Instance.Username : "Player");
        }
    }

    // Fusion calls this on every client whenever a [Networked] value changes
    public override void Render()
    {
        // Events fire locally so UI on this client updates
        // (Only fire for the player this client controls to avoid spamming other players' UI)
        if (!Object.HasInputAuthority) return;

        onHealthUpgrade.Invoke();
        onSpeedUpgrade.Invoke();
        onAttackUpgrade.Invoke();
        onMoneyBonusUpgrade.Invoke();
        onMoneyChanged.Invoke();
    }

    // ==================== UPGRADE ====================

    public bool TryUpgradeStat(StatType type)
    {
        if (!Object.HasInputAuthority) return false;

        int currentLevel = GetCurrentLevel(type);
        if (currentLevel >= statsConfig.maxLevel)
        {
            Debug.Log("Already at max level.");
            return false;
        }

        int cost = GetCostForLevel(type, currentLevel);
        if (TotalMoney < cost)
        {
            Debug.Log($"Not enough money. Need {cost}, have {TotalMoney}");
            return false;
        }

        RPC_ApplyUpgrade(type, currentLevel + 1, TotalMoney - cost);
        return true;
    }

    public int GetNextUpgradeCost(StatType type)
    {
        int currentLevel = GetCurrentLevel(type);
        if (currentLevel >= statsConfig.maxLevel) return -1;
        return GetCostForLevel(type, currentLevel);
    }

    // ==================== MONEY ====================

    public void EarnMoney(int amount)
    {
        if (!Object.HasStateAuthority) return;
        TotalMoney += amount;
    }

    // ==================== RPC ====================

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ApplyUpgrade(StatType type, int newLevel, int newMoney)
    {
        TotalMoney = newMoney;
        switch (type)
        {
            case StatType.Health:      HealthLevel = newLevel;      break;
            case StatType.Speed:       SpeedLevel = newLevel;       break;
            case StatType.Attack:      AttackLevel = newLevel;      break;
            case StatType.BonusMoney:  BonusMoneyLevel = newLevel;  break;
        }
    }

    // ==================== HELPERS ====================

    public int GetCurrentLevel(StatType type)
    {
        switch (type)
        {
            case StatType.Health:      return HealthLevel;
            case StatType.Speed:       return SpeedLevel;
            case StatType.Attack:      return AttackLevel;
            case StatType.BonusMoney:  return BonusMoneyLevel;
            default: return 1;
        }
    }

    private int GetCostForLevel(StatType type, int index)
    {
        if (index >= statsConfig.levels.Length) return 999999;
        switch (type)
        {
            case StatType.Health:      return statsConfig.levels[index].healthCost;
            case StatType.Speed:       return statsConfig.levels[index].speedCost;
            case StatType.Attack:      return statsConfig.levels[index].attackCost;
            case StatType.BonusMoney:  return statsConfig.levels[index].bonusMoneyCost;
            default: return 0;
        }
    }

    private PlayerStatsConfig.LevelData StatValue(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, statsConfig.levels.Length - 1);
        return statsConfig.levels[index];
    }
}
