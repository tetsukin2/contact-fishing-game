using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    public GameData CurrentGameData { get; private set; }

    private readonly UnityEvent<int> _gameStarting = new(); // (start state)
    //private readonly UnityEvent _endScoreShowed = new();
    private readonly UnityEvent _newBestScoreReached = new();
    private readonly UnityEvent<GameState> _gameStateUpdated = new();
    private readonly UnityEvent<int, int> _fishCaughtUpdated = new(); // (fishCaught, totalFish)

    public int FishCaught => _fishCaught;
    public int FishTotalToCatch => _fishTotalToCatch;

    // Can see references (on VS at least) if doing it this way
    public UnityEvent<int> GameStarting => _gameStarting;
    //public UnityEvent EndScoreShowed => _endScoreShowed;
    public UnityEvent NewBestScoreReached => _newBestScoreReached;
    public UnityEvent<GameState> GameStateUpdated => _gameStateUpdated;
    public UnityEvent<int, int> FishCaughtUpdated => _fishCaughtUpdated; // (fishCaught, totalFish)

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
        // TODO: Load new data every time total fish to catch is updated
        CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => Time.timeScale = 1f);
        Time.timeScale = 0f;

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
    }

    public void TransitionToState(GameState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        GameStateUpdated.Invoke(CurrentState); // score kinda needs to be initialized later in enter
        CurrentState?.Enter();
    }

    public void AddFish()
    {
        // Only add fish if the game if we playing
        if (CurrentState != PlayingState) return;
        _fishCaught++;
        FishCaughtUpdated.Invoke(FishCaught, FishTotalToCatch);
        if (_fishCaught >= _fishTotalToCatch)
        {
            TransitionToState(GameEndState);
        }
    }

    public void ResetFish()
    {
        _fishCaught = 0;
        FishCaughtUpdated.Invoke(FishCaught, FishTotalToCatch);
    }

    public void ResetTimer()
    {
        Timer = 0f;
    }

    //public void CompleteLevel(string path, string fileName, bool endOfArea)
    //{
    //    gameWinMenu.SetActive(true);
    //    nextLevelButton.gameObject.SetActive(!endOfArea);
    //    endOfAreaText.SetActive(endOfArea);
    //    if (timer < BestTime)
    //    {
    //        BestTime = timer;
    //        TGData.SaveLevelData(BestTime, path, fileName);
    //        newBestObject.SetActive(true);
    //    }
    //    gameClearTime.text = TGData.ConvertToTimeFormat(timer);
    //    bestClearTime.text = TGData.ConvertToTimeFormat(BestTime);
    //}

    ////toggle pause
    //public void TogglePause()
    //{
    //    if (gameState != GameStateName.PLAYING && gameState != GameStateName.PAUSED)
    //        return;

    //    if (!gameFrozen)
    //    {
    //        pause.Invoke();
    //        GameFrozen = true;
    //        gameState = GameStateName.PAUSED;
    //    }
    //    else
    //    {
    //        GameFrozen = false;
    //        gameState = GameStateName.PLAYING;
    //    }
    //    UIManager.Instance.pauseMenu.SetActive(gameFrozen);
    //}

    public static void QuitGame()
    {
        Application.Quit();
    }

}
