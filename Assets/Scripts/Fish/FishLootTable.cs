using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles fishing loot table
/// </summary>
public class FishLootTable : MonoBehaviour
{
    public static FishLootTable Instance { get; private set; }

    //an instance of fish with weight for loot calcs
    [System.Serializable]
    private class FishLoot
    {
        public Fish Fish;
        public int Weight;
    }

    //diff loot tables depending on distance
    [SerializeField] private List<FishLoot> _fishLootTable = new();

    private void Awake()
    {
        // Singleton but doesn't need scene persistence
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //called during upgrades, changes weight per rarity
    //public void ChangeWeights(FishRarity fishRarity, int value)
    //{
    //    foreach (FishLoot fishLoot in _fishLootTable)
    //        if (fishLoot.Fish.Rarity == fishRarity)
    //            fishLoot.Weight = Mathf.Clamp(fishLoot.Weight + value, 0, int.MaxValue);
    //}

    /// <summary>
    /// called by GetFish to get a fish from a specific table
    /// </summary>
    /// <returns>a fish</returns>
    public Fish GetFishFromTable()
    {
        // Calculate total table weight
        int totalWeight = 0;

        foreach (FishLoot fishLoot in _fishLootTable)
        {
            totalWeight += fishLoot.Weight;
        }

        // Generate a random weight to select a fish
        int weight = Random.Range(0, totalWeight + 1);

        // Select a fish based on the random weight
        foreach (FishLoot fishLoot in _fishLootTable)
        {
            if (weight <= fishLoot.Weight)
            {
                if (fishLoot.Fish != null)
                    return fishLoot.Fish;
                break;
            }

            weight -= fishLoot.Weight;
        }

        Debug.LogError("Get Fish from DB error");
        return null;
    }
}
