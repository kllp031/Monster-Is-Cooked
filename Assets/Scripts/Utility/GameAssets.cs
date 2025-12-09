using UnityEngine;

public class GameAssets : MonoBehaviour
{
    // The internal singleton instance
    private static GameAssets _instance;

    // Public read-only access
    public static GameAssets Instance
    {
        get
        {
            if (_instance == null)
            {
                // Safety check: Find it if it exists but wasn't cached
                _instance = FindFirstObjectByType<GameAssets>();

                // Optional: Load from Resources if it doesn't exist (Advanced)
                /* if (_instance == null) 
                    _instance = Instantiate(Resources.Load<GameAssets>("GameAssets")); 
                */
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------------------------
    // YOUR ASSETS GO HERE
    // --------------------------------------------------------

    [Header("Core Prefabs")]
    [Tooltip("The generic Food shell that holds Recipe data")]
    public Transform pfFood;

    //[Header("VFX")]
    //public Transform pfSmokeParticle;
    //public Transform pfSuccessPopup;
}