using System.Collections.Generic;

/// <summary>
/// Telemetry data for a single game session, from fishing ready to performance metrics
/// </summary>
[System.Serializable]
public class GameTelemetryData
{
    public string UserID;
    public string StageID;
    public bool IsReplay;
    public System.DateTime StartTime;
    public System.DateTime EndTime;
    public int AverageActionsPerReel;
    public bool GameCompleted;
    public int FishCatchRequirement;
    public Dictionary<string, float> AverageTimeTaken; // ActionName, AverageTimeTaken
    public Dictionary<string, int> RepetitionCounts; // Number of times each action was done
    public Dictionary<string, float> MaxAngles;       // Highest angle achieved per action
}