using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Drag the PlayerInventory ScriptableObject here")]
    [SerializeField] InventorySO inventory;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;

    [Header("Container")]
    [SerializeField] private Transform itemsParent;

    // ---------------------------------------------------------
    // Event Subscriptions
    // ---------------------------------------------------------

    private void OnEnable()
    {
        if (inventory != null)
        {
            // Subscribe to changes
            inventory.OnInventoryChanged += RefreshInventoryUI;

            // Force an update immediately so the UI is correct the moment it opens
            RefreshInventoryUI();
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            // Always unsubscribe when the UI closes to prevent memory leaks
            inventory.OnInventoryChanged -= RefreshInventoryUI;
        }
    }

    // ---------------------------------------------------------
    // UI Logic
    // ---------------------------------------------------------

    public void RefreshInventoryUI()
    {
        // 1. Clear existing slots (Destroying children is simple and effective)
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Get the data from the Scriptable Object
        Dictionary<Ingredient, int> allItems = inventory.GetAllItems();

        // 3. Create new slots
        foreach (KeyValuePair<Ingredient, int> entry in allItems)
        {
            // Only show if we actually have the item
            if (entry.Value > 0)
            {
                CreateSlot(entry.Key, entry.Value);
            }
        }
    }

    private void CreateSlot(Ingredient ingredient, int amount)
    {
        GameObject newSlot = Instantiate(slotPrefab, itemsParent);

        // Ensure your slot prefab has this script attached!
        InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();

        if (slotUI != null)
        {
            slotUI.SetItem(ingredient, amount);
        }
    }
}