using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class PickupItem : MonoBehaviour
{
    [Header("Item Data")]
    public Ingredient ingredient;
    public int amount = 1;

    [Header("Visuals")]
    [SerializeField] private bool autoUpdateSprite = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Automatically set the sprite based on the Ingredient data
        // This saves you from manually setting sprites for every object
        if (autoUpdateSprite && ingredient != null)
        {
            spriteRenderer.sprite = ingredient.icon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if the object colliding is the Player
        if (other.CompareTag("Player"))
        {
            // 2. Try to find the Inventory script on the player
            Inventory playerInventory = other.GetComponent<Inventory>();

            if (playerInventory != null)
            {
                Collect(playerInventory);
            }
            else
            {
                Debug.LogWarning("Player detected, but no Inventory script found on the GameObject!");
            }
        }
    }

    private void Collect(Inventory inventory)
    {
        // 3. Add to inventory
        inventory.Add(ingredient, amount);

        Debug.Log($"Picked up {amount} {ingredient.ingredientName}");

        // 4. (Optional) Spawn a sound effect or particle here

        // 5. Remove object from scene
        Destroy(gameObject);
    }

    // This runs in the Editor whenever you change a value
    private void OnValidate()
    {
        if (autoUpdateSprite && ingredient != null)
        {
            GetComponent<SpriteRenderer>().sprite = ingredient.icon;
        }
    }
}