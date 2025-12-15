using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
//using NUnit.Framework; // Resolve conflict with System.Random

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 5;

    // Data
    //private Recipe[] hotbarSlots;
    private List<Food> hotbarSlots;

    private int selectedSlotIndex = 0;

    // Events
    public event Action OnHotbarUpdated;
    public event Action<int> OnSelectionChanged;

    //public Recipe CurrentRecipe
    //{
    //    get
    //    {
    //        if (IsValidIndex(selectedSlotIndex))
    //            return hotbarSlots[selectedSlotIndex];
    //        return null;
    //    }
    //}
    public Food CurrentFood
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
        //hotbarSlots = new Recipe[maxSlots];
        hotbarSlots = (new Food[maxSlots]).ToList(); //List<Food>(maxSlots);
        selectedSlotIndex = 0; // There will always be a selected slot
    }

    private void Start()
    {
        SelectSlot(selectedSlotIndex);
        if (GameManager.Instance == null)
        {
            Debug.LogError("HotbarManager: GameManager instance not found!");
            return;
        }
        GameManager.Instance.OnLevelEnd.AddListener(DestroyAllFood);

        if (CookingManager.Instance == null)
        {
            Debug.LogError("HotbarManager: CookingManager instance not found!");
            return;
        }
        CookingManager.Instance.OnFoodSpawned.AddListener(OnFoodSpawned);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLevelEnd.RemoveListener(DestroyAllFood);
        CookingManager.Instance.OnFoodSpawned.RemoveListener(OnFoodSpawned);
    }

    //private void sta()
    //{
    //    GameManager.Instance.OnLevelEnd.AddListener(DestroyAllFood);
    //}

    // ---------------------------------------------------------
    //  Selection Logic
    // ---------------------------------------------------------

    private void OnFoodSpawned(Food food)
    {
        AddFoodToHotBar(food);
    }

    public void SelectSlot(int index)
    {
        if (!IsValidIndex(index)) return;
        //if (index == selectedSlotIndex) return;
        // Hide the previously selected slot (if valid)
        if (IsValidIndex(selectedSlotIndex) && hotbarSlots[selectedSlotIndex] != null)
        {
            hotbarSlots[selectedSlotIndex].gameObject.SetActive(false);
        }
        // Activate the selected slot
        selectedSlotIndex = index;
        if (hotbarSlots[selectedSlotIndex] != null)
            hotbarSlots[selectedSlotIndex].gameObject.SetActive(true);
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

    //public bool AddRecipeToHotbar(Recipe recipe)
    //{
    //    //for (int i = 0; i < maxSlots; i++)
    //    //    if (hotbarSlots[i] == recipe) return false;

    //    for (int i = 0; i < maxSlots; i++)
    //    {
    //        if (hotbarSlots[i] == null)
    //        {
    //            hotbarSlots[i] = recipe;
    //            OnHotbarUpdated?.Invoke();

    //            if (selectedSlotIndex == -1) SelectSlot(i);

    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public bool AddFoodToHotBar(Food food)
    {
        //for (int i = 0; i < maxSlots; i++)
        //    if (hotbarSlots[i] == recipe) return false;
        if (food == null) return false;

        for (int i = 0; i < maxSlots; i++)
        {
            if (hotbarSlots[i] == null)
            {
                hotbarSlots[i] = food;
                food.IsPickedUp();
                food.gameObject.SetActive(false); // Every food getting added to hotbar will be deactivated at first
                OnHotbarUpdated?.Invoke();
                SelectSlot(selectedSlotIndex); //  Activate the added food if it is at selected index
                //if (selectedSlotIndex == -1) SelectSlot(i);

                return true;
            }
        }
        return false;
    }

    // Default value of -1 makes the parameter optional
    //public void RemoveRecipe(int index = -1)
    //{
    //    // If no index passed (stays -1) or invalid index passed, 
    //    // default to the currently selected slot.
    //    if (!IsValidIndex(index))
    //    {
    //        index = selectedSlotIndex;
    //    }

    //    // Proceed only if we ended up with a valid index
    //    if (IsValidIndex(index))
    //    {
    //        hotbarSlots[index] = null;
    //        OnHotbarUpdated?.Invoke();

    //        // If we removed the item we are currently holding, 
    //        // re-trigger selection to update the rest of the game (clearing the hand)
    //        if (index == selectedSlotIndex)
    //        {
    //            SelectSlot(index);
    //        }
    //    }

    //    for (int i = 0; i < maxSlots; i++)
    //    {
    //        Debug.Log("hotbar slot " + i + ": " + (hotbarSlots[i] != null ? hotbarSlots[i].name : "Empty"));
    //    }
    //}

    public void RemoveFood(int index)
    {
        if (!IsValidIndex(index))
        {
            //index = selectedSlotIndex;
            Debug.LogWarning("Invalid food index to remove!");
            return;
        }

        // Drop the food -> Set the specified slot to empty -> Update hot bar
        if (hotbarSlots[index] == null)
        {
            Debug.LogWarning("No food to remove at index: " + index);
            return;
        }
        
        hotbarSlots[index].IsDropped();
        hotbarSlots[index] = null;
        OnHotbarUpdated?.Invoke();

        // If we removed the item we are currently holding
        if (index == selectedSlotIndex)
        {
            SelectSlot(index);
        }

        // Used for debugging
        for (int i = 0; i < maxSlots; i++)
        {
            Debug.Log("hotbar slot " + i + ": " + (hotbarSlots[i] != null ? hotbarSlots[i].name : "Empty"));
        }
    }

    private void DestroyAllFood(bool bruh)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (hotbarSlots[i] != null)
            {
                Destroy(hotbarSlots[i].gameObject);
                RemoveFood(i);
            }
        }
    }

    public void RemoveSelectedFood()
    {
        RemoveFood(selectedSlotIndex);
    }

    //public Recipe GetRecipeAt(int index)
    //{
    //    if (IsValidIndex(index))
    //        return hotbarSlots[index];
    //    return null;
    //}

    public Food GetFoodAt(int index)
    {
        if (IsValidIndex(index))
            return hotbarSlots[index];
        return null;
    }

    private bool IsValidIndex(int index) => index >= 0 && index < maxSlots;

    public bool IsHotbarFull()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (hotbarSlots[i] == null)
                return false;
        }
        return true;
    }

    // =========================================================
    //  DEBUG / TESTING SECTION
    // =========================================================

    [Header("Debug Testing")]
    [Tooltip("Drag recipes here to use the Right-Click debug functions below")]
    //[SerializeField] private Recipe[] testRecipes;
    private List<Food> testFoods;

    // Right-click the Script Component title in Inspector -> Select "Debug Add Random"
    [ContextMenu("Debug Add Random")]
    public void DebugAddRandom()
    {
        //if (testRecipes == null || testRecipes.Length == 0)
        //{
        //    Debug.LogWarning("Please assign 'Test Recipes' in the Inspector first!");
        //    return;
        //}
        if (testFoods == null || testFoods.Count == 0)
        {
            Debug.LogWarning("Please assign 'Test Foods' in the Inspector first!");
            return;
        }

        //Recipe randomRecipe = testRecipes[Random.Range(0, testRecipes.Length)];
        Food randomFood = testFoods[Random.Range(0, testFoods.Count)];
        //bool success = AddRecipeToHotbar(randomRecipe);
        bool success = AddFoodToHotBar(randomFood);

        Debug.Log(success
            ? $"Debug Added: {randomFood.name}"
            : "Debug Failed: Hotbar is full");
    }

    // Right-click -> "Debug Fill All"
    [ContextMenu("Debug Fill All")]
    public void DebugFillAll()
    {
        //if (testRecipes == null || testRecipes.Length == 0) return;

        //foreach (var r in testRecipes)
        //{
        //    AddRecipeToHotbar(r);
        //}
        if (testFoods == null || testFoods.Count == 0) return;

        foreach (var r in testFoods)
        {
            AddFoodToHotBar(r);
        }
    }

    // Right-click -> "Debug Clear Hotbar"
    [ContextMenu("Debug Clear Hotbar")]
    public void DebugClear()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            //RemoveRecipe(i);
            RemoveFood(i);
        }
        Debug.Log("Debug: Hotbar Cleared");
    }
}