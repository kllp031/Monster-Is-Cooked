using System.Collections; // Required for Coroutines
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private RectTransform hotbarPanel;
    [SerializeField] private RectTransform hotbarAnchor; //Object that defines where the hotbar should be anchored
    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f; // How fast it moves

    private HotbarSlotUI[] uiSlots;

    private Vector2 onscreenPosition;
    private Vector2 offscreenPosition;
    private Coroutine currentAnimation;

    // Track locally so we can redraw on update
    private int currentSelectionIndex = -1;

    private void Awake()
    {

        // Assume where it starts in the scene is the "Visible" position
        onscreenPosition = hotbarAnchor.anchoredPosition;

        // Calculate the hidden position: convert height to negative Y movement
        // (This assumes your anchor is at the bottom. If centered, math might vary slightly)
        offscreenPosition = new Vector2(onscreenPosition.x, -onscreenPosition.y);
        print(onscreenPosition + " " + offscreenPosition);
    }

    private void Start()
    {
        if (hotbarAnchor == null)
        {
            Debug.LogError("HotbarUI: hotbarAnchor is not assigned!");
            return;
        }

        if (HotbarManager.Instance == null)
        {
            Debug.LogError("HotbarUI: HotbarManager instance not found!");
            return;
        }

        uiSlots = hotbarPanel.GetComponentsInChildren<HotbarSlotUI>();

        HotbarManager.Instance.OnHotbarUpdated += RefreshVisuals;
        HotbarManager.Instance.OnSelectionChanged += HandleSelectionChange;

        RefreshVisuals();
    }

    private void OnEnable()
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.OnHotbarUpdated += RefreshVisuals;
            HotbarManager.Instance.OnSelectionChanged += HandleSelectionChange;
        }
    }

    private void OnDisable()
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.OnHotbarUpdated -= RefreshVisuals;
            HotbarManager.Instance.OnSelectionChanged -= HandleSelectionChange;
        }
    }

    // ---------------------------------------------------------
    // NEW FUNCTIONS TO HIDE/SHOW PANEL
    // ---------------------------------------------------------

    public void HidePanel()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SlidePanel(offscreenPosition));
    }

    public void ShowPanel()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SlidePanel(onscreenPosition));
    }

    private IEnumerator SlidePanel(Vector2 targetPosition)
    {
        Vector2 startPosition = hotbarAnchor.anchoredPosition;
        float elapsedTime = 0;

        while (elapsedTime < slideDuration)
        {
            // Calculate the percentage of time passed (0.0 to 1.0)
            float t = elapsedTime / slideDuration;

            // APPLY EASING:
            // This converts linear time to a generic "SmoothStep" curve (Ease In/Out)
            // It makes the movement start slow and end slow.
            t = Mathf.SmoothStep(0f, 1f, t);

            hotbarAnchor.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it lands exactly on the target
        hotbarAnchor.anchoredPosition = targetPosition;
    }

    // ---------------------------------------------------------

    private void HandleSelectionChange(int newIndex)
    {
        currentSelectionIndex = newIndex;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (HotbarManager.Instance == null) { Debug.LogWarning("Hotbar Manager is not found!"); return; }
        if (uiSlots == null) { Debug.Log("Error"); return; }
        for (int i = 0; i < uiSlots.Length; i++)
        {
            //Food food = HotbarManager.Instance.GetFoodAt(i); // Uncommented for context
            Food food = HotbarManager.Instance.GetFoodAt(i);

            bool isSelected = (i == currentSelectionIndex);
            uiSlots[i].Setup(i, food, isSelected);
        }
    }

    [ContextMenu("Debug Toggle Panel")]
    private void DebugTogglePanel()
    {
        if (hotbarAnchor.anchoredPosition == onscreenPosition)
            HidePanel();
        else
            ShowPanel();
    }
}