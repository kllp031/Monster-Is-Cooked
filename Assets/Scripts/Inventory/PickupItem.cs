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
    [SerializeField] private GameObject itemEffect;

    private SpriteRenderer spriteRenderer;
    private ItemsSpawner itemSpawner;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
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
            // 2. Look for the new "PlayerInventory" bridge script
            PlayerInventory playerBridge = other.GetComponent<PlayerInventory>();

            if (playerBridge != null && playerBridge.inventoryData != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.playerCollect);

                if(itemSpawner != null)
                    itemSpawner.OnItemCollected();

                // 3. Pass the ScriptableObject data to the collect function
                Collect(playerBridge.inventoryData);
            }
            else
            {
                Debug.LogWarning("Player detected, but PlayerInventory script (or the Asset reference) is missing!");
            }
        }
    }

    private void Collect(InventorySO inventory)
    {
        // 4. Add to the ScriptableObject directly
        inventory.Add(ingredient, amount);

        Debug.Log($"Picked up {amount} {ingredient.ingredientName}");
        if(itemEffect != null)  Instantiate(itemEffect, transform.position, Quaternion.identity);
        // 5. (Optional) Play sound/particles here

        // 6. Remove object from scene
        Destroy(gameObject);
    }

    // This runs in the Editor to update the sprite immediately when you drag an ingredient in
    private void OnValidate()
    {
        if (autoUpdateSprite && ingredient != null)
        {
            // Simple check to avoid errors if SpriteRenderer isn't attached yet in Editor
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().sprite = ingredient.icon;
        }
    }
    public void SetupItemSpawner(ItemsSpawner spawner)
    {
        if(spawner != null)
            itemSpawner = spawner;
    }
}