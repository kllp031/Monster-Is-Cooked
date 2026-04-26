using UnityEngine;

/// <summary>
/// Tutorial overlay. Ở chế độ single-player gốc, khi mở tutorial sẽ pause game
/// bằng Time.timeScale = 0. Tuy nhiên ở chế độ multiplayer (Photon Fusion):
///  - Time.timeScale chỉ ảnh hưởng local client.
///  - Remote players vẫn tiếp tục chơi bình thường.
///  - Coroutine/physics local sẽ dừng, gây desync nghiêm trọng với Fusion
///    simulation.
///
/// Vì vậy component này phát hiện khi nào đang multiplayer thì KHÔNG đụng vào
/// Time.timeScale. Logic chặn input (nếu có) nên được xử lý riêng bằng UI
/// (CanvasGroup.interactable/blocksRaycasts, overlay block click, v.v.).
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Tooltip("Nếu true, pause game bằng Time.timeScale khi hiển thị (chỉ an toàn cho single-player). " +
             "Ở multiplayer, để false.")]
    [SerializeField] private bool pauseTimeInSinglePlayer = true;

    private void OnEnable()
    {
        if (pauseTimeInSinglePlayer && !IsMultiplayerActive())
        {
            Time.timeScale = 0f;
        }
    }

    private void OnDisable()
    {
        // Luôn khôi phục để không kẹt ở 0 nếu có ai set trong khi multiplayer
        // chưa sẵn sàng.
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    private static bool IsMultiplayerActive()
    {
        // NetworkManager là singleton, chỉ tồn tại khi player đã join room.
        // NetworkRunner khác null -> đang trong session multiplayer.
        return NetworkManager.Instance != null
               && NetworkManager.Instance.NetworkRunner != null
               && NetworkManager.Instance.NetworkRunner.IsRunning;
    }
}
