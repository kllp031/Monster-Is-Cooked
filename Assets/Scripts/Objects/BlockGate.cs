using UnityEngine;

public class BlockGate : MonoBehaviour
{
    [SerializeField] private int levelUnlock = 0;

    // Update is called once per frame
    void Update()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (PlayerDataManager.Instance.Level >= levelUnlock)
            {
                Destroy(gameObject);
            }
        }
    }
}
