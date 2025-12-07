using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [Header("Data & Config")]
    public Recipe[] allRecipes;

    [Header("UI References")]
    public Image recipeImage;
    [SerializeField] private TextMeshProUGUI cookButtonText;
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private Transform ingredientSlotContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private GameObject ingredientSlotPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GenerateRecipeList();
        UpdateCookButtonState(false);
    }

    private void GenerateRecipeList()
    {
        foreach (var recipe in allRecipes)
        {
            GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContent, false);
            // No longer need to pass Managers to the button
            buttonObj.GetComponent<RecipeButtonUI>().Setup(recipe);
        }
    }

    public void UpdateUI(Recipe recipe)
    {
        UpdateRecipeSlot(recipe);
        UpdateIngredientSlots(recipe);
    }

    /// <summary>
    /// Updates the text of the main action button based on the cooking state.
    /// </summary>
    public void UpdateCookButtonState(bool isCooking)
    {
        if (cookButtonText != null)
        {
            cookButtonText.text = isCooking ? "Stop" : "Start";
        }
    }

    void UpdateRecipeSlot(Recipe recipe)
    {
        recipeImage.sprite = recipe.icon;
        recipeImage.enabled = true;
    }

    void UpdateIngredientSlots(Recipe recipe)
    {
        foreach (Transform child in ingredientSlotContainer) Destroy(child.gameObject);

        // Accessing Inventory via the CookingManager Singleton ensures we share state
        Inventory inv = CookingManager.Instance.inventory;

        for (int i = 0; i < recipe.ingredientsRequired.Length; i++)
        {
            Ingredient ingredient = recipe.ingredientsRequired[i];
            int requiredAmount = recipe.ingredientAmounts[i];
            int ownedAmount = inv.GetAmount(ingredient);

            GameObject slotObj = Instantiate(ingredientSlotPrefab, ingredientSlotContainer);
            slotObj.GetComponent<IngredientSlotUI>().Setup(ingredient, ownedAmount, requiredAmount);
        }
    }

    void OnDisable()
    {
        recipeImage.enabled = false;
        foreach (Transform child in ingredientSlotContainer) Destroy(child.gameObject);
    }
}