using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [Header("Add items here in the Inspector")]
    // Use the same global class here!
    public List<IngredientAmount> startingItems = new List<IngredientAmount>();

    private Dictionary<Ingredient, int> items = new Dictionary<Ingredient, int>();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        foreach (var entry in startingItems)
        {
            // Note the Capital I and A because we are using properties now
            if (entry.Ingredient == null) continue;

            if (!items.ContainsKey(entry.Ingredient))
                items[entry.Ingredient] = 0;

            items[entry.Ingredient] += entry.Amount;
        }
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

    public Dictionary<Ingredient, int> GetAllItems() => items;
}