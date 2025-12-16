using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [SerializeField] string eatAnimationTrigger = "Eat";
    [SerializeField] Animator animator;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.GetComponent<Food>() != null)
        {
            Destroy(collision.gameObject);
            if (animator != null) animator.SetTrigger(eatAnimationTrigger);
        }
    }
}
