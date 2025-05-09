using System.Collections.Generic;
using System;

[Serializable]
public class GameData
{
    public float BestTime = float.MaxValue;
    public List<string> UnlockedFish = new();

    public void AddFish(string fishID)
    {
        if (!UnlockedFish.Contains(fishID))
        {
            UnlockedFish.Add(fishID);
        }
    }

    public bool HasDiscoveredFish(string fishID)
    {
        return UnlockedFish.Contains(fishID);
    }
}