using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// Networked held-food state cho online player. Thay thế hoàn toàn offline
/// <see cref="FoodHolder"/> trên online prefab.
///
/// Cách hoạt động:
/// - <see cref="HeldRecipeId"/> = <c>[Networked]</c> int. -1 = không cầm gì.
///   Lookup recipe qua <see cref="RecipeRegistry"/>.
/// - Mỗi client tự render icon trên một child SpriteRenderer (auto-create).
/// - Local hotbar dùng <see cref="HotbarManagerOnline"/> (lưu Recipe trực tiếp,
///   không cần local Food gameobject).
/// - Drop / Throw spawn <see cref="FoodOnline"/> qua <c>Runner.Spawn</c>.
/// - Pickup networked: add recipe vào hotbar online, RPC despawn networked
///   instance.
/// </summary>
public class FoodHolderOnline : NetworkBehaviour
{
    [Header("Recipe / Visual")]
    [Tooltip("Asset RecipeRegistry — phải cùng asset trên mọi client.")]
    [SerializeField] private RecipeRegistry recipeRegistry;

    [Tooltip("Vị trí icon món cầm trên đầu player (local space).")]
    [SerializeField] private Vector3 holdLocalPosition = new Vector3(0f, 1f, 0f);

    [Tooltip("Sorting order của icon. Lớn hơn để icon nổi trên sprite player.")]
    [SerializeField] private int iconSortingOrder = 10;

    [Tooltip("Sorting layer của icon. Để trống = layer mặc định.")]
    [SerializeField] private string iconSortingLayer = "";

    [Header("World Food (Networked)")]
    [Tooltip("Prefab FoodOnline (NetworkObject) spawn vào world khi drop/throw.")]
    [SerializeField] private NetworkPrefabRef foodOnlinePrefab;

    [Tooltip("Gravity scale dùng cho công thức tính throw speed. Phải khớp giá trị trên FoodOnline prefab để parabola landing đúng target.")]
    [SerializeField] private float foodGravityScale = 1.0f;

    [Header("Throw Settings")]
    [Tooltip("Transform dùng làm gốc tính ground point (chân player). Có thể là child của player.")]
    [SerializeField] private Transform playerGroundPosition;

    [Tooltip("UI/Scene object hiển thị target khi đang aim throw. SetActive(true/false) trong coroutine.")]
    [SerializeField] private GameObject targetCircle;

    [Tooltip("Giữ button quá ngưỡng này (giây) sẽ chuyển sang aim throw thay vì drop.")]
    [SerializeField] private float holdToThrowTime = 0.3f;

    [SerializeField] private float targetCircleMovingTime = 1.0f;
    [SerializeField] private Vector2 throwDirection = Vector2.right;
    [SerializeField] private float throwStartDistance = 2.0f;
    [SerializeField] private float throwEndDistance = 10.0f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private float throwHeightOffset = 1.0f;
    [SerializeField] private LayerMask obstacleLayer;

    [Networked] public int HeldRecipeId { get; set; }

    private SpriteRenderer iconRenderer;
    private Coroutine throwingCoroutine;

    // Single-button drop/throw state.
    private bool isHolding;
    private float pressStartTime;
    private bool aimStarted;

    public Recipe HeldRecipe =>
        recipeRegistry != null ? recipeRegistry.GetRecipe(HeldRecipeId) : null;

    /// <summary>Recipe đang chọn ở local hotbar online.</summary>
    private Recipe HeldRecipeLocal =>
        HotbarManagerOnline.Instance != null ? HotbarManagerOnline.Instance.CurrentRecipe : null;

    public override void Spawned()
    {
        EnsureIconRenderer();
        if (Object.HasStateAuthority)
            HeldRecipeId = -1;
        UpdateVisual();
    }

    private void Update()
    {
        if (Object == null || !Object.IsValid) return;
        EnsureIconRenderer();
        UpdateVisual();
        TickHoldDetect();
    }

    private void TickHoldDetect()
    {
        if (!isHolding || aimStarted) return;
        if (HeldRecipeLocal == null) return;
        if (Time.time - pressStartTime < holdToThrowTime) return;

        aimStarted = true;
        OnThrowFoodStarted();
    }

    private void OnDisable()
    {
        if (throwingCoroutine != null) { StopCoroutine(throwingCoroutine); throwingCoroutine = null; }
        if (targetCircle != null) targetCircle.SetActive(false);
    }

    private void EnsureIconRenderer()
    {
        if (iconRenderer != null) return;
        var go = new GameObject("HeldFoodIcon");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = holdLocalPosition;
        iconRenderer = go.AddComponent<SpriteRenderer>();
        iconRenderer.sortingOrder = iconSortingOrder;
        if (!string.IsNullOrEmpty(iconSortingLayer))
            iconRenderer.sortingLayerName = iconSortingLayer;
    }

    private void UpdateVisual()
    {
        if (iconRenderer == null) return;
        var r = HeldRecipe;
        if (r != null)
        {
            iconRenderer.sprite = r.Icon;
            iconRenderer.enabled = true;
        }
        else
        {
            iconRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Đặt món đang cầm. Chỉ state authority mới được ghi. Pass null để clear.
    /// </summary>
    public void SetHeldRecipe(Recipe recipe)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        HeldRecipeId = recipeRegistry != null ? recipeRegistry.GetId(recipe) : -1;
    }

    // ------------------------------------------------------------
    //  Pickup / Drop / Throw — chỉ chạy trên input authority client.
    //  Bridge sync networked id sau khi HotbarManagerOnline thay đổi.
    // ------------------------------------------------------------

    /// <summary>
    /// Pickup từ offline <see cref="Food"/> trong scene (vd legacy props).
    /// Trích recipe → add vào hotbar online → destroy local Food gameobject.
    /// </summary>
    public void PickUpFood(Food food)
    {
        if (!IsLocalAuthority()) return;
        if (food == null) return;
        if (HotbarManagerOnline.Instance == null) return;

        bool added = HotbarManagerOnline.Instance.AddRecipe(food.Recipe);
        if (added) Destroy(food.gameObject);
    }

    /// <summary>
    /// Pickup networked food từ world (drop/throw của bất kỳ ai). Add recipe
    /// vào hotbar online → RPC sang state authority để despawn networked
    /// instance.
    /// </summary>
    public void PickUpFromWorld(FoodOnline networkedFood)
    {
        if (!IsLocalAuthority()) return;
        if (networkedFood == null) return;
        if (HotbarManagerOnline.Instance == null) return;
        if (HotbarManagerOnline.Instance.IsHotbarFull()) return;

        bool added = HotbarManagerOnline.Instance.AddRecipe(networkedFood.Recipe);
        if (!added) return;

        networkedFood.RPC_RequestDespawn();
    }

    private void SpawnFoodOnlineAt(Vector3 position, float groundY, Recipe recipe, Vector2 groundVelocity, float verticalVelocity)
    {
        if (Runner == null) { /*Debug.LogWarning("Runner null — không thể Spawn FoodOnline.");*/ return; }
        if (foodOnlinePrefab == null || !foodOnlinePrefab.IsValid)
        {
            //Debug.LogWarning($"{nameof(FoodHolderOnline)}: foodOnlinePrefab chưa gán — drop/throw sẽ không tạo networked food.");
            return;
        }
        if (recipe == null) return;

        var foodObj = Runner.Spawn(
            foodOnlinePrefab,
            position,
            Quaternion.identity,
            Object.InputAuthority);

        if (foodObj == null) return;

        var foodOnline = foodObj.GetComponent<FoodOnline>();
        if (foodOnline == null) return;

        foodOnline.SetRecipe(recipe);
        foodOnline.SetGroundY(groundY);
        if (groundVelocity != Vector2.zero || verticalVelocity != 0f)
            foodOnline.ApplyVelocity(groundVelocity, verticalVelocity);
    }

    public void DropHeldFood()
    {
        if (!IsLocalAuthority()) return;
        var recipe = HeldRecipeLocal;
        if (recipe == null) return;

        // Drop ngay cạnh player. Ground Y = chân player nếu có, else
        // Y hiện tại (food đứng yên ngay tại đó).
        float groundY = playerGroundPosition != null
            ? playerGroundPosition.position.y
            : transform.position.y;

        Vector3 dropPos = new Vector3(
            transform.position.x + (transform.lossyScale.x <= 0 ? 1f : -1f),
            groundY,
            transform.position.z);

        SpawnFoodOnlineAt(dropPos, groundY, recipe, Vector2.zero, 0f);
        if (HotbarManagerOnline.Instance != null)
            HotbarManagerOnline.Instance.RemoveSelectedRecipe();
    }

    public void OnThrowFoodStarted()
    {
        if (!IsLocalAuthority()) return;
        if (targetCircle == null || playerGroundPosition == null || HeldRecipeLocal == null) return;

        if (throwingCoroutine != null) StopCoroutine(throwingCoroutine);
        throwingCoroutine = StartCoroutine(ThrowingCoroutine());
    }

    public void OnThrowFoodEnded()
    {
        if (!IsLocalAuthority()) return;
        if (throwingCoroutine == null) return;

        StopThrowFood();

        var recipe = HeldRecipeLocal;
        if (targetCircle == null || recipe == null || playerGroundPosition == null) return;

        Vector2 groundStartPoint = (Vector2)playerGroundPosition.position;
        Vector2 groundEndPoint = targetCircle.transform.position;

        float speed = CalculateThrowSpeed(
            (groundEndPoint - groundStartPoint).magnitude,
            -throwHeightOffset,
            throwAngle,
            foodGravityScale);

        Vector2 groundVelocity = Vector2.zero;
        float verticalVelocity = 0f;
        if (speed > 0)
        {
            float groundSpeed = speed * Mathf.Cos(throwAngle * Mathf.Deg2Rad);
            groundVelocity = (groundEndPoint - groundStartPoint).normalized * groundSpeed;
            verticalVelocity = speed * Mathf.Sin(throwAngle * Mathf.Deg2Rad);
        }

        float groundY = groundStartPoint.y;
        Vector3 spawnPos = new Vector3(groundStartPoint.x, groundY + throwHeightOffset, transform.position.z);

        SpawnFoodOnlineAt(spawnPos, groundY, recipe, groundVelocity, verticalVelocity);
        if (HotbarManagerOnline.Instance != null)
            HotbarManagerOnline.Instance.RemoveSelectedRecipe();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerThrow);
    }

    public void StopThrowFood()
    {
        if (targetCircle != null) targetCircle.SetActive(false);
        if (throwingCoroutine != null) StopCoroutine(throwingCoroutine);
        throwingCoroutine = null;
    }

    // ------------------------------------------------------------
    //  Single-button drop/throw API.
    //  - PointerDown gọi OnFoodActionPressed.
    //  - PointerUp gọi OnFoodActionReleased.
    //  - Tap (release < holdToThrowTime) → DropHeldFood.
    //  - Hold (>= ngưỡng) → vào aim throw → release để ném.
    // ------------------------------------------------------------

    public void OnFoodActionPressed()
    {
        if (!IsLocalAuthority()) return;
        if (HeldRecipeLocal == null) return;

        isHolding = true;
        pressStartTime = Time.time;
        aimStarted = false;
    }

    public void OnFoodActionReleased()
    {
        if (!IsLocalAuthority()) return;
        if (!isHolding) return;

        isHolding = false;

        if (aimStarted)
        {
            OnThrowFoodEnded();
            aimStarted = false;
        }
        else
        {
            DropHeldFood();
        }
    }

    private bool IsLocalAuthority() => Object != null && Object.HasInputAuthority;

    private IEnumerator ThrowingCoroutine()
    {
        if (targetCircle == null) yield break;

        targetCircle.SetActive(true);
        targetCircle.transform.position = (Vector2)playerGroundPosition.position
            + throwDirection.normalized * throwStartDistance;

        Vector2 startPosition;
        Vector2 endPosition;

        float timeElapsed = 0f;
        bool movingForward = true;

        while (HeldRecipeLocal != null)
        {
            throwDirection = (transform.localScale.x <= 0) ? Vector2.right : Vector2.left;

            var checkCollider = Physics2D.Raycast(
                transform.position, throwDirection, throwStartDistance, obstacleLayer);
            if (checkCollider.collider != null) { StopThrowFood(); yield break; }

            startPosition = (Vector2)playerGroundPosition.position
                + throwDirection.normalized * throwStartDistance;
            endPosition = (Vector2)playerGroundPosition.position
                + throwDirection.normalized * throwEndDistance;

            timeElapsed += Time.deltaTime;
            if (timeElapsed >= targetCircleMovingTime)
            {
                movingForward = !movingForward;
                timeElapsed = 0f;
            }

            Vector2 targetPos = movingForward
                ? Vector2.Lerp(startPosition, endPosition, timeElapsed / targetCircleMovingTime)
                : Vector2.Lerp(endPosition, startPosition, timeElapsed / targetCircleMovingTime);

            targetCircle.transform.position = targetPos;

            yield return null;
        }

        targetCircle.SetActive(false);
    }

    private float CalculateThrowSpeed(float horizontalDistance, float verticalDistance, float angle, float gravityScale)
    {
        angle = angle * Mathf.Deg2Rad;
        float g = -Physics2D.gravity.magnitude * gravityScale;
        float x = Mathf.Abs(horizontalDistance);
        float y = verticalDistance;
        float numerator = g * x * x;
        float denominator = 2f * (y - Mathf.Tan(angle) * x) * Mathf.Pow(Mathf.Cos(angle), 2);
        if (denominator >= 0)
        {
            //Debug.LogWarning("Invalid throw parameters. Cannot calculate throw speed.");
            return -1f;
        }
        return Mathf.Sqrt(numerator / denominator);
    }

    public void ServeFood()
    {
        Debug.Log("Serve food to FoodHolderOnline!");
    }
}
