using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/NewRecipe")]
public class NewRecipeSO : ScriptableObject
{
    [SerializeField] string recipeName;
    [SerializeField] Sprite icon;
    [SerializeField] int price;

    [SerializeField] List<IngredientRequirement> ingredients = new(); // Use list instead of Array

    public string RecipeName { get => recipeName; }
    public Sprite Icon { get => icon; }
    public int Price { get => price; }

    [Serializable]
    public class IngredientRequirement
    {
        [SerializeField] Ingredient ingredient;
        [SerializeField] int amount;
    }
}