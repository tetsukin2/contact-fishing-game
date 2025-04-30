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

    public float timer;

    [Min(1)]
    [SerializeField] private int _fishTotalToCatch = 10;
    [SerializeField] private int _fishCaught = 0;

    //public UnityEvent death;
    //public UnityEvent pause;

    //[SerializeField]
    //private InputAction pauseAction;

    //private bool gameFrozen = false;

    //public enum GameState
    //{
    //    MAIN_MENU,
    //    PLAYING,
    //    PAUSED,
    //    GAME_WIN,
    //    GAME_OVER,
    //    TRANSITION
    //}

    //public GameState gameState = GameState.MAIN_MENU;

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
    public UnityEvent FishCaughtUpdated = new();

    public int FishTotalToCatch => _fishTotalToCatch;
    public int FishCaught => _fishCaught;

    //public bool IsPlaying
    //{
    //    get { return gameState == GameState.PLAYING; }
    //}

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

    //void Start()
    //{
    //    death.AddListener(GameOver);
    //}

    private void Update()
    {
        timer += Time.deltaTime;
    }

    public void AddFish()
    {
        _fishCaught++;
        FishCaughtUpdated.Invoke();
    }

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

    //public static void QuitGame()
    //{
    //    Application.Quit();
    //}

}
