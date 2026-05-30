using Fusion;
using UnityEngine;

/// <summary>
/// Networked food trong world (kết quả của drop/throw từ player). Khác offline
/// <see cref="Food"/>: vị trí + velocity sync qua <see cref="NetworkRigidbody2D"/>,
/// recipe sync qua <see cref="RecipeRegistry"/> id.
///
/// Lifecycle:
/// - Drop/Throw: client của player gọi <c>Runner.Spawn(foodOnlinePrefab)</c>;
///   spawner = state authority (Shared mode default). State auth set recipe
///   + velocity ngay sau spawn.
/// - Pickup: <see cref="OnInteract"/> chạy trên client của picker → forward
///   sang <see cref="FoodHolderOnline.PickUpFromWorld"/>; picker thêm vào local
///   hotbar và RPC sang state auth để despawn networked instance.
///
/// Prefab requirements:
/// - <see cref="NetworkObject"/>
/// - <see cref="Rigidbody2D"/> + <see cref="NetworkRigidbody2D"/> (Fusion physics addon)
/// - <see cref="Collider2D"/> (trigger để pickup)
/// - SpriteRenderer (icon)
/// - Tag/Layer phù hợp với InteractableDetector của player.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class FoodOnline : NetworkBehaviour, IInteractable
{
    [Tooltip("Sprite renderer để hiển thị icon món ăn. Tự GetComponent nếu để trống.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Asset RecipeRegistry — phải cùng asset trên mọi client.")]
    [SerializeField] private RecipeRegistry recipeRegistry;

    [Tooltip("Gravity scale dùng khi food đã rời khỏi tay (drop/throw). Mirror Food.gravityScale.")]
    [SerializeField] private float gravityScale = 1.0f;

    [Networked] public int RecipeId { get; set; }

    // Mức Y mà food sẽ "ground out" — gravity + velocity bị triệt khi food
    // rơi tới đây. Networked để client mới spawn-in cũng sync được, và để
    // state authority migration không mất tham số.
    [Networked] public float GroundY { get; set; }

    [Networked] public NetworkBool Grounded { get; set; }

    private Rigidbody2D rb;

    public Recipe Recipe => recipeRegistry != null ? recipeRegistry.GetRecipe(RecipeId) : null;
    public float GravityScale => gravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Spawned()
    {
        if (rb != null)
        {
            if (Object.HasStateAuthority)
                rb.position = (Vector2)transform.position;
            rb.gravityScale = gravityScale;
        }
        UpdateVisual();
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        var r = Recipe;
        if (r != null) spriteRenderer.sprite = r.Icon;
    }

    /// <summary>State authority set recipe id ngay sau spawn.</summary>
    public void SetRecipe(Recipe recipe)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        RecipeId = recipeRegistry != null ? recipeRegistry.GetId(recipe) : -1;
    }

    /// <summary>State authority set ground Y — food sẽ stop khi rơi tới đây.</summary>
    public void SetGroundY(float y)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        GroundY = y;
        Grounded = false;
    }

    /// <summary>State authority apply velocity. Mirror Food.ApplyVelocity API.</summary>
    public void ApplyVelocity(Vector2 groundVelocity, float verticalVelocity)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (rb == null) return;

        rb.linearVelocity = groundVelocity + new Vector2(0f, verticalVelocity);
        rb.gravityScale = gravityScale;
        Grounded = false;
    }

    public override void Render()
    {
        if (rb == null) return;
        if (Grounded)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.HasStateAuthority) return;
        if (rb == null) return;

        if (Grounded) return;

        // Detect chạm ground: Y rơi xuống tới hoặc dưới GroundY và đang đi
        // xuống. Triệt gravity + velocity → food đứng yên.
        if (rb.position.y <= GroundY && rb.linearVelocity.y <= 0f)
        {
            rb.position = new Vector2(rb.position.x, GroundY);
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            Grounded = true;
        }
    }

    // ------------------------------------------------------------
    //  IInteractable: chạy trên client của picker (input authority của
    //  player, không phải của food). Forward sang FoodHolderOnline.
    // ------------------------------------------------------------
    public void OnInteract(GameObject player)
    {
        if (player == null) return;
        var holder = player.GetComponent<FoodHolderOnline>();
        if (holder == null) return;
        holder.PickUpFromWorld(this);
    }

    /// <summary>
    /// Bất kỳ client nào cũng gọi được; chỉ state authority thực thi despawn.
    /// Gọi sau khi picker đã add vào local hotbar.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDespawn()
    {
        if (Runner == null || Object == null) return;
        Runner.Despawn(Object);
    }
}
