using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishRarity
{
    Common,
    Rare,
    Legendary
}

[CreateAssetMenu(menuName = "Scriptable Object/FishData")]
public class FishData : ScriptableObject
{
    public string FishName;
    public string FishID;
    public Sprite Sprite;
    public FishRarity Rarity;
}
