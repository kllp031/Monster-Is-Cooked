using UnityEngine;

// General Idea: We can create different kinds of food and each food will have different implementation of the OnInteract() function
// This is a simple example of a food that will call the PickUpFood() function on the player's FoodHolder script
public class Food : MonoBehaviour
{
    private Recipe recipe;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetUp(Recipe recipe)
    {
        this.recipe = recipe;
        spriteRenderer.sprite = recipe.icon;
    }

    public void OnInteract(GameObject player)
    {
        if(player && player.GetComponent<FoodHolder>())
        {
            player.GetComponent<FoodHolder>().PickUpFood(this.gameObject);
        }
    }
}
