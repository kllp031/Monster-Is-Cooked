using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] string recipeName;
    [SerializeField] Sprite icon;
    [SerializeField] int price = 0;

    // Use the new global class here
    [SerializeField] List<IngredientAmount> ingredients = new();

    public string RecipeName { get => recipeName; }
    public Sprite Icon { get => icon; }
    public List<IngredientAmount> Ingredients { get => ingredients; }
    public int Price { get => price; }
}