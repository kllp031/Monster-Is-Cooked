using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Threading;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    private Vector2 moveInput = Vector2.zero;

    [Header("Movement")]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.05f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isDashing = false;
    private bool isHurting = false;
    [SerializeField] private float hurtingTime = 0.5f;
    private float hurtingTimer = 0f;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

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
        if (health.isDeath)
            return;

        moveInput = context.ReadValue<Vector2>();

        animator.SetBool("isRunning", moveInput != Vector2.zero);

        // Flip
        if (moveInput.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
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

        Vector2 move = moveInput * PlayerDataManager.Instance.Speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
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
}
