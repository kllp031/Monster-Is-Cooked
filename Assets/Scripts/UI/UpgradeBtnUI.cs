using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeBtnUI : MonoBehaviour
{
    public PlayerDataManager.StatType statType;

    [SerializeField] string statName; // e.g., "SPD"
    [SerializeField] TMP_Text statNameText;
    [SerializeField] TMP_Text costText;

    public void Start()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager instance not found!");
            return;
        }

        //if (string.IsNullOrEmpty(statName))
        //    statName = statType.ToString(); 
        PlayerDataManager.Instance.onMoneyChanged.AddListener(UpdateBtnUI);
        UpdateBtnUI();
    }

    public void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.onMoneyChanged.RemoveListener(UpdateBtnUI);
        }
    }

    public void UpdateBtnUI()
    {
        // --- 1. HANDLE COST ---
        int cost = PlayerDataManager.Instance.GetNextUpgradeCost(statType);

        // Check Player Money for color (Red if broke, White if affordable)
        int myMoney = PlayerDataManager.Instance.TotalMoney;

        if (cost == -1)
        {
            costText.text = "MAX";
            costText.color = Color.white; // Or gold/green to signify completed
        }
        else
        {
            costText.text = cost.ToString();
            costText.color = (myMoney >= cost) ? Color.white : Color.red;
        }

        int currentLevel = PlayerDataManager.Instance.GetCurrentLevel(statType);

        // We want to show the NEXT level (Current + 1)
        // If maxed out (cost == -1), we usually just stay at current level or show "MAX"

        string romanNumeral = ToRoman(currentLevel);

        // Result: "SPD II" or "ATK IV"
        statNameText.text = $"{statName} {romanNumeral}";
    }

    public void OnClick()
    {
        if (PlayerDataManager.Instance.TryUpgradeStat(statType))
        {
            UpdateBtnUI();
            Debug.Log($"{statType} upgraded!");
        }
        else
        {
            Debug.Log($"{statType} upgrade failed.");
        }
    }

    // --- HELPER: CONVERT INT TO ROMAN NUMERAL ---
    private string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) return number.ToString(); // Fallback
        if (number < 1) return "";

        StringBuilder result = new StringBuilder();

        string[] romanHundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] romanTens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] romanOnes = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        // Calculate (handles up to 999 comfortably)
        result.Append(romanHundreds[(number % 1000) / 100]);
        result.Append(romanTens[(number % 100) / 10]);
        result.Append(romanOnes[number % 10]);

        return result.ToString();
    }
}