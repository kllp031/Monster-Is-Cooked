using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    [Header("Core Systems")]
    public Inventory inventory;
    public CookingMinigame minigame;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cookButtonText;

    [Header("Spawning Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Runtime State")]
    public Recipe currentRecipe;

    // ---------------------------------------------------------
    // Initialization
    // ---------------------------------------------------------

    void Start()
    {
        if (minigame != null)
        {
            minigame.OnCookFinished += HandleCookFinished;
        }
    }

    private void OnDestroy()
    {
        // Good practice to unsubscribe from events to prevent memory leaks
        if (minigame != null)
        {
            minigame.OnCookFinished -= HandleCookFinished;
        }
    }

    // ---------------------------------------------------------
    // Public Interactions
    // ---------------------------------------------------------

    /// <summary>
    /// Sets the current recipe to be cooked.
    /// </summary>
    public void SelectRecipe(Recipe recipe)
    {
        currentRecipe = recipe;
    }

    /// <summary>
    /// Called via UI Button to toggle cooking state.
    /// </summary>
    public void OnCookButtonPressed(CookingUIManager uiManager)
    {
        // If we are NOT cooking, try to start
        if (!minigame.IsCooking)
        {
            bool hasStarted = AttemptStartRecipe(currentRecipe);

            if (hasStarted)
            {
                cookButtonText.text = "Stop";

                // Refresh UI to show updated ingredient counts immediately
                uiManager.UpdateUI(currentRecipe);
            }
        }
        // If we ARE cooking, finish the minigame
        else
        {
            minigame.FinishCook();
            cookButtonText.text = "Start";
        }
    }

    // ---------------------------------------------------------
    // Internal Logic
    // ---------------------------------------------------------

    /// <summary>
    /// Validates ingredients and starts the minigame.
    /// </summary>
    private bool AttemptStartRecipe(Recipe recipe)
    {
        currentRecipe = recipe;

        if (!inventory.HasIngredients(recipe))
        {
            Debug.LogWarning("Cannot start recipe: Not enough ingredients.");
            return false;
        }

        inventory.SpendIngredients(recipe);
        minigame.StartCooking();

        Debug.Log($"Started cooking: {recipe.name}");
        return true;
    }

    private void HandleCookFinished(CookingMinigame.CookResult result)
    {
        Debug.Log($"Cook result: {result}");

        // Only create the food object if the result is Normal (Success)
        if (result == CookingMinigame.CookResult.Normal)
        {
            SpawnFood(currentRecipe);
        }

        // Reset UI text
        cookButtonText.text = "Start";
    }

    private void SpawnFood(Recipe recipe)
    {
        if (foodPrefab == null)
        {
            Debug.LogError("Food Prefab is not assigned in CookingManager!");
            return;
        }

        GameObject foodObj = Instantiate(foodPrefab);
        Food foodComponent = foodObj.GetComponent<Food>();

        if (foodComponent != null)
        {
            foodComponent.SetUp(recipe);
        }
        else
        {
            Debug.LogError("Food component missing on the instantiated prefab!");
        }
    }
}