/// <summary>
/// Telemetry data for a full user session, from startup to application exit
/// </summary>
[System.Serializable]
public class SessionTelemetryData
{
    public string SessionID;
    public float SessionDuration;
    public System.DateTime SessionStartTime;
    public string UserID;
    public float ControllerConnectionInitializeDuration;
    public GameplayConfig Config;
}