using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //// Singleton instance of the ui manager
    //public static UIManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GUIPanel _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    [Header("Game Start Screen")]
    [SerializeField] private GameStartPanel _gameStartGUI;

    [Space]
    [Header("Gameplay")]
    [SerializeField] private GUIPanel _gameplayGUI;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;

    //[SerializeField]
    //public float BestTime = float.MaxValue;

    [Space]
    [Header("Game End Screen")]
    [SerializeField] private GUIPanel _gameEndGUI;
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
        // Start off with no game panels visible
        _gameStartGUI.Show(false);
        _gameplayGUI.Show(false);
        _gameEndGUI.Show(false);

        // Loading screen things
        _loadingScreen.Show(true);
        InputDeviceManager.Instance.ConnectionStatusLog.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => _loadingScreen.Show(false));        

        // Listen to GameManager's Ramblings
        GameManager.Instance.FishCaughtUpdated.AddListener(OnFishCaughtUpdated);
        GameManager.Instance.GameStarting.AddListener(OnGameStarting);
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
        GameManager.Instance.GameEnded.AddListener(OnGameEnd);
        GameManager.Instance.NewBestScoreReached.AddListener(OnNewBestScoreReached);

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

    private void OnGameStateUpdated()
    {
        // Set visibility of game end menu
        _gameEndGUI.Show(GameManager.Instance.CurrentGameState == GameManager.GameState.GAME_END);
        // Set visibility of game start menu
        _gameStartGUI.Show(GameManager.Instance.CurrentGameState == GameManager.GameState.GAME_START);
        // Set visibility of gameplay menu
        _gameplayGUI.Show(GameManager.Instance.CurrentGameState == GameManager.GameState.PLAYING);
    }

    private void OnGameStarting(int phase)
    {
        // Show the appropriate phase of the game start menu
        switch (phase)
        {
            case 0:
                _gameStartGUI.OnReady();
                break;
            case 1:
                _gameStartGUI.OnSet();
                break;
            case 2:
                _gameStartGUI.OnFish();
                break;
        }
    }

    // Setup End UI
    private void OnGameEnd()
    {
        _gameEndGUI.Show(true);
        _gameEndSessionText.text = $"Nice Haul! {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.timer)}";
        _gameEndBestText.color = _gameEndBestTextColorNormal;
        _gameEndBestText.text = $"Can you top your best of {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.CurrentGameData.BestTime)}?";
    }

    // Setup New Best details
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






}

