using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    public Inventory inventory;
    public Recipe currentRecipe;
    public CookingMinigame minigame;

    public TextMeshProUGUI cookButtonText;

    [SerializeField] private GameObject foodTemplate;
    void Start()
    {
        minigame.OnCookFinished += OnCookDone;
    }

    /// <summary>
    /// CookingManager set up ui for selected recipe
    /// </summary>

    public void SelectRecipe(Recipe recipe)
    {
        currentRecipe = recipe;
    }

    /// <summary>
    /// /// Called when cook button is pressed
    /// </summary>

    public void OnCookButtonPressed(CookingUIManager UIManager)
    {
        if (!minigame.IsCooking)
        {
            bool started = BeginRecipe(currentRecipe);
            if (started)
                cookButtonText.text = "Stop";
            //temp fix
            UIManager.UpdateUI(currentRecipe);
            //UIManager.SetActiveCookingMinigame(true);
        }
        else
        {
            minigame.FinishCook();
            cookButtonText.text = "Start";
        }
    }

    /// <summary>
    /// /// Starts the cooking process for the given recipe
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>

    public bool BeginRecipe(Recipe recipe)
    {
        currentRecipe = recipe;

        if (!inventory.HasIngredients(recipe))
        {
            Debug.Log("Not enough ingredients.");
            return false;
        }

        inventory.SpendIngredients(recipe);

        minigame.StartCooking();
        print("Started cooking " + recipe.name);
        return true;
    }

    void OnCookDone(CookingMinigame.CookResult r)
    {
        // Only create the food object if the result is Normal
        if (r == CookingMinigame.CookResult.Normal)
        {
            GameObject foodObj = Instantiate(foodTemplate);

            Food food = foodObj.GetComponent<Food>();
            if (food == null)
            {
                Debug.LogError("Food component missing on foodTemplate prefab!");
                return;
            }

            food.SetUp(currentRecipe);
        }

        cookButtonText.text = "Start";
        Debug.Log("Cook result: " + r);
    }
}
