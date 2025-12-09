using Unity.VisualScripting;
using UnityEngine;

// General Idea: We can create different kinds of food and each food will have different implementation of the OnInteract() function
// This is a simple example of a food that will call the PickUpFood() function on the player's FoodHolder script

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    [SerializeField] Recipe recipe;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject foodObject;
    [SerializeField] GameObject foodShadowObject;
    [SerializeField] Vector2 foodObjectPivot = new(); // The distance between the food object and the virtual point on the ground

    [Header("Physics Settings")]
    [SerializeField] float gravityScale = 1.0f;
    [SerializeField] Vector2 groundCollisionSize = new(1.0f, 0.5f); // The size of the virtual collision on the ground (used to check whether the customer is in range to receive food)
    [SerializeField] Vector2 groundCollisionPivot = new(0, 0);
    [SerializeField] Color groundCollisionColor = Color.green;
    //[SerializeField] float minHeightToReceiveCollider = 0.5f;

    public Recipe Recipe { get => recipe; }
    public float GravityScale { get => gravityScale; }

    private Vector2 foodPos;
    private Rigidbody2D rb;
    private Rigidbody2D foodRb;
    bool isPickedUp = false;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogWarning("Please assisn a sprite renderer or add SpriteRenderer component to the food object!");
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogWarning("Please assign a Rigibody2D to handle Physics");
        if (foodObject == null) Debug.LogWarning("Please assign a food object to handle physics!");
        else { foodRb = foodObject.GetComponent<Rigidbody2D>(); }
    }

    private void Update()
    {
        HandleFallingPhysics();
        CorrectFoodObjectPosition();
    }

    public void SetUp(Recipe recipe)
    {
        Debug.Log("Set up: " + recipe + "for " + spriteRenderer);
        if (recipe == null) { Debug.LogWarning("This recipe is invalid!"); return; }
        this.recipe = recipe;
        if (spriteRenderer != null) spriteRenderer.sprite = this.recipe.Icon;
    }

    public void SetHeight(float height)
    {
        if (foodObject != null) foodObject.transform.position = new Vector2(foodObject.transform.position.x, transform.position.y + foodObjectPivot.y + height);
    }

    public void ApplyVelocity(Vector2 groundVelocity, float verticalVelocity)
    {
        if (rb != null && groundVelocity != null) rb.linearVelocity = groundVelocity;
        if (foodRb != null)
        {
            foodRb.linearVelocityY = verticalVelocity;
            foodRb.gravityScale = gravityScale;
        }
    }

    private void HandleFallingPhysics()
    {
        if (foodRb == null) { Debug.LogWarning("No food rigidbody assigned!"); return; }
        if (isPickedUp) return; // Dont handle Physics if this food has been picked up
        foodRb.linearVelocityX = 0; // Make sure the food object only moves vertically
        if (foodPos.y <= transform.position.y + foodObjectPivot.y && foodRb.linearVelocityY <= 0) // If the food object is below the ground point and it's moving downward
        {
            rb.linearVelocity = Vector2.zero;
            foodRb.linearVelocityY = 0;
            foodRb.gravityScale = 0;
        }
        else
        {
            foodRb.gravityScale = gravityScale;
        }
    }

    private void CorrectFoodObjectPosition()
    {
        if (foodObject != null && foodRb != null)
        {
            if (!isPickedUp)
            {
                foodPos.x = transform.position.x + foodObjectPivot.x;
                foodPos.y = foodObject.transform.position.y;
                if (foodPos.y < transform.position.y + foodObjectPivot.y && foodRb.linearVelocityY == 0) // Only correct it's possition if it has hit the ground
                {
                    // Limit the food object's y position if it's on the ground
                    foodPos.y = transform.position.y + foodObjectPivot.y;
                }
                foodObject.transform.position = foodPos;
            }
            else
            {
                foodObject.transform.position = transform.position;
            }
        }
    }

    public void IsPickedUp()
    {
        isPickedUp = true;
        if (foodShadowObject != null) foodShadowObject.SetActive(false);
    }
    public void IsDropped()
    {
        isPickedUp = false;
        if (foodShadowObject != null) foodShadowObject.SetActive(true);
    }
    public void ReceiveCollider(Collider2D collider)
    {
        // Prototype
        if (collider == null) return;
        if (collider.tag == "Customer" && collider.GetComponent<IInteractable>() != null && !isPickedUp)
        {
            //if (foodObject != null && foodObject.transform.localPosition.y <= minHeightToReceiveCollider)
            //    collider.GetComponent<IInteractable>().OnInteract(this.gameObject);

            // Check if the customer is actually able to receive the food (within range)
            var colliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y), groundCollisionSize, 0f);
            foreach(var col in colliders)
            {
                if (col.gameObject == collider.gameObject)
                {
                    collider.GetComponent<IInteractable>().OnInteract(this.gameObject);
                }
            }
        }
    }

    public void OnInteract(GameObject player)
    {
        if(player && player.GetComponent<FoodHolder>() && !isPickedUp)
        {
            player.GetComponent<FoodHolder>().PickUpFood(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = groundCollisionColor;
        Gizmos.DrawWireCube((Vector2)transform.position + groundCollisionPivot, groundCollisionSize);
    }
}
