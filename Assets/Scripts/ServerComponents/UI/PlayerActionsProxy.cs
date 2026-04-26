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
    }

    // Thêm các action khác ở đây khi cần, ví dụ:
    //
    // public void DropFood(GameObject player)
    // {
    //     if (player == null) return;
    //     var holder = player.GetComponent<FoodHolder>();
    //     if (holder != null) holder.DropFood();
    // }
    //
    // public void ThrowFood(GameObject player)
    // {
    //     if (player == null) return;
    //     var holder = player.GetComponent<FoodHolder>();
    //     if (holder != null) holder.ThrowFood();
    // }
}
