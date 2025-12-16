using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Game/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [System.Serializable]
    public struct LevelData
    {
        // ============ HEALTH ============
        [Header("Health")]
        [Tooltip("Value when you reach this level")]
        public int health;
        [Tooltip("Cost to upgrade TO this level")]
        public int healthCost;

        // ============ SPEED ============
        [Header("Speed")]
        public float speed;
        public int speedCost;

        // ============ ATTACK ============
        [Header("Attack")]
        public int attack;
        public int attackCost;

        // ============ MONEY MULTIPLIER ============
        [Header("Bonus Money")]
        public int bonusMoney;
        public int bonusMoneyCost;
    }

    [Header("Configuration")]
    public int maxLevel = 10;

    [Header("Stat Progression")]
    // Element 0 = Level 1 (Starting Stats, usually Cost 0)
    // Element 1 = Level 2 (Stats you get, and Cost to buy them)
    public LevelData[] levels;

    private void OnValidate()
    {
        if (levels == null || levels.Length != maxLevel)
        {
            System.Array.Resize(ref levels, maxLevel);
        }
    }
}