using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private Recipe recipe;

    public void Setup(Recipe recipeData)
    {
        recipe = recipeData;
        iconImage.sprite = recipe.Icon;
    }

    public void OnRecipeButtonClicked()
    {
        // Direct Singleton access
        CookingManager.Instance.SelectRecipe(recipe);
    }

    public void OnHotBarSlotClicked()
    {
        return;
    }
}