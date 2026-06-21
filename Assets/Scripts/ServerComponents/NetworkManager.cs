using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner networkRunnerPrefab;

    private static NetworkManager instance;
    private NetworkRunner networkRunner;

    [SerializeField] private string username;
    public string Username { get => username; set { /*Debug.Log(value);*/ if (!value.IsNullOrEmpty()) username = value; } }

    public NetworkRunner NetworkRunner { get => networkRunner; }

    // Unity events
    private UnityEvent<NetworkRunner, PlayerRef> onPlayerJoined = new();
    private UnityEvent<NetworkRunner, PlayerRef> onPlayerLeft = new();

    public UnityEvent<NetworkRunner, PlayerRef> OnPlayerJoinedEvent { get => onPlayerJoined; }
    public UnityEvent<NetworkRunner, PlayerRef> OnPlayerLeftEvent { get => onPlayerLeft; }

    public static NetworkManager Instance { get => instance; }

    private void Awake()
    {
        // if (networkRunner == null && networkRunnerPrefab != null) networkRunner = Instantiate(networkRunnerPrefab);

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public Task<StartGameResult> JoinRoom(GameMode gameMode, string roomId, int lobbySceneIndex)
    {
        return InitializeNetworkRunner(gameMode,
                                roomId,
                                NetAddress.Any(),
                                SceneRef.FromIndex(lobbySceneIndex),
                                null);
    }

    /// <summary>
    /// Cleanup sau khi StartGame fail hoặc khi player chủ động rời phòng.
    /// Cần gọi để cho phép user thử join lại — nếu không, <see cref="NetworkRunner"/>
    /// còn reference → <see cref="JoinRoom.OnJoinRoom"/> sẽ bỏ qua im lặng.
    /// </summary>
    public async Task CleanupNetworkRunnerAsync()
    {
        if (networkRunner == null) return;

        try
        {
            if (networkRunner.IsRunning)
            {
                await networkRunner.Shutdown(destroyGameObject: true);
            }
            else if (networkRunner != null && networkRunner.gameObject != null)
            {
                Destroy(networkRunner.gameObject);
            }
        }
        catch (Exception)
        {
            //Debug.LogWarning($"NetworkManager.CleanupNetworkRunnerAsync");
        }

        networkRunner = null;
    }
    private Task<StartGameResult> InitializeNetworkRunner(GameMode gameMode, string roomId, NetAddress address, SceneRef sceneRef, Action<NetworkRunner> initialized)
    {
        networkRunner = FindAnyObjectByType<NetworkRunner>();
        if(networkRunner == null)
        {
            if (networkRunnerPrefab == null) return null;
            networkRunner = Instantiate(networkRunnerPrefab);
        }

        networkRunner.ProvideInput = true;
        networkRunner.AddCallbacks(this);

        return networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Address = address,
            AuthValues = new AuthenticationValues(System.Guid.NewGuid().ToString()),
            Scene = sceneRef,
            SessionName = roomId,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });
        //return task;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log("Player joins manager: " + player.PlayerId);

        onPlayerJoined.Invoke(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        onPlayerLeft.Invoke(runner, player);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
