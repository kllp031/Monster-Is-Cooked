using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] string recipeName;
    [SerializeField] Sprite icon;

    // Use the new global class here
    [SerializeField] List<IngredientAmount> ingredients = new();

    public string RecipeName => recipeName;
    public Sprite Icon => icon;
    public List<IngredientAmount> Ingredients => ingredients;
}