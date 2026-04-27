using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [Header("Data & Config")]
    public Recipe[] allRecipes;
    [SerializeField] InventorySO inventory;

    [Header("UI References")]
    public Image recipeImage;
    [SerializeField] private TextMeshProUGUI cookButtonText;
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private Transform ingredientSlotContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private GameObject ingredientSlotPrefab;

    [Header("Food flyout (on successful cook)")]
    [Tooltip("Parent for the flying icon (usually a full-screen rect under the HUD canvas). Falls back to recipe canvas root.")]
    [SerializeField] private RectTransform foodFlyoutContainer;
    [Tooltip("Where the icon lands — e.g. anchor preset Bottom Center. If unset, uses bottom-center of the container with Flyout bottom offset.")]
    [SerializeField] private RectTransform foodFlyoutDestination;
    [SerializeField] private float foodFlyoutDuration = 0.45f;
    [SerializeField] private Vector2 flyoutImageSize = new Vector2(88f, 88f);
    [SerializeField] private float flyoutBottomOffset = 112f;

    private Coroutine _foodFlyoutRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDisable()
    {
        if (_foodFlyoutRoutine != null)
        {
            StopCoroutine(_foodFlyoutRoutine);
            _foodFlyoutRoutine = null;
        }
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
        recipeImage.sprite = recipe.Icon;
        recipeImage.enabled = true;
    }

    void UpdateIngredientSlots(Recipe recipe)
    {
        foreach (Transform child in ingredientSlotContainer) Destroy(child.gameObject);

        // Accessing Inventory via the CookingManager Singleton ensures we share state
        //Inventory inv = CookingManager.Instance.inventory;

        // Use the Recipe.IngredientRequirement list instead of ingredientsRequired/ingredientAmounts
        var requirements = recipe.Ingredients;
        for (int i = 0; i < requirements.Count; i++)
        {
            Ingredient ingredient = requirements[i].Ingredient;
            int requiredAmount = requirements[i].Amount;
            int ownedAmount = inventory.GetAmount(ingredient);

            GameObject slotObj = Instantiate(ingredientSlotPrefab, ingredientSlotContainer);
            slotObj.GetComponent<IngredientSlotUI>().Setup(ingredient, ownedAmount, requiredAmount);
        }
    }

    /// <summary>
    /// Animates the recipe icon from the recipe slot toward the bottom-center of the screen.
    /// </summary>
    public void PlayFoodFlyout(Recipe recipe)
    {
        if (recipe == null || recipe.Icon == null || recipeImage == null)
            return;

        RectTransform parent = foodFlyoutContainer;
        if (parent == null)
        {
            Canvas canvas = recipeImage.canvas;
            if (canvas == null) return;
            parent = canvas.transform as RectTransform;
        }

        if (_foodFlyoutRoutine != null)
            StopCoroutine(_foodFlyoutRoutine);
        _foodFlyoutRoutine = StartCoroutine(FoodFlyoutRoutine(recipe.Icon, parent));
    }

    private IEnumerator FoodFlyoutRoutine(Sprite sprite, RectTransform parent)
    {
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        if (canvas == null)
            yield break;

        Camera eventCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        GameObject go = new GameObject("FoodFlyoutIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        Image img = go.GetComponent<Image>();
        rt.SetParent(parent, false);
        rt.sizeDelta = flyoutImageSize;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        if (!TryWorldToLocalInParent(parent, recipeImage.rectTransform, canvas, eventCam, out Vector2 startLocal))
            startLocal = Vector2.zero;

        Vector2 endLocal;
        if (foodFlyoutDestination == null ||
            !TryWorldToLocalInParent(parent, foodFlyoutDestination, canvas, eventCam, out endLocal))
        {
            Rect pr = parent.rect;
            endLocal = new Vector2(0f, pr.yMin + flyoutBottomOffset);
        }

        rt.anchoredPosition = startLocal;
        yield return StartCoroutine(AnimateAnchoredPosition(rt, startLocal, endLocal, foodFlyoutDuration));

        Destroy(go);
        _foodFlyoutRoutine = null;
    }

    private static bool TryWorldToLocalInParent(RectTransform parent, RectTransform uiElement, Canvas canvas, Camera eventCam, out Vector2 localPoint)
    {
        Vector3 world = uiElement.TransformPoint(uiElement.rect.center);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(eventCam, world);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, eventCam, out localPoint);
    }

    private static IEnumerator AnimateAnchoredPosition(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = u * u * (3f - 2f * u);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            yield return null;
        }

        rt.anchoredPosition = to;
    }

    //void OnDisable()
    //{
    //    if (recipeImage != null) recipeImage.enabled = false;
    //    foreach (Transform child in ingredientSlotContainer) Destroy(child.gameObject);
    //}
}