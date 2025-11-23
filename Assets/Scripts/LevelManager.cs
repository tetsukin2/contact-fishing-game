using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Firebase.Auth;
using Firebase;

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

         _fishTotalToCatch = ResourceSystem.Instance.GameplayConfig.FishTotalToCatch;
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
            if (state == GameEndState && !hasUploadedGameTelemetry)
            {
                 hasUploadedGameTelemetry = true;

                _telemetryData.EndTime = System.DateTime.Now;
                _telemetryData.AverageActionsPerReel = FishingManager.Instance.ReelingState.ActionsPerReelList.Count > 0
                    ? (int)FishingManager.Instance.ReelingState.ActionsPerReelList.Average()
                    : 0;
                _telemetryData.AverageTimeTaken = ActionTelemetryHandler.Instance.GetAverageTimeTaken();
                _telemetryData.RepetitionCounts = ActionTelemetryHandler.Instance.GetRepetitionCounts();
                _telemetryData.MaxAngles = ActionTelemetryHandler.Instance.GetMaxAngles();
                _telemetryData.GameCompleted = true;

                Debug.Log("[TELEMETRY] GameTelemetryData:\n" + JsonUtility.ToJson(_telemetryData, true));
                FirebaseUploadHandler.Instance.PostData("games", _telemetryData);

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
    }

    /// <summary>
    /// When the starts for the first time, technically depends on first connection established
    /// </summary>
    private void SetupGame()
    {
        Time.timeScale = 1f;

        // Reset for safety
        InputDeviceManager.Instance.BrailleOutput.SendBrailleASCII(0, 0, 0, 0);

        // Start
        TransitionToState(GameStartState);
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

    /// <summary>
    /// TODO: Better one-time registration to successful upload for quitting
    /// </summary>
    public void QuitGame()
    {
        FirebaseUploadHandler.Instance.OnUploadQueueCompletedEvent.AddListener(() => Application.Quit());
        GameManager.Instance.OnSessionEnd();  // Properly finalize session
    }

}
