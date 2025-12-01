using UnityEngine;

public class DropItem : MonoBehaviour
{
    [Header("Pickup Prefabs")]
    public GameObject[] pickupPrefabs;

    [Header("Settings")]
    public float scatterForce = 3f;    

    public void Drop()
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0) return;

        for( int i=0; i<pickupPrefabs.Length; i++)
        {
            GameObject prefab = pickupPrefabs[i];

            Vector3 spawnPos = transform.position + new Vector3(Random.RandomRange(-0.5f,0.5f) , Random.RandomRange(-0.5f, 0.5f));
            GameObject item = Instantiate(prefab, spawnPos, Quaternion.identity);

            //effect
        }

        
    }
}
