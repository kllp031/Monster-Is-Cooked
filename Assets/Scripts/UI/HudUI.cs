using UnityEngine;
using TMPro;

public class HudUI : MonoBehaviour
{
    [SerializeField] TMP_Text coinTxt;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            //Debug.LogWarning("HudUI: GameManager.Instance không có trong scene. Coin text sẽ không cập nhật.");
            return;
        }

        GameManager.Instance.OnCollectedMoneyChanged.AddListener(OnUpdateCoinText);
        OnUpdateCoinText(); // Khởi tạo giá trị ngay khi bắt đầu
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCollectedMoneyChanged.RemoveListener(OnUpdateCoinText);
    }

    private void OnUpdateCoinText()
    {
        if (GameManager.Instance == null) return;
        UpdateCoinText(GameManager.Instance.CollectedMoney);
    }

    private void UpdateCoinText(int amount)
    {
        if (coinTxt == null) return;
        coinTxt.text = amount.ToString();
    }
}
