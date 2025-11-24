using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private Ingredient currentIngredient;

    public void SetItem(Ingredient ingredient, int amount)
    {
        currentIngredient = ingredient;

        if (ingredient != null)
        {
            iconImage.sprite = ingredient.icon;
            iconImage.enabled = true;

            // Format the text (e.g., "x5")
            amountText.text = amount > 0 ? amount.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentIngredient = null;
        iconImage.enabled = false;
        amountText.text = "";
    }
}