using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Online variant of HealthBarUI. Reads from HealthOnline instead of Health.
/// Attach to world-space canvas on OnlinePlayer prefab, or bind via SetTarget() at runtime.
/// </summary>
public class HealthBarUIOnline : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frontBar;
    [SerializeField] private Image delayedBar;

    [Header("Effect Settings")]
    [SerializeField] private float delayTime = 0.25f;
    [SerializeField] private float dropSpeed = 1.5f;

    [Tooltip("Leave empty to auto GetComponent<HealthOnline>() on this GameObject.")]
    [SerializeField] private HealthOnline health;

    private float delayTimer;
    private int lastHealth;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthOnline>();
    }

    private void Start()
    {
        if (health == null) return;
        lastHealth = health.CurrentHealth;
        UpdateInstant();
    }

    public void SetTarget(HealthOnline newHealth)
    {
        health = newHealth;
        if (health != null)
        {
            lastHealth = health.CurrentHealth;
            UpdateInstant();
        }
        print($"HealthBarUIOnline on '{gameObject.name}' bound to '{health.gameObject.name}'");
    }

    private void Update()
    {
        if (health == null || frontBar == null || delayedBar == null) return;

        float maxHp = GetMaxHealth();
        if (maxHp <= 0f) return;

        float targetFill = Mathf.Clamp01(health.CurrentHealth / maxHp);
        frontBar.fillAmount = targetFill;

        if (health.CurrentHealth < lastHealth)
            delayTimer = delayTime;

        lastHealth = health.CurrentHealth;

        if (delayedBar.fillAmount > frontBar.fillAmount)
        {
            if (delayTimer > 0)
                delayTimer -= Time.deltaTime;
            else
                delayedBar.fillAmount = Mathf.MoveTowards(delayedBar.fillAmount, frontBar.fillAmount, dropSpeed * Time.deltaTime);
        }
        else
        {
            delayedBar.fillAmount = frontBar.fillAmount;
        }
    }

    private void UpdateInstant()
    {
        if (health == null || frontBar == null || delayedBar == null) return;
        float maxHp = GetMaxHealth();
        if (maxHp <= 0f) return;
        float fill = Mathf.Clamp01(health.CurrentHealth / maxHp);
        frontBar.fillAmount = fill;
        delayedBar.fillAmount = fill;
    }

    private float GetMaxHealth()
    {
        if (health == null) return 0f;

        // For player: read networked max health from their PlayerDataManagerOnline
        if (health.CompareTag("Player"))
        {
            PlayerDataManagerOnline data = health.GetComponent<PlayerDataManagerOnline>();
            if (data != null) return data.CurrentMaxHealth;
        }

        return health.maximumHealth;
    }
}
