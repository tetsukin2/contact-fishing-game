using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SessionTelemetryData
{
    public string session_id;
    public float session_duration;
    public System.DateTime session_start_time;
    public string user_id;
    public float controller_connection_initialize_duration;
    public GameConfig config;
    
}

[System.Serializable]
public class GameConfig
{
    public int bobber_sensitivity;
    public float rotate_up_angle;
    public float rotate_down_angle;
    public float roll_right_angle;
    public float roll_left_angle;
    public int bait_preparation_steps;
    public int reel_total_progress;
    public int reel_progress_amount;
    public float reel_decay_rate;
    public List<string> reel_action_sequence;
    public float side_rotate_up_angle;
    public float side_rotate_down_angle;
    public float braille_pattern_interval;
}

[System.Serializable]
public class GameTelemetryData
{
    public string game_id;
    public string stage_id;
    public System.DateTime start_time;
    public bool is_replay;
    public bool game_completed;
    public int fish_catch_requirement;
    public Dictionary<string, float> average_time_taken;
    public float average_actions_per_reel;
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
            session_id = "dummy_session_id",
            session_duration = 12345,
            session_start_time = System.DateTime.UtcNow,
            user_id = "dummy_user",
            controller_connection_initialize_duration = 100,
            config = new GameConfig
            {
                bobber_sensitivity = 5,
                rotate_up_angle = 10.5f,
                rotate_down_angle = 8.2f,
                roll_right_angle = 15.0f,
                roll_left_angle = 14.0f,
                bait_preparation_steps = 3,
                reel_total_progress = 100,
                reel_progress_amount = 10,
                reel_decay_rate = 0.5f,
                reel_action_sequence = new List<string> { "pull", "release", "pull" },
                side_rotate_up_angle = 12.0f,
                side_rotate_down_angle = 9.0f,
                braille_pattern_interval = 1.5f
            }
        };

        UploadSessionData(dummySession.session_id, token, dummySession);
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
            game_id = "dummy_game_id",
            stage_id = "stage_1",
            start_time = System.DateTime.UtcNow,
            is_replay = false,
            game_completed = true,
            fish_catch_requirement = 5,
            average_time_taken = new Dictionary<string, float>
            {
               { "action1", 200 },
               { "action2", 500 },
            },
            average_actions_per_reel = 3.5f
        };

        // Use a dummy session id for demonstration
        UploadGameData("dummy_session_id", dummyRound.game_id, token, dummyRound);
    }
}
