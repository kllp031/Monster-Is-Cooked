using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoom : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField roomIdInput;
    [SerializeField] int lobbySceneIndex = 4;
    [SerializeField] GameMode gameMode = GameMode.Shared;

    [Header("UI Feedback (optional)")]
    [Tooltip("(Optional) Nút Join — script tự disable khi đang kết nối để chặn double-click.")]
    [SerializeField] Button joinButton;
    [Tooltip("(Optional) Text hiển thị trạng thái/lỗi cho người chơi.")]
    [SerializeField] TMP_Text statusText;

    // Guard local: tránh double-click trong lúc await StartGame.
    private bool isJoining;

    public async void OnJoinRoom()
    {
        if (isJoining)
        {
            Debug.Log("[JoinRoom] Đang join, bỏ qua click lặp.");
            return;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[JoinRoom] NetworkManager.Instance null — thiếu NetworkManager trong scene hoặc chưa được khởi tạo.");
            SetStatus("Lỗi: thiếu NetworkManager.");
            return;
        }

        if (NetworkManager.Instance.NetworkRunner != null)
        {
            // Runner cũ còn sót (do lần join trước fail mà không cleanup).
            // Tự dọn để cho phép retry — không return im lặng như trước.
            Debug.LogWarning("[JoinRoom] NetworkRunner cũ còn sót, đang cleanup trước khi join lại...");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
        }

        if (roomIdInput == null)
        {
            Debug.LogError("[JoinRoom] Room ID input field chưa assign.");
            SetStatus("Lỗi: thiếu Room ID input.");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomIdInput.text))
        {
            Debug.LogWarning("[JoinRoom] Room ID trống.");
            SetStatus("Nhập Room ID trước khi join.");
            return;
        }

        if (usernameInput != null) NetworkManager.Instance.Username = usernameInput.text;

        isJoining = true;
        if (joinButton != null) joinButton.interactable = false;
        SetStatus("Đang kết nối...");

        StartGameResult res;
        try
        {
            res = await NetworkManager.Instance.JoinRoom(gameMode, roomIdInput.text, lobbySceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JoinRoom] Exception khi StartGame: {e}");
            SetStatus("Lỗi kết nối. Xem Console.");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            if (joinButton != null) joinButton.interactable = true;
            return;
        }

        if (res == null || !res.Ok)
        {
            string reason = res != null ? res.ShutdownReason.ToString() : "null result";
            Debug.LogError($"[JoinRoom] Failed to join room: {reason}");
            SetStatus($"Join failed: {reason}");
            // Dọn runner lỗi để user có thể bấm lại.
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            if (joinButton != null) joinButton.interactable = true;
            return;
        }

        // Thành công: NetworkRunner sẽ tự load scene Lobby. Không reset isJoining
        // vì scene chuyển, object này bị destroy.
        SetStatus("Kết nối thành công, đang chuyển scene...");
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}
