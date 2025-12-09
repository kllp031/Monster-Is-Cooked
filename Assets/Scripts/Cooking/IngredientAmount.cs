using UnityEngine;
using System;

[Serializable]
public class IngredientAmount
{
    [SerializeField] private Ingredient ingredient;
    [SerializeField] private int amount;

    // Public Properties for easy access
    public Ingredient Ingredient => ingredient;
    public int Amount => amount;

    // Constructor (optional, but useful for code-based additions)
    public IngredientAmount(Ingredient ing, int amt)
    {
        ingredient = ing;
        amount = amt;
    }
}