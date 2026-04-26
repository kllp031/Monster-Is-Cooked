using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Wire một <see cref="Button"/> trong scene tới local player spawn runtime.
///
/// Cách dùng:
/// - Gắn component này lên GameObject chứa Button (Interact, Drop Food, ...).
/// - Ở Inspector, drag Button vào field <c>button</c> (hoặc để trống nếu Button
///   nằm trên cùng GameObject — component sẽ tự GetComponent).
/// - Kéo thả vào <c>onClickWithPlayer</c> method cần gọi trên player, ví dụ:
///   <c>InteractableDetector.OnInteract(GameObject)</c>,
///   <c>FoodHolder.DropFood(GameObject)</c>, ...
///
/// Khi local player chưa spawn, click button sẽ không làm gì. Không cần chỉnh
/// sửa <see cref="LocalPlayerHUD"/> khi thêm button mới.
/// </summary>
[RequireComponent(typeof(Button))]
public class LocalPlayerButton : MonoBehaviour
{
    [Tooltip("Button sẽ trigger hành động. Để trống để tự GetComponent<Button>() trên cùng GameObject.")]
    [SerializeField] private Button button;

    [Tooltip("Method cần gọi trên local player khi button click. GameObject truyền vào là local player.")]
    [SerializeField] private UnityEvent<GameObject> onClickWithPlayer;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        var hud = LocalPlayerHUD.Instance;
        var player = hud != null ? hud.BoundPlayer : null;
        if (player == null)
        {
            // Local player chưa spawn — bỏ qua click một cách im lặng. Dùng
            // Debug.Log (không phải Warning) để tránh spam console.
            Debug.Log($"{nameof(LocalPlayerButton)} on '{name}' clicked but local player is not bound yet.");
            return;
        }

        onClickWithPlayer?.Invoke(player.gameObject);
    }
}
