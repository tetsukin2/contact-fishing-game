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
    public Dictionary<string, int> AverageTimeTaken; // ActionName, AverageTimeTaken
}