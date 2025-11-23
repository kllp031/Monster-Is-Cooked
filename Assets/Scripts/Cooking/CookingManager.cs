using TMPro;
using UnityEngine;
using TMPro; // if using TextMeshPro

public class CookingManager : MonoBehaviour
{
    public Inventory inventory;
    public Recipe currentRecipe;
    public CookingMinigame minigame;
    public GameObject cookingUI;

    public TextMeshProUGUI cookButtonText;
    void Start()
    {
        minigame.OnCookFinished += OnCookDone;
    }

    public bool BeginRecipe(Recipe recipe)
    {
        currentRecipe = recipe;

        if (!inventory.HasIngredients(recipe))
        {
            Debug.Log("Not enough ingredients.");
            return false;
        }

        inventory.SpendIngredients(recipe);

        // ACTIVATES UI + minigame script
        cookingUI.SetActive(true);

        minigame.StartCooking();
        return true;
    }

    void OnCookDone(CookingMinigame.CookResult r)
    {
        GameObject resultPrefab = r switch
        {
            CookingMinigame.CookResult.Delicious => currentRecipe.result_Delicious,
            CookingMinigame.CookResult.Normal => currentRecipe.result_Normal,
            _ => currentRecipe.result_Suspicious
        };

        Instantiate(resultPrefab);
        cookButtonText.text = "Start";

        // HIDE UI
        cookingUI.SetActive(false);

        Debug.Log("Cook result: " + r);
    }
    public void OnCookButtonPressed()
    {
        if (!minigame.IsCooking)
        {
            bool started = BeginRecipe(currentRecipe);
            if (started)
                cookButtonText.text = "Stop";
        }
        else
        {
            minigame.FinishCook();
            cookButtonText.text = "Start";

            // HIDE UI immediately
            //cookingUI.SetActive(false);
        }
    }
}
