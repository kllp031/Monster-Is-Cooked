using UnityEngine;

public class TrashCan : MonoBehaviour, IInteractable
{
    [SerializeField] string eatAnimationTrigger = "Eat";
    [SerializeField] Animator animator;

    public void OnInteract(GameObject obj)
    {
        if (obj.GetComponent<FoodHolder>() == null) return;
        var heldFood = obj.GetComponent<FoodHolder>().HeldFood;
        obj.GetComponent<FoodHolder>().DropHeldFood();
        Destroy(heldFood.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.GetComponent<Food>() != null)
        {
            Destroy(collision.gameObject);
            if (animator != null) animator.SetTrigger(eatAnimationTrigger);
        }
    }
}
