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

    //[SerializeField]
    //private TextMeshProUGUI gameClearTime;
    //[SerializeField]
    //private TextMeshProUGUI bestClearTime;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;

    //[SerializeField]
    //public float highScore = float.MaxValue;

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
        // Subscribe to the FishCaughtUpdated event
        GameManager.Instance.FishCaughtUpdated.AddListener(OnFishCaughtUpdated);
        // Initialize the fish caught number text
        _fishCaughtNumberText.text = $"{GameManager.Instance.FishCaught}/{GameManager.Instance.FishTotalToCatch}";
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

    //public void CompleteLevel(string path, string fileName, bool endOfArea)
    //{
    //    gameWinMenu.SetActive(true);
    //    nextLevelButton.gameObject.SetActive(!endOfArea);
    //    endOfAreaText.SetActive(endOfArea);
    //    if (timer < highScore)
    //    {
    //        highScore = timer;
    //        TGData.SaveLevelData(highScore, path, fileName);
    //        newBestObject.SetActive(true);
    //    }
    //    gameClearTime.text = TGData.ConvertToTimeFormat(timer);
    //    bestClearTime.text = TGData.ConvertToTimeFormat(highScore);
    //}




}

