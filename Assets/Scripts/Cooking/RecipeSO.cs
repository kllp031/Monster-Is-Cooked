using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Sprite icon;

    public Ingredient[] ingredientsRequired;
    public int[] ingredientAmounts;  // Match indices
}
