using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    [Header("Data & Config")]
    public Recipe[] allRecipes;
    public Inventory inventory;

    [Header("External Dependencies")]
    public CookingManager cookingManager;

    [Header("UI References")]
    public Image recipeImage;
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private Transform ingredientSlotContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private GameObject ingredientSlotPrefab;

    // ---------------------------------------------------------
    // Initialization
    // ---------------------------------------------------------

    void Start()
    {
        if (cookingManager == null)
        {
            Debug.LogError("CookingManager reference is missing in CookingUIManager.");
        }

        GenerateRecipeList();
    }

    private void GenerateRecipeList()
    {
        foreach (var recipe in allRecipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContent, false);
            RecipeButtonUI buttonUI = buttonObj.GetComponent<RecipeButtonUI>();

            buttonUI.Setup(recipe, cookingManager, this);
        }
    }

    // ---------------------------------------------------------
    // UI Updates
    // ---------------------------------------------------------

    public void UpdateUI(Recipe recipe)
    {
        UpdateRecipeSlot(recipe);
        UpdateIngredientSlots(recipe);
    }

    void UpdateRecipeSlot(Recipe recipe)
    {
        recipeImage.sprite = recipe.icon;
        recipeImage.enabled = true;
    }

    void UpdateIngredientSlots(Recipe recipe)
    {
        // Clear existing slots
        foreach (Transform child in ingredientSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new slots based on recipe requirements
        for (int i = 0; i < recipe.ingredientsRequired.Length; i++)
        {
            Ingredient ingredient = recipe.ingredientsRequired[i];
            int requiredAmount = recipe.ingredientAmounts[i];
            int ownedAmount = inventory.GetAmount(ingredient);

            GameObject slotObj = Instantiate(ingredientSlotPrefab, ingredientSlotContainer);
            IngredientSlotUI slotUI = slotObj.GetComponent<IngredientSlotUI>();

            slotUI.Setup(ingredient, ownedAmount, requiredAmount);
        }
    }

    void OnDisable()
    {
        // Clear recipe image when UI is disabled
        recipeImage.enabled = false;
        ClearIngredientSlot();
    }

    void ClearIngredientSlot()
    {
        foreach (Transform child in ingredientSlotContainer)
        {
            Destroy(child.gameObject);
        }
    }
}