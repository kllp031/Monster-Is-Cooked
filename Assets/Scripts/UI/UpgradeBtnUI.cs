using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeBtnUI : MonoBehaviour
{
    public PlayerDataManager.StatType statType;
    public TextMeshProUGUI costText;
    //public TextMeshProUGUI levelText;


    public void Start()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager instance not found!");
            return;
        }

        PlayerDataManager.Instance.onMoneyChanged.AddListener(UpdateBtnUI);

        UpdateBtnUI();
    }

    public void OnDisable()
    {
        PlayerDataManager.Instance.onMoneyChanged.RemoveListener(UpdateBtnUI);
    }

    public void UpdateBtnUI()
    {
        // 1. Get Cost
        int cost = PlayerDataManager.Instance.GetNextUpgradeCost(statType);

        // 2. Update Text
        if (cost == -1)
        {
            costText.text = "MAX";
        }
        else
        {
            costText.text = cost.ToString();
        }

        // 3. Optional: Make button red if too expensive
        int myMoney = PlayerDataManager.Instance.TotalMoney;
        costText.color = (myMoney >= cost) ? Color.white : Color.red;
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
}