using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : MonoBehaviour
{
    public InputActionAsset actions;
    private InputAction moveAction;
    private Vector2 moveInput;


    [SerializeField] private float moveSpeed = 5f;


    private void OnEnable()
    {
        actions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        actions.FindActionMap("Player").Disable();
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.fixedDeltaTime;
        transform.Translate(move, Space.World);
    }
}
