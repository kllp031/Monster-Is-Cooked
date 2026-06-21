using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Online variant of UpgradeBtnUI. Reads/writes PlayerDataManagerOnline via LocalPlayerData.
/// </summary>
public class UpgradeBtnUIOnline : MonoBehaviour
{
    public PlayerDataManagerOnline.StatType statType;

    [SerializeField] string statName;
    [SerializeField] TMP_Text statNameText;
    [SerializeField] TMP_Text costText;

    private PlayerDataManagerOnline data => LocalPlayerData.Instance?.Data;

    private void OnEnable() => UpdateBtnUI();

    private void Update()
    {
        if (data == null) return;
        UpdateBtnUI();
    }

    public void UpdateBtnUI()
    {
        if (data == null) return;

        int cost = data.GetNextUpgradeCost(statType);
        int myMoney = data.TotalMoney;

        if (costText != null)
        {
            if (cost == -1)
            {
                costText.text = "MAX";
                costText.color = Color.white;
            }
            else
            {
                costText.text = cost.ToString();
                costText.color = myMoney >= cost ? Color.white : Color.red;
            }
        }

        if (statNameText != null)
        {
            int currentLevel = data.GetCurrentLevel(statType);
            statNameText.text = $"{statName} {ToRoman(currentLevel)}";
        }
    }

    public void OnClick()
    {
        if (data == null) return;
        data.TryUpgradeStat(statType);
        //Debug.Log($"{statType} upgraded or failed");
    }

    private string ToRoman(int number)
    {
        if (number < 0 || number > 3999) return number.ToString();
        if (number < 1) return "";
        var result = new StringBuilder();
        string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] tens =     { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] ones =     { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
        result.Append(hundreds[(number % 1000) / 100]);
        result.Append(tens[(number % 100) / 10]);
        result.Append(ones[number % 10]);
        return result.ToString();
    }
}
