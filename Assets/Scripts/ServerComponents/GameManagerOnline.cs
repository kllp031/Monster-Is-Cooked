using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManagerOnline : NetworkBehaviour
{
    // Spawning Settings
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<Vector2> spawnPositions = new();
    [SerializeField] List<PlayerRef> spawnedPlayers = new();

    public override void Spawned()
    {
        if (NetworkManager.Instance == null) return;
        if (playerPrefab == null || spawnPositions.Count == 0) return;
        //RPC_AnnounceReady(NetworkManager.Instance.NetworkRunner.LocalPlayer);
        NetworkManager.Instance.NetworkRunner.Spawn(playerPrefab, spawnPositions[Random.Range(0, spawnPositions.Count())], Quaternion.identity, NetworkManager.Instance.NetworkRunner.LocalPlayer);
    }

    //[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    //public void RPC_AnnounceReady(PlayerRef player)
    //{
    //    if (spawnedPlayers.Contains(player) || playerPrefab == null || spawnPositions.Count == 0) return;
    //    Debug.Log("Announce ready: " + player.PlayerId);
    //    var newPlayer = NetworkManager.Instance.NetworkRunner.Spawn(playerPrefab, spawnPositions[currentSpawnPosition], Quaternion.identity, player);
    //    if (newPlayer != null)
    //    {
    //        spawnedPlayers.Add(player);
    //        RPC_AnnounceSpawned(player);
    //    }

    //}

    //[Rpc(RpcSources.All, RpcTargets.All)]
    //public void RPC_AnnounceSpawned([RpcTarget] PlayerRef player)
    //{
    //    // Show announcement "Wait for Room Master to start the level"
    //    Debug.Log("Spawned successfully!");
    //}

}
