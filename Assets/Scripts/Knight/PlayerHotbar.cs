using UnityEngine;
using UnityEngine.InputSystem;

// This code is no longer in use
public class PlayerHotbar : MonoBehaviour
{
    //[Header("Dependencies")]
    //[SerializeField] private FoodHolder foodHolder;

    //private void Start()
    //{
    //    // 1. Listen to the Hotbar
    //    if (HotbarManager.Instance != null)
    //    {
    //        HotbarManager.Instance.OnSelectionChanged += OnHotbarSlotSelected;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("HotbarManager missing from scene!");
    //    }

    //    // Optional: Auto-find FoodHolder if forgot to assign
    //    if (foodHolder == null) foodHolder = GetComponent<FoodHolder>();
    //}

    //private void OnDestroy()
    //{
    //    // 2. Clean up listener
    //    if (HotbarManager.Instance != null)
    //    {
    //        HotbarManager.Instance.OnSelectionChanged -= OnHotbarSlotSelected;
    //    }
    //}

    //private void OnHotbarSlotSelected(int index)
    //{
    //    // A. Get the recipe from the manager
    //    Recipe newRecipe = HotbarManager.Instance.GetRecipeAt(index);

    //    // If the slot is empty, we generally do nothing 
    //    // (Or you could force the player to drop their current item if you wanted)
    //    if (newRecipe == null) return;

    //    // B. Spawn the physical object into the world
    //    // Note: We instantiate it at the player's position so it doesn't fly in from zero
    //    if (GameAssets.Instance == null || GameAssets.Instance.pfFood == null)
    //    {
    //        Debug.LogError("GameAssets or pfFood is not assigned!");
    //        return;
    //    }
    //    Transform newFoodObj = Instantiate(GameAssets.Instance.pfFood, transform.position, Quaternion.identity);

    //    // C. Configure the food data
    //    if (newFoodObj.TryGetComponent(out Food foodComponent))
    //    {
    //        foodComponent.SetUp(newRecipe);
    //        foodHolder.ClearHeldFood();
    //        // D. Tell the FoodHolder to grab it
    //        // This calls the existing logic on your teammate's script
    //        foodHolder.PickUpFood(foodComponent);
    //    }
    //    else
    //    {
    //        Debug.LogError("The Food Prefab assigned to PlayerHotbar is missing the 'Food' script!");
    //        Destroy(newFoodObj);
    //    }
    //}

    //public void OnDropFood(InputAction.CallbackContext context)
    //{
    //    if (context.started)
    //    {
    //        HotbarManager.Instance.RemoveRecipe();
    //    }
    //}
}