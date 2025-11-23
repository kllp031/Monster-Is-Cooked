using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryEntry
{
    public Ingredient ingredient;
    public int amount;
}

public class Inventory : MonoBehaviour
{
    [Header("Add items here in the Inspector")]
    public List<InventoryEntry> startingItems = new List<InventoryEntry>();

    private Dictionary<Ingredient, int> items = new Dictionary<Ingredient, int>();

    private void Awake()
    {
        // Convert startingItems list to the dictionary
        foreach (var entry in startingItems)
        {
            if (entry.ingredient == null) continue;

            if (!items.ContainsKey(entry.ingredient))
                items[entry.ingredient] = 0;

            items[entry.ingredient] += entry.amount;
        }
    }

    public void Add(Ingredient ingredient, int amount)
    {
        if (!items.ContainsKey(ingredient))
            items[ingredient] = 0;

        items[ingredient] += amount;
    }

    public bool HasIngredients(Recipe recipe)
    {
        for (int i = 0; i < recipe.ingredientsRequired.Length; i++)
        {
            Ingredient ing = recipe.ingredientsRequired[i];
            int needed = recipe.ingredientAmounts[i];

            if (!items.ContainsKey(ing) || items[ing] < needed)
                return false;
        }
        return true;
    }

    public void SpendIngredients(Recipe recipe)
    {
        for (int i = 0; i < recipe.ingredientsRequired.Length; i++)
        {
            items[recipe.ingredientsRequired[i]] -= recipe.ingredientAmounts[i];
        }
    }
}

