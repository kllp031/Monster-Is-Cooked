using UnityEngine;
using UnityEngine.EventSystems; // Required for click detection
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionOutline; // Assign a border image here

    private int slotIndex;
    private Recipe recipe;

    //public void Setup(int slotIndex, Recipe recipe, bool isSelected)
    //{
    //    this.slotIndex = slotIndex;
    //    this.recipe = recipe;

    //    // 1. Icon Setup
    //    if (recipe != null)
    //    {
    //        iconImage.sprite = recipe.Icon;
    //        iconImage.enabled = true;
    //    }
    //    else
    //    {
    //        iconImage.enabled = false;
    //    }

    //    // 2. Selection Setup
    //    if (selectionOutline != null)
    //    {
    //        selectionOutline.enabled = isSelected;
    //    }
    //}

    public void Setup(int slotIndex, Food food, bool isSelected)
    {
        this.slotIndex = slotIndex;
        recipe = (food != null)? food.Recipe : null;

        // 1. Icon Setup
        if (recipe != null)
        {
            iconImage.sprite = recipe.Icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // 2. Selection Setup
        if (selectionOutline != null)
        {
            selectionOutline.enabled = isSelected;
        }
    }

    // This detects BOTH Left and Right clicks
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left Click = Select
            HotbarManager.Instance.SelectSlot(slotIndex);
        }
        //else if (eventData.button == PointerEventData.InputButton.Right)
        //{
        //    // Right Click = Remove
        //    if (recipe != null)
        //    {
        //        HotbarManager.Instance.RemoveRecipe(slotIndex);
        //    }
        //}
    }

    public void OnHotbarSlotClicked()
    {
        HotbarManager.Instance.SelectSlot(slotIndex);
        print("clicked hotbar slot " + slotIndex + ": " + (recipe != null ? recipe.name : "Empty"));
    }
}