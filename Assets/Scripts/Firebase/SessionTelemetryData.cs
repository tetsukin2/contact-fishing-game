/// <summary>
/// Telemetry data for a full user session, from startup to application exit
/// </summary>
[System.Serializable]
public class SessionTelemetryData
{
    public System.DateTime StartTime;
    public System.DateTime EndTime;
    public float ConInitDur; // Controller connection initialization duration
}