using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIOnline : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frontBar;
    [SerializeField] private Image delayedBar;

    [Header("Effect Settings")]
    [SerializeField] private float delayTime = 0.25f;
    [SerializeField] private float dropSpeed = 1.5f;

    [Tooltip("Leave empty to auto GetComponent<Health>() on this GameObject.")]
    [SerializeField] private Health health;

    private float delayTimer;
    private int lastHealth;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Start()
    {
        if (health == null) return;
        lastHealth = health.currentHealth;
        UpdateInstant();
    }

    public void SetTarget(Health newHealth)
    {
        health = newHealth;
        if (health != null)
        {
            lastHealth = health.currentHealth;
            UpdateInstant();
        }
        print($"HealthBarUIOnline on '{gameObject.name}' bound to '{health.gameObject.name}'");
    }

    private void Update()
    {
        if (health == null || frontBar == null || delayedBar == null) return;

        float maxHp = GetMaxHealth();
        if (maxHp <= 0f) return;

        float targetFill = Mathf.Clamp01(health.currentHealth / maxHp);
        frontBar.fillAmount = targetFill;

        if (health.currentHealth < lastHealth)
            delayTimer = delayTime;

        lastHealth = health.currentHealth;

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
        float fill = Mathf.Clamp01(health.currentHealth / maxHp);
        frontBar.fillAmount = fill;
        delayedBar.fillAmount = fill;
    }

    private float GetMaxHealth()
    {
        if (health == null) return 0f;

        if (health.CompareTag("Player"))
        {
            PlayerDataManagerOnline data = health.GetComponent<PlayerDataManagerOnline>();
            if (data != null) return data.CurrentMaxHealth;
        }

        return health.maximumHealth;
    }
}
