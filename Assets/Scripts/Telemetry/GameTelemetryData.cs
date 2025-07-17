using System.Collections.Generic;

/// <summary>
/// Telemetry data for a single game session, from fishing ready to performance metrics
/// </summary>
[System.Serializable]
public class GameTelemetryData
{
    public string GameID;
    public string StageID;
    public System.DateTime StartTime;
    public bool IsReplay;
    public bool GameCompleted;
    public float CompletionTime;
    public int FishCatchRequirement;
    public Dictionary<string, float> AverageTimeTaken; // ActionName, AverageTimeTaken
    public float AverageActionsPerReel;
}