using System.Collections.Generic;

/// <summary>
/// Data structure for storing level scoring data.
/// </summary>
[System.Serializable]
public struct LevelScoreData
{
    public string LevelName;
    public int FishCount;
    public float CompletionTime;
    public LevelScoreData(string levelName, int fishCount, float completionTime)
    {
        LevelName = levelName;
        FishCount = fishCount;
        CompletionTime = completionTime;
    }
}

[System.Serializable]
public class GameData
{
    private List<string> _discoveredFish = new();
    private List<LevelScoreData> _bestScores = new();

    /// <summary>
    /// Attempts to create a new best time for the given level and fish count. 
    ///     Creates a new record if a record for the given level and fish count does not exist.
    ///     Overwrites the record if one exists, and the new time is better.
    /// </summary>
    /// <returns>true if a record was successfully added or overwritten, false otherwise</returns>
    public bool TryAddNewBestTime(string levelName, int fishCount, float newTime)
    {
        // Check if a record for the given level name exists  
        var existingLevel = _bestScores.Find(level => level.LevelName == levelName);

        // If no record exists for the level, or for the fish count, add a new one  
        if (existingLevel.LevelName == null || existingLevel.FishCount != fishCount)
        {
            _bestScores.Add(new LevelScoreData(levelName, fishCount, newTime));
            return true;
        }

        // If the record exists and time is better, update it  
        if (newTime< existingLevel.CompletionTime)
        {
            _bestScores.Remove(existingLevel);
            _bestScores.Add(new LevelScoreData(levelName, fishCount, newTime));
            return true;
        }

        // No update was made  
        return false;
    }

    /// <summary>
    /// Returns the best score for the given level name and fish count.
    /// </summary>
    /// <returns>best score if record exists, -1 otherwise</returns>
    public float GetBestScore(string levelName, int fishCount)
    {
        // Check if a record for the given level name exists  
        var existingLevel = _bestScores.Find(level => level.LevelName == levelName && level.FishCount == fishCount);
        // If no record exists for the level, return -1
        if (existingLevel.LevelName == null)
            return -1;
        // Return the score for the given level and fish count  
        return existingLevel.CompletionTime;
    }

    public void AddDiscoveredFish(string fishID)
    {
        if (!_discoveredFish.Contains(fishID))
        {
            _discoveredFish.Add(fishID);
        }
    }

    public bool HasDiscoveredFish(string fishID)
    {
        return _discoveredFish.Contains(fishID);
    }
}