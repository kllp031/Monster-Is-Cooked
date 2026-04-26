using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene-local singleton giữ các reference UI/scene mà local player cần
/// (joystick, dash cooldown image, spawn point, ...). Không dùng DontDestroyOnLoad
/// vì những reference này chỉ hợp lệ trong scene hiện tại.
///
/// Cách hoạt động:
/// - Gắn component này vào một GameObject trong scene gameplay, kéo thả reference
///   trong Inspector.
/// - Khi local player spawn, gọi LocalPlayerHUD.Instance?.Bind(player) để đẩy
///   các reference vào player.
/// - Khi scene unload, Unity destroy GameObject -> Instance tự về null.
///
/// Các UI khác (ví dụ Button Interact, Drop Food, ...) KHÔNG cần đăng ký trực
/// tiếp ở đây. Gắn component <see cref="LocalPlayerButton"/> lên button đó và
/// tự wire UnityEvent trong Inspector — button sẽ subscribe vào
/// <see cref="OnLocalPlayerBound"/> tại runtime.
/// </summary>
public class LocalPlayerHUD : MonoBehaviour
{
    public static LocalPlayerHUD Instance { get; private set; }

    [Header("HUD References")]
    [Tooltip("Joystick UI cho local player.")]
    [SerializeField] private Joystick joystick;

    [Tooltip("Image hiển thị cooldown của skill Dash.")]
    [SerializeField] private Image dashCooldownEffect;

    [Tooltip("Vị trí respawn cho local player trong scene này.")]
    [SerializeField] private Transform spawnPosition;

    [Tooltip("Health bar UI dạng screen-space (không chung GameObject với player). " +
             "Để trống nếu health bar đã được gắn trực tiếp trên player prefab.")]
    [SerializeField] private HealthBarUI screenSpaceHealthBar;

    public Joystick Joystick => joystick;
    public Image DashCooldownEffect => dashCooldownEffect;
    public Transform SpawnPosition => spawnPosition;
    public HealthBarUI ScreenSpaceHealthBar => screenSpaceHealthBar;

    /// <summary>Player đang được bind (local player của máy này). Null khi chưa spawn.</summary>
    public KnightControllerOnline BoundPlayer { get; private set; }

    /// <summary>
    /// Fire khi local player đã spawn và được bind. UI elements (ví dụ
    /// <see cref="LocalPlayerButton"/>) subscribe để tự wire handlers tới player.
    /// Subscriber đăng ký SAU khi player đã bind vẫn sẽ được callback ngay lập tức
    /// thông qua <see cref="BoundPlayer"/>.
    /// </summary>
    public static event Action<KnightControllerOnline> OnLocalPlayerBound;

    /// <summary>
    /// Fire khi player hiện tại sắp bị unbind (scene unload, HUD destroy, hoặc
    /// player bị thay thế). Subscriber nên remove listener khỏi player cũ ở đây.
    /// </summary>
    public static event Action<KnightControllerOnline> OnLocalPlayerUnbound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                $"Duplicate {nameof(LocalPlayerHUD)} in scene on '{gameObject.name}'. Destroying the newer one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Trường hợp player spawn TRƯỚC khi HUD Awake (hiếm nhưng có thể xảy ra
        // nếu script execution order bị đổi): tự tìm local player đã tồn tại và
        // bind thủ công.
        if (BoundPlayer != null) return;
        foreach (var player in FindObjectsByType<KnightControllerOnline>(FindObjectsSortMode.None))
        {
            if (player == null || player.Object == null) continue;
            if (player.Object.HasInputAuthority)
            {
                Bind(player);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (BoundPlayer != null)
        {
            OnLocalPlayerUnbound?.Invoke(BoundPlayer);
            BoundPlayer = null;
        }
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Đẩy các reference UI/scene vào player local. Được gọi bởi chính player
    /// trong Spawned() khi HasInputAuthority == true.
    /// </summary>
    public void Bind(KnightControllerOnline player)
    {
        if (player == null) return;

        // Nếu đang bind player khác, thông báo unbind trước để subscriber cleanup.
        if (BoundPlayer != null && BoundPlayer != player)
        {
            OnLocalPlayerUnbound?.Invoke(BoundPlayer);
        }

        BoundPlayer = player;
        player.ConfigureLocalHUD(joystick, dashCooldownEffect, spawnPosition);

        // Nếu có screen-space health bar, bind vào Health component của local player
        if (screenSpaceHealthBar != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null)
            {
                screenSpaceHealthBar.SetTarget(health);
            }
        }

        OnLocalPlayerBound?.Invoke(player);
    }
}
