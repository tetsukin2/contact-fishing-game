using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SessionTelemetryData
{
    public string SessionID;
    public float SessionDuration;
    public System.DateTime SessionStartTime;
    public string UserID;
    public float ControllerConnectionInitializeDuration;
    public GameConfig Config;
}

[System.Serializable]
public class GameTelemetryData
{
    public string GameID;
    public string StageID;
    public System.DateTime StartTime;
    public bool IsReplay;
    public bool GameCompleted;
    public int FishCatchRequirement;
    public Dictionary<string, float> AverageTimeTaken;
    public float AverageActionsPerReel;
}

/// <summary>
/// File-based upload system.
/// </summary>
public class FirebaseUploadHandler : SingletonPersistent<FirebaseUploadHandler>
{
    [System.Serializable]
    private class UploadListWrapper
    {
        public List<UploadQueueItem> queue;
    }

    /// <summary>
    /// Individual item in the upload queue.
    /// </summary>
    [System.Serializable]
    public class UploadQueueItem
    {
        public string url;
        public string jsonBody;
    }

    private string queueFilePath;
    private List<UploadQueueItem> uploadQueue = new();
    private bool isUploading = false;

    protected override void OnAwake()
    {
        queueFilePath = Path.Combine(Application.persistentDataPath, "uploadQueue.json");
        LoadQueueFromFile();
        StartCoroutine(ProcessQueue());
    }

    public void UploadSessionData(string sessionId, string idToken, SessionTelemetryData sessionData)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/sessions/{sessionId}?access_token={idToken}";
        string jsonBody = FirestoreFormatUtility.WrapClass(sessionData);
        EnqueueUpload(url, jsonBody);
    }

    public void UploadGameData(string sessionId, string gameId, string idToken, GameTelemetryData roundData)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/sessions/{sessionId}/games/{gameId}?access_token={idToken}";
        string jsonBody = FirestoreFormatUtility.WrapClass(roundData);
        EnqueueUpload(url, jsonBody);
    }

    private void LoadQueueFromFile()
    {
        if (File.Exists(queueFilePath))
        {
            string json = File.ReadAllText(queueFilePath);
            UploadListWrapper wrapper = JsonUtility.FromJson<UploadListWrapper>(json);
            if (wrapper != null && wrapper.queue != null)
                uploadQueue = wrapper.queue;
            else
                uploadQueue = new List<UploadQueueItem>();
        }
    }

    private void SaveQueueToFile()
    {
        UploadListWrapper wrapper = new() { queue = uploadQueue };
        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(queueFilePath, json);
    }

    public void EnqueueUpload(string url, string jsonBody)
    {
        UploadQueueItem item = new() { url = url, jsonBody = jsonBody };
        uploadQueue.Add(item);
        SaveQueueToFile();
        if (!isUploading)
            StartCoroutine(ProcessQueue());
    }

    // Handle the upload queue processing
    private IEnumerator ProcessQueue()
    {
        isUploading = true;

        while (uploadQueue.Count > 0)
        {
            UploadQueueItem current = uploadQueue[0];

            UnityWebRequest request = new UnityWebRequest(current.url, "PATCH");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(current.jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload success: " + current.url);
                uploadQueue.RemoveAt(0);
                SaveQueueToFile();
            }
            else
            {
                Debug.LogWarning("Upload failed, will retry: " + current.url);
                break; // Exit and retry later
            }
        }

        isUploading = false;
    }

    public void UploadDummySessionData()
    {
        var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Cannot upload dummy session data: Auth token is not available.");
            return;
        }

        var dummySession = new SessionTelemetryData
        {
            SessionID = "dummy_session_id",
            SessionDuration = 12345,
            SessionStartTime = System.DateTime.UtcNow,
            UserID = "dummy_user",
            ControllerConnectionInitializeDuration = 100,
            Config = ResourceSystem.Instance.GameConfig
        };

        UploadSessionData(dummySession.SessionID, token, dummySession);
    }

    public void UploadDummyGameData()
    {
        var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Cannot upload dummy game data: Auth token is not available.");
            return;
        }

        var dummyRound = new GameTelemetryData
        {
            GameID = "dummy_game_id",
            StageID = "stage_1",
            StartTime = System.DateTime.UtcNow,
            IsReplay = false,
            GameCompleted = true,
            FishCatchRequirement = 5,
            AverageTimeTaken = new Dictionary<string, float>
            {
               { "action1", 200 },
               { "action2", 500 },
            },
            AverageActionsPerReel = 3.5f
        };

        // Use a dummy session id for demonstration
        UploadGameData("dummy_session_id", dummyRound.GameID, token, dummyRound);
    }
}
