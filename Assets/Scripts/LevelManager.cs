using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Firebase.Auth;
using Firebase;
using System.Collections.Generic;

/// <summary>
/// Handles level state transitions and other level-level stuff
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("Level Info")]
    [SerializeField] private string _levelName;

    [Space]
    [Header("Fish Scoring")]
    [SerializeField] private int _fishCaught = 0;
    [Min(1)]
    [SerializeField] private int _fishTotalToCatch = 10;

    [Space]
    [Header("Timings")]
    public float GameStartDuration = 3f;
    public float GameEndDuration = 1f;
    [HideInInspector] public float Timer;

    public bool IsGamePaused { get; private set; } = false;

    private GameTelemetryData _telemetryData;

    private UnityEvent<bool> _gamePaused = new();

    public UnityEvent<bool> GamePaused => _gamePaused;

    // Game states
    public GameStartLevelState GameStartState { get; private set; }
    public PlayingLevelState PlayingState { get; private set; }
    public GameEndLevelState GameEndState { get; private set; }
    public EndScoreLevelState EndScoreState { get; private set; }
    public LevelState CurrentState { get; private set; }

    // State change events
    public UnityEvent<LevelState> GameStateExited { get; private set; } = new();
    public UnityEvent<LevelState> GameStateEntered { get; private set; } = new();

    public TactileDiscriminationState TactileDiscriminationState { get; private set; }

    /// <summary>
    /// Invoked when amount of fish caught is updated. Passes new fish caught as parameter.
    /// </summary>
    public UnityEvent<int> FishCaughtUpdated { get; private set; } = new();

    // Other Accessors, this makes it easy to see references via Visual Studio in addition to being safe getters
    public string LevelName => _levelName;
    public int FishCaught => _fishCaught;
    public int FishTotalToCatch => _fishTotalToCatch;

    private bool hasUploadedGameTelemetry = false;

    protected override void OnAwake()
    {
        // Setup when transition complete
        //SceneSwitchHandler.Instance.SceneTransitionComplete += SetupGame;

        // Initialize states
        GameStartState = new GameStartLevelState(this);
        PlayingState = new PlayingLevelState(this);
        GameEndState = new GameEndLevelState(this);
        EndScoreState = new EndScoreLevelState(this);
        TactileDiscriminationState = new TactileDiscriminationState(this);
    }

    protected override void OnRegister()
    {
        Debug.Log("Registering Button0Pressed listener");
        InputDeviceManager.Instance.ButtonInput.Button0Pressed.AddListener(() => SetGamePaused(!IsGamePaused));
    }

    // Get everything registered first before setting up
    protected override void OnSetup()
    {
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(SetupGame);

        _fishTotalToCatch = UserTestConfig.OverrideFishTotalToCatch > 0
            ? UserTestConfig.OverrideFishTotalToCatch
            : ResourceSystem.Instance.GameplayConfig.FishTotalToCatch;

        // SetGamePaused(false); // Ensure game is not paused at start
        _gamePaused.Invoke(false); // Manual invoke cuz of pause safeguards

        // Telemetry
        ActionTelemetryHandler.Instance.ClearAllActionData(); // Work on a clean slate
        _telemetryData = new GameTelemetryData
        {
            UserID = FirebaseAuth.DefaultInstance.CurrentUser?.UserId,
            StageID = _levelName,
            GameCompleted = false,
            FishCatchRequirement = _fishTotalToCatch,
        };

        GameStateExited.AddListener((state) =>
        {
            if (state == GameStartState) _telemetryData.StartTime = System.DateTime.Now;
        });

        GameStateEntered.AddListener((state) =>
        {
            if (state == GameStartState)
            {
                hasUploadedGameTelemetry = false;
            }

            if (state == GameEndState)
            {
                AudioManager.Instance?.StopRodReelLoop();
                // optional:
                AudioManager.Instance?.PlayStageComplete();
                AudioManager.Instance?.StopGameplayBgm();
            }

            // Normal gameplay uploads on GameEndState.
            // User Test mode waits until the discrimination test is finished.
            if (state == GameEndState && !hasUploadedGameTelemetry)
            {
                if (!UserTestConfig.IsUserTestMode)
                {
                    UploadTelemetryNow();
                }
            }
        });
    }

    private void Update()
    {
        CurrentState?.Update();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.Invoke();
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameDataHandler.CurrentGameData.AddDiscoveredFish("milkfish");
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameDataHandler.CurrentGameData.AddDiscoveredFish("seabass");
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameDataHandler.CurrentGameData.AddDiscoveredFish("tilapia");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Shortcut: Jumping to Tactile Discrimination Test");

            UserTestConfig.IsUserTestMode = true;
            UserTestConfig.CurrentPhase = 3;
            UserTestConfig.HapticsEnabled = true;

            TransitionToState(TactileDiscriminationState);
        }
#endif
    }

    /// <summary>
    /// When the starts for the first time, technically depends on first connection established
    /// </summary>
    private void SetupGame()
    {
        Time.timeScale = 1f;

        // Re-apply fish requirement here too, in case User Test mode was toggled from UI before game start.
        _fishTotalToCatch = UserTestConfig.OverrideFishTotalToCatch > 0
            ? UserTestConfig.OverrideFishTotalToCatch
            : ResourceSystem.Instance.GameplayConfig.FishTotalToCatch;

        // Reset run state for safety
        ResetFish();

        // Reset for safety
        InputDeviceManager.Instance.BrailleOutput.SendBrailleASCII(0, 0, 0, 0);

        AudioManager.Instance?.PlayGameplayBgm();

        // Debug shortcut: open tactile discrimination test directly
        if (PlayerPrefs.GetInt("Debug_OpenTactileDiscriminationTest", 0) == 1)
        {
            PlayerPrefs.SetInt("Debug_OpenTactileDiscriminationTest", 0);
            PlayerPrefs.Save();

            TransitionToState(TactileDiscriminationState);
            return;
        }

        // Normal start
        TransitionToState(GameStartState);
    }

    public void StartGameFromUI()
    {
        SetupGame();
    }

    /// <summary>
    /// Handles game state transitions
    /// </summary>
    /// <param name="newState">New game state to _transitionAnimator to</param>
    public void TransitionToState(LevelState newState)
    {
        CurrentState?.Exit();
        GameStateExited.Invoke(CurrentState);
        CurrentState = newState;
        CurrentState?.Enter();
        GameStateEntered.Invoke(CurrentState);
    }

    /// <summary>
    /// Adds fish to the caught fish count.
    /// </summary>
    public void AddFish()
    {
        _fishCaught++;
        FishCaughtUpdated.Invoke(FishCaught);
        if (_fishCaught >= _fishTotalToCatch)
        {
            TransitionToState(GameEndState);
        }
    }

    /// <summary>
    /// Resets the fish caught count to 0.
    /// </summary>
    public void ResetFish()
    {
        _fishCaught = 0;
        FishCaughtUpdated.Invoke(FishCaught);
    }

    public void SetGamePaused(bool isPaused)
    {
        if (IsGamePaused == isPaused) return; // No change, do nothing
        if (!IsGamePaused && CurrentState != PlayingState) return; // Can only pause during gameplay

        IsGamePaused = isPaused;
        _gamePaused.Invoke(IsGamePaused);
        Time.timeScale = IsGamePaused ? 0f : 1f;
    }

    public void SetDiscriminationTelemetry(
        int score,
        int total,
        List<string> patterns,
        List<string> answers,
        List<bool> correctness)
    {
        // Keep your existing flat fields
        _telemetryData.DiscriminationScore = score;
        _telemetryData.DiscriminationTotal = total;
        _telemetryData.DiscriminationTrialPatterns = patterns;
        _telemetryData.DiscriminationUserAnswers = answers;
        _telemetryData.DiscriminationCorrectAnswers = correctness;

        // Nested object for cleaner Firebase structure
        _telemetryData.DiscriminationTest = new DiscriminationTestTelemetryData
        {
            Score = score,
            Total = total,
            TrialPatterns = patterns,
            UserAnswers = answers,
            CorrectAnswers = correctness
        };

        Debug.Log("=== SET DISCRIMINATION TELEMETRY ===");
        Debug.Log("Score: " + score);
        Debug.Log("Total: " + total);
        Debug.Log("Patterns count: " + (patterns != null ? patterns.Count : 0));
        Debug.Log("Answers count: " + (answers != null ? answers.Count : 0));
        Debug.Log("Correct count: " + (correctness != null ? correctness.Count : 0));

        if (patterns != null)
            Debug.Log("Patterns: " + string.Join(", ", patterns));

        if (answers != null)
            Debug.Log("Answers: " + string.Join(", ", answers));
    }

    public void UploadTelemetryNow()
    {
        if (hasUploadedGameTelemetry)
            return;

        hasUploadedGameTelemetry = true;

        _telemetryData.EndTime = System.DateTime.Now;
        _telemetryData.AverageActionsPerReel = FishingManager.Instance.ReelingState.ActionsPerReelList.Count > 0
            ? (int)FishingManager.Instance.ReelingState.ActionsPerReelList.Average()
            : 0;
        _telemetryData.AverageTimeTaken = ActionTelemetryHandler.Instance.GetAverageTimeTaken();
        _telemetryData.RepetitionCounts = ActionTelemetryHandler.Instance.GetRepetitionCounts();
        _telemetryData.MaxAngles = ActionTelemetryHandler.Instance.GetMaxAngles();
        _telemetryData.GameCompleted = true;
        _telemetryData.FishCatchRequirement = _fishTotalToCatch;

        Debug.Log("=== FINAL TELEMETRY BEFORE UPLOAD ===");
        Debug.Log("Score: " + _telemetryData.DiscriminationScore);
        Debug.Log("Total: " + _telemetryData.DiscriminationTotal);
        Debug.Log("Patterns count: " + (_telemetryData.DiscriminationTrialPatterns != null ? _telemetryData.DiscriminationTrialPatterns.Count : 0));
        Debug.Log("Answers count: " + (_telemetryData.DiscriminationUserAnswers != null ? _telemetryData.DiscriminationUserAnswers.Count : 0));
        Debug.Log("Correct count: " + (_telemetryData.DiscriminationCorrectAnswers != null ? _telemetryData.DiscriminationCorrectAnswers.Count : 0));

        if (_telemetryData.DiscriminationTrialPatterns != null)
            Debug.Log("Patterns: " + string.Join(", ", _telemetryData.DiscriminationTrialPatterns));

        if (_telemetryData.DiscriminationUserAnswers != null)
            Debug.Log("Answers: " + string.Join(", ", _telemetryData.DiscriminationUserAnswers));

        Debug.Log("[TELEMETRY] GameTelemetryData:\n" + JsonUtility.ToJson(_telemetryData, true));
        FirebaseUploadHandler.Instance.PostData("games", _telemetryData, null, (success, response) =>
        {
            Debug.Log("=== FIREBASE GAME TELEMETRY UPLOAD RESULT ===");
            Debug.Log("Success: " + success);
            Debug.Log("Response: " + response);
        });

        if (UserTestConfig.IsUserTestMode)
        {
            UserTestConfig.ResetToNormalMode();
        }
    }

    /// <summary>
    /// TODO: Better one-time registration to successful upload for quitting
    /// </summary>
    public void QuitGame()
    {
        FirebaseUploadHandler.Instance.OnUploadQueueCompletedEvent.AddListener(() => Application.Quit());
        GameManager.Instance.OnSessionEnd();  // Properly finalize session
    }
}