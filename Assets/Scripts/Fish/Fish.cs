using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishRarity
{
    Common,
    Rare,
    Legendary
}

[CreateAssetMenu(menuName = "Scriptable Object/Fish")]
public class Fish : ScriptableObject
{
    public string FishName;
    public Sprite Sprite;
    public FishRarity Rarity;
}
