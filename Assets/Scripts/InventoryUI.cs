using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Dependencies")]
    public Inventory inventory;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;

    [Header("Container")]
    [Tooltip("The GameObject with the GridLayoutGroup component")]
    [SerializeField] private Transform itemsParent;

    private void Start()
    {
        // Subscribe to the event
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshInventoryUI;

        RefreshInventoryUI();
    }

    // Optional: Update UI whenever this object is turned on
    private void OnEnable()
    {
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        // 1. Clear existing items to avoid duplicates
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Get the raw data from your Inventory script
        Dictionary<Ingredient, int> allItems = inventory.GetAllItems();

        // 3. Loop through the dictionary
        foreach (KeyValuePair<Ingredient, int> entry in allItems)
        {
            // Only show items if amount is greater than 0
            if (entry.Value > 0)
            {
                CreateSlot(entry.Key, entry.Value);
            }
        }
    }

    private void CreateSlot(Ingredient ingredient, int amount)
    {
        GameObject newSlot = Instantiate(slotPrefab, itemsParent);
        InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();

        if (slotUI != null)
        {
            slotUI.SetItem(ingredient, amount);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent errors
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshInventoryUI;
    }
}