using UnityEngine;

/// <summary>
/// Scene-resident dispatcher cho các UI button muốn trigger hành động trên
/// local player (spawn runtime bằng Fusion/Photon).
///
/// Vì local player chưa tồn tại ở edit-time, <see cref="UnityEngine.Events.UnityEvent"/>
/// không thể drag-drop method của player vào Inspector. Proxy này nằm sẵn trong
/// scene nên drag-drop được — mỗi method nhận <c>GameObject player</c> và
/// forward tới component tương ứng trên player prefab.
///
/// Cách dùng:
/// - Gắn component này lên một GameObject trong scene (gợi ý: cùng GameObject
///   với <see cref="LocalPlayerHUD"/>, hoặc một child dedicated "UI/PlayerActions").
/// - Ở mỗi UI button dùng <see cref="LocalPlayerButton"/>: drag GameObject chứa
///   proxy này vào slot target của UnityEvent, chọn method (Interact, DropFood,
///   ...), set parameter thành "Dynamic GameObject".
/// - Thêm action mới = thêm một method public nhận <c>GameObject player</c>.
/// </summary>
public class PlayerActionsProxy : MonoBehaviour
{
    public void Interact(GameObject player)
    {
        if (player == null) return;
        var detector = player.GetComponent<InteractableDetector>();
        if (detector != null) detector.OnInteract();
        print($"PlayerActionsProxy: Interact called on player {player.name}");
    }

    public void Dash(GameObject player)
    {
        if (player == null) return;
        var knight = player.GetComponent<KnightControllerOnline>();
        if (knight != null) knight.StartDash();
    }

    public void Attack(GameObject player)
    {
        if (player == null) return;
        var attack = player.GetComponent<KnightAttackOnline>();
        if (attack != null) attack.OnButtonAttack();
    }

    /// <summary>
    /// Single-button drop/throw: gọi từ PointerDown event của button. Holder
    /// quyết định drop (tap) hay throw (hold) dựa trên thời gian giữ.
    /// </summary>
    public void OnFoodActionPress(GameObject player)
    {
        if (player == null) return;
        var holder = player.GetComponent<FoodHolderOnline>();
        if (holder != null) holder.OnFoodActionPressed();
    }

    /// <summary>Gọi từ PointerUp event của button (nhả tay).</summary>
    public void OnFoodActionRelease(GameObject player)
    {
        if (player == null) return;
        var holder = player.GetComponent<FoodHolderOnline>();
        if (holder != null) holder.OnFoodActionReleased();
    }
}
