using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mapping deterministic giữa <see cref="Recipe"/> ScriptableObject và int ID.
///
/// Lý do tồn tại: networked vars (Fusion <c>[Networked]</c>) không sync
/// reference tới SO trực tiếp được — phải dùng giá trị nguyên thuỷ. Mỗi
/// client load cùng asset registry → dùng index trong list làm ID ổn định
/// để truyền qua mạng và lookup ngược về Recipe.
///
/// Setup:
/// - Tạo asset: Project → right-click → Create → Cooking → Recipe Registry.
/// - Kéo MỌI Recipe asset vào list <c>recipes</c>. Thứ tự phải giống nhau
///   trên mọi client (asset cùng GUID → cùng nội dung).
/// - Drag asset registry vào field <c>recipeRegistry</c> của
///   <see cref="FoodHolderOnline"/>.
/// </summary>
[CreateAssetMenu(menuName = "Cooking/Recipe Registry")]
public class RecipeRegistry : ScriptableObject
{
    [SerializeField] private List<Recipe> recipes = new();

    public Recipe GetRecipe(int id)
    {
        if (id < 0 || id >= recipes.Count) return null;
        return recipes[id];
    }

    public int GetId(Recipe recipe)
    {
        if (recipe == null) return -1;
        return recipes.IndexOf(recipe);
    }
}
