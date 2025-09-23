using UnityEngine;

public class AttackFX : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.Die();
        }
    }
}
