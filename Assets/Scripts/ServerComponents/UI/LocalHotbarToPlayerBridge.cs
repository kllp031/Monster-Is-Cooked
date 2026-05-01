using UnityEngine;

/// <summary>
/// Cầu nối giữa singleton <see cref="HotbarManagerOnline"/> (UI hotbar online,
/// scoped per device) và <see cref="FoodHolderOnline"/> trên local player.
/// Khi hotbar đổi selection hoặc add/remove recipe, đẩy recipe của slot đang
/// chọn vào networked state của local player → mọi client render sprite đúng
/// cho player đó.
///
/// Setup:
/// - Đặt component này lên GameObject scene-resident bất kỳ (gợi ý: cùng
///   GameObject với <see cref="LocalPlayerHUD"/>). Không cần kéo gì.
/// - Yêu cầu: scene có HotbarManagerOnline + LocalPlayerHUD, OnlinePlayer
///   prefab có FoodHolderOnline.
/// </summary>
public class LocalHotbarToPlayerBridge : MonoBehaviour
{
    private FoodHolderOnline boundHolder;
    private HotbarManagerOnline subscribedManager;

    private void OnEnable()
    {
        LocalPlayerHUD.OnLocalPlayerBound += HandleLocalPlayerBound;
        LocalPlayerHUD.OnLocalPlayerUnbound += HandleLocalPlayerUnbound;

        if (LocalPlayerHUD.Instance != null && LocalPlayerHUD.Instance.BoundPlayer != null)
            HandleLocalPlayerBound(LocalPlayerHUD.Instance.BoundPlayer);

        TrySubscribeHotbar();
    }

    private void OnDisable()
    {
        LocalPlayerHUD.OnLocalPlayerBound -= HandleLocalPlayerBound;
        LocalPlayerHUD.OnLocalPlayerUnbound -= HandleLocalPlayerUnbound;
        UnsubscribeHotbar();
        boundHolder = null;
    }

    private void Update()
    {
        // Lazy subscribe: HotbarManagerOnline.Awake order not guaranteed
        // relative to bridge.OnEnable across separate GameObjects. Retry
        // until Instance available.
        if (subscribedManager == null) TrySubscribeHotbar();
    }

    private void TrySubscribeHotbar()
    {
        if (subscribedManager != null) return;
        var mgr = HotbarManagerOnline.Instance;
        if (mgr == null) return;

        mgr.OnSelectionChanged += HandleSelectionChanged;
        mgr.OnHotbarUpdated += HandleHotbarUpdated;
        subscribedManager = mgr;
        Sync();
    }

    private void UnsubscribeHotbar()
    {
        if (subscribedManager == null) return;
        subscribedManager.OnSelectionChanged -= HandleSelectionChanged;
        subscribedManager.OnHotbarUpdated -= HandleHotbarUpdated;
        subscribedManager = null;
    }

    private void HandleLocalPlayerBound(KnightControllerOnline player)
    {
        if (player == null) return;
        boundHolder = player.GetComponent<FoodHolderOnline>();
        Sync();
    }

    private void HandleLocalPlayerUnbound(KnightControllerOnline player)
    {
        boundHolder = null;
    }

    private void HandleSelectionChanged(int _) => Sync();
    private void HandleHotbarUpdated() => Sync();

    private void Sync()
    {
        if (boundHolder == null) return;

        Recipe recipe = HotbarManagerOnline.Instance != null
            ? HotbarManagerOnline.Instance.CurrentRecipe
            : null;

        boundHolder.SetHeldRecipe(recipe);
    }
}
