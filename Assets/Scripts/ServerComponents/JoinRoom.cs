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
    [Tooltip("(Optional) Nút Tạo phòng — cũng bị disable khi đang kết nối.")]
    [SerializeField] Button createButton;
    [Tooltip("(Optional) Text hiển thị trạng thái/lỗi cho người chơi.")]
    [SerializeField] TMP_Text statusText;

    private static readonly char[] RoomCodeChars = "0123456789".ToCharArray();

    // Guard local: tránh double-click trong lúc await StartGame.
    private bool isJoining;

    public async void OnCreateRoom()
    {
        if (roomIdInput != null)
            roomIdInput.text = GenerateRoomCode();
        await StartJoin();
    }

    public async void OnJoinRoom()
    {
        if (roomIdInput == null || string.IsNullOrWhiteSpace(roomIdInput.text))
        {
            SetStatus("Nhập mã phòng trước khi vào.");
            return;
        }
        await StartJoin();
    }

    private async System.Threading.Tasks.Task StartJoin()
    {
        //Debug.Log("[JoinRoom] Đang join, bỏ qua click lặp.");
        if (isJoining) return;

        if (NetworkManager.Instance == null)
        {
            //Debug.LogError("[JoinRoom] NetworkManager.Instance null — thiếu NetworkManager trong scene hoặc chưa được khởi tạo.");
            SetStatus("Lỗi: thiếu NetworkManager.");
            return;
        }

        if (NetworkManager.Instance.NetworkRunner != null)
        {
            // Runner cũ còn sót (do lần join trước fail mà không cleanup).
            // Tự dọn để cho phép retry — không return im lặng như trước.
            //Debug.LogWarning("[JoinRoom] NetworkRunner cũ còn sót, đang cleanup trước khi join lại...");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
        }

        if (string.IsNullOrWhiteSpace(roomIdInput.text))
        {
            //Debug.LogWarning("[JoinRoom] Room ID trống.");
            SetStatus("Nhập mã phòng trước khi vào.");
            return;
        }

        if (usernameInput != null) NetworkManager.Instance.Username = usernameInput.text;

        isJoining = true;
        SetButtonsInteractable(false);
        SetStatus("Đang kết nối...");

        StartGameResult res;
        try
        {
            res = await NetworkManager.Instance.JoinRoom(gameMode, roomIdInput.text, lobbySceneIndex);
        }
        catch (System.Exception)
        {
            //Debug.LogError($"[JoinRoom] Exception khi StartGame");
            SetStatus("Lỗi kết nối. Xem Console.");
            // Dọn runner lỗi để user có thể bấm lại.
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            SetButtonsInteractable(true);
            return;
        }

        if (res == null || !res.Ok)
        {
            string reason = res != null ? res.ShutdownReason.ToString() : "null result";
            //Debug.LogError($"[JoinRoom] Failed to join room: {reason}");
            SetStatus($"Join failed: {reason}");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            SetButtonsInteractable(true);
            return;
        }

        // Thành công: NetworkRunner sẽ tự load scene Lobby. Không reset isJoining
        // vì scene chuyển, object này bị destroy.
        SetStatus("Kết nối thành công, đang chuyển scene...");
    }

    private string GenerateRoomCode()
    {
        var sb = new System.Text.StringBuilder(5);
        for (int i = 0; i < 5; i++)
            sb.Append(RoomCodeChars[Random.Range(0, RoomCodeChars.Length)]);
        return sb.ToString();
    }

    private void SetButtonsInteractable(bool value)
    {
        if (joinButton != null) joinButton.interactable = value;
        if (createButton != null) createButton.interactable = value;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}
