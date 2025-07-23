using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
    private string DummyUserId;

    public string SessionId { get; private set; }

    protected override void OnAwake()
    {
        // Start of the session
        SessionId = System.Guid.NewGuid().ToString();

        queueFilePath = Path.Combine(Application.persistentDataPath, "uploadQueue.json");
        LoadQueueFromFile();
        StartCoroutine(ProcessQueue());
    }

    public void UploadSessionData(string idToken, SessionTelemetryData sessionData)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/sessions/{SessionId}?access_token={idToken}";
        string jsonBody = FirestoreFormatUtility.WrapClass(sessionData);
        EnqueueUpload(url, jsonBody);
    }

    public void UploadGameData(string gameId, string idToken, GameTelemetryData roundData)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/sessions/{SessionId}/games/{gameId}?access_token={idToken}";
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
            StartTime = System.DateTime.UtcNow,
            EndTime = System.DateTime.UtcNow.AddMinutes(30), // Simulate a 30-minute session
            ConInitDur = 100,
        };

        // Use a dummy session id for demonstration  
        // SessionId = "dummy_" + System.Guid.NewGuid().ToString();

        UploadSessionData(token, dummySession);
    }

    public void UploadDummyUserData()
    {
        var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("Cannot upload dummy user data: Auth token is not available.");
            return;
        }
        DummyUserId = "dummy_" + System.Guid.NewGuid().ToString();
        var dummySession = new SessionTelemetryData
        {
            StartTime = System.DateTime.UtcNow,
            EndTime = System.DateTime.UtcNow.AddMinutes(30), // Simulate a 30-minute session
            ConInitDur = 100,
        };
        UploadSessionData(token, dummySession);
    }

    //public void UploadDummyGameData()
    //{
    //    var token = FirebaseConnectionHandler.Instance.CurrentAuthToken;
    //    if (string.IsNullOrEmpty(token))
    //    {
    //        Debug.LogWarning("Cannot upload dummy game data: Auth token is not available.");
    //        return;
    //    }

    //    var dummyRound = new GameTelemetryData
    //    {
    //        GameID = "dummyGameID",
    //        StageID = "stage_1",
    //        StartTime = System.DateTime.UtcNow,
    //        IsReplay = false,
    //        GameCompleted = true,
    //        FishCatchRequirement = 5,
    //        AverageTimeTaken = new Dictionary<string, float>
    //        {
    //           { "action1", 200 },
    //           { "action2", 500 },
    //        },
    //        AverageActionsPerReel = 3.5f
    //    };

    //    if (string.IsNullOrEmpty(SessionId))
    //    {
    //        Debug.LogWarning("Cannot upload dummy game data: Dummy Session must be uploaded first");
    //        return;
    //    }

    //    // Use a dummy session id for demonstration
    //    UploadGameData(dummyRound.GameID, token, dummyRound);
    //}
}
