using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Singleton instance of the game manager
    public static GameManager Instance { get; private set; }

    [Space]

    public float timer;

    

    [Min(1)]
    [SerializeField] private int _fishTotalToCatch = 10;
    [SerializeField] private int _fishCaught = 0;

    //public UnityEvent death;
    //public UnityEvent pause;

    //[SerializeField]
    //private InputAction pauseAction;

    //private bool gameFrozen = false;

    public enum GameState
    {
        MAIN_MENU,
        PLAYING,
        PAUSED,
        GAME_END,
        TRANSITION
    }

    [SerializeField] private GameState _currentGameState = GameState.PLAYING;

    //public bool GameFrozen
    //{
    //    get { return gameFrozen; }
    //    set
    //    {
    //        if (value)
    //        {
    //            gameFrozen = true;
    //            Time.timeScale = 0f;
    //        }
    //        else
    //        {
    //            gameFrozen = false;
    //            Time.timeScale = 1f;
    //        }
    //    }

    //}
    public GameData CurrentGameData { get; private set; }

    private UnityEvent _gameStateUpdated = new();
    private UnityEvent _gameEnded = new();
    private UnityEvent _newBestScoreReached = new();
    private UnityEvent _fishCaughtUpdated = new();

    // Can see references (on VS at least) if doing it this way
    public UnityEvent FishCaughtUpdated => _fishCaughtUpdated;
    public UnityEvent GameStateUpdated => _gameStateUpdated;
    public UnityEvent GameEnded => _gameEnded;
    public UnityEvent NewBestScoreReached => _newBestScoreReached;

    public GameState CurrentGameState
    {
        get => _currentGameState;
        private set
        {
            _currentGameState = value;
            _gameStateUpdated.Invoke();
        }
    }
    
    public int FishTotalToCatch => _fishTotalToCatch;
    public int FishCaught => _fishCaught;

    public bool IsPlaying
    {
        get { return _currentGameState == GameState.PLAYING; }
    }

    //public bool IsTransitioning
    //{
    //    get { return gameState == GameState.TRANSITION; }
    //}

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
        }
    }

    private void Start()
    {
        // TODO: Load new data every time total fish to catch is updated
        CurrentGameData = GameDataHandler.GetGameData("data", $"{_fishTotalToCatch}");
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => Time.timeScale = 1f);
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (_currentGameState == GameState.PLAYING) timer += Time.deltaTime;

        // Testing
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddFish();
            Debug.Log("Debug: Adding Fish");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteAllData();
            Debug.Log("Debug: Deleting Data");
        }

    }

    public void AddFish()
    {
        _fishCaught++;
        FishCaughtUpdated.Invoke();
        if (_fishCaught >= _fishTotalToCatch)
        {
            CurrentGameState = GameState.GAME_END;
            OnGameEnd();
        }
    }

    private void OnGameEnd()
    {
        bool newBest = false;
        if (timer < CurrentGameData.BestTime)
        {
            CurrentGameData.BestTime = timer;
            GameDataHandler.SaveGameData(CurrentGameData, "data", $"{_fishTotalToCatch}");
            newBest = true;
        }
        // Logic must complete before anything else can act
        GameEnded.Invoke();
        // Ensure everything else has acted before announcing a new best
        if (newBest) _newBestScoreReached.Invoke();
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

    //void GameOver()
    //{
    //    gameState = GameState.GAME_OVER;
    //    UIManager.Instance.hideTimer();
    //    GameFrozen = true;
    //    UIManager.Instance.gameOverMenu.SetActive(true);
    //}

    ////toggle pause
    //public void TogglePause()
    //{
    //    if (gameState != GameState.PLAYING && gameState != GameState.PAUSED)
    //        return;

    //    if (!gameFrozen)
    //    {
    //        pause.Invoke();
    //        GameFrozen = true;
    //        gameState = GameState.PAUSED;
    //    }
    //    else
    //    {
    //        GameFrozen = false;
    //        gameState = GameState.PLAYING;
    //    }
    //    UIManager.Instance.pauseMenu.SetActive(gameFrozen);
    //}

    public static void QuitGame()
    {
        Application.Quit();
    }

}
