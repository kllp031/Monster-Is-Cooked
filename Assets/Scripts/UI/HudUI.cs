using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HudUI : MonoBehaviour
{
    [SerializeField] TMP_Text coinTxt;

    private void Start()
    {
        GameManager.Instance.OnCollectedMoneyChanged.AddListener(OnUpdateCoinText);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnCollectedMoneyChanged.RemoveListener(OnUpdateCoinText);
    }

    private void OnUpdateCoinText()
    {
        UpdateCoinText(GameManager.Instance.CollectedMoney);
    }

    private void UpdateCoinText(int amount)
    {
        //print("UI updated");
        coinTxt.text = amount.ToString();
    }
}
