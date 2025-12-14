using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StatInfoPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyMulText;

    private void Start()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager instance not found!");
            return;
        }
        PlayerDataManager.Instance.onHealthUpgrade.AddListener(UpdateStatInfo);
        PlayerDataManager.Instance.onSpeedUpgrade.AddListener(UpdateStatInfo);
        PlayerDataManager.Instance.onAttackUpgrade.AddListener(UpdateStatInfo);
        PlayerDataManager.Instance.onMoneyMulUpgrade.AddListener(UpdateStatInfo);

        UpdateStatInfo();
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance == null) return;
        PlayerDataManager.Instance.onHealthUpgrade.RemoveListener(UpdateStatInfo);
        PlayerDataManager.Instance.onSpeedUpgrade.RemoveListener(UpdateStatInfo);
        PlayerDataManager.Instance.onAttackUpgrade.RemoveListener(UpdateStatInfo);
        PlayerDataManager.Instance.onMoneyMulUpgrade.RemoveListener(UpdateStatInfo);
    }

    private void UpdateStatInfo()
    {
        if (PlayerDataManager.Instance == null) return;
        atkText.text = "ATK: " + PlayerDataManager.Instance.CurrentAttack.ToString();
        speedText.text = "SPD: " + PlayerDataManager.Instance.CurrentSpeed.ToString("F1");
        healthText.text = "HEALTH " + PlayerDataManager.Instance.CurrentMaxHealth.ToString();
        moneyMulText.text = "MONEY MUL: " + PlayerDataManager.Instance.CurrentMoneyMul.ToString("F1") + "x";
    }
}
