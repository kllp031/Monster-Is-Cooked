using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// This class is used by the player to manage holding and dropping food items.
// General Idea: When we interact with a food that is interactable, that food will call PickUpFood() with itself as a parameter on this script
public class FoodHolder : MonoBehaviour
{
    private GameObject heldFood = null;
    [SerializeField] private Transform foodPivot;
    [SerializeField] GameObject dropFoodButton;

    private void Update()
    {
        if (heldFood)
        {
            dropFoodButton?.SetActive(true);
        }
        else
        {
            dropFoodButton?.SetActive(false);
        }
    }
    public void DropHeldFood()
    {
        if (heldFood == null) return;
        // Drop the food
        heldFood.transform.SetParent(null);
        heldFood.GetComponent<Collider2D>().enabled = true; // Re-enable collider
        heldFood.transform.position = transform.position + transform.right; // Drop in front of player
        heldFood = null;
    }
    public void PickUpFood(GameObject food)
    {
        if(heldFood != null) DropHeldFood();

        heldFood = food;
        heldFood.transform.SetParent(foodPivot);
        heldFood.transform.localPosition = Vector3.zero; // Adjust as needed
        heldFood.GetComponent<Collider2D>().enabled = false; // Disable collider to prevent further interactions
    }
    public void OnDropFood(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            DropHeldFood();
        }
    }
    public void ServeFood()
    {
        Debug.Log("Do nothing");
    }
}
