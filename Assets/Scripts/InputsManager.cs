using Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputsManager : MonoBehaviour
{
    private static InputsManager instance = null;

    public static InputsManager Instance { get => instance; }

    private MainInput mainInput;
    private PlayerInputsReceiver playerInputsReceiver;

    public PlayerInputsReceiver PlayerInputs { get => playerInputsReceiver; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        mainInput = new();
        playerInputsReceiver = new();
        mainInput.Player.AddCallbacks(playerInputsReceiver);

    }

    private void OnEnable()
    {
        mainInput.Player.Enable();
    }

    private void OnDisable()
    {
        mainInput.Player.RemoveCallbacks(playerInputsReceiver);
        mainInput.Disable();
    }
}

public class PlayerInputsReceiver : MainInput.IPlayerActions
{
    private Vector2 movementInput = new();
    private Vector2 joystickInput = new();
    private bool joystickActive = false;

    public Vector2 MovementInput { get => joystickActive ? joystickInput : movementInput; }

    public void SetJoystickInput(Vector2 value, bool active)
    {
        joystickInput = value;
        joystickActive = active;
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
    }

    public void OnDash(InputAction.CallbackContext context)
    {
    }

    public void OnDropFood(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log("OnMove: " + context.ReadValue<Vector2>().normalized);
        movementInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnThrowFood(InputAction.CallbackContext context)
    {
    }
}