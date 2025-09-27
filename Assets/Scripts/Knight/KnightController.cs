using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : MonoBehaviour
{
    private Vector2 moveInput = Vector2.zero;

    [SerializeField] private float moveSpeed = 5f;

    // Update is called once per frame
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.fixedDeltaTime;
        transform.Translate(move, Space.World);
    }
}
