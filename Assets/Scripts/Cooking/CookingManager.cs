using TMPro;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [Header("Core Systems")]
    public Inventory inventory;
    public CurvedCookingMinigame minigame;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cookButtonText;

    [Header("Spawning Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Runtime State")]
    public Recipe currentRecipe;

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
        if (foodPrefab == null) return;

        GameObject foodObj = Instantiate(foodPrefab);
        if (foodObj.TryGetComponent(out Food foodComponent))
        {
            foodComponent.SetUp(recipe);
        }
    }

    private void OnDisable()
    {
        currentRecipe = null;
    }
}