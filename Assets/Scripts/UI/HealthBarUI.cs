using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frontBar;  
    [SerializeField] private Image delayedBar; 

    [Header("Effect Settings")]
    [SerializeField] private float delayTime = 0.25f;
    [SerializeField] private float dropSpeed = 1.5f;

    private Health health;
    private float delayTimer;

    private int lastHealth;

    // =========================
    // INIT
    // =========================
    private void Awake()
    {
        health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogError("HealthBarUI need component Health!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        lastHealth = health.currentHealth;
        UpdateInstant();
    }

    // =========================
    // UPDATE
    // =========================
    private void Update()
    {
        if (health.currentHealth != lastHealth)
        {
            OnHealthChanged();
            lastHealth = health.currentHealth;
        }

        if (delayedBar.fillAmount > frontBar.fillAmount)
        {
            if (delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
            }
            else
            {
                delayedBar.fillAmount = Mathf.MoveTowards(
                    delayedBar.fillAmount,
                    frontBar.fillAmount,
                    dropSpeed * Time.deltaTime
                );
            }
        }
    }

    // =========================
    // HEALTH CHANGE HANDLER
    // =========================
    private void OnHealthChanged()
    {
        float maxHp = GetMaxHealth();
        float targetFill = Mathf.Clamp01(health.currentHealth / maxHp);

        frontBar.fillAmount = targetFill;

        delayTimer = delayTime;
    }

    private void UpdateInstant()
    {
        float fill = Mathf.Clamp01(health.currentHealth / GetMaxHealth());
        frontBar.fillAmount = fill;
        delayedBar.fillAmount = fill;
    }

    private float GetMaxHealth()
    {
        if (CompareTag("Player"))
            return PlayerDataManager.Instance.CurrentMaxHealth;

        return health.maximumHealth;
    }
}
