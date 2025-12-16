using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    private Vector2 moveInput = Vector2.zero;

    [Header("Movement")]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.05f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private Transform spawnPosition;

    private bool isDashing = false; 
    private bool isHurting = false;
    [SerializeField] private float hurtingTime = 0.5f;
    private float hurtingTimer = 0f;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    [SerializeField] private Image dashCooldownEffect;

    [SerializeField] private Joystick dynamicJoystick; // Joystick Pack
    [SerializeField] private float deadZone = 0.05f;
    private Vector2 inputFromJoystick;
    private bool isUsingJoystick = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    // -------------------------
    // MOVEMENT INPUT
    // -------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        if (health.isDeath || isUsingJoystick)
            return;

        moveInput = context.ReadValue<Vector2>().normalized;
    }

    // -------------------------
    // DASH INPUT
    // -------------------------
    public void OnDash(InputAction.CallbackContext context)
    {
        if (health.isDeath)
            return;

        if (context.started && canDash && moveInput != Vector2.zero)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerDash);
            StartCoroutine(DashRoutine());
        }
    }

    public void OnDashButton()
    {
        if (health.isDeath)
            return;

        if (canDash && moveInput != Vector2.zero)
        {
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

        // Set dash cooldown UI
        dashCooldownEffect.fillAmount = 1;

        Vector2 dashDir = moveInput.normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            Vector2 newPos = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            //Dash effect
            CreateAfterImage();

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // -------------------------
    // FIXED UPDATE MOVEMENT
    // -------------------------
    private void FixedUpdate()
    {
        if (health.isDeath)
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

        Vector2 move = moveInput * PlayerDataManager.Instance.CurrentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private void Update()
    {
       //Update dash cooldown UI
       if (!canDash)
          {
                dashCooldownEffect.fillAmount -= 1f / dashCooldown * Time.deltaTime;
                if (dashCooldownEffect.fillAmount < 0)
                 dashCooldownEffect.fillAmount = 0;
          }

        // Input từ joystick UI
        if (dynamicJoystick != null)
        {
            inputFromJoystick = new Vector2(
                dynamicJoystick.Horizontal,
                dynamicJoystick.Vertical
            ).normalized;

            if (inputFromJoystick.magnitude > deadZone)
            {
                isUsingJoystick = true;
                moveInput = inputFromJoystick;
            }
            else if (isUsingJoystick)
            {
                // vừa thả joystick
                isUsingJoystick = false;
                moveInput = Vector2.zero;
            }

        }

       

        // Animator
        animator.SetBool("isRunning", moveInput != Vector2.zero);

        // Flip
        if (moveInput.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(1, 1, 1);

    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    //Dash effect
    [SerializeField] GameObject afterImagePrefab;
    private void CreateAfterImage()
    {
        GameObject img = Instantiate(afterImagePrefab, transform.position, Quaternion.identity);
        img.transform.localScale = transform.localScale;
    }

    public void SetIsHurting(bool isHurting)
    {
        this.isHurting = isHurting;
    }

    public void Respawn()
    {
        health.isDeath = false;
        health.ReceiveHealing(1000000);
        transform.position = spawnPosition.position;
        animator.SetTrigger("Revive");
        Camera.main.GetComponent<CameraFollow>().BackHome();
    }
}
