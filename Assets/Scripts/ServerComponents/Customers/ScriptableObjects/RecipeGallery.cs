using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/RecipeGallery")]
public class RecipeGallery : ScriptableObject
{
    [SerializeField] List<RecipeDetails> recipes = new();

    public Recipe GetRecipe(string recipeId)
    {
        foreach (var recipe in recipes)
            if (recipe.RecipeId == recipeId) return recipe.Recipe;
        return null;
    }
}


[Serializable]
public struct RecipeDetails
{
    [SerializeField] public string RecipeId;
    [SerializeField] public Recipe Recipe;
}
