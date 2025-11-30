using UnityEngine;
using UnityEngine.UI;
public class IngredientSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMPro.TextMeshProUGUI countText;
    public void Setup(Ingredient ingredient, int owned, int required)
    {
        icon.sprite = ingredient.icon;
        countText.text = owned + "/" + required;
        countText.color = owned < required ? Color.red : Color.white;
    }
}
