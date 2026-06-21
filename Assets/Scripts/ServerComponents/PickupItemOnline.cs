using Fusion;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class PickupItemOnline : NetworkBehaviour
{
    [Header("Item Data")]
    public Ingredient ingredient;
    public int amount = 1;

    [Header("Visuals")]
    [SerializeField] private bool autoUpdateSprite = true;
    [SerializeField] private GameObject itemEffect;

    private SpriteRenderer spriteRenderer;
    private ItemsSpawnerOnline itemSpawner;

    [Header("Pickup Delay")]
    [SerializeField] private float pickupDelay = 0.3f;

    [Networked] private NetworkBool IsCollected { get; set; }
    [Networked] private TickTimer PickupDelayTimer { get; set; }

    public override void Spawned()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (autoUpdateSprite && ingredient != null)
            spriteRenderer.sprite = ingredient.icon;
        if (Object.HasStateAuthority)
            PickupDelayTimer = TickTimer.CreateFromSeconds(Runner, pickupDelay);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        string reason = IsCollected ? "đã được collect (RPC_Collect)" : "KHÔNG rõ lý do — không qua RPC_Collect!";
        //Debug.Log($"[Item] Despawned → IsCollected = {IsCollected} → {reason}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[Item] OnTriggerEnter2D với '{other.name}' (tag={other.tag})");

        if (!other.CompareTag("Player"))
        {
            //Debug.Log($"[Item] Bỏ qua: không phải Player");
            return;
        }
        if (IsCollected)
        {
            //Debug.Log($"[Item] Bỏ qua: đã IsCollected = true");
            return;
        }
        if (!PickupDelayTimer.ExpiredOrNotRunning(Runner))
        {
            Debug.Log($"[Item] Bỏ qua: chưa hết delay ({PickupDelayTimer.RemainingTime(Runner):F2}s còn lại)");
            return;
        }

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasInputAuthority)
        {
            //Debug.Log($"[Item] Bỏ qua: netObj={netObj != null}, HasInputAuthority={netObj?.HasInputAuthority}");
            return;
        }

        //Debug.Log($"[Item] Player local nhặt item → thêm vào inventory và gửi RPC_Collect");

        var playerBridge = other.GetComponent<PlayerInventory>();
        if (playerBridge != null && playerBridge.inventoryData != null)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.playerCollect);

            playerBridge.inventoryData.Add(ingredient, amount);

            if (itemEffect != null)
                Instantiate(itemEffect, transform.position, Quaternion.identity);
        }
        else
        {
            //Debug.LogWarning($"[Item] PlayerInventory hoặc inventoryData bị null trên player '{other.name}'");
        }

        RPC_Collect();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Collect()
    {
        //Debug.Log($"[Item][StateAuth] RPC_Collect nhận, IsCollected hiện tại = {IsCollected}");

        if (IsCollected)
        {
            //Debug.Log($"[Item][StateAuth] Bỏ qua: đã collect rồi (race condition)");
            return;
        }

        IsCollected = true;
        //Debug.Log($"[Item][StateAuth] IsCollected = true, itemSpawner = {(itemSpawner != null ? itemSpawner.name : "NULL")}");

        if (itemSpawner != null)
            itemSpawner.OnItemCollected();

        //Debug.Log($"[Item][StateAuth] Gọi Runner.Despawn...");
        Runner.Despawn(Object);
    }

    public void SetupItemSpawner(ItemsSpawnerOnline spawner)
    {
        itemSpawner = spawner;
        //Debug.Log($"[Item] SetupItemSpawner được gọi, spawner = {(spawner != null ? spawner.name : "null")}");
    }
}
