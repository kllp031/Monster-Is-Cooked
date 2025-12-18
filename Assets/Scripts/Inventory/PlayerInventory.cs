using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Reference")]
    public InventorySO inventoryData;

    [SerializeField] private RectTransform inventoryUITransform;

    private void Awake()
    {
        // Vital: Reset data so we don't keep items from previous play sessions
        inventoryData.Initialize();
    }

    /// <summary>
    /// Test method to clear inventory from the Inspector.
    /// </summary>
    [ContextMenu("Clear Inventory")] // <--- Add this line
    public void TestClearInventory()
    {
        inventoryData.ClearInventory();
        Debug.Log("Inventory Cleared via Inspector!"); // Optional: visual confirmation
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
            if (inventoryUITransform != null)
            {
                print("Toggling Inventory UI");
                bool isActive = inventoryUITransform.gameObject.activeSelf;
                inventoryUITransform.gameObject.SetActive(!isActive);
            }
    }
}