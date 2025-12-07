using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : MonoBehaviour
{
    private Vector2 moveInput = Vector2.zero;

    [SerializeField] private float moveSpeed = 5f;
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        Debug.Log("moveinput" + moveInput);
        //Animation
        if (moveInput != Vector2.zero)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);

        //Flip
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.fixedDeltaTime;
        transform.Translate(move, Space.World);
    }

    public Vector2 getMoveInput()
    {
        return moveInput;
    }
}
