#pragma warning disable 0414
using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI usernameField;
    [SerializeField] TextMeshProUGUI roomNameField;
    [SerializeField] TextMeshProUGUI readyPlayerCountText;
    [SerializeField] TextMeshProUGUI startGameTimerText;
    [SerializeField] Button startButton;
    [SerializeField] List<PlayerInRoomButton> playerInRoomButtons = new();
    [SerializeField] string mainGameSceneName = "";
    [Range(0f, 1f)]
    [SerializeField] float minimumReadyPercentage = 0.5f;
    [SerializeField] float startGameTime = 45;

    [Networked]
    TickTimer startGameTimer { get; set; }

    //[Networked]
    //int readyPlayerCount { get; set; }

    //private Dictionary<int, bool> readyPlayers = new();

    private void OnDisable()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerJoinedEvent.RemoveListener(OnPlayerJoined);
            NetworkManager.Instance.OnPlayerLeftEvent.RemoveListener(OnPlayerLeft);
        }
    }

    public override void Render()
    {
        //if (readyPlayerCountText != null)
        //{
        //    readyPlayerCountText.text = $"{readyPlayerCount} / {NetworkManager.Instance.NetworkRunner.ActivePlayers.Count()} players are ready";
        //}

        if (startGameTimerText != null)
        {
            if (startGameTimer.IsRunning && NetworkManager.Instance != null)
            {
                float totalSeconds = startGameTimer.RemainingTime(NetworkManager.Instance.NetworkRunner) ?? 0f;
                int minutes = Mathf.FloorToInt(totalSeconds / 60);
                int seconds = Mathf.FloorToInt(totalSeconds % 60);

                startGameTimerText.text = $"Game starts in: {minutes: 00} : {seconds: 00}";
            }
        }

        if (startButton != null && NetworkManager.Instance?.NetworkRunner != null)
            startButton.interactable = NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient;

        base.Render();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (NetworkManager.Instance != null && NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient)
        {
            if (startGameTimer.Expired(NetworkManager.Instance.NetworkRunner))
            {
                startGameTimer = TickTimer.None;
                NetworkManager.Instance.NetworkRunner.SessionInfo.IsOpen = false;
                NetworkManager.Instance.NetworkRunner.SessionInfo.IsVisible = false;
                // Shared Mode: runner.LoadScene() chỉ load cục bộ cho master.
                // Phải dùng RPC để mỗi client tự gọi LoadScene() của mình.
                NetworkManager.Instance.NetworkRunner.LoadScene(mainGameSceneName);
                //RPC_LoadGameScene();
            }
        }
    }

    // StateAuthority (MasterClient) broadcast lệnh chuyển scene tới tất cả clients.
    // Mỗi client gọi runner.LoadScene() của chính mình — đây là cách đúng trong Shared Mode.
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LoadGameScene()
    {
        if (NetworkManager.Instance?.NetworkRunner != null)
            NetworkManager.Instance.NetworkRunner.LoadScene(mainGameSceneName);
    }

    public override void Spawned()
    {
        if (NetworkManager.Instance == null) return;

        if (usernameField != null)
        {
            usernameField.text = NetworkManager.Instance.Username;
        }
        if (roomNameField != null)
        {
            roomNameField.text = NetworkManager.Instance.NetworkRunner.SessionInfo.Name;
        }

        NetworkManager.Instance.OnPlayerJoinedEvent.AddListener(OnPlayerJoined);
        NetworkManager.Instance.OnPlayerLeftEvent.AddListener(OnPlayerLeft);

        if (NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient)
        {
            NetworkManager.Instance.NetworkRunner.SessionInfo.IsOpen = true;
            NetworkManager.Instance.NetworkRunner.SessionInfo.IsVisible = true;
        }

        var playersInRoom = NetworkManager.Instance.NetworkRunner.ActivePlayers;
        foreach (var playerInRoomButton in playerInRoomButtons)
        {
            playerInRoomButton.gameObject.SetActive(false);
        }
        foreach (var player in playersInRoom)
        {
            //Debug.Log(player);
            OnPlayerJoined(NetworkManager.Instance.NetworkRunner, player);
        }

        base.Spawned();
    }

    public async void LeaveRoom()
    {
        if (NetworkManager.Instance == null) return;

        //Debug.Log(NetworkManager.Instance.NetworkRunner + " is shutting down");

        await NetworkManager.Instance.NetworkRunner.Shutdown();

        await SceneManager.LoadSceneAsync("JoinRoom");
    }

    public void AskToLeaveRoom(int id)
    {
        RPC_AskToLeave(PlayerRef.FromIndex(id));
    }

    //public void AnnounceReady()
    //{
    //    if (NetworkManager.Instance == null) return;
    //    RPC_AnnounceReady(NetworkManager.Instance.NetworkRunner.LocalPlayer);
    //}

    public void StartGame()
    {
        if (NetworkManager.Instance == null) return;
        if (!NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient) return;
        if (!startGameTimer.IsRunning)
        {
            startGameTimer = TickTimer.CreateFromSeconds(NetworkManager.Instance.NetworkRunner, startGameTime);
            NetworkManager.Instance.NetworkRunner.SessionInfo.IsOpen = false;
            NetworkManager.Instance.NetworkRunner.SessionInfo.IsVisible = false;
        }
    }

    private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log("Player joined: " + player);
        foreach (var playerInRoomButton in playerInRoomButtons)
        {
            if (playerInRoomButton.gameObject.activeInHierarchy && playerInRoomButton.ID == player.PlayerId) return;
            if (playerInRoomButton.gameObject.activeInHierarchy == false)
            {
                playerInRoomButton.ID = player.PlayerId;
                playerInRoomButton.gameObject.SetActive(true);
                playerInRoomButton.ResetValue();
                break;
            }
        }
        RPC_AskForUsername(player, NetworkManager.Instance.NetworkRunner.LocalPlayer);
        //CheckPlayersReady();
    }

    private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log(player.PlayerId + " left");
        foreach (var playerInRoomButton in playerInRoomButtons)
        {
            if (playerInRoomButton.gameObject.activeInHierarchy == false) continue;
            if (playerInRoomButton.ID == player.PlayerId)
            {
                playerInRoomButton.gameObject.SetActive(false);
            }
        }

        //if (NetworkManager.Instance == null) return;
        //if (NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient)
        //{
        //    if (readyPlayers.ContainsKey(player.PlayerId)) readyPlayers.Remove(player.PlayerId);
        //    CheckPlayersReady();
        //}
    }

    //private void CheckPlayersReady()
    //{
    //    if(NetworkManager.Instance == null) return;
    //    readyPlayerCount = 0;
    //    foreach (var player in readyPlayers)
    //    {
    //        if (player.Value == true) readyPlayerCount++;
    //    }

    //    if (readyPlayerCount >= Mathf.CeilToInt((float)NetworkManager.Instance.NetworkRunner.ActivePlayers.Count() * minimumReadyPercentage))
    //    {
    //        //Debug.Log("Enough player!");
    //        if (!startGameTimer.IsRunning)
    //        {
    //            startGameTimer = TickTimer.CreateFromSeconds(NetworkManager.Instance.NetworkRunner, startGameTime);
    //        }
    //    }
    //    else if (startGameTimer.IsRunning)
    //    {
    //        //Debug.Log("Not enough player!");
    //        startGameTimer = TickTimer.None;
    //    }
    //}

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_AskForUsername([RpcTarget] PlayerRef target, PlayerRef requester)
    {
        if (NetworkManager.Instance == null) return;
        //Debug.Log(requester + " asked " + target + " receiver: " + NetworkManager.Instance.NetworkRunner.LocalPlayer);
        RPC_AnswerUsername(requester, target, NetworkManager.Instance.Username);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_AnswerUsername([RpcTarget] PlayerRef requester, PlayerRef responder, string username)
    {
        //Debug.Log(responder + " answered " + requester);
        foreach (var playerInRoomButton in playerInRoomButtons)
        {
            if (playerInRoomButton == null) continue;
            if (playerInRoomButton.gameObject.activeInHierarchy == true && playerInRoomButton.ID == responder.PlayerId)
            {
                playerInRoomButton.Username = username;
                return;
            }
        }
    }
    //[Rpc(RpcSources.All, RpcTargets.All)]
    //private void RPC_AnnounceReady(PlayerRef player)
    //{
    //    if (NetworkManager.Instance == null) return;
    //    if (NetworkManager.Instance.NetworkRunner.IsSharedModeMasterClient)
    //    {
    //        //Debug.Log($"Player: {player.PlayerId} is ready!");
    //        if (!readyPlayers.ContainsKey(player.PlayerId))
    //        {
    //            readyPlayers.Add(player.PlayerId, true);
    //            CheckPlayersReady();
    //        }
    //        else if (readyPlayers[player.PlayerId] == false)
    //        {
    //            readyPlayers[player.PlayerId] = true;
    //            CheckPlayersReady();
    //        }
    //    }
    //}
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_AskToLeave([RpcTarget] PlayerRef targetPlayer)
    {
        LeaveRoom();
    }

}
