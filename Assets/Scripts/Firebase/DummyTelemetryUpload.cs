using System.Collections;
using UnityEngine;

public class DummyTelemetryUpload : MonoBehaviour
{
    [Tooltip("Number of simulations to run for dummy game data upload. Each simulation will simulate a series of actions.")]
    [Min(1)]
    [SerializeField] private int _simulationsCount = 2;

    private bool _uploadingGameData = false;

    public void UploadDummySessionData()
    {
        var dummySession = new SessionTelemetryData
        {
            StartTime = System.DateTime.UtcNow,
            EndTime = System.DateTime.UtcNow.AddMinutes(30), // Simulate a 30-minute session
            ConInitDur = 5
        };

        // Use a dummy session id for demonstration  
        string sessionId = "dummy_" + System.Guid.NewGuid().ToString("N");

        FirebaseUploadHandler.Instance.PostData("sessions", dummySession, sessionId);
    }

    //public void UploadDummyUserData()
    //{
    //    var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
    //    if (string.IsNullOrEmpty(token))
    //    {
    //        Debug.LogWarning("Cannot upload dummy user data: Auth token is not available.");
    //        return;
    //    }
    //    DummyUserId = "dummy_" + System.Guid.NewGuid().ToString();
    //    var dummySession = new SessionTelemetryData
    //    {
    //        StartTime = System.DateTime.UtcNow,
    //        EndTime = System.DateTime.UtcNow.AddMinutes(30), // Simulate a 30-minute session
    //        ConInitDur = 100,
    //    };
    //    UploadSessionData(dummySession);
    //}

    public void UploadDummyGameData()
    {
        if (_uploadingGameData)
        {
            Debug.LogWarning("Cannot upload dummy game data: Dummy game data simulation is in progress.");
            return;
        }

        var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Cannot upload dummy game data: Auth token is not available.");
            return;
        }

        _uploadingGameData = true;

        var dummyRound = new GameTelemetryData
        {
            UserID = "dummyGameID",
            StageID = "stage1",
            StartTime = System.DateTime.UtcNow,
            IsReplay = false,
            GameCompleted = true,
            FishCatchRequirement = 5,
            AverageActionsPerReel = 3
        };

        //if (string.IsNullOrEmpty(SessionId))
        //{
        //    Debug.LogWarning("Cannot upload dummy game data: Dummy Session must be uploaded first");
        //    return;
        //}

        StartCoroutine(SimulateGameplay(dummyRound));
    }

    private IEnumerator SimulateGameplay(GameTelemetryData data)
    {
        ActionTelemetryHandler.Instance.ClearAllActionData(); // Clean slate
        string[] actionNames = new string[]
        {
            "BaitPreparationRight",
            "BaitPreparationLeft",
            "FishSelection",
            "CastBack",
            "CastForward",
            "ReelBack",
            "ReelForward",
            "ReelClockwise",
            "ReelCounterClockwise",
            "InspectPrepare",
            "InspectFish",
            "ReleasePrepare",
            "ReleaseFish"
        };

        for (int i = 0; i < _simulationsCount; i++)
        {
            foreach (var action in actionNames)
            {
                ActionTelemetryHandler.Instance.StartActionTimer(action);
                float randomTime = Random.Range(0.5f, 3f);
                yield return new WaitForSeconds(randomTime); // Simulate time taken for each action
                Debug.Log($"Simulated action {action} taking {randomTime} seconds");
                ActionTelemetryHandler.Instance.EndAndRecordActionTimer(action);
            }
        }
        
        data.EndTime = System.DateTime.UtcNow;
        data.AverageTimeTaken = ActionTelemetryHandler.Instance.GetAverageTimeTaken();

        string sessionId = "dummy_" + System.Guid.NewGuid().ToString("N");

        FirebaseUploadHandler.Instance.PostData("games", data, sessionId);
    }
}
