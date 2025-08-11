using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 
/// </summary>
public class FirebaseUploadHandler : Singleton<FirebaseUploadHandler>
{
    [System.Serializable]
    public class UploadQueueItem
    {
        public string method;
        public string url;
        public string jsonBody;
    }

    [SerializeField] private bool _enableTelemetry = true;

    private List<UploadQueueItem> uploadQueue = new();
    private bool isUploading = false;

    protected override void OnAwake()
    {
        StartCoroutine(ProcessQueue());
    }

    /// <summary>
    /// Starts processing the upload queue if not already uploading.
    /// </summary>
    public void StartUploadQueue()
    {
        if (!isUploading) StartCoroutine(ProcessQueue());
    }

    /// <summary>
    /// Uploads data to Firestore at the specified collection.
    /// </summary>
    public void PostData(string collection, object data, string documentId = null)
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
        EnqueueUpload(url, jsonBody, "POST");
    }

    /// <summary>
    /// Patches data to an existing Firestore document.
    /// </summary>
    public void PatchData(string documentPath, object data)
    {
        if (!_enableTelemetry) return;
        var idToken = FirebaseConnectionHandler.Instance.CurrentAuthToken;
        if (string.IsNullOrEmpty(idToken))
        {
            Debug.LogWarning("Cannot patch data: Auth token is not available.");
            return;
        }

        string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{documentPath}?access_token={idToken}";
        Debug.Log("Patching data to: " + url);
        string jsonBody = FirestoreFormatUtility.WrapClass(data);
        EnqueueUpload(url, jsonBody, "PATCH");
    }

    private void EnqueueUpload(string url, string jsonBody, string method)
    {
        UploadQueueItem item = new() { method = method, url = url, jsonBody = jsonBody };
        uploadQueue.Add(item);
        StartUploadQueue();
    }

    // Handle the upload queue processing from memory
    private IEnumerator ProcessQueue()
    {
        isUploading = true;

        while (uploadQueue.Count > 0)
        {
            UploadQueueItem current = uploadQueue[0];

            UnityWebRequest request = new UnityWebRequest(current.url, current.method);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(current.jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload success: " + current.url);
                uploadQueue.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("Upload failed, will retry: " + current.url);
                break; // Exit and retry later
            }
        }

        isUploading = false;
    }
}
