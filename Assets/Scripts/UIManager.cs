using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening.Core.Easing;

public class UIManager : MonoBehaviour
{
    //// Singleton instance of the ui manager
    //public static UIManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GUIPanel _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;

    //[SerializeField]
    //public float BestTime = float.MaxValue;

    [Space]
    [Header("Game End Screen")]
    [SerializeField] private GUIPanel _gameEndMenu;
    [SerializeField] private TextMeshProUGUI _gameEndSessionText;
    [SerializeField] private Color _gameEndBestTextColorNormal;
    [SerializeField] private Color _gameEndBestTextColorNew;
    [SerializeField] private TextMeshProUGUI _gameEndBestText;
    //public GameObject pauseMenu;
    //public GameObject gameOverMenu;
    //public GameObject gameWinMenu;
    //public Button nextLevelButton;
    //public GameObject endOfAreaText;
    //public GameObject timerObject;
    //public GameObject newBestObject;

    //private void Awake()
    //{
    //    // Ensure only one instance of the GameManager exists
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    //private void Start()
    //{
    //    HideUI();
    //}

    private void Start()
    {
        // Loading screen things
        InputDeviceManager.Instance.ConnectionStatusLog.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => _loadingScreen.Show(false));

        GameManager.Instance.FishCaughtUpdated.AddListener(OnFishCaughtUpdated);
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
        GameManager.Instance.GameEnded.AddListener(OnGameEnd);
        GameManager.Instance.NewBestScoreReached.AddListener(OnNewBestScoreReached);

        // Initialize the fish caught number text
        _fishCaughtNumberText.text = $"{GameManager.Instance.FishCaught}/{GameManager.Instance.FishTotalToCatch}";

        _gameEndMenu.Show(false);
    }

    private void Update()
    {
        //only run if timer object exists
        if (_timerText)
            _timerText.text = GameDataHandler.ConvertToTimeFormat(GameManager.Instance.timer);
    }

    private void OnFishCaughtUpdated()
    {
        _fishCaughtNumberText.text = $"{GameManager.Instance.FishCaught}/{GameManager.Instance.FishTotalToCatch}";
    }

    private void OnGameStateUpdated()
    {
        // Set visibility of game end menu
        _gameEndMenu.gameObject.SetActive(GameManager.Instance.CurrentGameState == GameManager.GameState.GAME_END);
    }

    private void OnGameEnd()
    {
        _gameEndMenu.Show(true);
        _gameEndSessionText.text = $"Nice Haul! {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.timer)}";
        _gameEndBestText.color = _gameEndBestTextColorNormal;
        _gameEndBestText.text = $"Can you top your best of {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.CurrentGameData.BestTime)}?";
    }

    private void OnNewBestScoreReached()
    {
        _gameEndBestText.color = _gameEndBestTextColorNew;
        _gameEndBestText.text = "New personal best!";
    }

    //public void Resume()
    //{
    //    GameManager.Instance.TogglePause();
    //}

    //public void LoadNextLevel()
    //{
    //    SceneLoadManager.Instance.LoadNextLevel();
    //}

    //public void Restart()
    //{
    //    SceneLoadManager.Instance.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    //}

    //public void ReturnToMenu()
    //{
    //    SceneLoadManager.Instance.ReturnToMenu();
    //}

    //public void hideTimer()
    //{
    //    timerObject.SetActive(false);
    //}

    //public void showTimer()
    //{
    //    timerObject.SetActive(true);
    //}

    //public void resetTimer()
    //{
    //    timer = 0f;
    //}

    //public void HideUI()
    //{
    //    timerObject.SetActive(false);
    //    pauseMenu.SetActive(false);
    //    gameOverMenu.SetActive(false);
    //    gameWinMenu.SetActive(false);
    //    newBestObject.SetActive(false);
    //}






}

