using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    CookingManager manager;
    CookingUIManager uIManager;

    private Recipe recipe;
    public void Setup(Recipe recipeData, CookingManager manager, CookingUIManager uIManager)
    {
        recipe = recipeData;
        iconImage.sprite = recipe.icon;
        this.manager = manager;
        this.uIManager = uIManager;
    }

    public void OnRecipeButtonClicked()
    {
        manager.SelectRecipe(recipe);
        uIManager.UpdateUI(recipe);
    }
}
