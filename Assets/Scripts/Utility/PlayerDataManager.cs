using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("Default Values (Editable in Inspector)")]
    [SerializeField] private int defaultMaxHealth = 100;
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private int defaultAttack = 10;
    [SerializeField] private int defaultLevel = 1;
    [SerializeField] private int defaultEarnMoney = 0;

    // Runtime cache
    public int MaxHealth { get; private set; }
    public float Speed { get; private set; }
    public int Attack { get; private set; }
    public int Level { get; private set; }
    public int EarnMoney { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==================== LOAD ====================
    private void LoadAll()
    {
        MaxHealth = PlayerPrefs.GetInt("player_maxHealth", defaultMaxHealth);
        Speed = PlayerPrefs.GetFloat("player_speed", defaultSpeed);
        Attack = PlayerPrefs.GetInt("player_attack", defaultAttack);
        Level = PlayerPrefs.GetInt("player_level", defaultLevel);
        EarnMoney = PlayerPrefs.GetInt("player_earnMoney", defaultEarnMoney);
    }

    // ==================== SET ====================
    public void SetMaxHealth(int value)
    {
        MaxHealth = value;
        PlayerPrefs.SetInt("player_maxHealth", value);
    }

    public void SetSpeed(float value)
    {
        Speed = value;
        PlayerPrefs.SetFloat("player_speed", value);
    }

    public void SetAttack(int value)
    {
        Attack = value;
        PlayerPrefs.SetInt("player_attack", value);
    }

    public void SetLevel(int value)
    {
        Level = value;
        PlayerPrefs.SetInt("player_level", value);
    }

    public void SetEarnMoney(int value)
    {
        EarnMoney = value;
        PlayerPrefs.SetInt("player_earnMoney", value);
    }

    //Save all
    public void Save()
    {
        PlayerPrefs.Save();

        Debug.Log(
        $"[PlayerDataManager Save]\n" +
        $"MaxHealth: {MaxHealth}\n" +
        $"Speed: {Speed}\n" +
        $"Attack: {Attack}\n" +
        $"Level: {Level}\n" +
        $"EarnMoney: {EarnMoney}"
        );
    }
}
