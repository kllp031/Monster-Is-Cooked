using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("Core Systems")]
    public InventorySO inventory;
    public CurvedCookingMinigame minigame;
    public Transform spawnFoodPosition;

    [Header("Runtime State")]
    public Recipe currentRecipe;

    [Header("Events")]
    public UnityEvent<Food> OnFoodSpawned; //Send signal with food data when cooking is finished


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
        if (minigame != null)
            minigame.OnCookFinished += HandleCookFinished;
    }

    private void OnDestroy()
    {
        if (minigame != null)
            minigame.OnCookFinished -= HandleCookFinished;
    }

    // ---------------------------------------------------------
    // Public Interactions
    // ---------------------------------------------------------

    public void SelectRecipe(Recipe recipe)
    {
        if (minigame.IsCooking)
        {
            print("Cannot change recipe while cooking!");
            return;
        }
        currentRecipe = recipe;
        // Auto-update UI when recipe is selected
        CookingUIManager.Instance.UpdateUI(currentRecipe);
    }

    /// <summary>
    /// Called via UI Button to toggle cooking state.
    /// </summary>
    public void OnCookButtonPressed()
    {
        // If we are NOT cooking, try to start
        if (!minigame.IsCooking)
        {
            if (HotbarManager.Instance == null || HotbarManager.Instance.IsHotbarFull())
            {
                print("Cannot start cooking: Hotbar is full or HotbarManager missing.");
                return;
            }

            if (AttemptStartRecipe(currentRecipe))
            {
                CookingUIManager.Instance.UpdateCookButtonState(true);
                CookingUIManager.Instance.UpdateUI(currentRecipe);
            }
        }
        // If we ARE cooking, finish the minigame
        else
        {
            minigame.FinishCook();
        }
    }

    // ---------------------------------------------------------
    // Internal Logic
    // ---------------------------------------------------------

    private bool AttemptStartRecipe(Recipe recipe)
    {
        if (recipe == null) return false;
        if (!inventory.HasIngredients(recipe)) return false;

        inventory.SpendIngredients(recipe);
        minigame.StartCooking();

        return true;
    }

    private void HandleCookFinished(bool result)
    {
        if (result)
        {
            SpawnFood(currentRecipe);
        }

        CookingUIManager.Instance.UpdateCookButtonState(false);
    }

    private void SpawnFood(Recipe recipe)
    {
        Transform foodTransform = Instantiate(GameAssets.Instance.pfFood);

        foodTransform.position = spawnFoodPosition.position;

        if (foodTransform.TryGetComponent(out Food foodComponent))
        {
            foodComponent.SetUp(recipe);
            OnFoodSpawned?.Invoke(foodComponent);
        }
    }

    private void OnDisable()
    {
        currentRecipe = null;
    }
}