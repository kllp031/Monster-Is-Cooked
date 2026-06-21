using UnityEngine;
using Fusion;

public class DropItem : MonoBehaviour
{
    [Header("Pickup Prefabs (NetworkObject prefabs)")]
    public NetworkObject[] pickupPrefabs;

    public void DropNetworked(NetworkRunner runner, Vector3 position)
    {
        Debug.Log($"[DropItem] DropNetworked gọi tại {position}, runner={runner != null}, prefab count={pickupPrefabs?.Length ?? 0}");

        if (pickupPrefabs == null || pickupPrefabs.Length == 0)
        {
            Debug.LogWarning($"[DropItem] Không có prefab nào được gán trên '{gameObject.name}'!");
            return;
        }

        foreach (var prefab in pickupPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[DropItem] Một phần tử trong pickupPrefabs bị null trên '{gameObject.name}'!");
                continue;
            }
            Vector3 spawnPos = position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );
            Debug.Log($"[DropItem] Spawn '{prefab.name}' tại {spawnPos}");
            runner.Spawn(prefab, spawnPos, Quaternion.identity);
        }
    }
}
