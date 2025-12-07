using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Player Inventory", menuName = "Systems/Inventory")]
public class InventorySO : ScriptableObject
{
    [Header("Config")]
    [Tooltip("Items the player starts with every time the game begins.")]
    public List<IngredientAmount> startingItems = new List<IngredientAmount>();

    // The runtime storage (Dictionary is faster than List)
    private Dictionary<Ingredient, int> items = new Dictionary<Ingredient, int>();

    // Events to update UI
    public event Action OnInventoryChanged;

    /// <summary>
    /// Call this when the game starts (e.g., from Player or GameManager) 
    /// to reset the inventory to its default state.
    /// </summary>
    public void Initialize()
    {
        items.Clear();

        foreach (var entry in startingItems)
        {
            if (entry.Ingredient != null)
            {
                Add(entry.Ingredient, entry.Amount);
            }
        }

        // Notify UI that we reset
        OnInventoryChanged?.Invoke();
    }

    public void Add(Ingredient ingredient, int amount)
    {
        if (!items.ContainsKey(ingredient))
            items[ingredient] = 0;

        items[ingredient] += amount;
        OnInventoryChanged?.Invoke();
    }

    public bool HasIngredients(Recipe recipe)
    {
        foreach (var req in recipe.Ingredients)
        {
            Ingredient ing = req.Ingredient;
            int needed = req.Amount;

            if (!items.ContainsKey(ing) || items[ing] < needed)
                return false;
        }
        return true;
    }

    public void SpendIngredients(Recipe recipe)
    {
        foreach (var req in recipe.Ingredients)
        {
            if (items.ContainsKey(req.Ingredient))
            {
                items[req.Ingredient] -= req.Amount;
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public int GetAmount(Ingredient ing)
    {
        return items.ContainsKey(ing) ? items[ing] : 0;
    }

    /// <summary>
    /// Returns a copy or reference to the current inventory for UI display.
    /// </summary>
    public Dictionary<Ingredient, int> GetAllItems()
    {
        return items;
    }
}