using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GUIPanel _loadingScreen;
    [SerializeField] private TextMeshProUGUI _loadingText;

    [Space]
    [Header("Main Menu")]
    [SerializeField] private GUIPanel _mainMenuSelectGUI;
    [SerializeField] private GUIPanel _mainMenuGUI;
    [SerializeField] private InputPromptPanel _mainMenuPrompt; // Reference to the input prompt panel
    [SerializeField] private InputPromptPanel _mainMenuSecondPrompt; // Reference to the input prompt panel
    [SerializeField] private InputPrompt _mainMenuInput; // List of sprites for input prompts
    [SerializeField] private InputPrompt _mainMenuSecondInput; // List of sprites for input prompts

    [Space]
    [Header("Encyclopedia")]
    [SerializeField] private GUIPanel _encyclopediaGUI;
    [SerializeField] private JoystickCursor _joystickCursor;

    [Space]
    [Header("Game Start Screen")]
    [SerializeField] private GameStartPanel _gameStartGUI;

    [Space]
    [Header("Gameplay")]
    [SerializeField] private GUIPanel _gameplayGUI;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _fishCaughtNumberText;

    [Space]
    [Header("Game End Screen")]
    [SerializeField] private GUIPanel _gameEndGUI;

    [Space]
    [Header("End Score Screen")]
    [SerializeField] private GUIPanel _endScoreGUI;
    [SerializeField] private GUIPanel _gameEndSelectGUI;
    [SerializeField] private TextMeshProUGUI _gameEndSessionText;
    [SerializeField] private Color _gameEndBestTextColorNormal;
    [SerializeField] private Color _gameEndBestTextColorNew;
    [SerializeField] private TextMeshProUGUI _gameEndBestText;

    public JoystickCursor JoystickCursor => _joystickCursor;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Sync with the current game state
        OnGameStateUpdated(GameManager.Instance.CurrentState);

        // Loading screen things
        _loadingScreen.Show(true);
        InputDeviceManager.Instance.ConnectionStatusLog.AddListener((string message) => _loadingText.SetText(message));
        InputDeviceManager.Instance.CharacteristicsLoaded.AddListener(() => _loadingScreen.Show(false));        

        // Listen to GameManager's Ramblings
        GameManager.Instance.FishCaughtUpdated.AddListener(OnFishCaughtUpdated);
        GameManager.Instance.GameStarting.AddListener(OnGameStarting);
        GameManager.Instance.GameStateUpdated.AddListener(OnGameStateUpdated);
        GameManager.Instance.NewBestScoreReached.AddListener(OnNewBestScoreReached);
        GameManager.Instance.NewBestScoreReached.AddListener(OnNewBestScoreReached);

        // Initialize the fish caught number text
        _fishCaughtNumberText.text = $"{GameManager.Instance.FishCaught}/{GameManager.Instance.FishTotalToCatch}";
    }

    private void Update()
    {
        //only run if timer object exists
        if (_timerText)
            _timerText.text = GameDataHandler.ConvertToTimeFormat(GameManager.Instance.Timer);
    }

    private void OnFishCaughtUpdated(int caught, int total)
    {
        _fishCaughtNumberText.text = $"{caught}/{total}";
    }

    private void OnGameStateUpdated(GameState newState)
    {
        // Set visibility of main menu
        _mainMenuSelectGUI.Show(newState == GameManager.Instance.MainMenuState);
        _mainMenuGUI.Show(newState == GameManager.Instance.MainMenuState);
        _mainMenuPrompt.SetInputPrompt(_mainMenuInput);
        _mainMenuSecondPrompt.SetInputPrompt(_mainMenuSecondInput);

        // Set visibility of encyclopedia
        _encyclopediaGUI.Show(newState == GameManager.Instance.EncyclopediaState);

        // Set visibility of game start menu
        _gameStartGUI.Show(newState == GameManager.Instance.GameStartState);

        // Game end is technically an extension of playing
        _gameplayGUI.Show(newState == GameManager.Instance.PlayingState 
            || newState == GameManager.Instance.GameEndState);

        // Set visibility of game end menu
        _gameEndGUI.Show(newState == GameManager.Instance.GameEndState);

        // End Score Menu
        bool isEndScoreState = (newState == GameManager.Instance.EndScoreState);
        _endScoreGUI.Show(isEndScoreState);
        _gameEndSelectGUI.Show(isEndScoreState);
        if (isEndScoreState) OnShowEndScore(); // Setup
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
    private void OnShowEndScore()
    {
        _gameEndSessionText.text = $"Nice Haul! {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.Timer)}";
        _gameEndBestText.color = _gameEndBestTextColorNormal;
        _gameEndBestText.text = $"Can you top your best of {GameManager.Instance.FishTotalToCatch} fish in {GameDataHandler.ConvertToTimeFormat(GameManager.Instance.CurrentGameData.BestTime)}?";
    }

    // Setup New Best details
    private void OnNewBestScoreReached()
    {
        _gameEndBestText.color = _gameEndBestTextColorNew;
        _gameEndBestText.text = "New personal best!";
    }
}

