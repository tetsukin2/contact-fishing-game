using UnityEngine;
using Firebase.Auth;
using Firebase;
using System;
using System.Collections.Generic;

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
    // public void OnSessionStart()
    // {
    //     Debug.Log("Starting session...");
    //     SessionData = new SessionTelemetryData()
    //     {
    //         UserID = FirebaseAuth.DefaultInstance.CurrentUser?.UserId,
    //         StartTime = System.DateTime.Now,
    //         EndTime = System.DateTime.MinValue, // nonfunctional for now
    //         ConInitDur = null // Will be set when connection is established
    //     };
    //     InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
    // }

    private void Start()
    {
        //TryPreloadGameplayConfig();

        FirebaseConnectionHandler.Instance.SignInSuccess.AddListener(() =>
        {
            TryPreloadGameplayConfig();
        });
    }

    private void TryPreloadGameplayConfig()
    {
        if (FirebaseConnectionHandler.Instance.AuthInitialized &&
            FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

            Debug.Log($"[PRELOAD] Checking for gameplay config for user: {userId}");

            FirebaseUploadHandler.Instance.GetGameplayConfig(userId,
                onSuccess: firestoreData =>
                {
                    Debug.Log("[PRELOAD] GameplayConfig found in Firestore, applying...");
                    GameplayConfigLoader.ApplyFromFirestore(ResourceSystem.Instance.GameplayConfig, firestoreData);

                    // Log each field from the config to verify
                    var config = ResourceSystem.Instance.GameplayConfig;
                    Debug.Log($"[PRELOAD] Config Applied:\n" +
                            $"  RotateUpAngle: {config.RotateUpAngle}\n" +
                            $"  RotateDownAngle: {config.RotateDownAngle}\n" +
                            $"  RollRightAngle: {config.RollRightAngle}\n" +
                            $"  RollLeftAngle: {config.RollLeftAngle}\n" +
                            $"  SideRotateUpAngle: {config.SideRotateUpAngle}\n" +
                            $"  SideRotateDownAngle: {config.SideRotateDownAngle}\n" +
                            $"  BobberSensitivity: {config.BobberSensitivity}\n" +
                            $"  BaitPreparationSteps: {config.BaitPreparationSteps}\n" +
                            $"  ReelTotalProgress: {config.ReelTotalProgress}\n" +
                            $"  ReelProgressAmount: {config.ReelProgressAmount}\n" +
                            $"  ReelDecayRate: {config.ReelDecayRate}\n" +
                            $"  BraillePatternInterval: {config.BraillePatternInterval}\n" +
                            $"  FishTotalToCatch: {config.FishTotalToCatch}\n" +
                            $"  DiscoveredFish.Count: {config.DiscoveredFish?.Count}\n" +
                            $"  ReelActionSequence: {string.Join(", ", config.ReelActionSequence ?? new())}");
                },
                onNotFound: () =>
                {
                    Debug.Log("[PRELOAD] No config found. Uploading local default...");

                    FirebaseUploadHandler.Instance.PostData(
                        "gameplay_configs",
                        ResourceSystem.Instance.GameplayConfig,
                        userId,
                        onComplete: (success, errorMsg) =>
                        {
                            if (!success && errorMsg.Contains("ALREADY_EXISTS"))
                            {
                                Debug.LogWarning("[PRELOAD] Upload failed: already exists. Fetching instead...");

                                FirebaseUploadHandler.Instance.GetGameplayConfig(userId,
                                    onSuccess: firestoreData =>
                                    {
                                        Debug.Log("[PRELOAD] Re-fetched config after upload conflict. Applying...");
                                        GameplayConfigLoader.ApplyFromFirestore(ResourceSystem.Instance.GameplayConfig, firestoreData);
                                    },
                                    onNotFound: () =>
                                    {
                                        Debug.LogError("[PRELOAD] Unexpected: config not found even after upload conflict.");
                                    });
                            }
                            else if (!success)
                            {
                                Debug.LogError($"[PRELOAD] Upload failed: {errorMsg}");
                            }
                            else
                            {
                                Debug.Log("[PRELOAD] Upload successful.");
                            }
                        });
                });
        }
        else
        {
            Debug.LogWarning("[PRELOAD] Firebase not ready or user not signed in.");
        }
    }



    public void OnSessionStart()
    {
        Debug.Log("Starting session...");

        FirebaseConnectionHandler.Instance.SignInSuccess.AddListener(() =>
        {
            var userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
            Debug.Log("User ID after sign-in: " + userId);

            SessionData = new SessionTelemetryData()
            {
                UserID = userId,
                StartTime = System.DateTime.Now,
                EndTime = System.DateTime.MinValue,
                ConInitDur = null
            };


            // Load config BEFORE connecting BLE
            FirebaseUploadHandler.Instance.GetGameplayConfig(userId,
                onSuccess: firestoreData =>
                {
                    Debug.Log("[SESSION START] Fetched config. Applying...");
                    GameplayConfigLoader.ApplyFromFirestore(ResourceSystem.Instance.GameplayConfig, firestoreData);

                    // Now safe to start BLE connection
                    Debug.Log("[SESSION START] Starting BLE Connection after config loaded.");
                    InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
                },
                onNotFound: () =>
                {
                    Debug.LogWarning("[SESSION START] No config found. Using local config, starting BLE anyway.");
                    InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
                });

                //InputDeviceManager.Instance.BLEDevice.RunWhenConnected(OnConnectionEstablished);
        });

        if (FirebaseConnectionHandler.Instance.AuthInitialized &&
            FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseConnectionHandler.Instance.SignInSuccess.Invoke(); 
        }
    }


    private void OnConnectionEstablished()
    {
        SessionData.ConInitDur = (int)System.DateTime.Now.Subtract(SessionData.StartTime).TotalSeconds;
        Debug.Log($"Connection established. Initialization duration: {SessionData.ConInitDur} seconds");
    }
    
    public void OnSessionEnd()
    {
        if (SessionData == null) return;

        SessionData.EndTime = System.DateTime.Now;
        Debug.Log($"Session ended at {SessionData.EndTime}");

        FirebaseUploadHandler.Instance.PostData("sessions", SessionData);
    }
}
