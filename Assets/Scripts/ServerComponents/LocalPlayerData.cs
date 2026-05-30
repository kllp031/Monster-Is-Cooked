using UnityEngine;

/// <summary>
/// Singleton bridge. Set by PlayerDataManagerOnline when local player spawns.
/// Screen-space UI scripts call LocalPlayerData.Instance instead of PlayerDataManager.Instance.
/// </summary>
public class LocalPlayerData : MonoBehaviour
{
    public static LocalPlayerData Instance { get; private set; }

    public PlayerDataManagerOnline Data { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public static void Register(PlayerDataManagerOnline data)
    {
        if (Instance == null)
        {
            var go = new GameObject("LocalPlayerData");
            go.AddComponent<LocalPlayerData>();
        }
        Instance.Data = data;
    }
}
