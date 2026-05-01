using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Online version of <see cref="HotbarSlotUI"/>. Nhận <see cref="Recipe"/>
/// trực tiếp (không qua <see cref="Food"/>) vì <see cref="HotbarManagerOnline"/>
/// lưu Recipe.
/// </summary>
public class HotbarSlotUIOnline : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image selectionOutline;

    private int slotIndex;
    private Recipe recipe;

    public void Setup(int slotIndex, Recipe recipe, bool isSelected)
    {
        this.slotIndex = slotIndex;
        this.recipe = recipe;

        if (recipe != null)
        {
            iconImage.sprite = recipe.Icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        if (selectionOutline != null) selectionOutline.enabled = isSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HotbarManagerOnline.Instance == null) return;
        if (eventData.button == PointerEventData.InputButton.Left)
            HotbarManagerOnline.Instance.SelectSlot(slotIndex);
    }

    public void OnHotbarSlotClicked()
    {
        if (HotbarManagerOnline.Instance == null) return;
        HotbarManagerOnline.Instance.SelectSlot(slotIndex);
        print($"Hotbar slot {slotIndex} clicked. Recipe: {(recipe != null ? recipe.name : "None")}");
    }
}
