using UnityEngine;
using UnityEngine.Events;

public class PlayerDataManager : MonoBehaviour
{
    public enum StatType
    {
        Health,
        Speed,
        Attack,
        MoneyMul
    }

    public static PlayerDataManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private PlayerStatsConfig statsConfig;

    [Header("Events")]
    // Make these public so UI scripts can drag-and-drop into them or subscribe via code
    public UnityEvent onHealthUpgrade = new UnityEvent();
    public UnityEvent onSpeedUpgrade = new UnityEvent();
    public UnityEvent onAttackUpgrade = new UnityEvent();
    public UnityEvent onMoneyMulUpgrade = new UnityEvent();
    public UnityEvent onMoneyChanged = new UnityEvent();
    public UnityEvent onLevelChanged = new UnityEvent();

    // ==================== RUNTIME VALUES ====================
    // We use { get; private set; } and update them in UpdateRuntimeValues
    // This prevents the Stack Overflow crash you had in Health
    public int CurrentMaxHealth { get; private set; }
    public float CurrentSpeed { get; private set; }
    public int CurrentAttack { get; private set; }
    public float CurrentMoneyMul { get; private set; }

    public int TotalMoney { get; private set; }
    public int Level { get; private set; }

    // Levels (Internal tracking)
    public int HealthLevel { get; private set; }
    public int SpeedLevel { get; private set; }
    public int AttackLevel { get; private set; }
    public int MoneyMulLevel { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }
        else { Destroy(gameObject); }
    }

    // ==================== UPGRADE LOGIC ====================

    public bool TryUpgradeStat(StatType type)
    {
        int currentLevel = GetCurrentLevel(type);

        if (currentLevel >= statsConfig.maxLevel)
        {
            Debug.Log("Already at Max Level.");
            return false;
        }

        int cost = GetCostForLevelIndex(type, currentLevel); // Cost of next level

        if (TotalMoney < cost)
        {
            Debug.Log($"Not enough money! Need {cost}, have {TotalMoney}");
            return false;
        }

        // Pay up
        TotalMoney -= cost;
        PlayerPrefs.SetInt("Wallet_Money", TotalMoney);
        onMoneyChanged.Invoke(); // <--- Signal Money Changed

        // Apply Upgrade
        ApplyUpgrade(type, currentLevel + 1);

        // Refresh Stats & Trigger Events
        UpdateRuntimeValues(type); // We pass the type to know exactly which event to fire
        Save();

        return true;
    }

    public int GetNextUpgradeCost(StatType type)
    {
        int currentLevel = GetCurrentLevel(type);
        if (currentLevel >= statsConfig.maxLevel) return -1;
        return GetCostForLevelIndex(type, currentLevel);
    }

    // ==================== MONEY LOGIC ====================

    public void EarnMoney(int baseAmount)
    {
        //int finalAmount = Mathf.RoundToInt(baseAmount * CurrentMoneyMul);
        //TotalMoney += finalAmount;
        TotalMoney += baseAmount;

        PlayerPrefs.SetInt("Wallet_Money", TotalMoney);

        // TRIGGER SIGNAL
        onMoneyChanged.Invoke();
    }

    // ==================== INTERNAL UPDATES ====================

    private void LoadAll()
    {
        HealthLevel = PlayerPrefs.GetInt("Level_Health", 1);
        SpeedLevel = PlayerPrefs.GetInt("Level_Speed", 1);
        AttackLevel = PlayerPrefs.GetInt("Level_Attack", 1);
        MoneyMulLevel = PlayerPrefs.GetInt("Level_MoneyMul", 1);
        TotalMoney = PlayerPrefs.GetInt("Wallet_Money", 0);
        Level = PlayerPrefs.GetInt("Level_Reached", 1);

        // Update ALL stats at start so UI is correct
        UpdateRuntimeValues(null);
    }

    /// <summary>
    /// Recalculates stats from the Config.
    /// If specificType is null, it updates EVERYTHING (good for initialization).
    /// </summary>
    private void UpdateRuntimeValues(StatType? specificType = null)
    {
        var levels = statsConfig.levels;
        int maxIndex = levels.Length - 1;

        // 1. Recalculate Values
        CurrentMaxHealth = levels[Mathf.Clamp(HealthLevel - 1, 0, maxIndex)].health;
        CurrentSpeed = levels[Mathf.Clamp(SpeedLevel - 1, 0, maxIndex)].speed;
        CurrentAttack = levels[Mathf.Clamp(AttackLevel - 1, 0, maxIndex)].attack;
        CurrentMoneyMul = levels[Mathf.Clamp(MoneyMulLevel - 1, 0, maxIndex)].moneyMultiplier;

        // 2. Trigger Events
        // If specificType is null, we fire ALL events (happens on Load)
        // Otherwise, we only fire the specific event to save performance

        if (specificType == null || specificType == StatType.Health)
            onHealthUpgrade.Invoke();

        if (specificType == null || specificType == StatType.Speed)
            onSpeedUpgrade.Invoke();

        if (specificType == null || specificType == StatType.Attack)
            onAttackUpgrade.Invoke();

        if (specificType == null || specificType == StatType.MoneyMul)
            onMoneyMulUpgrade.Invoke();

        if (specificType == null)
            onMoneyChanged.Invoke();
    }

    // ... GetCurrentLevel, GetCostForLevelIndex, ApplyUpgrade helper methods remain the same ...
    // (Included for completeness of the script if you copy-paste)
    public int GetCurrentLevel(StatType type)
    {
        switch (type)
        {
            case StatType.Health: return HealthLevel;
            case StatType.Speed: return SpeedLevel;
            case StatType.Attack: return AttackLevel;
            case StatType.MoneyMul: return MoneyMulLevel;
            default: return 1;
        }
    }

    private int GetCostForLevelIndex(StatType type, int index)
    {
        if (index >= statsConfig.levels.Length) return 999999;
        switch (type)
        {
            case StatType.Health: return statsConfig.levels[index].healthCost;
            case StatType.Speed: return statsConfig.levels[index].speedCost;
            case StatType.Attack: return statsConfig.levels[index].attackCost;
            case StatType.MoneyMul: return statsConfig.levels[index].moneyMulCost;
            default: return 0;
        }
    }

    private void ApplyUpgrade(StatType type, int newLevel)
    {
        switch (type)
        {
            case StatType.Health: HealthLevel = newLevel; PlayerPrefs.SetInt("Level_Health", HealthLevel); break;
            case StatType.Speed: SpeedLevel = newLevel; PlayerPrefs.SetInt("Level_Speed", SpeedLevel); break;
            case StatType.Attack: AttackLevel = newLevel; PlayerPrefs.SetInt("Level_Attack", AttackLevel); break;
            case StatType.MoneyMul: MoneyMulLevel = newLevel; PlayerPrefs.SetInt("Level_MoneyMul", MoneyMulLevel); break;
        }
    }

    public void Save() { PlayerPrefs.Save(); }

    [ContextMenu("Add 100 Money for Testing")]
    public void AddTestMoney()
    {
        TotalMoney += 100;
        PlayerPrefs.SetInt("Wallet_Money", TotalMoney);
        onMoneyChanged.Invoke();
    }

    [ContextMenu("Reset All Player Data")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        LoadAll();
    }
}