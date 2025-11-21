using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Auth;
using Firebase.Extensions;
using SimpleJSON;

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
        public string authToken;
        public Action<bool, string> onComplete;
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
    public void PostData(string collection, object data, string documentId = null, Action<bool, string> onComplete = null)
    {
        if (!_enableTelemetry) return;

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("Cannot upload data: No user logged in.");
            return;
        }

        user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
        {
            if (tokenTask.IsCanceled || tokenTask.IsFaulted)
            {
                Debug.LogWarning("Failed to get Firebase ID token: " + tokenTask.Exception);
                return;
            }

            string idToken = tokenTask.Result;

        Debug.Log("User is: " + FirebaseAuth.DefaultInstance.CurrentUser?.UserId);
        Debug.Log("Access Token: " + idToken.Substring(0, 20));

            string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{collection}";
            if (!string.IsNullOrEmpty(documentId))
                url += $"?documentId={documentId}";

            Debug.Log("Uploading data to: " + url);
            string jsonBody = FirestoreFormatUtility.WrapClass(data);
            EnqueueUpload(url, jsonBody, "POST", idToken, onComplete);
        });
    }

    // public void PostData(string collection, object data, string documentId = null)
    // {
    //     if (!_enableTelemetry) return;
    //     var idToken = FirebaseConnectionHandler.Instance.CurrentAuthToken;
    //     if (string.IsNullOrEmpty(idToken))
    //     {
    //         Debug.LogWarning("Cannot upload data: Auth token is not available.");
    //         return;
    //     }

    //     string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{collection}";
    //     if (!string.IsNullOrEmpty(documentId))
    //         url += $"?documentId={documentId}&access_token={idToken}";
    //     else
    //         url += $"?access_token={idToken}";

    //     Debug.Log("Uploading data to: " + url);
    //     string jsonBody = FirestoreFormatUtility.WrapClass(data);
    //     EnqueueUpload(url, jsonBody, "POST");
    // }

    /// <summary>
    /// Patches data to an existing Firestore document.
    /// </summary>
    public void PatchData(string documentPath, object data)
    {
        if (!_enableTelemetry) return;

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("Cannot patch data: No user logged in.");
            return;
        }

        user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
        {
            if (tokenTask.IsCanceled || tokenTask.IsFaulted)
            {
                Debug.LogWarning("Failed to get Firebase ID token: " + tokenTask.Exception);
                return;
            }

            string idToken = tokenTask.Result;

            string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{documentPath}?access_token={idToken}";
            Debug.Log("Patching data to: " + url);
            //string jsonBody = FirestoreFormatUtility.WrapClass(data);
            string jsonBody = "{\"fields\":" + FirestoreFormatUtility.WrapAsFieldsOnly(data) + "}";

            EnqueueUpload(url, jsonBody, "PATCH", idToken);
        });
    }

    // public void GetGameplayConfig(string userId, Action<Dictionary<string, object>> onSuccess, Action onNotFound = null)
    // {
    //     FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
    //     if (user == null)
    //     {
    //         Debug.LogWarning("No user signed in.");
    //         return;
    //     }

    //     user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
    //     {
    //         if (tokenTask.IsCanceled || tokenTask.IsFaulted)
    //         {
    //             Debug.LogWarning("Failed to get Firebase ID token: " + tokenTask.Exception);
    //             return;
    //         }

    //         string idToken = tokenTask.Result;
    //         string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/gameplay_configs/{userId}";

    //         UnityWebRequest request = UnityWebRequest.Get(url);
    //         request.SetRequestHeader("Authorization", $"Bearer {idToken}");

    //         var operation = request.SendWebRequest();
    //         operation.completed += _ =>
    //         {
    //             if (request.result == UnityWebRequest.Result.Success)
    //             {
    //                 string json = request.downloadHandler.text;
    //                 var doc = JsonUtility.FromJson<FirestoreDocumentWrapper>(json);

    //                 Debug.Log("Raw JSON from Firestore: " + json);

    //                 if (doc == null || doc.fields == null)
    //                 {
    //                     Debug.LogError("Failed to deserialize gameplay config document.");
    //                     onNotFound?.Invoke();
    //                     return;
    //                 }

    //                 var configData = FirestoreFormatUtility.Unwrap(doc);
    //                 onSuccess?.Invoke(configData);
    //             }
    //             else if (request.responseCode == 404)
    //             {
    //                 Debug.Log("Gameplay config not found.");
    //                 onNotFound?.Invoke();
    //             }
    //             else
    //             {
    //                 Debug.LogError("GET gameplay config failed: " + request.downloadHandler.text);
    //             }
    //         };
    //     });
    // }

    public void GetGameplayConfig(string userId, Action<Dictionary<string, object>> onSuccess, Action onNotFound = null)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("No user signed in.");
            return;
        }

        user.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
        {
            if (tokenTask.IsCanceled || tokenTask.IsFaulted)
            {
                Debug.LogWarning("Failed to get Firebase ID token: " + tokenTask.Exception);
                return;
            }

            string idToken = tokenTask.Result;
            string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/gameplay_configs/{userId}";

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {idToken}");

            var operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var json = request.downloadHandler.text;
                    var root = JSON.Parse(json);
                    var fields = root["fields"];

                    if (fields == null)
                    {
                        Debug.LogError("Failed to deserialize gameplay config document.");
                        onNotFound?.Invoke();
                        return;
                    }

                    var result = new Dictionary<string, object>();

                    foreach (KeyValuePair<string, JSONNode> kvp in fields)
                    {
                        string key = kvp.Key;
                        JSONNode valueObj = kvp.Value;

                        if (valueObj.HasKey("integerValue"))
                            result[key] = int.Parse(valueObj["integerValue"].Value);
                        else if (valueObj.HasKey("doubleValue"))
                            result[key] = float.Parse(valueObj["doubleValue"].Value);
                        else if (valueObj.HasKey("stringValue"))
                            result[key] = valueObj["stringValue"].Value;
                        else if (valueObj.HasKey("arrayValue"))
                        {
                            var list = new List<string>();
                            var array = valueObj["arrayValue"]["values"];
                            if (array != null)
                            {
                                foreach (JSONNode item in array.AsArray)
                                    list.Add(item["stringValue"].Value);
                            }
                            result[key] = list;
                        }
                        else
                        {
                            Debug.LogWarning($"Unhandled Firestore value type for key '{key}': {valueObj}");
                        }
                    }


                    onSuccess?.Invoke(result);
                    Debug.Log("[GET CONFIG] Gameplay config parsed and returned successfully.");

                }
                else if (request.responseCode == 404)
                {
                    Debug.Log("Gameplay config not found.");
                    onNotFound?.Invoke();
                }
                else
                {
                    Debug.LogError("GET gameplay config failed: " + request.downloadHandler.text);
                }
            };
        });
    }


    // public void PatchData(string documentPath, object data)
    // {
    //     if (!_enableTelemetry) return;
    //     var idToken = FirebaseConnectionHandler.Instance.CurrentAuthToken;
    //     if (string.IsNullOrEmpty(idToken))
    //     {
    //         Debug.LogWarning("Cannot patch data: Auth token is not available.");
    //         return;
    //     }

    //     string url = $"https://firestore.googleapis.com/v1/projects/contactreelease/databases/(default)/documents/{documentPath}?access_token={idToken}";
    //     Debug.Log("Patching data to: " + url);
    //     string jsonBody = FirestoreFormatUtility.WrapClass(data);
    //     EnqueueUpload(url, jsonBody, "PATCH");
    // }

    private void EnqueueUpload(string url, string jsonBody, string method, string authToken, Action<bool, string> onComplete = null)
    {
        UploadQueueItem item = new() { method = method, url = url, jsonBody = jsonBody,  authToken = authToken, onComplete = onComplete};
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
            //request.SetRequestHeader("Authorization", $"Bearer {FirebaseConnectionHandler.Instance.CurrentAuthToken}");
            request.SetRequestHeader("Authorization", $"Bearer {current.authToken}");

            Debug.Log($"Sending {current.method} to {current.url}");
            Debug.Log($"Payload: {current.jsonBody}");

            yield return request.SendWebRequest();

            Debug.Log($"Result: {request.result}, Response Code: {request.responseCode}");
            Debug.Log($"Response Body: {request.downloadHandler.text}");
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload success: " + current.url);
                uploadQueue.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("Upload failed, will retry: " + current.url);
                Debug.LogError($"Upload failed ({request.responseCode}): {request.downloadHandler.text}");

                if (request.responseCode == 409)
                {
                    Debug.Log("[UPLOAD] Document already exists. Calling onComplete with 'already exists' notice.");
                    current.onComplete?.Invoke(false, "ALREADY_EXISTS");

                    // Remove the item since this is not a fatal error.
                    uploadQueue.RemoveAt(0);
                    continue;
                }

                current.onComplete?.Invoke(false, request.downloadHandler.text);

                break; // Exit and retry later
            }
        }

        isUploading = false;
    }
}
