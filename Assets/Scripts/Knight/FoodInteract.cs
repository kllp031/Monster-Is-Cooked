using UnityEngine;
using UnityEngine.InputSystem;

public class FoodInteract : MonoBehaviour
{
    private GameObject interactableFoodItem;
    private GameObject heldFood;
    [SerializeField] private Transform foodPivot;


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
        if (interactAction != null && interactAction.WasPressedThisFrame() && 
            (interactableFoodItem != null || heldFood != null))
        {
            InteractFood();
        }
        if (heldFood != null)
        {
            heldFood.transform.position = foodPivot.position;
        }
    }

    private void InteractFood()
    {
        if (heldFood == null)
        {
            heldFood = interactableFoodItem;
            heldFood.transform.SetParent(foodPivot);
            heldFood.transform.localPosition = Vector3.zero; // Adjust as needed
            heldFood.GetComponent<Collider2D>().enabled = false; // Disable collider to prevent further interactions
            // Optionally disable any Rigidbody2D if present
            //var rb = heldFood.GetComponent<Rigidbody2D>();
            //if (rb != null) rb.isKinematic = true;
        }
        else
        {
            // Drop the food
            heldFood.transform.SetParent(null);
            heldFood.GetComponent<Collider2D>().enabled = true; // Re-enable collider
            heldFood.transform.position = transform.position + transform.right; // Drop in front of player
            //var rb = heldFood.GetComponent<Rigidbody2D>();
            //if (rb != null) rb.isKinematic = false;
            heldFood = null;
        }
    }

    public void ServeFood()
    {
        if (heldFood != null)
        {
            Destroy(heldFood);
            heldFood = null;
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // Use the component that actually exists on the object.
    //    if (collision.collider.TryGetComponent<InteractionPrompt>(out _))
    //    {
    //        interactableFoodItem = collision.collider.gameObject;
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.collider.gameObject == interactableFoodItem)
    //    {
    //        interactableFoodItem = null;
    //    }
    //}

    // If your "food" uses trigger colliders, use these instead of the collision callbacks.

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Food>(out _))
            interactableFoodItem = other.gameObject;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == interactableFoodItem)
            interactableFoodItem = null;
    }
    
}
