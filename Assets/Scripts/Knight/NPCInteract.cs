using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{
    private GameObject interactableNPC;

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
        if (interactAction != null && interactAction.WasPressedThisFrame() && interactableNPC != null)
        {
            InteractNPC();
        }
    }

    void InteractNPC()
    {
        if (interactableNPC != null)
        {
            Debug.Log("Interacting with NPC: " + interactableNPC.name);
            if (GetComponent<FoodInteract>() != null)
            {
                GetComponent<FoodInteract>().ServeFood();
            }
            else
            {
                Debug.LogWarning("Hello.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<NPC>(out _))
        {
            interactableNPC = collision.gameObject;
            Debug.Log("Collided with NPC: " + interactableNPC.name);
        }
    }

    // Fixed: Changed parameter type from Collision2D to Collider2D
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<NPC>(out _))
        {
            if (interactableNPC == other.gameObject)
            {
                Debug.Log("Exited collision with NPC: " + interactableNPC.name);
                interactableNPC = null;
            }
        }
    }
}
