using UnityEngine;
using System;
using Random = UnityEngine.Random; // Resolve conflict with System.Random

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 5;

    // Data
    private Recipe[] hotbarSlots;
    private int selectedSlotIndex = -1;

    // Events
    public event Action OnHotbarUpdated;
    public event Action<int> OnSelectionChanged;

    public Recipe CurrentRecipe
    {
        get
        {
            if (IsValidIndex(selectedSlotIndex))
                return hotbarSlots[selectedSlotIndex];
            return null;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        hotbarSlots = new Recipe[maxSlots];
    }

    // ---------------------------------------------------------
    //  Selection Logic
    // ---------------------------------------------------------

    public void SelectSlot(int index)
    {
        if (!IsValidIndex(index)) return;
        if (index == selectedSlotIndex) return;
        selectedSlotIndex = index;
        OnSelectionChanged?.Invoke(selectedSlotIndex);

        //Recipe r = hotbarSlots[index];
        //if (r != null)
        //{
        //    if (CookingManager.Instance != null)
        //        CookingManager.Instance.SelectRecipe(r);

        //    Debug.Log($"Selected Slot {index}: {r.name}");
        //}
        //else
        //{
        //    // If we select an empty slot, maybe we tell CookingManager to hold nothing?
        //    // CookingManager.Instance.SelectRecipe(null); 
        //    Debug.Log($"Selected Slot {index}: Empty");
        //}
    }

    // ---------------------------------------------------------
    //  Add / Remove Logic
    // ---------------------------------------------------------

    public bool AddRecipeToHotbar(Recipe recipe)
    {
        //for (int i = 0; i < maxSlots; i++)
        //    if (hotbarSlots[i] == recipe) return false;

        for (int i = 0; i < maxSlots; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = recipe;
                OnHotbarUpdated?.Invoke();

                if (selectedSlotIndex == -1) SelectSlot(i);

                return true;
            }
        }
        return false;
    }

    // Default value of -1 makes the parameter optional
    public void RemoveRecipe(int index = -1)
    {
        // If no index passed (stays -1) or invalid index passed, 
        // default to the currently selected slot.
        if (!IsValidIndex(index))
        {
            index = selectedSlotIndex;
        }

        // Proceed only if we ended up with a valid index
        if (IsValidIndex(index))
        {
            hotbarSlots[index] = null;
            OnHotbarUpdated?.Invoke();

            // If we removed the item we are currently holding, 
            // re-trigger selection to update the rest of the game (clearing the hand)
            if (index == selectedSlotIndex)
            {
                SelectSlot(index);
            }
        }

        for (int i = 0; i < maxSlots; i++)
        {
            Debug.Log("hotbar slot " + i + ": " + (hotbarSlots[i] != null ? hotbarSlots[i].name : "Empty"));
        }
    }

    public Recipe GetRecipeAt(int index)
    {
        if (IsValidIndex(index))
            return hotbarSlots[index];
        return null;
    }

    private bool IsValidIndex(int index) => index >= 0 && index < maxSlots;

    // =========================================================
    //  DEBUG / TESTING SECTION
    // =========================================================

    [Header("Debug Testing")]
    [Tooltip("Drag recipes here to use the Right-Click debug functions below")]
    [SerializeField] private Recipe[] testRecipes;

    // Right-click the Script Component title in Inspector -> Select "Debug Add Random"
    [ContextMenu("Debug Add Random")]
    public void DebugAddRandom()
    {
        if (testRecipes == null || testRecipes.Length == 0)
        {
            Debug.LogWarning("Please assign 'Test Recipes' in the Inspector first!");
            return;
        }

        Recipe randomRecipe = testRecipes[Random.Range(0, testRecipes.Length)];
        bool success = AddRecipeToHotbar(randomRecipe);

        Debug.Log(success
            ? $"Debug Added: {randomRecipe.name}"
            : "Debug Failed: Hotbar is full");
    }

    // Right-click -> "Debug Fill All"
    [ContextMenu("Debug Fill All")]
    public void DebugFillAll()
    {
        if (testRecipes == null || testRecipes.Length == 0) return;

        foreach (var r in testRecipes)
        {
            AddRecipeToHotbar(r);
        }
    }

    // Right-click -> "Debug Clear Hotbar"
    [ContextMenu("Debug Clear Hotbar")]
    public void DebugClear()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            RemoveRecipe(i);
        }
        Debug.Log("Debug: Hotbar Cleared");
    }
}