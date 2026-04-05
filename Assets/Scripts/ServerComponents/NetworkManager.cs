using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner networkRunnerPrefab;

    private static NetworkManager instance;
    private NetworkRunner networkRunner;

    [SerializeField] private string username;
    public string Username { get => username; set { Debug.Log(value); if (!username.IsNullOrEmpty()) username = value; } }

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

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player joins manager: " + player.PlayerId);

        onPlayerJoined.Invoke(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        onPlayerLeft.Invoke(runner, player);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
