using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Object/FishData")]
public class FishData : ScriptableObject
{
    public enum FishRarity
    {
        Common,
        Rare,
        Legendary
    }

    public string FishName;
    public string FishID;
    public Sprite Sprite;
    public FishRarity Rarity;
}
