using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Transform slotsParent;
    private HotbarSlotUI[] uiSlots;

    // Track locally so we can redraw on update
    private int currentSelectionIndex = -1;

    private void Start()
    {
        if (slotsParent == null)
        {
            Debug.LogError("HotbarUI: slotsParent is not assigned!");
            return;
        }

        if (HotbarManager.Instance == null)
        {
            Debug.LogError("HotbarUI: HotbarManager instance not found!");
            return;
        }

        uiSlots = slotsParent.GetComponentsInChildren<HotbarSlotUI>();

        HotbarManager.Instance.OnHotbarUpdated += RefreshVisuals;
        HotbarManager.Instance.OnSelectionChanged += HandleSelectionChange;

        RefreshVisuals();
    }

    private void OnDestroy()
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.OnHotbarUpdated -= RefreshVisuals;
            HotbarManager.Instance.OnSelectionChanged -= HandleSelectionChange;
        }
    }

    private void HandleSelectionChange(int newIndex)
    {
        currentSelectionIndex = newIndex;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            Recipe r = HotbarManager.Instance.GetRecipeAt(i);

            // Check if this specific slot is the selected one
            bool isSelected = (i == currentSelectionIndex);

            uiSlots[i].Setup(i, r, isSelected);
        }
    }
}