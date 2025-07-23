using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles game-wide management tasks
/// </summary>
public class GameManager : SingletonPersistent<GameManager>
{
    public SessionTelemetryData SessionData { get; private set; }

    protected override void OnSetup()
    {
        
    }

    private void Update()
    {
        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteGameData();
            //GameDataHandler.CurrentGameData = GameDataHandler.LoadGameData("data", $"{_fishTotalToCatch}");
            Debug.Log("Debug: Deleting Data");
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        //FirebaseUploadHandler.Instance.UploadSessionData(SessionData.IdToken, SessionData);
    }
}
