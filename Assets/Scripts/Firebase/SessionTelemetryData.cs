/// <summary>
/// Telemetry data for a full user session, from startup to application exit
/// </summary>
[System.Serializable]
public class SessionTelemetryData
{
    public string UserID; 
    public System.DateTime StartTime;
    public System.DateTime EndTime;
    public int? ConInitDur; // Controller connection initialization duration
}