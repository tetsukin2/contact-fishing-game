using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles global game data and game state transitions
/// </summary>
public class GameManager : Singleton<GameManager>
{
    // Game states
    public GameStartGameState GameStartState { get; private set; }
    public PlayingGameState PlayingState { get; private set; }
    public GameEndGameState GameEndState { get; private set; }
    public EndScoreGameState EndScoreState { get; private set; }
    public GameState CurrentState { get; private set; }

    [Header("Fish Scoring")]
    [SerializeField] private int _fishCaught = 0;
    [Min(1)]
    [SerializeField] private int _fishTotalToCatch = 10;

    [Space]
    [Header("Timings")]
    public float GameStartDuration = 3f;
    public float GameEndDuration = 1f;
    [HideInInspector] public float Timer;

    // Flag to check if a new best score was achieved
    // Prevents race condition at the start of the end score state
    public bool NewBestAchieved { get; private set; } = false; 

    /// <summary>
    /// Current game data being worked on by the game.
    /// </summary>
    public GameData CurrentGameData { get; private set; }

    /// <summary>
    /// Invoked when the score is processed and game saved
    /// </summary>
    public UnityEvent ScoreProcessed { get; private set; } = new();

    // State change events
    public UnityEvent<GameState> GameStateExited { get; private set; } = new();
    public UnityEvent<GameState> GameStateEntered { get; private set; } = new();

    /// <summary>
    /// Invoked when amount of fish caught is updated. Passes new fish caught as parameter.
    /// </summary>
    public UnityEvent<int> FishCaughtUpdated { get; private set; } = new(); // (fishCaught, totalFish)

    // Other Accessors
    public int FishCaught => _fishCaught;
    public int FishTotalToCatch => _fishTotalToCatch;

    protected override void OnAwake()
    {
        // Initialize states
        GameStartState = new GameStartGameState(this);
        PlayingState = new PlayingGameState(this);
        GameEndState = new GameEndGameState(this);
        EndScoreState = new EndScoreGameState(this);
    }

    private void Start()
    {
        // Reset ahead just in case
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(OnStartupLoad);

        // TODO: Load new data every time total fish to catch is updated
        CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
    }

    private void Update()
    {
        CurrentState?.Update();

        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteAllData();
            CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
            Debug.Log("Debug: Deleting Data");
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            InputDeviceManager.Instance.JoystickPressed.Invoke();
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            CurrentGameData.AddFish("milkfish");
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            CurrentGameData.AddFish("seabass");
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            CurrentGameData.AddFish("tilapia");
        }
    }

    /// <summary>
    /// When the starts for the first time, technically depends on first connection established
    /// </summary>
    private void OnStartupLoad()
    {
        Time.timeScale = 1f;

        // Reset for safety
        InputDeviceManager.SendBrailleASCII(0, 0, 0, 0);

        // Start
        TransitionToState(GameStartState);
    }

    /// <summary>
    /// Handles game state transitions
    /// </summary>
    /// <param name="newState">New game state to transition to</param>
    public void TransitionToState(GameState newState)
    {
        CurrentState?.Exit();
        GameStateExited.Invoke(CurrentState);
        CurrentState = newState;
        CurrentState?.Enter();
        GameStateEntered.Invoke(CurrentState);
    }

    /// <summary>
    /// Saves game data and checks if a new best score was achieved. Also invokes the ScoreProcessed event.
    /// </summary>
    public void ProcessScore()
    {
        if (Timer < CurrentGameData.BestTime)
        {
            CurrentGameData.BestTime = Timer;
            NewBestAchieved = true;
        }
        else
        {
            NewBestAchieved = false;
        }
        GameDataHandler.SaveGameData(CurrentGameData, "data", $"{FishTotalToCatch}");
        ScoreProcessed.Invoke();
    }

    public void AddFish()
    {
        _fishCaught++;
        FishCaughtUpdated.Invoke(FishCaught);
        if (_fishCaught >= _fishTotalToCatch)
        {
            TransitionToState(GameEndState);
        }
    }

    public void ResetFish()
    {
        _fishCaught = 0;
        FishCaughtUpdated.Invoke(FishCaught);
    }

    public void DeleteData()
    {
        GameDataHandler.DeleteAllData();
        CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
        Debug.Log("Debug: Deleting Data");
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

}
