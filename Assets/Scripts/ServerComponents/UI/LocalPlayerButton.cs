using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Wire một <see cref="Button"/> trong scene tới local player spawn runtime.
///
/// Cách dùng:
/// - Gắn component này lên GameObject chứa Button (Interact, Drop Food, ...).
/// - Ở Inspector, drag Button vào field <c>button</c> (hoặc để trống nếu Button
///   nằm trên cùng GameObject — component sẽ tự GetComponent).
/// - Click event: kéo thả method vào <c>onClickWithPlayer</c>.
/// - Press-and-hold event: dùng <c>onPressDownWithPlayer</c> + <c>onPressUpWithPlayer</c>.
///   Ví dụ throw button: PressDown → StartThrow, PressUp → EndThrow.
///
/// Khi local player chưa spawn, event sẽ bị bỏ qua im lặng. Không cần chỉnh
/// sửa <see cref="LocalPlayerHUD"/> khi thêm button mới.
/// </summary>
[RequireComponent(typeof(Button))]
public class LocalPlayerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Button sẽ trigger hành động. Để trống để tự GetComponent<Button>() trên cùng GameObject.")]
    [SerializeField] private Button button;

    [Tooltip("Click bình thường. GameObject truyền vào là local player.")]
    [SerializeField] private UnityEvent<GameObject> onClickWithPlayer;

    [Tooltip("Khi player nhấn xuống (PointerDown). Dùng cho press-and-hold pattern.")]
    [SerializeField] private UnityEvent<GameObject> onPressDownWithPlayer;

    [Tooltip("Khi player nhả ra (PointerUp). Dùng để kết thúc press-and-hold.")]
    [SerializeField] private UnityEvent<GameObject> onPressUpWithPlayer;

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
        var player = TryGetLocalPlayer();
        if (player == null) return;
        onClickWithPlayer?.Invoke(player);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        var player = TryGetLocalPlayer();
        if (player == null) return;
        onPressDownWithPlayer?.Invoke(player);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        var player = TryGetLocalPlayer();
        if (player == null) return;
        onPressUpWithPlayer?.Invoke(player);
    }

    private GameObject TryGetLocalPlayer()
    {
        var hud = LocalPlayerHUD.Instance;
        var player = hud != null ? hud.BoundPlayer : null;
        if (player == null)
        {
            //Debug.Log($"{nameof(LocalPlayerButton)} on '{name}' fired but local player is not bound yet.");
            return null;
        }
        return player.gameObject;
    }
}
