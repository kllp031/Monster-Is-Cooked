using UnityEngine;

public class LocalTimedDestroyer : MonoBehaviour
{
    public float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
