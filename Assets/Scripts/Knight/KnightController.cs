using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class KnightController : MonoBehaviour
{
    private Vector2 moveInput = Vector2.zero;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.05f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // -------------------------
    // MOVEMENT INPUT
    // -------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
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
        if (context.started && canDash && moveInput != Vector2.zero)
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
        if (isDashing) return;

        Vector2 move = moveInput * moveSpeed * Time.fixedDeltaTime;
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
}
