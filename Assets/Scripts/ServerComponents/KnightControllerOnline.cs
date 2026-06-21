using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KnightControllerOnline : NetworkBehaviour
{
    [Networked] private Vector2 moveInput { get; set; }

    [Header("Movement")]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 5.0f;
    [SerializeField] private float dashCooldown = 1f;

    // Scene references — bind runtime qua LocalPlayerHUD.Bind(this). KHÔNG gán
    // trực tiếp trên prefab vì prefab không thể tham chiếu scene object.
    private Transform spawnPosition;
    private Image dashCooldownEffect;
    private Joystick dynamicJoystick; // Joystick Pack

    [Networked] private NetworkBool isDashing { get; set; }
    private bool _pendingRespawn = false;
    private bool isHurting = false;
    [SerializeField] private float hurtingTime = 0.5f;
    private float hurtingTimer = 0f;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    [SerializeField] private float deadZone = 0.05f;
    private Vector2 inputFromJoystick;
    private bool isUsingJoystick = false;

    /// <summary>
    /// Gọi bởi LocalPlayerHUD để đẩy các reference UI/scene vào player local.
    /// Chỉ player có InputAuthority (player của máy này) mới nên nhận binding.
    /// </summary>
    public void ConfigureLocalHUD(Joystick joystick, Image dashCooldownImage, Transform spawnPoint)
    {
        dynamicJoystick = joystick;
        dashCooldownEffect = dashCooldownImage;
        spawnPosition = spawnPoint;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        GameManagerOnline.OnLevelStarted += HandleLevelStarted;

        if (Object.HasInputAuthority)
        {
            // Camera
            if (Camera.main != null)
            {
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null) camFollow.PlayerTransform = transform;
            }

            // Scene-local HUD (joystick, dash cooldown image, spawn point).
            if (LocalPlayerHUD.Instance != null)
            {
                LocalPlayerHUD.Instance.Bind(this);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(LocalPlayerHUD)} not found in scene. Local player UI/refs will be null.");
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GameManagerOnline.OnLevelStarted -= HandleLevelStarted;
    }

    private void HandleLevelStarted()
    {
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;
        //Debug.Log($"[KnightControllerOnline] HandleLevelStarted on {gameObject.name}, spawnPosition={(spawnPosition != null ? spawnPosition.position.ToString() : "NULL")}");
        health?.Respawn();
        _pendingRespawn = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (_pendingRespawn && Object.HasStateAuthority && Runner.IsForward)
        {
            _pendingRespawn = false;
            DoRespawnMovement();
        }

        if (GetInput(out InputData inputData))
        {
            moveInput = inputData.MovementInput;
        }

        if (health != null && health.isDeath)
            return;

        if (isDashing) return;

        if (isHurting)
        {
            hurtingTimer += Time.deltaTime;
            if (hurtingTimer >= hurtingTime)
            {
                isHurting = false;
                hurtingTimer = 0f;
            }
            else
            {
                return;
            }
        }

        Vector2 move = moveInput * 5.0f * Runner.DeltaTime;
        rb.MovePosition(rb.position + move);
    }

    public void StartDash()
    {
        if (health == null || health.isDeath)
            return;

        if (canDash && moveInput != Vector2.zero)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.playerDash);
            StartCoroutine(DashRoutine());
        }
    }

    // -------------------------
    // DASH USING MovePosition
    // -------------------------
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;

        // Set dash cooldown UI (local player only)
        if (dashCooldownEffect != null) dashCooldownEffect.fillAmount = 1;

        Vector2 dashDir = moveInput.normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            Vector2 newPos = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Update()
    {
        // UI + input chỉ chạy trên local player (có InputAuthority). Remote
        // player không có HUD và cũng không được ghi [Networked] moveInput.
        if (Object == null || !Object.IsValid) return;
        bool isLocal = Object.HasInputAuthority;

        // Update dash cooldown UI (local only)
        if (isLocal && !canDash && dashCooldownEffect != null)
        {
            dashCooldownEffect.fillAmount -= 1f / dashCooldown * Time.deltaTime;
            if (dashCooldownEffect.fillAmount < 0)
                dashCooldownEffect.fillAmount = 0;
        }

        // Input từ joystick UI (local only). Đẩy vào InputsManager để
        // OnInput poll thống nhất một nguồn → InputData → GetInput trong FUN.
        if (isLocal && dynamicJoystick != null && InputsManager.Instance != null)
        {
            inputFromJoystick = new Vector2(
                dynamicJoystick.Horizontal,
                dynamicJoystick.Vertical
            ).normalized;

            bool active = inputFromJoystick.magnitude > deadZone;
            if (active)
            {
                isUsingJoystick = true;
                InputsManager.Instance.PlayerInputs.SetJoystickInput(inputFromJoystick, true);
            }
            else if (isUsingJoystick)
            {
                isUsingJoystick = false;
                InputsManager.Instance.PlayerInputs.SetJoystickInput(Vector2.zero, false);
            }
        }

        // Animator + flip (chạy trên tất cả clients để remote proxy cũng
        // animate đúng theo [Networked] moveInput được đồng bộ).
        // Trong lúc dash, dashDir đã frozen — bỏ qua flip/anim theo moveInput
        // để sprite không quay ngược hướng dash khi joystick tilt khác chiều.
        if (animator != null)
        {
            animator.SetBool("isRunning", isDashing || moveInput != Vector2.zero);
        }

        if (!isDashing)
        {
            if (moveInput.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (moveInput.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    //Dash effect
    [SerializeField] GameObject afterImagePrefab;
    [SerializeField] private float afterImageInterval = 0.05f;
    private float afterImageTimer = 0f;

    public override void Render()
    {
        // Afterimage VFX phải chạy trên mọi client (kể cả remote) nên không
        // thể spawn trong DashRoutine — coroutine chỉ chạy ở client của
        // dasher. Render() chạy mỗi frame ở mọi client; gate theo [Networked]
        // isDashing để các client khác cũng thấy effect.
        if (isDashing && afterImagePrefab != null)
        {
            afterImageTimer += Time.deltaTime;
            if (afterImageTimer >= afterImageInterval)
            {
                afterImageTimer = 0f;
                CreateAfterImage();
            }
        }
        else
        {
            afterImageTimer = 0f;
        }
    }

    private void CreateAfterImage()
    {
        GameObject img = Instantiate(afterImagePrefab, transform.position, Quaternion.identity);
        img.transform.localScale = transform.localScale;
    }

    public void SetIsHurting(bool isHurting)
    {
        this.isHurting = isHurting;
    }

    // Called from FixedUpdateNetwork (Fusion-safe context) to teleport the player.
    private void DoRespawnMovement()
    {
        isDashing = false;
        moveInput = Vector2.zero;

        if (spawnPosition == null)
        {
            Debug.LogWarning($"[KnightControllerOnline] DoRespawnMovement: spawnPosition is NULL on {gameObject.name}");
            return;
        }
        //Debug.Log($"[KnightControllerOnline] DoRespawnMovement: teleporting {gameObject.name} to {spawnPosition.position}");

        Vector2 targetPos = spawnPosition.position;

        // Set rb.position directly — NetworkRigidbody2D reads this in its FixedUpdateNetwork
        // and syncs it to all clients. Avoids Teleport() which requires _physicsSimulator to be ready.
        if (rb != null)
        {
            rb.position = targetPos;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            transform.position = targetPos;
        }
    }

    // Public entry point for manual respawn (e.g., called from UI or other scripts).
    public void Respawn()
    {
        if (!Object.HasStateAuthority) return;
        health?.Respawn();
        _pendingRespawn = true;

        isHurting = false;
        hurtingTimer = 0f;
        canDash = true;

        if (Object.HasInputAuthority && Camera.main != null)
        {
            var camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.BackHome();
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMusic(SoundManager.Instance.cookingMusic);
    }
}
