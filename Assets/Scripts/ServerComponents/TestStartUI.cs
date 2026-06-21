using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Test-only entry UI. Sống trong 1 scene duy nhất:
///  - Join panel: hiện trước khi join. Click Join button -> join shared room với
///    username tạm "Player_<short-guid>". Sau khi LocalPlayer thực sự join thì
///    update username = "Player_<PlayerId>" để unique trong room.
///  - Start panel: hiện sau khi join. Start button chỉ enable cho master client.
///
/// Không đụng tới <see cref="JoinRoom"/> hay <see cref="Lobby"/>.
/// </summary>
public class TestStartUI : MonoBehaviour
{
    [Header("Join (pre-join, hides after join)")]
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text usernamePreviewText;

    [Header("Start Game (visible after join, master-only enabled)")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text masterOnlyHintText;
    [SerializeField] private TMP_Text playersInRoomText;
    [SerializeField] private TMP_Text localUsernameText;

    [Header("Network")]
    [SerializeField] private string testRoomName = "TEST_ROOM";
    [Tooltip("Build index của scene gameplay chính (load khi master bấm Start).")]
    [SerializeField] private int mainGameSceneIndex = -1;
    [SerializeField] private GameMode gameMode = GameMode.Shared;

    [Header("UI Feedback")]
    [SerializeField] private TMP_Text statusText;

    private bool isJoining;
    private bool localPlayerJoined;

    private void Awake()
    {
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinClicked);
        }
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
    }

    private void OnEnable()
    {
        TrySubscribeNetworkEvents();
    }

    private void OnDisable()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerJoinedEvent.RemoveListener(HandlePlayerJoined);
            NetworkManager.Instance.OnPlayerLeftEvent.RemoveListener(HandlePlayerLeft);
        }
    }

    private void TrySubscribeNetworkEvents()
    {
        if (NetworkManager.Instance == null) return;
        NetworkManager.Instance.OnPlayerJoinedEvent.RemoveListener(HandlePlayerJoined);
        NetworkManager.Instance.OnPlayerLeftEvent.RemoveListener(HandlePlayerLeft);
        NetworkManager.Instance.OnPlayerJoinedEvent.AddListener(HandlePlayerJoined);
        NetworkManager.Instance.OnPlayerLeftEvent.AddListener(HandlePlayerLeft);
    }

    private void Update()
    {
        UpdatePanels();
        UpdateJoinButtonState();
        UpdateStartButtonState();
        UpdatePlayerCount();
        UpdateLocalUsername();
    }

    private bool RunnerAlive()
    {
        return NetworkManager.Instance != null
               && NetworkManager.Instance.NetworkRunner != null
               && NetworkManager.Instance.NetworkRunner.IsRunning;
    }

    private void UpdatePanels()
    {
        bool joined = RunnerAlive() && localPlayerJoined;
        if (joinPanel != null) joinPanel.SetActive(!joined);
        if (startPanel != null) startPanel.SetActive(joined);
    }

    private void UpdateJoinButtonState()
    {
        if (joinButton != null) joinButton.interactable = !isJoining && !localPlayerJoined;
        if (usernamePreviewText != null && !localPlayerJoined && !isJoining)
        {
            usernamePreviewText.text = "Username: auto-generated on join";
        }
    }

    private void UpdateStartButtonState()
    {
        bool joined = RunnerAlive() && localPlayerJoined;
        bool isMaster = joined && NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient;

        if (startGameButton != null)
        {
            startGameButton.interactable = isMaster;
        }

        if (masterOnlyHintText != null)
        {
            if (!joined)
            {
                masterOnlyHintText.gameObject.SetActive(true);
                masterOnlyHintText.text = "Press Join to enter room...";
            }
            else if (!isMaster)
            {
                masterOnlyHintText.gameObject.SetActive(true);
                masterOnlyHintText.text = "Waiting for master client to start...";
            }
            else
            {
                masterOnlyHintText.gameObject.SetActive(true);
                masterOnlyHintText.text = "You are master. Press Start when ready.";
            }
        }
    }

    private void UpdatePlayerCount()
    {
        if (playersInRoomText == null) return;

        if (!RunnerAlive())
        {
            playersInRoomText.text = "Players in room: 0";
            return;
        }

        int count = 0;
        foreach (var _ in NetworkManager.Instance.NetworkRunner.ActivePlayers) count++;
        playersInRoomText.text = $"Players in room: {count}";
    }

    private void UpdateLocalUsername()
    {
        if (localUsernameText == null) return;
        if (NetworkManager.Instance != null)
        {
            localUsernameText.text = $"You: {NetworkManager.Instance.Username}";
        }
    }

    private void HandlePlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            localPlayerJoined = true;
            isJoining = false;

            // Replace username tạm bằng tên dựa PlayerId — unique trong room.
            string finalName = $"Player_{player.PlayerId}";
            NetworkManager.Instance.Username = finalName;

            SetStatus($"Joined as {finalName}.");
            //Debug.Log($"[TestStartUI] LocalPlayer joined. Id={player.PlayerId}, IsMaster={runner.IsSharedModeMasterClient}");
        }
    }

    private void HandlePlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            localPlayerJoined = false;
        }
    }

    private async void OnJoinClicked()
    {
        if (isJoining || localPlayerJoined) return;

        isJoining = true;

        if (NetworkManager.Instance == null)
        {
            SetStatus("Lỗi: thiếu NetworkManager.");
            //Debug.LogError("[TestStartUI] NetworkManager.Instance null.");
            return;
        }

        if (NetworkManager.Instance.NetworkRunner != null)
        {
            //Debug.LogWarning("[TestStartUI] Runner cũ còn sót, cleanup.");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
        }

        // Username tạm trước khi vào room — sẽ replace bằng PlayerId-based name
        // trong HandlePlayerJoined. Random suffix tránh trùng giữa các instance
        // ở giai đoạn pre-join.
        string tempName = $"Player_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        NetworkManager.Instance.Username = tempName;

        SetStatus($"Joining as {tempName}...");

        TrySubscribeNetworkEvents();

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        StartGameResult res;
        try
        {
            //testRoomName += UnityEngine.Random.Range(0, 9999); // tránh trùng room khi test nhiều instance
            res = await NetworkManager.Instance.JoinRoom(gameMode, testRoomName, currentSceneIndex);
        }
        catch (Exception)
        {
            //Debug.LogError($"[TestStartUI] Exception");
            SetStatus("Lỗi kết nối. Xem Console.");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            return;
        }

        if (res == null || !res.Ok)
        {
            string reason = res != null ? res.ShutdownReason.ToString() : "null result";
            //Debug.LogError($"[TestStartUI] Join failed: {reason}");
            SetStatus($"Join failed: {reason}");
            await NetworkManager.Instance.CleanupNetworkRunnerAsync();
            isJoining = false;
            return;
        }

        // localPlayerJoined sẽ bật qua HandlePlayerJoined.
    }

    private void OnStartGameClicked()
    {
        if (!RunnerAlive() || !localPlayerJoined)
        {
            SetStatus("Chưa join room.");
            return;
        }

        var runner = NetworkManager.Instance.NetworkRunner;
        if (!runner.IsSharedModeMasterClient)
        {
            SetStatus("Chỉ master client mới start được.");
            return;
        }

        if (mainGameSceneIndex < 0)
        {
            //Debug.LogError("[TestStartUI] mainGameSceneIndex chưa set.");
            SetStatus("mainGameSceneIndex chưa set.");
            return;
        }

        runner.SessionInfo.IsOpen = false;
        runner.SessionInfo.IsVisible = false;
        runner.LoadScene(SceneRef.FromIndex(mainGameSceneIndex));
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}
