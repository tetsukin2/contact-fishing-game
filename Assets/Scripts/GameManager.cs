using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles gameplay game data and game state transitions
/// </summary>
public class GameManager : Singleton<GameManager>
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

    // Game states
    public GameStartGameState GameStartState { get; private set; }
    public PlayingGameState PlayingState { get; private set; }
    public GameEndGameState GameEndState { get; private set; }
    public EndScoreGameState EndScoreState { get; private set; }
    public GameState CurrentState { get; private set; }

    // State change events
    public UnityEvent<GameState> GameStateExited { get; private set; } = new();
    public UnityEvent<GameState> GameStateEntered { get; private set; } = new();

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
        GameStartState = new GameStartGameState(this);
        PlayingState = new PlayingGameState(this);
        GameEndState = new GameEndGameState(this);
        EndScoreState = new EndScoreGameState(this);
    }

    // Get everything registered first before setting up
    protected override void OnSetup() 
    {
        InputDeviceManager.Instance.BLEDevice.RunWhenConnected(SetupGame);
    }

    private void Update()
    {
        CurrentState?.Update();

        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteGameData();
            //GameDataHandler.CurrentGameData = GameDataHandler.LoadGameData("data", $"{_fishTotalToCatch}");
            Debug.Log("Debug: Deleting Data");
        }
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
    public void TransitionToState(GameState newState)
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

    public static void QuitGame()
    {
        Application.Quit();
    }

}
