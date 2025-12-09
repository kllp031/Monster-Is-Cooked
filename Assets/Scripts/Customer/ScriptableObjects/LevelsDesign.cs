using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(menuName = "Game/LevelDesign")]
public class LevelsDesign : ScriptableObject
{
    [SerializeField] List<LevelDetail> levelsDetails = new();

    public LevelDetail GetLevelDetail(int index)
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
public class LevelDetail
{
    [SerializeField] int targetMoney;
    [SerializeField] List<SpriteLibraryAsset> customerSkins = new();
    [SerializeField] List<CustomerDetail> customerDetails = new();
    [Tooltip("The customer will appear 'appear time' seconds after the start of the level")]
    [SerializeField] List<float> appearTime = new();
    public int TargetMoney { get => targetMoney; }
    public List<SpriteLibraryAsset> CustomersSkins { get => customerSkins; }
    public List<CustomerDetail> CustomerDetails { get => customerDetails; }
    public List<float> AppearTime { get => appearTime; }
}
