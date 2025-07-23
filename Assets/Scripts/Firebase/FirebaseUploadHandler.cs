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

    // Only needs to be tracked here as it just needs to block the upload
    [SerializeField] private bool _enableTelemetry = true;

    private string queueFilePath;
    private List<UploadQueueItem> uploadQueue = new();
    private bool isUploading = false;
    private string DummyUserId;

    protected override void OnAwake()
    {
        queueFilePath = Path.Combine(Application.persistentDataPath, "uploadQueue.json");
        LoadQueueFromFile();
        StartCoroutine(ProcessQueue());
    }

    /// <summary>
    /// Uploads data to Firestore at the specified collection.
    /// </summary>
    /// <param name="collection">Destination collection</param>
    /// <param name="data">Object to serialize</param>
    public void UploadData(string collection, object data, string documentId = null)
    {
        if (!_enableTelemetry) return;
        var idToken = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(idToken))
        {
            Debug.LogWarning("Cannot upload data: Auth token is not available.");
            return;
        }

        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{collection}";
        if (!string.IsNullOrEmpty(documentId))
            url += $"?documentId={documentId}&access_token={idToken}";
        else
            url += $"?access_token={idToken}";

        Debug.Log("Uploading data to: " + url);
        string jsonBody = FirestoreFormatUtility.WrapClass(data);
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

    private void EnqueueUpload(string url, string jsonBody)
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

            UnityWebRequest request = new UnityWebRequest(current.url, "POST");
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
        var dummySession = new SessionTelemetryData
        {
            StartTime = System.DateTime.UtcNow,
            EndTime = System.DateTime.UtcNow.AddMinutes(30), // Simulate a 30-minute session
            ConInitDur = 5
        };

        // Use a dummy session id for demonstration  
        string sessionId = "dummy_" + System.Guid.NewGuid().ToString("N");

        UploadData("sessions", dummySession, sessionId);
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
