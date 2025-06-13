using UnityEngine;
using UnityEngine.Events;

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
        InputDeviceManager.Instance.ButtonInput.Button1Pressed.AddListener(() => SetGamePaused(!IsGamePaused));
    }

    // Get everything registered first before setting up
    protected override void OnSetup() 
    {
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(SetupGame);
        // SetGamePaused(false); // Ensure game is not paused at start
        _gamePaused.Invoke(false); // Manual invoke cuz of pause safeguards
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

    public static void QuitGame()
    {
        Application.Quit();
    }

}
