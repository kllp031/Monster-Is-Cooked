using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public GameObject itemToDrop;

    private UnityEvent onDeath = new UnityEvent();

    private void OnEnable()
    {
        onDeath.AddListener(DropItem);
    }

    private void OnDisable()
    {
        onDeath.RemoveListener(DropItem);
    }

    private void OnValidate()
    {
        // Only warn for scene instances (not prefab assets)
        if (!gameObject.scene.IsValid())
            return;

        if (itemToDrop == null)
        {
            Debug.LogWarning($"{nameof(Enemy)} on '{name}' has no {nameof(itemToDrop)} assigned.", this);
        }
    }

    public void Die()
    {
        DropItem();
        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (itemToDrop == null)
        {
            Debug.LogError("Enemy.itemToDrop is not assigned.", this);
            return;
        }

        Instantiate(itemToDrop, transform.position, Quaternion.identity);
    }
}
