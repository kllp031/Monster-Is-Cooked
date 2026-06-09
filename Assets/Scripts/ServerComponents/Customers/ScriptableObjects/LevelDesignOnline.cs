using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(menuName = "Game/OnlineLevelDesign")]
public class LevelDesignOnline : ScriptableObject
{
    [SerializeField] List<LevelDetailOnline> levelsDetails = new();

    public LevelDetailOnline GetLevelDetail(int index)
    {
        if (index < 0 || index >= levelsDetails.Count) return null;
        return levelsDetails[index];
    }

    public bool CheckValidLevel(int index)
    {
        if (index < 0 || index >= levelsDetails.Count) return false;
        if (levelsDetails[index] == null) return false;
        return true;
    }
}

[Serializable]
public class LevelDetailOnline
{
    [SerializeField] int targetMoney;
    [SerializeField] List<string> customerSkinIds = new(); // OnlineCustomesSpawner use this id and CustomerSkinGalery to spawn
    [SerializeField] List<CustomerDetailOnline> customerDetails = new(); // Use struct for synchronization purpose
    [Tooltip("The customer will appear 'appear time' seconds after the start of the level")]
    [SerializeField] List<float> appearTime = new();
    public int TargetMoney { get => targetMoney; }
    public List<string> CustomersSkinIds { get => customerSkinIds; }
    public List<CustomerDetailOnline> CustomerDetails { get => customerDetails; }
    public List<float> AppearTime { get => appearTime; }
}
