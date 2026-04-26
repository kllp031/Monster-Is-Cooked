using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar UI. Hai cách dùng:
///
/// 1) Gắn component trực tiếp lên GameObject có <see cref="Health"/> (ví dụ
///    world-space canvas trên đầu player). Tự động lấy Health qua
///    GetComponent trong Awake.
///
/// 2) Đặt làm screen-space UI trong scene (không có Health component cùng
///    GameObject). Gọi <see cref="SetTarget(Health)"/> để bind runtime
///    (ví dụ từ LocalPlayerHUD khi local player spawn).
///
/// Lưu ý multiplayer: Health hiện chưa được networked, nên health bar chỉ
/// phản ánh đúng giá trị cho local player. Remote player sẽ hiển thị giá trị
/// từ local Health component (không sync với authoritative state).
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image frontBar;
    [SerializeField] private Image delayedBar;

    [Header("Effect Settings")]
    [SerializeField] private float delayTime = 0.25f;
    [SerializeField] private float dropSpeed = 1.5f;

    [Tooltip("Nếu để trống, sẽ tự GetComponent<Health>() trên chính GameObject này.")]
    [SerializeField] private Health health;

    private float delayTimer;
    private int lastHealth;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }

    private void Start()
    {
        if (health == null)
        {
            // Không có target ngay — có thể sẽ bind sau qua SetTarget.
            // Không log error để tránh spam khi dùng pattern bind runtime.
            return;
        }

        lastHealth = health.currentHealth;
        UpdateInstant();
    }

    /// <summary>
    /// Bind một Health component runtime. Dùng khi HealthBarUI ở scene UI
    /// (không chung GameObject với Health).
    /// </summary>
    public void SetTarget(Health newHealth)
    {
        health = newHealth;
        if (health != null)
        {
            lastHealth = health.currentHealth;
            UpdateInstant();
        }
    }

    private void Update()
    {
        if (health == null || frontBar == null || delayedBar == null) return;

        float maxHp = GetMaxHealth();
        if (maxHp <= 0f) return; // tránh chia 0

        float targetFill = Mathf.Clamp01(health.currentHealth / maxHp);

        frontBar.fillAmount = targetFill;

        if (health.currentHealth < lastHealth)
        {
            delayTimer = delayTime;
        }

        lastHealth = health.currentHealth;

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

        // Với Player, max HP có thể nâng cấp qua PlayerDataManager (per-client).
        if (health.CompareTag("Player") && PlayerDataManager.Instance != null)
        {
            return PlayerDataManager.Instance.CurrentMaxHealth;
        }

        return health.maximumHealth;
    }
}
