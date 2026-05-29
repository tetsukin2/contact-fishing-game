using System.Collections.Generic;

/// <summary>
/// Nested telemetry for tactile discrimination test
/// </summary>
[System.Serializable]
public class DiscriminationTestTelemetryData
{
    public int Score;
    public int Total;
    public List<string> TrialPatterns;
    public List<string> UserAnswers;
    public List<bool> CorrectAnswers;
}

/// <summary>
/// Telemetry data for a single game session
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
    public int GameCyclesPerSession;
    public int CompletedGameCycles;
    public int GameSessionsPerWeek;
    public Dictionary<string, float> AverageTimeTaken;
    public Dictionary<string, int> RepetitionCounts;
    public Dictionary<string, float> MaxAngles;

    public int DiscriminationScore;
    public int DiscriminationTotal;
    public List<string> DiscriminationTrialPatterns;
    public List<string> DiscriminationUserAnswers;
    public List<bool> DiscriminationCorrectAnswers;

    public DiscriminationTestTelemetryData DiscriminationTest;
}