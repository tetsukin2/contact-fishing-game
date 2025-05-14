using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles global game data and game state transitions
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance of the game manager
    public static GameManager Instance { get; private set; }

    // Game states
    public MainMenuGameState MainMenuState { get; private set; }
    public EncyclopediaGameState EncyclopediaState { get; private set; }
    public GameStartGameState GameStartState { get; private set; }
    public PlayingGameState PlayingState { get; private set; }
    public GameEndGameState GameEndState { get; private set; }
    public EndScoreGameState EndScoreState { get; private set; }
    public GameState CurrentState { get; private set; }

    [SerializeField] private int _fishCaught = 0;
    [Min(1)]
    [SerializeField] private int _fishTotalToCatch = 10;

    [Space]
    [Header("Timings")]
    public float GameStartDuration = 3f;
    public float GameEndDuration = 1f;
    [HideInInspector] public float Timer;

    /// <summary>
    /// Current game data being worked on by the game.
    /// </summary>
    public GameData CurrentGameData { get; private set; }

    public UnityEvent NewBestScoreReached { get; private set; } = new();
    public UnityEvent<GameState> GameStateExited { get; private set; } = new();
    public UnityEvent<GameState> GameStateEntered { get; private set; } = new();
    /// <summary>
    /// Invoked when amount of fish caught is updated. Passes new fish caught as parameter.
    /// </summary>
    public UnityEvent<int> FishCaughtUpdated { get; private set; } = new(); // (fishCaught, totalFish)

    public int FishCaught => _fishCaught;
    public int FishTotalToCatch => _fishTotalToCatch;

    private void Awake()
    {
        // Ensure only one instance of the GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistence
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize states
        MainMenuState = new MainMenuGameState(this);
        EncyclopediaState = new EncyclopediaGameState(this);
        GameStartState = new GameStartGameState(this);
        PlayingState = new PlayingGameState(this);
        GameEndState = new GameEndGameState(this);
        EndScoreState = new EndScoreGameState(this);
    }

    private void Start()
    {
        // Reset ahead just in case, remove when inversion is fixed
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() =>
        {
            InputDeviceManager.SendBrailleASCII(0, 0, 0, 0);
        });

        // TODO: Load new data every time total fish to catch is updated
        CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => Time.timeScale = 1f);

        // Start in the main menu state
        TransitionToState(MainMenuState);
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

    public void TransitionToState(GameState newState)
    {
        CurrentState?.Exit();
        GameStateExited.Invoke(CurrentState);
        CurrentState = newState;
        //GameStateUpdated.Invoke(CurrentState); // score kinda needs to be initialized later in enter
        CurrentState?.Enter();
        GameStateEntered.Invoke(CurrentState);
    }

    public void AddFish()
    {
        _fishCaught++;
        FishCaughtUpdated.Invoke(FishCaught);
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
