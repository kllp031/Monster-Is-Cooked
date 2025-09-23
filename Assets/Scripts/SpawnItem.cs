using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab; // assign prefab in Inspector
    public Transform spawnPoint;  // optional: where to spawn

    void Update()
    {
        // Example: Press Space to spawn item
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (spawnPoint != null)
        {
            // Spawn at a fixed point
            Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            // Spawn at this object's position
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }
}
