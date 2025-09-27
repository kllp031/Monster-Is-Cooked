using UnityEngine;
using UnityEngine.InputSystem;

public class StoveInteract : MonoBehaviour
{
    [SerializeField] private GameObject stoveUI;
    private GameObject interactableStove;

    public InputActionAsset actions;
    private InputAction interactAction;

    private void OnEnable()
    {
        var map = actions.FindActionMap("Player", throwIfNotFound: false);
        map?.Enable();
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        var map = actions.FindActionMap("Player", throwIfNotFound: false);
        map?.Disable();
        interactAction?.Disable();
    }

    private void Start()
    {
        // Use the assigned InputActionAsset rather than InputSystem.actions
        interactAction = actions
            .FindActionMap("Player", throwIfNotFound: false)
            ?.FindAction("Interact", throwIfNotFound: false);
    }

    private void Update()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame() && interactableStove != null)
        {
            InteractStove();
        }
    }

    void InteractStove()
    {
        print("Interacting with Stove: " + interactableStove.name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Stove>(out Stove stove))
        {
            interactableStove = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Stove>(out Stove stove))
        {
            interactableStove = null;
        }
    }
}
