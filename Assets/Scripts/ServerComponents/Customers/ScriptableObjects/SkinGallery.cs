using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(menuName = "Customers/SkinGallery")]
public class SkinGallery : ScriptableObject
{
    [SerializeField] List<SkinDetails> skins = new();

    public SpriteLibraryAsset GetSkin(string skinId)
    {
        foreach(var skin in skins)
            if (skin.SkinId == skinId) return skin.SpriteLibraryAsset;
        return null;
    }
}

[Serializable]
public struct SkinDetails
{
    [SerializeField] public string SkinId;
    [SerializeField] public SpriteLibraryAsset SpriteLibraryAsset;
}
