using UnityEngine;

/// <summary>
/// Handles game-wide management tasks
/// </summary>
public class GameManager : SingletonPersistent<GameManager>
{
    private float _controllerConnectTime = 0f;
    private bool _controllerConnecting = false;

    public SessionTelemetryData SessionData { get; private set; }
    public string SessionId { get; private set; }

    private void Update()
    {
        if (_controllerConnecting)
            _controllerConnectTime += Time.deltaTime;

        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteGameData();
            //GameDataHandler.CurrentGameData = GameDataHandler.LoadGameData("data", $"{_fishTotalToCatch}");
            Debug.Log("Debug: Deleting Data");
        }
    }

    // This telemetry data processing is very rough
    public void OnSessionStart()
    {
        SessionId = System.Guid.NewGuid().ToString();
        SessionData = new SessionTelemetryData()
        {
            StartTime = System.DateTime.Now,
            EndTime = System.DateTime.MinValue, // Will be set on application quit
            ConInitDur = 0f,
            ConConnectSuccess = false
        };
        _controllerConnecting = true;
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
    }

    private void OnConnectionEstablished() 
    {
        _controllerConnecting = false;
        SessionData.ConInitDur = _controllerConnectTime;
        SessionData.ConConnectSuccess = true;
        _controllerConnectTime = 0f; // Reset for next session, in case
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if (SessionData == null) return;
        SessionData.EndTime = System.DateTime.Now;
        FirebaseUploadHandler.Instance.UploadData("sessions", SessionData);
    }
}
