using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Reference")]
    public InventorySO inventoryData;

    private void Awake()
    {
        // Vital: Reset data so we don't keep items from previous play sessions
        inventoryData.Initialize();
    }

}