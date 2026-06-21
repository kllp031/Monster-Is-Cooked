using UnityEngine;
using TMPro;

/// <summary>
/// Drives the in-level shared team-money text (HUD/Coin/SharedCoin) for online.
/// Subscribes GameManagerOnline.OnCollectedMoneyChanged — fires on every client
/// when the networked CollectedMoney changes, so all players see the same number.
/// </summary>
public class SharedCoinUIOnline : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void OnEnable()
    {
        GameManagerOnline.OnCollectedMoneyChanged += Refresh;
        Refresh(); // initial value (handles late-enable / mid-level)
    }

    private void OnDisable()
    {
        GameManagerOnline.OnCollectedMoneyChanged -= Refresh;
    }

    private void Refresh()
    {
        if (coinText == null) return;
        var gm = GameManagerOnline.Instance;
        // Networked props only readable after Spawned(); Instance is set earlier in Awake().
        int amount = (gm != null && gm.Object != null && gm.Object.IsValid)
            ? gm.CollectedMoney
            : 0;
        coinText.text = amount.ToString();
    }
}
