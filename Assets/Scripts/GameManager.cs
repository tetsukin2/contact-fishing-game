using UnityEngine;

/// <summary>
/// Handles game-wide management tasks
/// </summary>
public class GameManager : SingletonPersistent<GameManager>
{
    public SessionTelemetryData SessionData { get; private set; }
    public string SessionId { get; private set; }

    private void Update()
    {
        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteGameData();
            //GameDataHandler.CurrentGameData = GameDataHandler.LoadGameData("data", $"{_fishTotalToCatch}");
            //Debug.Log("Debug: Deleting Data");
        }
    }

    // This telemetry data processing is very rough
    public void OnSessionStart()
    {
        Debug.Log("Starting session...");
        SessionData = new SessionTelemetryData()
        {
            StartTime = System.DateTime.Now,
            EndTime = System.DateTime.MinValue, // Will be set on application quit
            ConInitDur = null // Will be set when connection is established
        };
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
    }

    private void OnConnectionEstablished() 
    {
        SessionData.ConInitDur = (int)System.DateTime.Now.Subtract(SessionData.StartTime).TotalSeconds;
        Debug.Log($"Connection established. Initialization duration: {SessionData.ConInitDur} seconds");
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if (SessionData == null) return;
        SessionData.EndTime = System.DateTime.Now;
        FirebaseUploadHandler.Instance.UploadData("sessions", SessionData);
    }
}
