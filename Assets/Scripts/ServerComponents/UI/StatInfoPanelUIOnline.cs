using UnityEngine;
using TMPro;

/// <summary>
/// Online variant of StatInfoPanelUI.
/// Polls LocalPlayerData each frame — no event subscription needed since PlayerDataManagerOnline
/// fires Render() every tick when values change.
/// </summary>
public class StatInfoPanelUIOnline : MonoBehaviour
{
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyBonusText;

    private PlayerDataManagerOnline data => LocalPlayerData.Instance?.Data;

    private void OnEnable() => UpdateStatInfo();

    private void Update()
    {
        if (data == null) return;
        UpdateStatInfo();
    }

    private void UpdateStatInfo()
    {
        if (data == null) return;
        if (atkText != null)        atkText.text        = "ATK: "          + data.CurrentAttack;
        if (speedText != null)      speedText.text      = "SPD: "          + data.CurrentSpeed.ToString("F1");
        if (healthText != null)     healthText.text     = "HEALTH: "       + data.CurrentMaxHealth;
        if (moneyBonusText != null) moneyBonusText.text = "BONUS MONEY: "  + data.CurrentBonusMoney;
    }
}
